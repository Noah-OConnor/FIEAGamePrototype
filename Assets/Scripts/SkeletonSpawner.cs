using UnityEngine;
using Unity.Netcode;

public class SkeletonSpawner : NetworkBehaviour
{
    public Transform skeletonPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!IsHost) return;
            Transform skeleton = Instantiate(skeletonPrefab, Vector3.zero, Quaternion.identity);
            skeleton.GetComponent<NetworkObject>().Spawn();
        }    
    }
}
