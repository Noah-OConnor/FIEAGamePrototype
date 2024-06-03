using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System;


public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] public NetworkList<ulong> playerIds;

    private Transform cameraMain;

    public event Action OnGameManagerSpawned;

    private void Awake()
    {
        Instance = this;

        cameraMain = Camera.main.transform;

        playerIds = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        OnGameManagerSpawned?.Invoke();
    }

    public void AddPlayerId(ulong playerId)
    {
        AddPlayerIdServerRpc(playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerIdServerRpc(ulong playerId, ServerRpcParams rpcParams = default)
    {
        playerIds.Add(playerId);
    }

    public Transform GetCameraMain()
    {
        return cameraMain;
    }
}
