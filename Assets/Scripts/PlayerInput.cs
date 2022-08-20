using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A very simplified input class
/// Only has directional inputs, jump, sprint, crouch and action for everything else (can be contextual)
/// </summary>
public class PlayerInput
{
    public Vector2 axisInputs;
    public bool jumpInput;
    public bool actionInput;
    public bool crouchInput;
    public bool sprintInput;
}
