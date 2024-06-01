using UnityEngine;
using Unity.Netcode;

public class FakeProjectile : NetworkBehaviour
{
    private float bulletSpeed = 100f; // This needs to be derived from a scriptable object later
    [SerializeField] private LayerMask collisionMask;
    private TrailRenderer trailRenderer;
    private bool stopped = false;

    public void Initialize(Vector3 aimDirection, Vector3 initialPosition, float bulletSpeed)
    {
        transform.forward = aimDirection;
        transform.position = initialPosition;
        this.bulletSpeed = bulletSpeed;
    }

    public override void OnNetworkSpawn()
    {
        trailRenderer = gameObject.GetComponent<TrailRenderer>();

        if (IsOwner)
        {
            trailRenderer.enabled = false;
        }

        Invoke(nameof(DespawnAndDestroy), 10f);
    }

    private void Update()
    {
        if (stopped) return;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, bulletSpeed * Time.deltaTime, collisionMask))
        {
            if (trailRenderer != null)
            {
                DespawnAndDestroy();
                trailRenderer.enabled = false;
            }
        }
        transform.position += transform.forward * Time.deltaTime * bulletSpeed;
    }

    private void DespawnAndDestroy()
    {
        stopped = true;
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
