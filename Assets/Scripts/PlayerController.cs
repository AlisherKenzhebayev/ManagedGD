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

    // Assuming the initial sprites are all right-facing
    private bool isOrientationFixed = false; // Some moves might require the orientation to be fixed.
    private bool isJumping = false;
    private Orientation orientation = Orientation.RightFacing;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private List<ICommand> Commands;
    private Animator Animator;

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
        Animator = this.GetComponentInChildren<Animator>();
        if (Animator == null)
        {
            Debug.LogError("No Animator component found!");
        }

        Commands = new List<ICommand>();
    }

    void Update()
    {
        HandleInput();
        RotatePlayerRenderer();

        Debug.Log("Horizontal Velocity - " + Mathf.Abs(rigidBody.velocity.x) + isJumping);
    }

    private void FixedUpdate()
    {
        CheckGround();
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundChecker.position, Vector2.down, 0.05f);
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
        Commands.Clear();

        if (Input.GetAxisRaw(GameConstants.k_AxisNameHorizontal) > 0)
        {
            Commands.Add(new MoveRightCommand(rigidBody)
                .setMoveForceAmplitude(moveForceAmplitude)
                .setMaxSpeed(maxGroundSpeed));
        }

        if (Input.GetAxisRaw(GameConstants.k_AxisNameHorizontal) < 0)
        {
            Commands.Add(new MoveLeftCommand(rigidBody)
                .setMoveForceAmplitude(moveForceAmplitude)
                .setMaxSpeed(maxGroundSpeed));
        }

        if (Input.GetButton(GameConstants.k_ButtonNameJump) && !isJumping)
        {
            Commands.Add(new JumpCommand(rigidBody)
                .setJumpForceAmplitude(jumpForceAmplitude));
            isJumping = true;
        }

        foreach (var c in Commands) {
            c.execute();
        }
    }
    
    private enum Orientation
    {
        RightFacing,
        LeftFacing,
    }
}


