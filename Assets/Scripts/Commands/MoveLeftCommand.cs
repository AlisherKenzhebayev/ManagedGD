using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLeftCommand : BaseCommand
{
    private float moveForceAmplitude = 100.0f;
    private float maxSpeed = 20.0f;

    public MoveLeftCommand(Rigidbody2D rb) : base()
    {
        this.rb = rb;
    }

    public MoveLeftCommand setMoveForceAmplitude(float value)
    {
        moveForceAmplitude = value;
        return this;
    }

    public MoveLeftCommand setMaxSpeed(float value)
    {
        maxSpeed = value;
        return this;
    }

    public override void execute()
    {
        Vector2 force = Vector2.left;
        force *= moveForceAmplitude;
        //rb.velocity = force;
        rb.AddForce(force, ForceMode2D.Force); // Force-based
        float yVel = rb.velocity.y;
        rb.velocity.Set(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), yVel);
    }
}
