using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpCommand : BaseCommand
{
    private float jumpForceAmplitude = 1.0f;

    public JumpCommand(Rigidbody2D rb) : base()
    {
        this.rb = rb;
    }

    public JumpCommand setJumpForceAmplitude(float value)
    {
        jumpForceAmplitude = value;
        return this;
    }

    public override void execute()
    {
        Vector2 force = Vector2.up;
        force *= jumpForceAmplitude;
        //rb.velocity = force;
        rb.AddForce(force, ForceMode2D.Impulse); // Force-based
    }
}