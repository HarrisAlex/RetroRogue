using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController), typeof(MouseLook), typeof(CombatController))]
public class InputReceiver : MonoBehaviour
{
    // Component references
    private MovementController movementController;
    private MouseLook mouseLook;
    private CombatController combatController;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        mouseLook = GetComponent<MouseLook>();
    }

    private void Update()
    {
        // Handle movement input
        movementController.SetInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        movementController.SetSprint(Input.GetButton("Sprint"));

        if (Input.GetButtonDown("Crouch")) movementController.ToggleCrouch();

        // Handle mouse input
        mouseLook.SetInput(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Handle weapon input
        if (Input.GetButtonDown("Attack")) combatController.Attack();
        if (Input.GetButtonDown("Block")) combatController.Block();
    }
}
