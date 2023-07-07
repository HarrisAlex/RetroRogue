using UnityEngine;

namespace Assets.Scripts.AI
{
    [RequireComponent(typeof(AIController))]
    public abstract class Character : MonoBehaviour, IDamagable, IHealable
    {
        public float MaxHealth { get; set; }
        public bool IsDead { get; private set; }
        private float health;

        private AIController aiController;

        private void Start()
        {
            aiController = GetComponent<AIController>();
            foreach (ISmartObject smartObject in SmartObjectManager.GetSmartObjects(this))
            {
                aiController.AddSmartObject(smartObject);
            }
        }

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
}