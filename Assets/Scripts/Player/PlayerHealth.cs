using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void TakeDamage(float damage, Transform source)
    {
        Debug.Log("Player took " + damage + " from " + source.name);
        health -= damage;
    }

    public void TakeDamage(float damage, Transform source, Vector3 Direction, float knockbackForce)
    {
        Debug.Log("Player took " + damage + " from " + source.name);
        Debug.Log("Player was knocked back with a force of " + knockbackForce + " in the direction of " + Direction);
        health -= damage;
        playerMovement.KnockbackPlayer(Direction, knockbackForce);
    }
}
