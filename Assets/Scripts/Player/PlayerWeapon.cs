using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] LayerMask aimColliderLayerMask;
    [SerializeField] Transform debugTransform;
    [SerializeField] Transform currentProjectilePrefab;
    [SerializeField] Transform projectileSpawnPosition;

    private bool readyToShoot = true;

    [SerializeField] private int magazineCapacity = 30;
    private int currentAmmo;
    [SerializeField] private float fireRate = 100f;
    private Vector3 mouseWorldPosition;

    private void Start()
    {
        currentAmmo = magazineCapacity;
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

        Vector3 aimDirection = (mouseWorldPosition - projectileSpawnPosition.position).normalized;

        // Create a new projectile
        Transform projectile = Instantiate(currentProjectilePrefab, projectileSpawnPosition.position,
            Quaternion.LookRotation(aimDirection, projectileSpawnPosition.forward));

        // Reduce the current ammo
        currentAmmo--;

        // Set the weapon to ready to shoot after the fire rate
        Invoke("ResetReadyToShoot", 60f / fireRate);
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
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }
    }
}
