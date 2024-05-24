using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    private bool canJump = true;
    private bool isGrounded;
    private Vector3 movement;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleGroundCheck();

        Vector2 input = InputManager.instance.Move;

        // Calculate the movement direction based on the player's rotation
        movement = input.y * transform.forward + input.x * transform.right;

        if (input == Vector2.zero && isGrounded)
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }

        HandlePlayerMovement();

        if (InputManager.instance.WeaponPrimaryPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (InputManager.instance.WeaponSecondaryPressed)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (canJump && InputManager.instance.JumpPressed && isGrounded)
        {
            HandleJump();
        }
    }

    private void HandlePlayerMovement()
    {
        Vector3 newPosition = rb.position + movement * moveSpeed * Time.deltaTime;

        // Cast a ray downwards from the player's position to detect the ground
        RaycastHit hit;
        if (Physics.Raycast(rb.position + new Vector3(0, 0.1f, 0), Vector3.down, out hit, 1.5f))
        {
            // If the player is grounded, adjust the movement vector to match the slope
            movement = Vector3.ProjectOnPlane(movement, hit.normal);
        }

        rb.MovePosition(newPosition);
    }

    private void HandleGroundCheck()
    {
        // Cast a ray downwards from the player's position to detect the ground
        RaycastHit hit;
        isGrounded = Physics.Raycast(rb.position + new Vector3(0, 0.1f, 0), Vector3.down, out hit, 1.1f);
    }

    private void HandleJump()
    {
        canJump = false;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }
    }
}