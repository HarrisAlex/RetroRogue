using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController), typeof(LookController), typeof(CombatController))]
public class InputReceiver : MonoBehaviour
{
    // Component references
    private MovementController movementController;
    private LookController lookController;
    private CombatController combatController;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        lookController = GetComponent<LookController>();
        combatController = GetComponent<CombatController>();
    }

    private void Update()
    {
        // Handle movement input
        movementController.SetInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (Input.GetButtonDown("Sprint")) movementController.SetSprint(true);
        else if (Input.GetButtonUp("Sprint")) movementController.SetSprint(false);

        if (Input.GetButtonDown("Crouch")) movementController.ToggleCrouch();

        // Handle mouse input
        lookController.SetInput(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Handle weapon input
        if (Input.GetButtonDown("Attack")) combatController.Attack();

        if (Input.GetButtonDown("Block")) combatController.StartBlock();
        else if (Input.GetButtonUp("Block")) combatController.StopBlock();
    }
}
