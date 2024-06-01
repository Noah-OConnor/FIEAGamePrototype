using UnityEngine;
using Unity.Netcode;

public class FakeProjectile : NetworkBehaviour
{
    private float bulletSpeed = 10f;
    [SerializeField] private LayerMask collisionMask;

    public void Initialize(Vector3 aimDirection, Vector3 initialPosition, float bulletSpeed)
    {
        transform.forward = aimDirection;
        transform.position = initialPosition;
        this.bulletSpeed = bulletSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameObject.GetComponent<TrailRenderer>().enabled = false;
        }
    }

    private void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, bulletSpeed * Time.deltaTime, collisionMask))
        {
            if (IsHost)
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
        transform.position += transform.forward * Time.deltaTime * bulletSpeed;
    }
}
