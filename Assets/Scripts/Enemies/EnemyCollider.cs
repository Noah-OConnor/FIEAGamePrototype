using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
    [SerializeField] bool criticalCollider = false;

    private NetcodeAIMain aiMain;

    private void Awake()
    {
        aiMain = GetComponentInParent<NetcodeAIMain>();
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
