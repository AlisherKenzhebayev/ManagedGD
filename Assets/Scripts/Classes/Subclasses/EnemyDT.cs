using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroy the specified GO.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EnemyDT : DamageTaker
{
    [SerializeField]
    private AudioClip clipToPlay;
    [SerializeField]
    private GameObject toDestroy;

    private AudioSource sourceAudio;
 
    internal override void Start()
    {
        sourceAudio = GetComponent<AudioSource>();
        if (sourceAudio == null) {
            Debug.LogError("No AudioSource component found!");
        }

        base.Start();
    }

    internal override void DoDeath()
    {
        sourceAudio.PlayOneShot(clipToPlay);
        base.DoDeath();
        Destroy(toDestroy);
    }
}