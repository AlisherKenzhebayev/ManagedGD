using System.Collections.Generic;
using UnityEngine;

public class DamageTaker : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Same as maxHealth, works with that assumption")]
    internal int currentHealth = 100;
    [SerializeField]
    internal float iFrameTimer = 0.0f;

    internal bool isAlive;
    internal int maxHealth;
    internal float currentIFrameTime;

    internal virtual void Start()
    {
        isAlive = true;
        maxHealth = currentHealth;
    }

    internal virtual void RestoreFlat(int health) {
        currentHealth = Mathf.Min(currentHealth + health, maxHealth);
    }

    internal virtual void FixedUpdate()
    {
        currentIFrameTime = Mathf.Max(0.0f, currentIFrameTime - Time.fixedDeltaTime);
    }

    internal virtual void OnEnable()
    {
        EventManager.StartListening("takeDamage" + this.gameObject.GetInstanceID(), OnTakeDamage);
    }

    internal virtual void OnDisable()
    {
        EventManager.StopListening("takeDamage" + this.gameObject.GetInstanceID(), OnTakeDamage);
    }

    internal virtual void OnTakeDamage(Dictionary<string, object> obj)
    {
        if (this.currentIFrameTime > 0)
        {
            return;
        }

        DoTakeDamage(obj);
    }

    internal virtual void DoTakeDamage(Dictionary<string, object> obj)
    {
        if (this.currentHealth <= 0f)
        {
            return;
        }

        this.currentIFrameTime = iFrameTimer;
        this.currentHealth -= (int)obj["amount"];

        if (this.currentHealth <= 0f)
        {
            DoDeath();
        }

    }

    internal virtual void DoDeath()
    {
        isAlive = false;
    }

    public float FracHealth
    {
        get
        {
            return currentHealth * 1.0f / maxHealth;
        }
        set {
            this.currentHealth = Mathf.Clamp(Mathf.CeilToInt(value * maxHealth), 0, maxHealth);
        }
    }
}