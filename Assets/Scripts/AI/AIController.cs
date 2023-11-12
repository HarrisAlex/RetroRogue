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
        private Animator animator;

        private List<Action> actions;

        public WorldState currentWorldState { get; private set; }
        private WorldState goalWorldState;

        private Stack<Action> currentPath;
        private Action currentAction;

        // Navigation
        private Vector3 destination = Vector3.zero;

        private void Awake()
        {
            movementController = GetComponent<MovementController>();
            lookController = GetComponent<LookController>();
            combatController = GetComponent<CombatController>();
            animator = GetComponent<Animator>();

            // Initialize AI
            Actions.CreateActions();

            actions = new();
            actions.Add(Actions.Investigate);
            actions.Add(Actions.Search);
            actions.Add(Actions.MeleeAttack);
            actions.Add(Actions.RangedAttack);
            actions.Add(Actions.Chase);
            actions.Add(Actions.Recover);

            goalWorldState = new(new());
            goalWorldState.SetCondition(Conditions.PlayerDead, true);

            currentWorldState = new WorldState(new());
            currentWorldState.SetCondition(Conditions.HasAmmo, false);
            currentWorldState.SetCondition(Conditions.HasRanged, true);
            currentWorldState.SetCondition(Conditions.HasMelee, false);
            currentWorldState.SetCondition(Conditions.PlayerDead, false);
            currentWorldState.SetCondition(Conditions.CanSeePlayer, false);
            currentWorldState.SetCondition(Conditions.NearPlayer, true);
            currentWorldState.SetCondition(Conditions.AwareOfPlayer, true);

            currentPath = null;
            currentAction = null;
        }

        private void Update()
        {
            if (currentWorldState == goalWorldState) return;

            if (currentPath == null)
            {
                if (!TryFindPath(out Stack<Action> path)) return;

                currentPath = path;
                StartNextAction();
            }

            if (currentAction == null) return;

            currentAction.Run(new ActionInput(transform, movementController, lookController, animator, this), () =>
            {
                foreach (ConditionValuePair condition in currentAction.Postconditions.Conditions)
                {
                    currentWorldState.SetCondition(condition.condition, condition.value);
                }

                StartNextAction();
            });
        }

        private void StartNextAction()
        {
            Debug.Log(currentPath.Peek());

            if (currentPath.TryPop(out Action action))
                currentAction = action;
            else
            {
                currentAction = null;
                currentPath = null;

                return;
            }
        }

        private bool TryFindPath(out Stack<Action> path)
        {
            path = new();

            StateNode startNode = new(currentWorldState);
            startNode.parentNode = null;
            startNode.parentAction = null;
            startNode.gCost = 0;
            startNode.hCost = FindHCost(currentWorldState, goalWorldState);

            List<StateNode> open = new();
            open.Add(startNode);
            List<StateNode> closed = new();

            List<Action> usedActions = new();
            List<StateNode> allNodes = new();

            StateNode current;
            while (open.Count > 0)
            {
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
                if (current.state.MatchesState(goalWorldState))
                {
                    while (current.parentAction != null)
                    {
                        path.Push(current.parentAction);

                        current = current.parentNode;
                    }

                    return true;
                }

                // Find connections
                foreach (Action action in actions)
                {
                    if (current.state.MatchesState(action.Preconditions))
                    {
                        StateNode newNode = new(WorldState.CombineStates(current.state, action.Postconditions));
                        newNode.parentAction = action;

                        if (usedActions.Contains(newNode.parentAction))
                        {
                            foreach (StateNode node in allNodes)
                            {
                                if (node.state == newNode.state)
                                {
                                    newNode = null;
                                    break;
                                }
                            }
                        }

                        if (newNode != null)
                        {
                            current.connections.Add(newNode);
                            usedActions.Add(newNode.parentAction);
                            allNodes.Add(newNode);
                        }
                    }
                }

                // Add connecting states that haven't been explored yet
                foreach (StateNode connection in current.connections)
                {
                    if (closed.Contains(connection)) continue;

                    float cost = current.gCost + connection.parentAction.Cost;

                    current.hCost = FindHCost(current.state, goalWorldState);

                    bool openContainsNeighbor = open.Contains(connection);

                    // Adds each neighbor cheaper than current node to open set
                    if (cost < connection.gCost || !openContainsNeighbor)
                    {
                        connection.gCost = current.fCost;
                        connection.hCost = FindHCost(connection.state, goalWorldState);
                        connection.parentNode = current;
                        connection.currentPosition = transform.position;

                        if (!openContainsNeighbor)
                            open.Add(connection);
                    }
                }
            }

            return false;
        }

        private Action GetConnectingAction(WorldState current, WorldState goal)
        {
            foreach (Action action in actions)
            {
                AGoTo goToTemp;
                if (action.GetType() == typeof(AGoTo))
                {
                    goToTemp = (AGoTo)action;
                    goToTemp.SetStartPosition(transform.position);
                }

                if (current.MatchesState(action.Preconditions) && goal.MatchesState(action.Postconditions))
                    return action;
            }

            return null;
        }

        private int FindHCost(WorldState current, WorldState goal)
        {
            if (current == goal)
                return 0;

            int cost = 0;

            foreach (ConditionValuePair condition in goal.Conditions)
            {
                if (current.ContainsCondition(condition))
                {
                    if (current.GetCondition(condition) == goal.GetCondition(condition))
                        continue;
                }

                cost++;
            }

            return cost;
        }

        public void AddSmartObject(ISmartObject smartObject)
        {
            WorldState precondition = smartObject.GetPreconditions();
            precondition.SetCondition(Conditions.NearObject + smartObject.GetHashCode(), true);

            WorldState postcondition = new WorldState(new());
            postcondition.SetCondition(Conditions.NearObject + smartObject.GetHashCode(), true);

            AUseObject useObject;
            switch (smartObject.GetAnimationTerminationType())
            {
                case TerminationType.Condition:
                    useObject = new(precondition, smartObject.GetPostconditions(), smartObject.GetAnimationName(), smartObject.GetAnimationCondition());
                    break;
                default:
                    useObject = new(precondition, smartObject.GetPostconditions(), smartObject.GetAnimationName(), smartObject.GetAnimationDuration());
                    break;
            }

            actions.Add(useObject);
            actions.Add(new AGoTo(smartObject.GetTransform(), postcondition));
        }
    }
}