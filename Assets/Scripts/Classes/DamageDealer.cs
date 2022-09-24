using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField]
    internal int damage = 5;
    [SerializeField]
    internal string soundToPlay = "Hit2";

    [SerializeField]
    internal LayerMask ignoreLayers;

    public GameObject originHitbox = null;

    internal virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (originHitbox != null && other.gameObject == originHitbox)
        {
            return;
        }

        DoDealDamage(other);
    }

    internal virtual void OnCollisionEnter2D(Collision2D collision)
    {
        AudioManager.Play(soundToPlay);
    }

    internal virtual bool DoDealDamage(Collider2D other)
    {
        if ((ignoreLayers | (1 << other.gameObject.layer)) == ignoreLayers)
        {
            return false;
        }

        EventManager.TriggerEvent("takeDamage" + other.gameObject.GetInstanceID(), new Dictionary<string, object> { { "amount", damage } });
        AudioManager.Play(soundToPlay);
        return true;
    }
}