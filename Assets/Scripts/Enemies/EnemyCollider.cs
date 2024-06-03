using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
    [SerializeField] bool criticalCollider = false;

    private AIMain aiMain;

    private void Awake()
    {
        aiMain = GetComponentInParent<AIMain>();
    }

    public void TakeDamage(float damage, ulong ownerId)
    {
        if (criticalCollider)
        {
            aiMain.TakeDamage(damage * 2, ownerId);
        }
        else
        {
            aiMain.TakeDamage(damage, ownerId);
        }
    }
}
