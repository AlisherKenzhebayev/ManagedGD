using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField()]
    [Tooltip("Effect multiplier X")]
    public float parallaxMulX = 0.5f;

    [SerializeField()]
    [Tooltip("Effect multiplier Y")]
    public float parallaxMulY = 0.5f;

    [SerializeField()]
    [Tooltip("Scrolling (false by default)")]
    public bool scrolling = false;

    private Transform cameraTransform;
    private Vector3 cameraPositionLast;

    private float texUnitsSizeX;

    void Start()
    {
        this.cameraTransform = Camera.main.transform;
        this.cameraPositionLast = cameraTransform.position;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        texUnitsSizeX = texture.width / sprite.pixelsPerUnit;
    }

    void LateUpdate()
    {
        Vector3 movementDelta = cameraTransform.position - cameraPositionLast;
        this.transform.position += Vector3.Scale(movementDelta, new Vector3(parallaxMulX, parallaxMulY, 1.0f));
        cameraPositionLast = cameraTransform.position;

        if (scrolling && Mathf.Abs(cameraTransform.position.x - transform.position.x) >= texUnitsSizeX) {
            float offsetPosX = (cameraTransform.position.x - transform.position.x) % texUnitsSizeX; 

            transform.position = new Vector3(cameraTransform.position.x + offsetPosX, transform.position.y);
        }
    }
}
