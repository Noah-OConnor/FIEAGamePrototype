using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
    [SerializeField] bool criticalCollider = false;

    private AIMain aiMain;

    private void Awake()
    {
        aiMain = GetComponentInParent<AIMain>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("PlayerProjectile"))
        {
            if (criticalCollider)
            {
                aiMain.TakeDamage(2);
            }
            else
            {
                aiMain.TakeDamage(1);
            }
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        if (criticalCollider)
        {
            aiMain.TakeDamage(damage * 2);
        }
        else
        {
            aiMain.TakeDamage(damage);
        }
    }
}
