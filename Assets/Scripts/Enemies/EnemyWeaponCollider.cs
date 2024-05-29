using UnityEngine;

public class EnemyWeaponCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //other.GetComponent<PlayerHealth>().TakeDamage(1);

            print(this.name + " hit player");
        }
    }
}
