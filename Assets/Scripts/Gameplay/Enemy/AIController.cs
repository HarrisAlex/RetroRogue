using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController), typeof(LookController), typeof(CombatController))]
public class AIController : MonoBehaviour
{
    // Component references
    private MovementController movementController;
    private LookController lookController;
    private CombatController combatController;

    // FSM
    public enum BehaviorState
    {
        Idle,
        Wandering,
        Searching,
        Pursuing
    }
    public BehaviorState State { get; private set; }

    // Navigation
    private Vector3 destination = Vector3.negativeInfinity;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        lookController = GetComponent<LookController>();
        combatController = GetComponent<CombatController>();
    }

    private void Update()
    {
        if (destination == Vector3.negativeInfinity) return;

        movementController.SetInput(destination.x - transform.position.x, destination.z - transform.position.z);
        lookController.SetInput(Vector3.Angle(transform.forward, (destination - transform.position).normalized), 0);
    }
}
