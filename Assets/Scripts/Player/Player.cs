using System;
using UnityEngine;

public class Player : MonoBehaviour, IDamagable, IHealable
{
    public static event Action PlayerDeath;

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


    private void Die()
    {
        IsDead = true;
        PlayerDeath.Invoke();
    }
}
