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

        private List<IAction> actions;

        public WorldState currentWorldState { get; private set; }
        private WorldState goalWorldState;

        private Stack<IAction> currentPath;
        private IAction currentAction;

        // Navigation
        private Vector3 destination = Vector3.zero;

        private void Awake()
        {
            movementController = GetComponent<MovementController>();
            lookController = GetComponent<LookController>();
            combatController = GetComponent<CombatController>();
            animator = GetComponent<Animator>();

            // Initialize AI
            CreateActions();

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
                if (!TryFindPath(out Stack<IAction> path)) return;

                currentPath = path;
                StartNextAction();
            }

            if (currentAction == null) return;

            currentAction.Run();
        }

        private void StartNextAction()
        {
            if (currentPath.TryPop(out IAction action))
                currentAction = action;
            else
            {
                currentAction = null;
                currentPath = null;

                return;
            }
        }

        private bool TryFindPath(out Stack<IAction> path)
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

            List<IAction> usedActions = new();
            List<StateNode> allNodes = new();

            StateNode current;
            while (open.Count > 0)
            {
                current = open[0];

                // Get cheapest
                StateNode cheapest = current;
                int cheapestCost = current.FCost;
                for (int i = 0; i < open.Count; i++)
                {
                    if (open[i].FCost < cheapestCost)
                    {
                        cheapest = open[i];
                        cheapestCost = cheapest.FCost;
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
                foreach (IAction action in actions)
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
                        connection.gCost = current.FCost;
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
            WorldState precondition = smartObject.Preconditions;
            precondition.SetCondition(Conditions.NearObject + smartObject.GetHashCode(), true);

            WorldState postcondition = new WorldState(new());
            postcondition.SetCondition(Conditions.NearObject + smartObject.GetHashCode(), true);

            actions.Add(new UseObject(precondition, smartObject.Postconditions, smartObject.AnimationData, this, animator, MoveToNextAction));
            actions.Add(new GoTo(smartObject.Transform, postcondition, movementController, lookController, MoveToNextAction));
        }

        private void CreateActions()
        {
            // Initialize AI
            actions = new();

            Investigate investigateAction = new Investigate(MoveToNextAction);

            Search searchAction = new Search(MoveToNextAction, this);
            MeleeAttack meleeAttackAction = new MeleeAttack(MoveToNextAction);
            RangedAttack rangedAttackAction = new RangedAttack(MoveToNextAction);
            Chase chaseAction = new Chase(MoveToNextAction);
            Recover recoverAction = new Recover(MoveToNextAction);

            actions.Add(investigateAction);
            actions.Add(searchAction);
            actions.Add(meleeAttackAction);
            actions.Add(rangedAttackAction);
            actions.Add(chaseAction);
            actions.Add(recoverAction);
        }

        private void MoveToNextAction()
        {
            foreach (ConditionValuePair condition in currentAction.Postconditions.Conditions)
            {
                currentWorldState.SetCondition(condition.condition, condition.value);
            }

            StartNextAction();
        }
    }
}