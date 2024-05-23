using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;

public class TestLobby : MonoBehaviour
{

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private string playerName;

    private async void Start()
    {
        DebugLogConsole.AddCommand("CreateLobby", "Create a lobby", CreateLobby);
        DebugLogConsole.AddCommand("ListLobbies", "Lists all Lobbies", ListLobbies);
        DebugLogConsole.AddCommand<string>("JoinLobby", "Joins a lobby", JoinLobbyByCode);
        DebugLogConsole.AddCommand("PrintPlayers", "Prints players in lobby", () => PrintPlayers(joinedLobby));
        DebugLogConsole.AddCommand<string>("UpdatePlayerName", "Updates player name", UpdatePlayerName);
        DebugLogConsole.AddCommand("LeaveLobby", "Leaves the lobby", LeaveLobby);
        DebugLogConsole.AddCommand("KickPlayer", "Kicks a player from the lobby", KickPlayer);
        DebugLogConsole.AddCommand("MigrateLobbyHost", "Migrates the lobby host", MigrateLobbyHost);
        DebugLogConsole.AddCommand("DeleteLobby", "Deletes the lobby", DeleteLobby);

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Nocx" + Random.Range(0, 1000);
        Debug.Log("Player Name: " + playerName);
    }

    public void HandleLobbyHeartbeat()
    {
        if (joinedLobby == null || hostLobby.HostId != AuthenticationService.Instance.PlayerId) return;
        SendHeartbeat();
    }

    private async void SendHeartbeat()
    {
        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
    }
   
    public void HandleLobbyPollForUpdates()
    {
        if (joinedLobby == null) return;
        SendLobbyPollForUpdates();
    }

    private async void SendLobbyPollForUpdates()
    {
        Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
        joinedLobby = lobby;
    }



    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "TestLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions 
            {
                IsPrivate = false,
                Player = GetPlayer(),
                //Data = new Dictionary<string, DataObject>
                //{
                //    {
                //        "LobbyName", new DataObject(DataObject.VisibilityOptions.Public, lobbyName)
                //    }
                //}
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;
            InvokeRepeating(nameof(HandleLobbyHeartbeat), 1f, 15f);
            InvokeRepeating(nameof(HandleLobbyPollForUpdates), 1f, 1.1f);

            Debug.Log(lobby.Name + " created: " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobbies in queryResponse.Results)
            {
                Debug.Log(lobbies.Name + " " + lobbies.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to list lobbies: " + e.Message);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            Debug.Log("Joined lobby: " + lobbyCode);

            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to join lobby: " + e.Message);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)
                }
            }
        };
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby: " + lobby.Players.Count);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)
                }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to update player: " + e.Message);
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

            joinedLobby = null;
            CancelInvoke(nameof(HandleLobbyHeartbeat));
            CancelInvoke(nameof(HandleLobbyPollForUpdates));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to leave lobby: " + e.Message);
        }
    }

    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to kick player: " + e.Message);
        }
    }

    private async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = hostLobby.Players[1].Id
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to migrate lobby host: " + e.Message);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await Lobbies.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to delete lobby: " + e.Message);
        }
    }
}
