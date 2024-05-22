using Unity.Netcode;
using UnityEngine;

public class TestNetcodePlayer : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) return;
            
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector3.back;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector3.right;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector3.left;
        }

        float moveSpeed = 3f;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
