using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    const float EPS = 0.005f;

    // SETTINGS

    [Header("Modifiable values")]
    [SerializeField()]
    [Tooltip("Move Force")]
    private float moveForceAmplitude = 350f;
    [SerializeField()]
    [Tooltip("Max Ground Speed")]
    private float maxGroundSpeed = 10f;
    [SerializeField()]
    [Tooltip("Dash Force")]
    private float dashForceAmplitude = 600f;
    [SerializeField()]
    [Tooltip("Max Dash Speed")]
    private float maxDashSpeed = 20f;
    [SerializeField()]
    [Tooltip("Jump Force")]
    private float jumpForceAmplitude = 350f;
    [SerializeField()]
    [Tooltip("Extended Jump Force")]
    private float extendForceAmplitude = 30f;
    [SerializeField()]
    [Tooltip("Force-Time x? amplifier")]
    private AnimationCurve jumpForceAmplifierCurve;
    [SerializeField()]
    [Tooltip("Jump Input Window (s)")]
    private float jumpInputTimeWindow = 1.2f;

    [Header("Ground checks")]
    [SerializeField()]
    [Tooltip("Length of the RayCast for ground check")]
    private float groundCheckerLength = 0.05f;
    [SerializeField()]
    [Tooltip("Ground RayCast mask")]
    private LayerMask groundLayerMask;

    [Header("Collision checks")]
    [SerializeField()]
    [Tooltip("Player collider")]
    private CapsuleCollider2D m_Collider;
    [SerializeField()]
    [Tooltip("Length of the RayCast for slide check (above)")]
    private float aboveCheckerLength = 0.07f;
    [SerializeField()]
    [Tooltip("Length of the RayCast for wall")]
    private float wallCheckerLength = 0.1f;
    [SerializeField()]
    [Tooltip("Checks RayCast mask")]
    private LayerMask checksLayerMask;
    [SerializeField()]
    [Tooltip("gravity alternative wall")]
    private float gravityValueWall;

    [Header("Animation controls")]
    [SerializeField()]
    [Tooltip("Array, multiple animation states")]
    private AnimatorOverrideController[] animationArray;
    [SerializeField()]
    [Tooltip("Initial state for the animation controller")]
    private int initialAnimationOverride = 0;
    [SerializeField()]
    [Tooltip("Animator")]
    private Animator animator;
    [SerializeField()]
    [Tooltip("In-Combat Window (s)")]
    private float inCombatTimeWindow = 2.0f;

    // PRIVATE

    private bool isOrientationFixed = false; // Some moves might require the orientation to be fixed.
    
    // Indicator of the jump status.
    // False -  jump was not started / has ended.
    // True -   jump was started, no input commands interrupted
    private bool hasJumped = false;

    // Indicates the ground status.
    private bool hasLanded = false;

    // Attachedness to the wall
    private bool hasWallAttached = false;

    private bool hasAttacked = false;
    private bool hasCrouched = false;

    private float m_savedDefaultGravity;
    private Orientation orientation = Orientation.RightFacing;
    private Rigidbody2D m_RigidBody;
    private SpriteRenderer m_SpriteRenderer;
    private HashSet<ICommand> Commands;
    private Animator m_Animator;
    private int m_CurrentAnimOverride;
    private float m_TimerSinceJumpStarted = float.MaxValue;
    private float m_TimerSinceLastCombat = float.MaxValue;

    void Start() {
        m_RigidBody = this.GetComponent<Rigidbody2D>();
        if (m_RigidBody == null)
        {
            Debug.LogError("No Rigidbody2D component found!");
        }
        m_savedDefaultGravity = m_RigidBody.gravityScale;
        m_SpriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        if(m_SpriteRenderer == null) 
        {
            Debug.LogError("No SpriteRenderer component found!");
        }
        m_Animator = this.GetComponentInChildren<Animator>();
        if (m_Animator == null)
        {
            Debug.LogError("No Animator component found!");
        }
        
        Commands = new HashSet<ICommand>();

        if (m_Animator != null)
        {
            m_CurrentAnimOverride = initialAnimationOverride;
            SetAnimationOverride(animationArray[m_CurrentAnimOverride]);
        }
    }

    void Update()
    {
        RotatePlayerRenderer();
        HandleInput();

        CheckForwardContact();

        InputStateUpdate();
        AnimationState();

        TimerUpdates();

        Debug.Log("Horizontal Velocity - " + Mathf.Abs(m_RigidBody.velocity.x));
    }

    private void TimerUpdates()
    {
        m_TimerSinceJumpStarted += Time.deltaTime;
        m_TimerSinceLastCombat += Time.deltaTime;
    }

    /// <summary>
    /// All logic that is responsible for animation switching
    /// </summary>
    private void AnimationState()
    {
        /// ---- STATE ANIMS ----

        // Air animations
        if (hasLanded)
        {
            animator.SetBool("Grounded", true);
            animator.SetFloat("AirSpeed", m_RigidBody.velocity.y);
            float velocityHor = Mathf.Abs(m_RigidBody.velocity.x);
            // Idles and ground movement 
            if (velocityHor <= EPS) {
                // Standing, idle
                animator.SetInteger("AnimState", 0);
            } else if (velocityHor <= maxGroundSpeed) {
                // Move
                animator.SetInteger("AnimState", 2);
            } else if (velocityHor > maxGroundSpeed) {
                // Sprint
                animator.SetInteger("AnimState", 3);
            }
        }
        else {
            animator.SetBool("Grounded", false);
            animator.SetFloat("AirSpeed", m_RigidBody.velocity.y);
        }

        bool isInCombat = true;
        // In-combat state
        if (m_TimerSinceLastCombat > inCombatTimeWindow) {
            isInCombat = false;
        }
        if (isInCombat) {
            // Combat-Idle
            animator.SetInteger("AnimState", 1);
        }

        if (hasCrouched)
        {
            animator.SetInteger("AnimState", 4);
        }

        /// ---- ACTIVE ANIMS ----

        if (hasAttacked)
        {
            animator.SetTrigger("Attack");
        }

        if (hasWallAttached)
        {
            animator.SetBool("Attached", true);
        }
        else {
            animator.SetBool("Attached", false);
        }
    }

    /// <summary>
    /// All logic that is responsible for controlling the states ON/OFF, with no regard to physics, i.e. animation variables
    /// </summary>
    private void InputStateUpdate()
    {
        PlayerInput playerInput = ControlsManager.instance.GetInput();

        ActionInputUpdate(playerInput.actionInput);

        CrouchInputUpdate(playerInput.crouchInput);
    }

    private void CrouchInputUpdate(bool crouchInput)
    {
        if (crouchInput)
        {
            if (!hasWallAttached && hasLanded)
            {
                hasCrouched = true;
                return;
            }
        }
        hasCrouched = false;
    }

    private void ActionInputUpdate(bool actionInput)
    {
        if (actionInput)
        {
            // Is on ground, can attack
            if (!hasWallAttached && !hasCrouched && hasLanded)
            {
                hasAttacked = true;
                m_TimerSinceLastCombat = 0.0f;
                return;
            }
        }
        hasAttacked = false;    
    }

    private void FixedUpdate()
    {
        CheckGround();

        AlterGravity();

        IssueCommandPhysics();
    }

    private void AlterGravity()
    {
        if (hasWallAttached)
        {
            m_RigidBody.gravityScale = gravityValueWall;
        } else {
            m_RigidBody.gravityScale = m_savedDefaultGravity;
        }
    }

    /// <summary>
    /// This is responsible for issuing the physics based commands mapped to the input
    /// </summary>
    private void IssueCommandPhysics()
    {
        PlayerInput playerInput = ControlsManager.instance.GetInput();
        
        Commands.Clear();
        if (playerInput.axisInputs.x != 0)
        {
            if (playerInput.sprintInput) {
                Sprint(playerInput.axisInputs);
            }
            else
            {
                Move(playerInput.axisInputs);
            }
        }
    
        Jump(playerInput.jumpInput);

        // Execute the commands themselves
        foreach (var c in Commands)
        {
            c.execute();
        }
    }

    private void Move(Vector2 playerInput)
    {
        if (playerInput.x < 0)
        {
            Commands.Add(new MoveLeftCommand(m_RigidBody)
                .setMoveForceAmplitude(moveForceAmplitude * Mathf.Abs(playerInput.x))
                .setMaxSpeed(maxGroundSpeed));
        }

        if (playerInput.x > 0)
        {
            Commands.Add(new MoveRightCommand(m_RigidBody)
                .setMoveForceAmplitude(moveForceAmplitude * Mathf.Abs(playerInput.x))
                .setMaxSpeed(maxGroundSpeed));
        }
    }

    private void Sprint(Vector2 playerInput)
    {
        if (playerInput.x < 0)
        {
            Commands.Add(new MoveLeftCommand(m_RigidBody)
                .setMoveForceAmplitude(dashForceAmplitude * Mathf.Abs(playerInput.x))
                .setMaxSpeed(maxDashSpeed));
        }

        if (playerInput.x > 0)
        {
            Commands.Add(new MoveRightCommand(m_RigidBody)
                .setMoveForceAmplitude(dashForceAmplitude * Mathf.Abs(playerInput.x))
                .setMaxSpeed(maxDashSpeed));
        }
    }


    private void Jump(bool jumpInput)
    {
        if (jumpInput)
        {
            if (CheckUpperContact())
            {
                // If there's something right above, can't jump
                return;
            }

            if (CheckForwardContact())
            {
                // If there's something in front (wall), can't jump
                return;
            }

            // Has not jumped yet, stands on ground
            if (hasLanded)
            {
                hasJumped = true;
                hasLanded = false;

                // Ensure the yvel is ok with 0 assignment
                float xVel = m_RigidBody.velocity.x;
                m_RigidBody.velocity = new Vector2(xVel, 0f);

                Commands.Add(new JumpCommand(m_RigidBody)
                    .setJumpForceAmplitude(jumpForceAmplitude));
                m_TimerSinceJumpStarted = 0.0f;
                return;
            }

            // Jump was initiated some time before already, but time window had passed
            if (m_TimerSinceJumpStarted > jumpInputTimeWindow) {
                hasJumped = false;
                return;
            }
                
            // Jump was initiated some time before already, but was not finished/interrupted
            if (hasJumped)
            {
                // The jump window is available
                float sampleTime = m_TimerSinceJumpStarted / jumpInputTimeWindow;
                float jumpAmplifier = jumpForceAmplifierCurve.Evaluate(sampleTime);

                // Apply the smaller force
                Commands.Add(new JumpCommand(m_RigidBody)
                    .setJumpForceAmplitude(extendForceAmplitude * jumpAmplifier));
                return;
            }
        } else {
            if (hasJumped) {
                // Jump was interrupted
                hasJumped = false;
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the ray for ground checks
        Vector2 pos2D = m_Collider.offset - new Vector2(0.0f, m_Collider.size.y * .5f);
        Gizmos.color = hasLanded ? Color.yellow : Color.red; 
        Gizmos.DrawRay(transform.position + new Vector3(pos2D.x, pos2D.y, 0.0f), Vector2.down * groundCheckerLength);

        //Upper Contact Check
        pos2D = m_Collider.offset + new Vector2(0.0f, m_Collider.size.y * .5f);
        Gizmos.color = CheckUpperContact() ? Color.yellow : Color.red;
        Gizmos.DrawRay(transform.position + new Vector3(pos2D.x, pos2D.y, 0.0f), Vector3.up * aboveCheckerLength);

        //Forward Contact Check
        if (orientation == Orientation.RightFacing)
        {
            pos2D = m_Collider.offset + new Vector2(m_Collider.size.x * .5f, 0.0f);
        } else {
            pos2D = m_Collider.offset - new Vector2(m_Collider.size.x * .5f, 0.0f);
        }
        Gizmos.color = CheckForwardContact() ? Color.yellow : Color.green;
        Gizmos.DrawRay(transform.position + new Vector3(pos2D.x, pos2D.y, 0.0f), m_Collider.transform.right * wallCheckerLength);
    }

    private void SetAnimationOverride(AnimatorOverrideController overrideController)
    {
        this.m_Animator.runtimeAnimatorController = overrideController;
        return;
    }

    private void CheckGround()
    {
        Vector2 pos2D = m_Collider.offset - new Vector2(0.0f, m_Collider.size.y * .5f);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(pos2D.x, pos2D.y, 0.0f), Vector2.down, groundCheckerLength, groundLayerMask);
        if (hit.collider != null)
        {
            hasLanded = true;
            hasJumped = false;
        }else{
            hasLanded = false;
        }
    }

    bool CheckUpperContact()
    {
        Vector2 pos2D = m_Collider.offset + new Vector2(0.0f, m_Collider.size.y * .5f);
        RaycastHit2D hit = (Physics2D.Raycast(transform.position + new Vector3(pos2D.x, pos2D.y, 0.0f), Vector3.up, aboveCheckerLength, checksLayerMask));
        return hit;
    }

    bool CheckForwardContact()
    {
        Vector2 pos2D;
        if (orientation == Orientation.RightFacing)
        {
            pos2D = m_Collider.offset + new Vector2(m_Collider.size.x * .5f, 0.0f);
        }
        else
        {
            pos2D = m_Collider.offset - new Vector2(m_Collider.size.x * .5f, 0.0f);
        }
        RaycastHit2D hit = (Physics2D.Raycast(transform.position + new Vector3(pos2D.x, pos2D.y, 0.0f), m_Collider.transform.right, wallCheckerLength, checksLayerMask));

        if (hit.collider != null)
        {
            // Hit something filtered
            hasWallAttached = true;
        } else {
            hasWallAttached = false;
        }

        return hit;
    }

    /// <summary>
    /// Rotates the renderer for doubling the number of animations, based on the current inputs
    /// </summary>
    private void RotatePlayerRenderer()
    {
        PlayerInput playerInput = ControlsManager.instance.GetInput();
        
        if (Mathf.Abs(playerInput.axisInputs.x) <= EPS) {
            return;
        }

        if (playerInput.axisInputs.x > 0)
        {
            if (orientation == Orientation.LeftFacing)
            {
                orientation = Orientation.RightFacing;
                m_SpriteRenderer.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
                m_Collider.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
            }
        }
        else
        {
            if (orientation == Orientation.RightFacing)
            {
                orientation = Orientation.LeftFacing;
                m_SpriteRenderer.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
                m_Collider.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
            }
        }
    }

    private void HandleInput()
    {
        ControlsManager.instance.ProcessInput();
    }

    private enum Orientation
    {
        RightFacing,
        LeftFacing,
    }
}


