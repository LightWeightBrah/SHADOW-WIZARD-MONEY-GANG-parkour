using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private LayerMask whatIsWall;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed;
    [SerializeField] private float maxClimbTime;
    private float climbCounter;
    private bool climbing;

    [Header("Climb jumping")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private float climbJumpUpForce;
    [SerializeField] private float climbJumpBackForce;
    [SerializeField] private int climbJumps;
    private int climbJumpsLeft;


    [Header("Detection")]
    [SerializeField] private float detectionLength;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float maxWallLookAngle;
    [SerializeField] private float minWallNomalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    [SerializeField] private float exitWallTime;
    private float exitWallCounter;

    private float wallLookAngle;
    private RaycastHit frontWallHit;
    private bool wallFront;
    private Transform lastWall;
    private Vector3 lastWallNormal;

    private void Update()
    {
        WallCheck();
        StateMachine();

        if (climbing && !exitingWall)
            ClimbingMovement();
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = (frontWallHit.transform != lastWall) || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNomalAngleChange;

        if((wallFront && newWall) || playerMovement.grounded)
        {
            climbCounter = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StateMachine()
    {
        //state - climbing
        if(wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbCounter > 0)
                StartClimbing();

            if (climbCounter > 0)
                climbCounter -= Time.deltaTime;
            if (climbCounter <= 0)
                StopClimbing();
        }
        else if(exitingWall)
        {
            if (climbing)
                StopClimbing();

            if (exitWallCounter > 0)
                exitWallCounter -= Time.deltaTime;
            if (exitWallCounter <= 0)
                exitingWall = false;
        }
        else
        {
            if (climbing)
                StopClimbing();
        }

        if(wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0)
            ClimbJump();
    }

    private void StartClimbing()
    {
        climbing = true;
        playerMovement.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;

        //idea - change camera fov
    }

    private void ClimbingMovement()
    {
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, climbSpeed, rigidbody.velocity.z);

        //idea - sound while climbing
    }

    private void StopClimbing()
    {
        climbing = false;
        playerMovement.climbing = false;

        //idea - particle effect when no longer climb
    }

    private void ClimbJump()
    {
        exitingWall = true;
        exitWallCounter = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
        rigidbody.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
