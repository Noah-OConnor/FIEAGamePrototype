using UnityEngine;
using Unity.Netcode;

public class EnemyMagic : NetworkBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 200f; // degrees per second
    public float knockbackForce = 10f;
    private Rigidbody rb;
    private Transform playerTransform;
    private NetworkObject networkObject;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();
    }

    public override void OnNetworkSpawn()
    {
        // Rotate the projectile to face the player
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position + new Vector3(0, 1, 0) - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Destroy the projectile after 3 seconds
        Invoke(nameof(MyDestroy), 3);
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic) return;
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position + new Vector3(0, 1, 0) - transform.position).normalized;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, turnSpeed * Time.deltaTime, 0.0f);

            rb.linearVelocity = newDirection * speed;
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<PlayerHealth>().TakeDamage(1, transform, transform.forward, knockbackForce);
            //Debug.Log("Player hit by magic");
        }
        else if (other.CompareTag("Enemy"))
        {
            return;
        }
        MyDestroy();
    }

    private void MyDestroy()
    {
        // spawn hit vfx
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        rb.isKinematic = true;
        Invoke(nameof(MyDespawnServerRpc), 1.5f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MyDespawnServerRpc()
    {
        networkObject.Despawn(true);
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }
}
