using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackAura : MonoBehaviour
{
    [Header("Animation values")]
    [SerializeField]
    private float SecRepeat = 5f;
    [SerializeField]
    private float ScaleMultiplierLow = 1f;
    [SerializeField]
    private float ScaleMultiplierHigh = 3f;
    [SerializeField]
    [Tooltip("Portion of time to expand")]
    [Range(0.0f, 1.0f)]
    private float PortionExpand = 0.7f;
    
    private float m_AnimTimer = 0f;

    private void Update()
    {
        m_AnimTimer += Time.deltaTime;

        if (m_AnimTimer > SecRepeat) {
            m_AnimTimer -= SecRepeat;
        }

        if (m_AnimTimer <= PortionExpand * SecRepeat)
        {
            transform.DOScale(ScaleMultiplierHigh, PortionExpand * SecRepeat - m_AnimTimer);
        }
        else
        {
            transform.DOScale(ScaleMultiplierLow, SecRepeat - m_AnimTimer);
        }
    }
}
