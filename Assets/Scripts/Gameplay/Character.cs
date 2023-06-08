using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IDamagable, IHealable
{
    public float MaxHealth { get; set; }
    public bool IsDead { get; private set; }
    private float health;

    public void Damage(float amount)
    {
        if (amount < 0) return;

        health -= amount;

        if (health <= 0) Die();
    }

    public void Heal(float amount)
    {
        if (amount < 0) return;

        health += amount;

        if (health >= MaxHealth)
            health = MaxHealth;
    }

    protected virtual void Die()
    {
        IsDead = true;
    }
}
