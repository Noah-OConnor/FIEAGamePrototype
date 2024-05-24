using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float groundDrag = 1f;

    [SerializeField] private float sprintCooldown;
    [SerializeField] private bool sprinting = false;
    private bool canSprint = true;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool canJump = true;

    [Header("Ground Check Settings")]
    private bool isGrounded;
    [SerializeField] private LayerMask groundLayers;

    [Header("Slope Handling Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;
    private bool exitingSlope;

    private Vector3 movement;
    private Rigidbody rb;
    public Vector3 rbVelocity;

    public MovementState state;

    public enum MovementState
    {
        Idle,
        Moving,
        Jumping,
        Airborne
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void HandleDrag()
    {
        if (isGrounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    private void Update()
    {
        HandleGroundCheck();
        //SpeedControl();
        //StateHandler();
        HandleDrag();


        rbVelocity = rb.linearVelocity;

        movement = Vector3.zero;


        Vector2 input = InputManager.instance.Move;

        if (rbVelocity.magnitude < 0.1 && input == Vector2.zero)
        {
            //rb.linearVelocity = Vector3.zero;
        }

        // Calculate the movement direction based on the player's rotation
        movement = input.y * transform.forward + input.x * transform.right;

        if (input == Vector2.zero && isGrounded && canJump)
        {
            rb.useGravity = false;
            rb.linearDamping = 10f;
        }
        else
        {
            rb.linearDamping = 0f;
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
        float slopeAngle = 0;
        // Cast a ray downwards from the player's position to detect the ground
        RaycastHit hit;
        if (Physics.Raycast(rb.position + new Vector3(0, 0.1f, 0), Vector3.down, out hit, 1f))
        {
            // If the player is grounded, adjust the movement vector to match the slope
            movement = Vector3.ProjectOnPlane(movement, hit.normal);

            // Calculate the angle of the slope
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // If the slope is too steep, limit upward movement
            if (slopeAngle > 45)
            {
                // Calculate a slope factor based on the slope angle
                float slopeFactor = Mathf.InverseLerp(45, 90, slopeAngle);
                // Limit the upward movement based on the slope factor
                movement.y *= (1 - slopeFactor);
            }
        }

        Vector3 movementVector = movement * walkSpeed;

        // Apply the movement as a force
        rb.linearVelocity = movementVector;
    }

    private void HandleGroundCheck()
    {
        // Cast a ray downwards from the player's position to detect the ground
        RaycastHit hit;
        isGrounded = Physics.Raycast(rb.position + new Vector3(0, 0.1f, 0), Vector3.down, out hit, 0.35f);
    }

    private void HandleJump()
    {
        canJump = false;
        Invoke(nameof(JumpReset), 0.25f);
        // Add the jump force to the player's current movement vector
        movement += Vector3.up * jumpForce;
    }

    private void JumpReset()
    {
        print("Jump Reset");
        if (isGrounded)
        {
            canJump = true;
        }
        else
        {
            Invoke(nameof(JumpReset), 0.1f);
        }
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