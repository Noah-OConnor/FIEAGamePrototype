using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class EnemySpawnTrigger : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject blocker;
    [SerializeField] private List<Transform> spawnPoints;

    private NetworkVariable<int> killCount = new NetworkVariable<int>(0);
    private NetworkVariable<bool> hasSpawned = new NetworkVariable<bool>(false);

    private void OnTriggerEnter(Collider other)
    {
        if (!hasSpawned.Value && other.CompareTag("Player"))
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (!IsServer) return;
                hasSpawned.Value = true;
                Transform enemyTransform = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation).transform; 
                enemyTransform.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
