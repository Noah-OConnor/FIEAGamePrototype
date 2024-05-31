using UnityEngine;

public class EnemyWeaponCollider : MonoBehaviour
{
    public float knockBackForce = 10f;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 direction = (other.ClosestPoint(transform.position) - transform.position).normalized;
            other.GetComponentInParent<PlayerHealth>().TakeDamage(1, transform, direction, knockBackForce);
        }
    }
}