using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour, IController
{
    // Editor variables
    [SerializeField] private RuntimeAnimatorController weaponController;
    [SerializeField] private string animatorBlockVariable;

    // Component references
    private Weapon currentWeapon;
    private Transform weaponHolder;
    private Animator animator;

    private float cooldownTimer = 0;
    private RaycastHit attackHit;

    private int attackAnimHash;
    private int blockVarHash;

    private void Start()
    {
        // Create weapon holder and animator
        weaponHolder = new GameObject("Weapon Holder").transform;
        weaponHolder.parent = transform;

        InitializeAnimator();
    }

    // Gets animator component and initializes settings
    private void InitializeAnimator()
    {
        animator = weaponHolder.gameObject.AddComponent<Animator>();

        // Get animation hashes and set animator variables
        attackAnimHash = Animator.StringToHash("Attack");
        blockVarHash = Animator.StringToHash(animatorBlockVariable);

        if (weaponController)
            animator.runtimeAnimatorController = weaponController;
    }

    public void Run()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
    }

    public void Attack()
    {
        if (!currentWeapon) return;
        if (cooldownTimer > 0) return;

        animator.CrossFade(attackAnimHash, 0.3f);
        cooldownTimer = currentWeapon.cooldownTime;

        if (Physics.Raycast(transform.position, transform.forward, out attackHit))
        {
            IDamagable damageable;

            if (attackHit.transform.TryGetComponent(out damageable))
                damageable.Damage(currentWeapon.damage);
        }
    }

    public void StartBlock()
    {
        animator.SetBool(blockVarHash, true);
    }

    public void StopBlock()
    {
        animator.SetBool(blockVarHash, false);
    }

    public void SetWeapon(Weapon weapon)
    {
        if (!weapon) return;

        currentWeapon = weapon;

        // Set weapon animation avatar
        if (animator.avatar)
            animator.avatar = currentWeapon.animationAvatar;
    }
}
