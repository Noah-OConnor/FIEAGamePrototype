using UnityEngine;

public class TestProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private int damage = 10;
    private bool hasHit = false;

    [SerializeField] private Transform floatingNumberPrefab;
    [SerializeField] private Transform hitEffectGreenPrefab;
    [SerializeField] private Transform hitEffectRedPrefab;

    [SerializeField] private float showMeshDelay;

    private Vector3 originalPosition;

    private Rigidbody rb;
    private MeshRenderer meshRenderer;

    void Start()
    {
        originalPosition = transform.position;

        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        else
        {
            Debug.LogError("Rigidbody component missing from this gameobject. Add one.");
        }
    }

    private void Update()
    {
        if (!meshRenderer.enabled && (transform.position - originalPosition).sqrMagnitude > 0.1f)
        {
            meshRenderer.enabled = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        Vector3 hitPosition = transform.position; // Offset the hit position in the direction of the projectile's movement

        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyCollider>().TakeDamage(damage);

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