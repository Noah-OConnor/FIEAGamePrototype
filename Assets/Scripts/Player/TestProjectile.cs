using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class TestProjectile : NetworkBehaviour
{
    public float bulletSpeed = 100f; // This needs to be derived from a scriptable object later
    public float maxBulletRange = 100f;
    public int bulletDamage = 10;
    public LayerMask aimColliderLayerMask;
    public Transform floatingNumberPrefab;
    public Transform hitEffectGreenPrefab;
    public Transform hitEffectRedPrefab;
    public TrailRenderer trailRenderer;

    private Vector3 aimDirection;
    private Vector3 initialPosition;
    private bool isWithinMaxAngle;

    [SerializeField] private Transform fakeProjectilePrefab;

    public void Initialize(Vector3 aimDirection, Vector3 initialPosition, bool isWithinMaxAngle)
    {
        this.aimDirection = aimDirection;
        this.initialPosition = initialPosition;
        this.isWithinMaxAngle = isWithinMaxAngle;

        StartCoroutine(BulletImpactDelay());
    }

    private IEnumerator BulletImpactDelay()
    {
        float totalDistance = 0;

        Vector3 projectilePosition = initialPosition;

        if (isWithinMaxAngle)
        {
            trailRenderer.enabled = true;
            transform.forward = aimDirection;
        }

        while (totalDistance < maxBulletRange)
        {
            float travelDistance = isWithinMaxAngle ? bulletSpeed * Time.deltaTime : bulletSpeed * Time.deltaTime * 5;
            totalDistance += travelDistance;

            if (trailRenderer.enabled)
            {
                // Move the debug object to the projectile's position
                transform.position = projectilePosition;
            }

            if (Physics.Raycast(projectilePosition, aimDirection, out RaycastHit hit, travelDistance, aimColliderLayerMask))
            {
                // Check if the raycast hit an enemy
                if (hit.collider.gameObject.CompareTag("Enemy"))
                {
                    // Apply damage to the enemy
                    hit.collider.gameObject.GetComponent<EnemyCollider>().TakeDamage(bulletDamage);

                    // spawn damage number
                    Transform floatingNumber = Instantiate(floatingNumberPrefab, hit.point, Quaternion.identity);
                    floatingNumber.GetComponent<FloatingNumber>().SetNumber((int)bulletDamage);
                    // spawn hit enemy effect
                    Instantiate(hitEffectGreenPrefab, hit.point, Quaternion.identity);
                }
                else
                {
                    // spawn hit effect
                    Instantiate(hitEffectRedPrefab, hit.point, Quaternion.identity);
                }
                
                Destroy(gameObject, 0.1f);
                break;
            }
            // if the raycast didn't hit anything, move the projectile forward
            projectilePosition += aimDirection * travelDistance;

            yield return null;
        }
    }
}
