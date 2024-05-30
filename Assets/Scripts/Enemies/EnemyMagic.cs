using UnityEngine;

public class EnemyMagic : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 200f; // degrees per second
    private Rigidbody rb;
    private Transform playerTransform;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

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
            Debug.Log("Player hit by magic");
        }
        MyDestroy();
    }

    private void MyDestroy()
    {
        GetComponent<Collider>().enabled = false;
        rb.isKinematic = true;
        Destroy(gameObject, 2f);
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }
}
