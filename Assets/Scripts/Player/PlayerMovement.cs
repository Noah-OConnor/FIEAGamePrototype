using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private float moveSpeed = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float groundDrag = 1f;
    [SerializeField] private bool sprinting = false;
    private float desiredMoveSpeed;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool canJump = true;

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayers;
    private bool isGrounded;

    [Header("Slope Handling Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    private float currentSpeed;
    private bool useGravity = false;
    private bool increaseDragGradually = false;
    [SerializeField] private float gravityForce = 20f;
    private Vector3 movement;
    private Rigidbody rb;

    [Header("Debugging Info")]
    public float flatSpeed;
    public Vector3 rbVelocity;
    public MovementState state;

    public enum MovementState
    {
        idle,
        walk,
        sprint
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleGroundCheck();
        SpeedControl();
        StateHandler();
        HandleGravity();
        HandleJump();
        HandleMisc();

        flatSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        currentSpeed = rb.linearVelocity.magnitude;
        if (currentSpeed < 0.5 && isGrounded && state == MovementState.idle)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        if (rb.linearVelocity.y < -50)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -50, rb.linearVelocity.z);
        }
    }

    private void FixedUpdate()
    {
        HandlePlayerMovement();
    }

    private void StateHandler()
    {
        // sprint
        if (sprinting)
        {
            state = MovementState.sprint;
            desiredMoveSpeed = sprintSpeed;
        }
        // walk
        else if (InputManager.instance.Move != Vector2.zero)
        {
            state = MovementState.walk;
            desiredMoveSpeed = walkSpeed;
        }
        // idle
        else
        {
            state = MovementState.idle;
            desiredMoveSpeed = 0;
        }

        // air
        if (!isGrounded && OnSlope())
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            if (angle > maxSlopeAngle && angle != 0)
            {
                desiredMoveSpeed = 3;
            }
        }
        else if (!isGrounded)
        {
            increaseDragGradually = false;
            rb.linearDamping = 0;
        }

        moveSpeed = desiredMoveSpeed;
        
    }

    private bool OnSlope()
    {
        if (Physics.BoxCast(rb.position + new Vector3(0, 0.8f, 0), new Vector3(0.35f, 0.35f, 0.35f), Vector3.down, out RaycastHit hit, Quaternion.identity, 0.55f, groundLayers))
        {
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private void HandleMisc()
    {
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

        rbVelocity = rb.linearVelocity;

        if (InputManager.instance.SprintPressed && state != MovementState.idle)
        {
            sprinting = !sprinting;
        }
    }

    private void HandleGravity()
    {
        if (useGravity)
        {
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }
    }

    private void HandlePlayerMovement()
    {
        Vector2 input = InputManager.instance.Move;

        // Calculate the movement direction based on the player's rotation
        movement = input.y * transform.forward + input.x * transform.right;
        movement.Normalize();

        if (isGrounded && canJump)
        {
            useGravity = false;
            //rb.linearDamping = 10f;
        }
        else
        {
            //rb.linearDamping = 0f;
            useGravity = true;
        }

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeDirection(movement) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y < 0)
            {
                rb.AddForce(Vector3.down * 100f, ForceMode.Force);
            }
        }
        else if (isGrounded)                  // on ground
        {
            rb.AddForce(movement.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!isGrounded)                 // in air
        {
            rb.AddForce(movement.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private Vector3 GetSlopeDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal);
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)      // limiting speed on slopes
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else                // limiting speed on ground or in air
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void HandleGroundCheck()
    {
        // Cast a ray downwards from the player's position to detect the ground
        isGrounded = Physics.SphereCast(rb.position + new Vector3(0, 0.5f, 0), 0.45f, Vector3.down, out RaycastHit hit, 0.1f, groundLayers);
        //isGrounded = canJump;
    }

    private void HandleJump()
    {
        if (canJump && InputManager.instance.JumpHeld && isGrounded)
        {
            exitingSlope = true;
            canJump = false;
            Invoke(nameof(JumpReset), 0.7f);

            // reset y velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            increaseDragGradually = false;
            rb.linearDamping = 0;

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void JumpReset()
    {
        canJump = true;
        exitingSlope = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            //canJump = true;
            StartCoroutine(IncreaseDragGradually());
        }
    }

    private IEnumerator IncreaseDragGradually()
    {
        increaseDragGradually = true;
        float time = 0;
        while (time < 1)
        {
            if (!increaseDragGradually) yield break;

            rb.linearDamping = Mathf.Lerp(0, groundDrag, time);
            time += Time.deltaTime;
            yield return null;
        }
        rb.linearDamping = groundDrag;
    }
}