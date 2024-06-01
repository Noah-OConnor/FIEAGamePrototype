using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public List<Transform> playerTransforms;

    public Transform cameraMain;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        cameraMain = Camera.main.transform;
    }

    public void AddPlayerTransform(Transform playerTransform)
    {
        playerTransforms.Add(playerTransform);
    }

    public void RemovePlayerTransform(Transform playerTransform)
    {
        playerTransforms.Remove(playerTransform);
    }
}
