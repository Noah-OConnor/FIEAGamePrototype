using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    public float knockbackForce = 10f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<PlayerHealth>().TakeDamage(1, transform, transform.forward, knockbackForce);
            //Debug.Log("Player hit by magic");
        }
        else if (other.CompareTag("Enemy"))
        {
            return;
        }
        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponentInChildren<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        Destroy(gameObject, 2f);
    }
}
