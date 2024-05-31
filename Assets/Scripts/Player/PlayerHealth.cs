using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health;
    private PlayerMovement playerMovement;

    private PlayerEvents playerEvents;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        health = maxHealth;

        playerEvents = GetComponent<PlayerEvents>();
    }

    public void TakeDamage(float damage, Transform source)
    {
        Debug.Log("Player took " + damage + " from " + source.name);
        health -= damage;

        playerEvents.TriggerOnPlayerHealthChanged(health, maxHealth);
    }

    public void TakeDamage(float damage, Transform source, Vector3 Direction, float knockbackForce)
    {
        Debug.Log("Player took " + damage + " from " + source.name);
        Debug.Log("Player was knocked back with a force of " + knockbackForce + " in the direction of " + Direction);
        health -= damage;
        playerMovement.KnockbackPlayer(Direction, knockbackForce);

        playerEvents.TriggerOnPlayerHealthChanged(health, maxHealth);
    }
}
