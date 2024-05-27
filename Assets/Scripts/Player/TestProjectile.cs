using UnityEngine;

public class TestProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private int damage = 10;
    private bool hasHit = false;

    [SerializeField] private Transform floatingNumberPrefab;
    [SerializeField] private Transform hitEffectGreenPrefab;
    [SerializeField] private Transform hitEffectRedPrefab;

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
        if (hasHit) return;
        hasHit = true;

        Vector3 hitPosition = transform.position - transform.forward * 0.5f; // Offset the hit position in the direction of the projectile's movement

        if (other.gameObject.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyCollider>().TakeDamage(damage);

            // spawn damage number
            Transform floatingNumber = Instantiate(floatingNumberPrefab, hitPosition, Quaternion.identity);
            floatingNumber.GetComponent<FloatingNumber>().SetNumber(damage);
            // spawn hit enemy effect
            Instantiate(hitEffectGreenPrefab, hitPosition, Quaternion.identity);
        }
        else
        {
            // spawn hit effect
            Instantiate(hitEffectRedPrefab, hitPosition, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
