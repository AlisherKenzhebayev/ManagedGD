using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotDD : DamageDealer
{
    [SerializeField]
    internal float damageIntervals = 0.5f;
    internal float currentDamageIntervalTime;

    internal override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
    }

    internal override void OnTriggerEnter2D(Collider2D other)
    {
        if (currentDamageIntervalTime > 0) {
            return;
        }

        currentDamageIntervalTime = damageIntervals;
        EventManager.TriggerEvent("takeDamage" + other.gameObject.GetInstanceID(), new Dictionary<string, object> { { "amount", damage } });
        DoDealDamage(other);
    }

    private void FixedUpdate()
    {
        currentDamageIntervalTime = Mathf.Max(0.0f, currentDamageIntervalTime - Time.fixedDeltaTime);
    }
}
