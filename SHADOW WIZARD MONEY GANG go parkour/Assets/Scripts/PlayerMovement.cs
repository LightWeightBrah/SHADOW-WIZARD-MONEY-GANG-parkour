using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.Rendering;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float sprintngSpeed;
    private float moveSpeed;

    [SerializeField] private float groundDrag;

    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool readyToJump;

    [Header("Crouching")]
    [SerializeField] private float crouchingSpeed;
    [SerializeField] private float crouchingYScale;
    private float startYScale;


    [Header("GroundCheck")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    private bool grounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool tryingToExitSlope;

    [SerializeField] private Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private Rigidbody rigidbody;

    [SerializeField] private MovementState state;
    private enum MovementState
    {
        WALKING,
        SPRINTING,
        CROUCHING,
        AIR
    }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;

        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        GetInputs();
        SpeedControl();
        StateHandler();

        if (grounded)
            rigidbody.drag = groundDrag;
        else
            rigidbody.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void GetInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(KeyCode.Space) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //start crouching
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchingYScale, transform.localScale.z);
            rigidbody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        //stop crouching
        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        }
    }

    private void StateHandler()
    {
        if(Input.GetKey(KeyCode.LeftControl))
        {
            state = MovementState.CROUCHING;
            moveSpeed = crouchingSpeed;
        }
        else if(grounded && Input.GetKey(KeyCode.LeftShift))
        {
            state = MovementState.SPRINTING;
            moveSpeed = sprintngSpeed;
        }
        else if(grounded)
        {
            state = MovementState.WALKING;
            moveSpeed = walkingSpeed;
        }
        else //in air
        {
            state = MovementState.AIR;
        }
    }

    private void MovePlayer()
    {
        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(OnSlope() && !tryingToExitSlope) //on slope
        {
            rigidbody.AddForce(GetSlopeMoveDirection() * moveSpeed * 20.0f, ForceMode.Force);

            if (rigidbody.velocity.y > 0)
                rigidbody.AddForce(Vector3.down * 80.0f, ForceMode.Force);
        }
        else if(grounded) //on ground
            rigidbody.AddForce(moveDirection.normalized * moveSpeed * 10.0f, ForceMode.Force);
        else if(!grounded)
            rigidbody.AddForce(moveDirection.normalized * moveSpeed * airMultiplier * 10.0f, ForceMode.Force);

        //turn off gravity while on slope
        rigidbody.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        //limit speed on slope
        if (OnSlope() && !tryingToExitSlope)
        {
            if (rigidbody.velocity.magnitude > moveSpeed)
                rigidbody.velocity = rigidbody.velocity.normalized * moveSpeed;
        }
        else //limit speed on ground or in air
        {
            Vector3 flatVelocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);

            //limit velocity if neeeded
            if(flatVelocity.magnitude > moveSpeed) //if you go faster than your movement speed
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed; //you calculate what your max velocity would be
                rigidbody.velocity = new Vector3(limitedVelocity.x, rigidbody.velocity.y, limitedVelocity.z); //and apply it
            }
        }    

    }

    private void Jump()
    {
        tryingToExitSlope = true;

        //reset y velocity
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);

        rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        tryingToExitSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            Debug.Log($"slopeHit.normal {slopeHit.normal}");
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
        //since its direction also normalize it
    }
}
