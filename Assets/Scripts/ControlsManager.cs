using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsManager : MonoBehaviour
{
    private static ControlsManager controlsManager;
    private static PlayerInput input;

    [Header("Android Inputs")]
    [SerializeField]
    private FloatingJoystick m_FloatingJoystick;

    private ControlsManager() { }

    public static ControlsManager instance
    {
        get
        {
            if (!controlsManager)
            {
                controlsManager = FindObjectOfType(typeof(ControlsManager)) as ControlsManager;
                if (!controlsManager)
                {
                    Debug.LogError("There needs to be one active ControlsManager script on a GameObject in your scene.");
                }
                else
                {
                    controlsManager.Init();
                    DontDestroyOnLoad(controlsManager);
                }
            }
            return controlsManager;
        }
    }

    void Init()
    {
        Debug.LogError("Init");
        input = new PlayerInput();

        ResetInput();
    }

    private void ResetInput()
    {
        input.axisInputs = Vector2.zero;
        input.actionInput = false;
        input.crouchInput = false;
        input.jumpInput = false;
        input.sprintInput = false;
    }

    /// <summary>
    /// Centralized point for processing and recording all user inputs
    /// </summary>
    /// <returns></returns>
    public void ProcessInput()
    {
        ProcessMovement();

        ProcessAction();
        ProcessJump();
        ProcessSprint();
        ProcessCrouch();

        return;
    }

    private void ProcessCrouch()
    {
        input.crouchInput = false;

#if UNITY_STANDALONE_WIN
        Debug.LogWarning("WINDOWS-Crouch");
        // Input methods for the Win platform
        if (Input.GetButton(GameConstants.k_ButtonNameJump))
        {
            input.crouchInput = true;
        }
#else 
        Debug.LogWarning("ANDROID-Crouch");
        // TODO: Write input processing for android, simplified.
#endif
    }

    private void ProcessSprint()
    {
        input.sprintInput = false;

#if UNITY_STANDALONE_WIN
        Debug.LogWarning("WINDOWS-Sprint");
        // Input methods for the Win platform
        if (Input.GetButton(GameConstants.k_ButtonNameJump))
        {
            input.sprintInput = true;
        }
#else 
        Debug.LogWarning("ANDROID-Sprint");
        // TODO: Write input processing for android, simplified.
#endif
    }

    public PlayerInput GetInput()
    {
        return input;
    }

    private void ProcessJump()
    {
        input.jumpInput = false;

#if UNITY_STANDALONE_WIN
        Debug.LogWarning("WINDOWS-Jump");
        // Input methods for the Win platform
        if (Input.GetButton(GameConstants.k_ButtonNameJump))
        {
            input.jumpInput = true;
        }
#else 
        Debug.LogWarning("ANDROID-Jump");
        // TODO: Write input processing for android, simplified.
#endif
    }

    private void ProcessAction()
    {
        input.actionInput = false;

#if UNITY_STANDALONE_WIN
        Debug.LogWarning("WINDOWS-Action");
        // Input methods for the Win platform
        if (Input.GetButton(GameConstants.k_ButtonNameAction))
        {
            input.actionInput = true;
        }
#else 
        Debug.LogWarning("ANDROID-Action");
        // TODO: Write input processing for android, simplified.
#endif
    }

    private void ProcessMovement()
    {
        input.axisInputs = Vector2.zero;

#if UNITY_STANDALONE_WIN
        Debug.LogWarning("WINDOWS-Movement");
        // Input methods for the Win platform
        if (Input.GetAxisRaw(GameConstants.k_AxisNameHorizontal) > 0)
        {
            input.axisInputs += Vector2.right;
        }

        if (Input.GetAxisRaw(GameConstants.k_AxisNameHorizontal) < 0)
        {
            input.axisInputs += Vector2.left;
        }

        if (Input.GetAxisRaw(GameConstants.k_AxisNameVertical) > 0)
        {
            input.axisInputs += Vector2.up;
        }

        if (Input.GetAxisRaw(GameConstants.k_AxisNameVertical) < 0)
        {
            input.axisInputs += Vector2.down;
        }
#else 
        Debug.LogWarning("ANDROID-Movement");
        // TODO: Write input processing for android, simplified.
        input.axisInputs = m_FloatingJoystick.Direction;
#endif
    }
}
