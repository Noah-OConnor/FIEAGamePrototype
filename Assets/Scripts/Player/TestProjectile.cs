using UnityEngine;

public class TestProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private bool hasHit = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        else
        {
            Debug.LogError("Rigidbody component missing from this gameobject. Add one.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") && !hasHit)
        {
            hasHit = true;
            other.GetComponent<EnemyCollider>().TakeDamage(1);
        }
        Destroy(gameObject);
    }   
}
