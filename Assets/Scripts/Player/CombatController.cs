using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    // Editor variables
    [SerializeField] private RuntimeAnimatorController weaponController;

    // Component references
    private Weapon currentWeapon;
    private Transform weaponHolder;
    private Animator animator;

    private float cooldownTimer = 0;
    private RaycastHit attackHit;

    private int attackAnimHash;
    private int blockAnimHash;

    private void Start()
    {
        // Create weapon holder and animator
        weaponHolder = new GameObject("Weapon Holder").transform;
        weaponHolder.parent = transform;
        animator = weaponHolder.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = weaponController;

        // Get animation hashes
        attackAnimHash = Animator.StringToHash("Attack");
        blockAnimHash = Animator.StringToHash("Block");
    }

    private void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
    }

    public void Attack()
    {
        if (cooldownTimer > 0) return;

        animator.CrossFade(attackAnimHash, 0.3f);
        cooldownTimer = currentWeapon.cooldownTime;

        if (Physics.Raycast(transform.position, transform.forward, out attackHit))
        {

        }
    }

    public void Block()
    {
        animator.CrossFade(blockAnimHash, 0.3f);
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
