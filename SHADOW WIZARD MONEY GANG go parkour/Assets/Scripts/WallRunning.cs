using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float wallJumpUpForce;
    [SerializeField] private float wallJumpSideForce;
    [SerializeField] private float wallClimbSpeed;
    [SerializeField] private float maxWallRunTime;

    private float wallRunTimer;

    [Header("Input")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode upwardsRunKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode downwardsRunKey = KeyCode.LeftControl;

    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Wall Detecion")]
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    [SerializeField] private float exitWallTime;

    private float exitWallCounter;
    private bool exitingWall;

    [Header("Gravity")]
    [SerializeField] private bool useGravityOnWallRunning;
    [SerializeField] private float gravityCounterForce;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerCamera playerCamera;

    private PlayerMovement playerMovement;
    private Rigidbody rigidbody;

    [Header("Camera effects")]
    [SerializeField] private float cameraTilt;
    [SerializeField] private float cameraStartFov;
    [SerializeField] private float cameraEndFov;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (playerMovement.wallRunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        //State wallrunning
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            //start wallrun
            if (!playerMovement.wallRunning)
                StartWallRun();

            //wall jump
            if (Input.GetKeyDown(jumpKey))
                WallJump();
        }
        else if(exitingWall)
        {
            if (playerMovement.wallRunning)
                StopWallRun();

            if (exitWallCounter > 0)
                exitWallCounter -= Time.deltaTime;

            if (exitWallCounter <= 0)
                exitingWall = false;
        }
        else
        {
            if (playerMovement.wallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        playerMovement.wallRunning = true;
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);

        //camera effects
        playerCamera.DoFov(cameraEndFov);
        if (wallLeft)
            playerCamera.DoTilt(-cameraTilt);
        if (wallRight)
            playerCamera.DoTilt(cameraTilt);
    }

    private void WallRunningMovement()
    {
        rigidbody.useGravity = useGravityOnWallRunning;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((orientation.forward - wallForward).magnitude > (orientation.forward - (-wallForward)).magnitude)
        {
            wallForward = -wallForward;
        }

        //forward force
        rigidbody.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //upwards/downwards force
        if (upwardsRunning)
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, wallClimbSpeed, rigidbody.velocity.z);
        if (downwardsRunning)
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, -wallClimbSpeed, rigidbody.velocity.z);

        //push player to wall
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rigidbody.AddForce(-wallNormal * 100.0f, ForceMode.Force);

        //weaken gravity
        if (useGravityOnWallRunning)
            rigidbody.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        playerMovement.wallRunning = false;

        //reset camera effects
        playerCamera.DoFov(cameraStartFov);
        playerCamera.DoTilt(0f);
    }

    private void WallJump()
    {
        exitingWall = true;
        exitWallCounter = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
        rigidbody.AddForce(forceToApply, ForceMode.Impulse);

    }

}
