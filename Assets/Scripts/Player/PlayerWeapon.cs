using System.Collections;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxBulletRange = 100f;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int bulletDamage = 10;
    [SerializeField] private int magazineCapacity = 30;
    [SerializeField] private float fireRate = 100f;

    public float maxBulletAngle;

    [Header("References")]
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform currentProjectilePrefab;
    [SerializeField] private Transform projectileSpawnTransform;
    [SerializeField] private Transform floatingNumberPrefab;
    [SerializeField] private Transform hitEffectGreenPrefab;
    [SerializeField] private Transform hitEffectRedPrefab;

    [Header("Settings")]
    [SerializeField] private LayerMask aimColliderLayerMask;

    private int currentAmmo;
    private bool readyToShoot = true;
    private Vector3 mouseWorldPosition;
    private Vector3 cameraPosition;

    private void Start()
    {
        currentAmmo = magazineCapacity;
        cameraPosition = Camera.main.transform.position;
    }

    private void Update()
    {
        HandleWeaponPrimary();
        DebugRayCast();
        HandleReload();
    }

    private void HandleWeaponPrimary()
    {
        // Check if the weapon is ready to shoot and there's ammo left
        if (readyToShoot && currentAmmo > 0 && InputManager.instance.WeaponPrimaryHeld)
        {
            // Call the Shoot method directly
            Shoot();
        }
    }

    private void Shoot()
    {
        // Set the weapon to not ready to shoot
        readyToShoot = false;

        Vector3 aimDirection = (mouseWorldPosition - projectileSpawnTransform.position).normalized;
        float angle = Vector3.Angle(aimDirection, transform.forward);
        bool isWithinMaxAngle = Mathf.Abs(angle) < maxBulletAngle;

        Vector3 initialPosition = isWithinMaxAngle ? projectileSpawnTransform.position : cameraPosition;
        aimDirection = isWithinMaxAngle ? aimDirection : (mouseWorldPosition - cameraPosition).normalized;

        // Start a coroutine to delay the bullet's impact
        StartCoroutine(BulletImpactDelay(aimDirection, initialPosition, isWithinMaxAngle));

        // Reduce the current ammo
        currentAmmo--;

        // Set the weapon to ready to shoot after the fire rate
        Invoke("ResetReadyToShoot", 60f / fireRate);
    }

    private IEnumerator BulletImpactDelay(Vector3 aimDirection, Vector3 initialPosition, bool isWithinMaxAngle)
    {
        //print(isWithinMaxAngle);
        float totalDistance = 0;

        Vector3 projectilePosition = initialPosition;

        Transform bulletTransform = null;

        if (isWithinMaxAngle)
        {
            bulletTransform = Instantiate(currentProjectilePrefab, projectilePosition, Quaternion.identity);
            // rotate the bullet to face the direction it's moving
            bulletTransform.forward = aimDirection;
        }

        while (totalDistance < maxBulletRange)
        {
            float travelDistance = isWithinMaxAngle ? bulletSpeed * Time.deltaTime : bulletSpeed * Time.deltaTime * 5;
            totalDistance += travelDistance;

            if (bulletTransform != null)
            {
                // Move the debug object to the projectile's position
                bulletTransform.position = projectilePosition;
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

                if (isWithinMaxAngle) Destroy(bulletTransform.gameObject, 0.1f);

                break;
            }
            // if the raycast didn't hit anything, move the projectile forward
            projectilePosition += aimDirection * travelDistance;

            yield return null;
        }
    }

    private void ResetReadyToShoot()
    {
        readyToShoot = true;
    }

    private void HandleReload()
    {
        if (InputManager.instance.ReloadPressed)
        {
            currentAmmo = magazineCapacity;
        }
    }

    private void DebugRayCast()
    {   // this allows an object to appear where ever the center of the player's screen is
        // this allowed me to make sure that the crosshair on the screen matched up with where the player is shooting
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }
    }
}