using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    public float health;
    public float speed;
    public float sightRange;
    public float alertRange;
    public float sightAngle;
}
