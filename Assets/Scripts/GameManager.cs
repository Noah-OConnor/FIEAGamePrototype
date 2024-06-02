using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System;


public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public NetworkVariable<List<ulong>> playerIds = new NetworkVariable<List<ulong>>(default);

    private Transform cameraMain;

    public event EventHandler OnValueChanged;

    private void Awake()
    {
        Instance = this;

        cameraMain = Camera.main.transform;
    }

    private void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        playerIds.OnValueChanged += OnPlayerTransformsChanged;
    }

    private void OnPlayerTransformsChanged(List<ulong> previousValue, List<ulong> newValue)
    {
        OnValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddPlayerTransform(NetworkObjectReference playerTransform)
    {
        playerIds.Value.Add(playerTransform.NetworkObjectId);

        print(playerIds.Value[0]);
    }

    public void RemovePlayerTransform(NetworkObjectReference playerTransform)
    {
        playerIds.Value.Remove(playerTransform.NetworkObjectId);
    }

    public Transform GetCameraMain()
    {
        return cameraMain;
    }
}
