using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField()]
    [Tooltip("Effect multiplier")]
    private float parallaxEffectMultiplier = 0.5f;

    private Transform cameraTransform;
    private Vector3 cameraPositionLast;

    void Start()
    {
        this.cameraTransform = Camera.main.transform;
        this.cameraPositionLast = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 movementDelta = cameraTransform.position - cameraPositionLast;
        this.transform.position += movementDelta * parallaxEffectMultiplier;
        cameraPositionLast = cameraTransform.position;
    }
}
