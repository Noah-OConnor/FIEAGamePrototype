using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
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
