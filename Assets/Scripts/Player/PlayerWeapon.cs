using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

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

    [SerializeField] private GameObject weaponMeshParent;

    [Header("Settings")]
    [SerializeField] private LayerMask aimColliderLayerMask;

    private int currentAmmo;
    private bool readyToShoot = true;
    private Vector3 mouseWorldPosition;
    private Vector3 cameraPosition;

    private PlayerEvents playerEvents;

    private NetworkVariable<Vector3> aimDirection = new NetworkVariable<Vector3>(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> initialPosition = new NetworkVariable<Vector3>(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isWithinMaxAngle = new NetworkVariable<bool>(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Start()
    {
        currentAmmo = magazineCapacity;
        cameraPosition = Camera.main.transform.position;
        playerEvents = GetComponent<PlayerEvents>();
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

    private void Shoot()
    {
        aimDirection.Value = (mouseWorldPosition - projectileSpawnTransform.position).normalized;
        float angle = Vector3.Angle(aimDirection.Value, transform.forward);
        isWithinMaxAngle.Value = Mathf.Abs(angle) < maxBulletAngle;

        initialPosition.Value = isWithinMaxAngle.Value ? projectileSpawnTransform.position : cameraPosition;
        aimDirection.Value = isWithinMaxAngle.Value ? aimDirection.Value : (mouseWorldPosition - cameraPosition).normalized;

        // Start a coroutine to delay the bullet's impact
        if (!IsHost)
        { 
            Transform testProjectile = Instantiate(currentProjectilePrefab, initialPosition.Value, Quaternion.identity);
            testProjectile.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, true);
            testProjectile.GetComponent<TestProjectile>().Initialize(aimDirection.Value, initialPosition.Value, isWithinMaxAngle.Value);
        }
        else
        {
            //SpawnBulletServerRpc();
        }

        // Reduce the current ammo
        currentAmmo--;
        playerEvents.TriggerOnPlayerAmmoChanged(currentAmmo, magazineCapacity);

        // Trigger the OnPlayerShoot event
        playerEvents.TriggerOnPlayerShoot();

        // Set the weapon to ready to shoot after the fire rate
        Invoke("ResetReadyToShoot", 60f / fireRate);
    }

    [ServerRpc]
    private void SpawnBulletServerRpc()
    {
        // Start a coroutine to delay the bullet's impact
        Transform testProjectile = Instantiate(currentProjectilePrefab, initialPosition.Value, Quaternion.identity);
        testProjectile.GetComponent<NetworkObject>().Spawn(true);
        testProjectile.GetComponent<TestProjectile>().Initialize(aimDirection.Value, initialPosition.Value, isWithinMaxAngle.Value);
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