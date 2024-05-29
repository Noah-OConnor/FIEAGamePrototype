using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWeaponStats", menuName = "Scriptable Objects/EnemyWeaponStats")]
public class EnemyWeaponStats : ScriptableObject
{
    public Weapons weaponType;
    public float damage;
    public float speed;
    public float meleeRange;
    public float rangedRange; // for weapons with multiple attack ranges
    public float cooldown;
    public float knockback;

    public enum Weapons
    {
        unarmed,
        shield,
        sword,
        axe,
        crossbow,
        staff
    }
}