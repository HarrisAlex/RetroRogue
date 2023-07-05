using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI
{
    [RequireComponent(typeof(MovementController), typeof(LookController), typeof(CombatController))]
    public class AIController : MonoBehaviour
    {
        public struct Attributes
        {
            public float boredom;
            public float tiredness;
        }

        // Component references
        private MovementController movementController;
        private LookController lookController;
        private CombatController combatController;

        private List<Action> actions;

        private WorldState currentState;
        private WorldState goalState;

        private Action currentAction;

        // Navigation
        private Vector3 destination = Vector3.zero;

        private void Awake()
        {
            movementController = GetComponent<MovementController>();
            lookController = GetComponent<LookController>();
            combatController = GetComponent<CombatController>();

            // Initialize AI
            Actions.CreateActions();

            actions = new();
            actions.Add(Actions.Investigate);
            actions.Add(Actions.Search);
            actions.Add(Actions.MeleeAttack);
            actions.Add(Actions.RangedAttack);
            actions.Add(Actions.Chase);
            actions.Add(Actions.Recover);

            goalState = new(new());
            goalState.SetCondition(Conditions.PLAYER_DEAD, true);

            currentState = new WorldState(new());
            currentState.SetCondition(Conditions.HAS_AMMO, false);
            currentState.SetCondition(Conditions.HAS_RANGED, true);
            currentState.SetCondition(Conditions.HAS_MELEE, true);
            currentState.SetCondition(Conditions.PLAYER_DEAD, false);
            currentState.SetCondition(Conditions.CAN_SEE_PLAYER, false);
            currentState.SetCondition(Conditions.NEAR_PLAYER, true);
            currentState.SetCondition(Conditions.AWARE_OF_PLAYER, true);

            List<Action> path = FindPath();
            if (path.Count > 0)
            {
                int count = 1;
                foreach (Action action in path)
                {
                    Debug.Log(count + ": " + action.GetType().ToString().Split('.')[3].Remove(0, 1));
                    count++;
                }
            }
            else
                Debug.Log("No path");
        }

        private void Update()
        {
            movementController.SetInput(destination.x - transform.position.x, destination.z - transform.position.z);
            lookController.SetInput(Vector3.Angle(transform.forward, (destination - transform.position).normalized), 0);

            if (currentState != goalState)
            {

            }
        }

        private List<Action> FindPath()
        {
            StateNode startNode = new(currentState);
            startNode.parentNode = null;
            startNode.parentAction = null;
            startNode.gCost = 0;
            startNode.hCost = FindHCost(currentState, goalState);

            StateNode endNode = new(goalState);

            List<StateNode> open = new();
            open.Add(startNode);

            List<StateNode> closed = new();

            List<StateNode> allNodes = new();
            allNodes.Add(startNode);
            allNodes.Add(endNode);

            StateNode current;

            int limiter = 0;

            while (open.Count > 0)
            {
                limiter++;
                if (limiter > 1000)
                    return new();

                current = open[0];

                // Get cheapest
                StateNode cheapest = current;
                int cheapestCost = current.fCost;
                for (int i = 0; i < open.Count; i++)
                {
                    if (open[i].fCost < cheapestCost)
                    {
                        cheapest = open[i];
                        cheapestCost = cheapest.fCost;
                    }
                }

                current = cheapest;

                open.Remove(current);
                closed.Add(current);

                // No path
                if (current == null)
                    return new();

                // If found path
                if (current.state.MatchesState(goalState))
                {
                    List<Action> path = new();

                    while (current.parentAction != null)
                    {
                        path.Add(current.parentAction);

                        current = current.parentNode;
                    }

                    path.Reverse();
                    return path;
                }

                // Find connections
                foreach (Action action in actions)
                {
                    foreach (Condition condition in action.preconditions.Conditions)
                    {
                        if (!currentState.ContainsCondition(condition)) continue;
                        if (currentState.GetCondition(condition) != action.preconditions.GetCondition(condition)) continue;

                        StateNode newNode = new(WorldState.CombineStates(current.state, action.postconditions));

                        foreach (StateNode node in allNodes)
                        {
                            if (node.state == newNode.state)
                            {
                                current.connections.Add(node);
                                newNode = null;
                                break;
                            }
                        }

                        if (newNode != null)
                        {
                            current.connections.Add(newNode);
                            allNodes.Add(newNode);
                        }

                        break;
                    }
                }

                // TODO: Add GoTo and UseObject to connections based on context

                foreach (StateNode connection in current.connections)
                {
                    if (closed.Contains(connection)) continue;

                    current.hCost = FindHCost(current.state, goalState);

                    bool openContainsNeighbor = open.Contains(connection);

                    // Adds each neighbor cheaper than current node to open set
                    if (current.fCost < connection.gCost || !openContainsNeighbor)
                    {
                        connection.gCost = current.fCost;
                        connection.hCost = FindHCost(connection.state, goalState);
                        connection.parentNode = current;
                        connection.parentAction = GetConnectingAction(current.state, connection.state);

                        if (!openContainsNeighbor && connection.parentAction != null)
                            open.Add(connection);
                    }
                }
            }

            return new();
        }

        private Action GetConnectingAction(WorldState current, WorldState goal)
        {
            foreach (Action action in actions)
            {
                if (current.MatchesState(action.preconditions) && goal.MatchesState(action.postconditions))
                    return action;
            }

            return null;
        }

        private int FindHCost(WorldState current, WorldState goal)
        {
            if (current == goal)
                return 0;

            int cost = 0;

            foreach (Condition condition in goal.Conditions)
            {
                if (!current.ContainsCondition(condition)) continue;

                if (current.GetCondition(condition) != goal.GetCondition(condition))
                    cost++;
            }

            return cost;
        }
    }
}