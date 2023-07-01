using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(MovementController), typeof(LookController), typeof(CombatController))]
public class AIController : MonoBehaviour
{
    // Component references
    private MovementController movementController;
    private LookController lookController;
    private CombatController combatController;

    [SerializeField] private Goal goal;
    [SerializeField] private List<AIAction> availableActions;

    private List<AIActionWrapper> actions;
    private WorldState currentState;
    private AIActionWrapper currentAction;

    private static AssetBundle aiBundle;

    // Navigation
    private Vector3 destination = Vector3.zero;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        lookController = GetComponent<LookController>();
        combatController = GetComponent<CombatController>();

        actions = new();
        foreach (AIAction action in availableActions)
        {
            actions.Add(new(action));
        }

        foreach (AIActionWrapper current in actions)
        {
            foreach (AIActionWrapper check in actions)
            {
                if (AreConditionsEqual(current.action.postConditions, check.action.preConditions))
                    current.connections.Add(check);
            }
        }

        aiBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "ai"));

        currentState = ScriptableObject.CreateInstance<WorldState>();
        currentState.conditions.Add(new ConditionValuePair((Condition)aiBundle.LoadAsset("hasAmmo"), true));
        currentState.conditions.Add(new ConditionValuePair((Condition)aiBundle.LoadAsset("hasRanged"), true));
        currentState.conditions.Add(new ConditionValuePair((Condition)aiBundle.LoadAsset("hasMelee"), true));
        currentState.conditions.Add(new ConditionValuePair((Condition)aiBundle.LoadAsset("nearPlayer"), true));
        currentState.conditions.Add(new ConditionValuePair((Condition)aiBundle.LoadAsset("playerDead"), false));

        PlanActions();
    }

    private void Update()
    {
        movementController.SetInput(destination.x - transform.position.x, destination.z - transform.position.z);
        lookController.SetInput(Vector3.Angle(transform.forward, (destination - transform.position).normalized), 0);

        if (currentState != goal)
        {

        }
    }

    private void PlanActions()
    {
        List<AIActionWrapper> possibleActions = new();

        foreach (AIActionWrapper action in actions)
        {
            if (action.action.preConditions == currentState)
            {
                Debug.Log(action.CanReachState(new(), currentState));
                if (action.CanReachState(new(), goal) < 0) continue;

                possibleActions.Add(action);
            }
        }

        List<AIActionWrapper> open = new();
        open.Add(currentAction);

        HashSet<AIAction> closed = new();
        while (open.Count > 0)
        {
            AIActionWrapper current = open[0];

            AIActionWrapper cheapest;
            for (int i = 1; i < open.Count; i++)
            {
                cheapest = open[i];
                if (cheapest.FCost < current.FCost || Mathf.Approximately(cheapest.FCost, current.FCost))
                {
                    if (cheapest.hCost < current.hCost)
                        current = cheapest;
                }
            }
        }
    }

    private bool AreConditionsEqual(List<ConditionValuePair> conditions1, List<ConditionValuePair> conditions2)
    {
        foreach (ConditionValuePair condition1 in conditions1)
        {
            foreach (ConditionValuePair condition2 in conditions2)
            {
                if (condition1.condition == condition2.condition)
                {
                    if (condition1.value != condition2.value)
                        return false;
                }
            }
        }

        return true;
    }
}
