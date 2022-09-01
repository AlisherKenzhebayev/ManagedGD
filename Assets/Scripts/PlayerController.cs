using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
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

    [Header("Animation controls")]
    [SerializeField()]
    [Tooltip("Array, multiple animation states")]
    private AnimatorOverrideController[] animationArray;
    [SerializeField()]
    [Tooltip("Initial state for the animation controller")]
    private int initialAnimationOverride = 0;

    // PRIVATE

    private bool isOrientationFixed = false; // Some moves might require the orientation to be fixed.
    
    // Indicator of the jump status.
    // False -  jump was not started / has ended.
    // True -   jump was started, no input commands interrupted
    private bool hasJumped = false;
    
    // Indicates the ground status.
    private bool hasLanded = false; 
    
    private Orientation orientation = Orientation.RightFacing;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private HashSet<ICommand> Commands;
    private Animator m_Animator;
    private int m_CurrentAnimOverride;
    private float m_TimerSinceJumpStarted = float.MaxValue;

    void Start() {
        rigidBody = this.GetComponent<Rigidbody2D>();
        if (rigidBody == null)
        {
            Debug.LogError("No Rigidbody2D component found!");
        }
        spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        if(spriteRenderer == null) 
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

        InputState();
        AnimationState();

        TimerUpdates();

        Debug.Log("Horizontal Velocity - " + Mathf.Abs(rigidBody.velocity.x));
    }

    private void TimerUpdates()
    {
        m_TimerSinceJumpStarted += Time.deltaTime;
    }

    /// <summary>
    /// All logic that is responsible for animation switching
    /// </summary>
    private void AnimationState()
    { 

    }

    /// <summary>
    /// All logic that is responsible for controlling the states ON/OFF
    /// </summary>
    private void InputState()
    {

    }

    private void FixedUpdate()
    {
        CheckGround();

        IssueCommandPhysics();
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
            Move(playerInput.axisInputs);
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
            Commands.Add(new MoveLeftCommand(rigidBody)
                .setMoveForceAmplitude(moveForceAmplitude)
                .setMaxSpeed(maxGroundSpeed));
        }

        if (playerInput.x > 0)
        {
            Commands.Add(new MoveRightCommand(rigidBody)
                .setMoveForceAmplitude(moveForceAmplitude)
                .setMaxSpeed(maxGroundSpeed));
        }
    }

    private void Jump(bool jumpInput)
    {
        if (jumpInput)
        {
            // Has not jumped yet, stands on ground
            if (hasLanded)
            {
                hasJumped = true;
                hasLanded = false;

                Commands.Add(new JumpCommand(rigidBody)
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
                Commands.Add(new JumpCommand(rigidBody)
                    .setJumpForceAmplitude(extendForceAmplitude * jumpAmplifier));
                return;
            }
        }
        else {
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
        Gizmos.color = Color.green;
        Vector2 direction = Vector2.down * groundCheckerLength;
        Gizmos.DrawRay(this.transform.position, direction);
    }

    private void SetAnimationOverride(AnimatorOverrideController overrideController)
    {
        this.m_Animator.runtimeAnimatorController = overrideController;
        return;
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, Vector2.down, groundCheckerLength, groundLayerMask);
        if (hit.collider != null)
        {
            hasLanded = true;
            hasJumped = false;
        }else{
            hasLanded = false;
        }
    }

    /// <summary>
    /// Rotates the renderer for doubling the number of animations, based on the current horizontal speed
    /// </summary>
    private void RotatePlayerRenderer()
    {
        float velocity = rigidBody.velocity.x;

        if (Mathf.Abs(velocity) <= 0.005f) {
            return;
        }

        if (velocity > 0)
        {
            if (orientation == Orientation.LeftFacing)
            {
                orientation = Orientation.RightFacing;
                spriteRenderer.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
            }
        }
        else
        {
            if (orientation == Orientation.RightFacing)
            {
                orientation = Orientation.LeftFacing;
                spriteRenderer.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
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


