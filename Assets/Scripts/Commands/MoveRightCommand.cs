using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRightCommand : BaseCommand
{
    private float moveForceAmplitude = 100.0f;
    private float maxSpeed = 20.0f;

    public MoveRightCommand(Rigidbody2D rb) : base()
    {
        this.rb = rb;
    }

    public MoveRightCommand setMoveForceAmplitude(float value)
    {
        moveForceAmplitude = value;
        return this;
    }
    public MoveRightCommand setMaxSpeed(float value)
    {
        maxSpeed = value;
        return this;
    }

    public override void execute()
    {
        Vector2 force = Vector2.right;
        force *= moveForceAmplitude;
        
        if (Mathf.Abs(rb.velocity.x) < maxSpeed)
        {
            rb.AddForce(force, ForceMode2D.Force); // Force-based
        }

        float yVel = rb.velocity.y;
        rb.velocity.Set(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), yVel);
    }
}