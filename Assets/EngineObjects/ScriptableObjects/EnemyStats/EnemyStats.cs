using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Main")]
    public float health;
    public float sightRange;
    public float alertRange;
    public float sightAngle;

    [Header("Wander")]
    public Vector2 wanderIntervalRange;
    public float wanderRadius;
    public float wanderSpeed;

    [Header("Search")]
    public float searchDuration;
    public float searchRadius;
    public float searchSpeed;

    [Header("Chase")]
    public float chaseSpeed;
    public float rotationSpeed;

    [Header("Prefabs")]
    public Transform arrowPrefab;
    public Transform magicPrefab;
    public Transform minionPrefab;

    [Header("Weapons")]
    public EnemyWeaponStats unarmedWeapon;
}
