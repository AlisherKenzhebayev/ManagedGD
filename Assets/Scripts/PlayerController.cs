using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Modifiable values")]
    [SerializeField()]
    [Tooltip("Move Force")]
    private float moveForceAmplitude = 350f;
    [SerializeField()]
    [Tooltip("Max Ground Speed")]
    private float maxGroundSpeed = 20f;
    [SerializeField()]
    [Tooltip("Jump Force")]
    private float jumpForceAmplitude = 350f;
    [SerializeField()]
    [Tooltip("Ground Check transform")]
    private Transform groundChecker;
    [SerializeField()]
    [Tooltip("Ground RayCast mask")]
    private LayerMask groundLayerMask;
    [SerializeField()]
    [Tooltip("Array, multiple animation states")]
    private AnimatorOverrideController[] animationArray;
    
    // Assuming the initial sprites are all right-facing
    private bool isOrientationFixed = false; // Some moves might require the orientation to be fixed.
    private bool isJumping = false;
    private Orientation orientation = Orientation.RightFacing;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private List<ICommand> Commands;
    private Animator m_Animator;
    private int m_CurrentAnimOverride = 0;

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
        
        Commands = new List<ICommand>();

        if (m_Animator != null)
        {
            SetAnimationOverride(animationArray[m_CurrentAnimOverride]);
        }
    }

    void SetAnimationOverride(AnimatorOverrideController overrideController) {
        this.m_Animator.runtimeAnimatorController = overrideController;
        return;
    }

    void Update()
    {
        RotatePlayerRenderer();
        HandleInput();

        Debug.Log("Horizontal Velocity - " + Mathf.Abs(rigidBody.velocity.x) + isJumping);
    }

    private void FixedUpdate()
    {
        CheckGround();

        PlayerInput playerInput = ControlsManager.instance.GetInput();
        Debug.LogError(playerInput.axisInputs +  " " + playerInput.jumpInput + " " + playerInput.actionInput );

        Commands.Clear();
        if (playerInput.axisInputs.x > 0)
        {
            Commands.Add(new MoveRightCommand(rigidBody)
                .setMoveForceAmplitude(moveForceAmplitude)
                .setMaxSpeed(maxGroundSpeed));
        }

        if (playerInput.axisInputs.x < 0)
        {
            Commands.Add(new MoveLeftCommand(rigidBody)
                .setMoveForceAmplitude(moveForceAmplitude)
                .setMaxSpeed(maxGroundSpeed));
        }

        if (!isJumping)
        {
            if (playerInput.jumpInput)
            {
                Commands.Add(new JumpCommand(rigidBody)
                    .setJumpForceAmplitude(jumpForceAmplitude)); // TODO: add low, extended jumps with Dotween
                isJumping = true;
            }
        }

        foreach (var c in Commands)
        {
            c.execute();
        }
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundChecker.position, Vector2.down, 0.05f, groundLayerMask);
        if (hit.collider != null)
        {
            isJumping = false;
        }else{
            isJumping = true;
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


