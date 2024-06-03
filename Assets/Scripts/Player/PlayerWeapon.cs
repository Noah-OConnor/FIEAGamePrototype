using UnityEngine;
using Unity.Netcode;

public class PlayerWeapon : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private int magazineCapacity = 30;
    [SerializeField] private float fireRate = 100f;

    public float maxBulletAngle;

    [Header("References")]
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform currentProjectilePrefab;
    [SerializeField] private Transform projectileSpawnTransform;

    [SerializeField] private Transform fakeProjectilePrefab;

    [SerializeField] private GameObject weaponMeshParent;

    [Header("Settings")]
    [SerializeField] private LayerMask aimColliderLayerMask;

    private int currentAmmo;
    private bool readyToShoot = true;
    private Vector3 mouseWorldPosition;
    private Vector3 cameraPosition;

    private PlayerEvents playerEvents;
    private NetworkObject networkObject;

    private void Start()
    {
        currentAmmo = magazineCapacity;
        cameraPosition = Camera.main.transform.position;
        playerEvents = GetComponent<PlayerEvents>();
        networkObject = GetComponentInParent<NetworkObject>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        weaponMeshParent.SetActive(true);
    }

    private void Update()
    {
        if (!IsOwner) return;
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
            readyToShoot = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnFakeProjectileServerRpc(Vector3 aimDirection, Vector3 initialPosition, float bulletSpeed, ulong ownerId)
    {
        Transform fakeProjectile = Instantiate(fakeProjectilePrefab, initialPosition, Quaternion.identity);
        fakeProjectile.GetComponent<FakeProjectile>().Initialize(aimDirection, initialPosition, bulletSpeed);
        fakeProjectile.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId);
    }

    private void Shoot()
    {
        Vector3 aimDirection = (mouseWorldPosition - projectileSpawnTransform.position).normalized;
        float angle = Vector3.Angle(aimDirection, transform.forward);
        bool isWithinMaxAngle = Mathf.Abs(angle) < maxBulletAngle;

        Vector3 initialPosition = isWithinMaxAngle ? projectileSpawnTransform.position : cameraPosition;
        aimDirection = isWithinMaxAngle ? aimDirection : (mouseWorldPosition - cameraPosition).normalized;

        ulong ownerId = GetComponentInParent<NetworkObject>().NetworkObjectId;

        Transform projectileTransform = Instantiate(currentProjectilePrefab, initialPosition, Quaternion.identity);
        projectileTransform.GetComponent<TestProjectile>().Initialize(aimDirection, initialPosition, isWithinMaxAngle, ownerId);

        SpawnFakeProjectileServerRpc(aimDirection, initialPosition, projectileTransform.GetComponent<TestProjectile>().bulletSpeed, NetworkObjectId);

        // Reduce the current ammo
        currentAmmo--;
        playerEvents.TriggerOnPlayerAmmoChanged(currentAmmo, magazineCapacity);

        // Trigger the OnPlayerShoot event
        playerEvents.TriggerOnPlayerShoot(networkObject.NetworkObjectId);

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
            playerEvents.TriggerOnPlayerAmmoChanged(currentAmmo, magazineCapacity);
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