using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.AI;
using UnityEngine;
using static Assets.Scripts.Generation.DungeonGeneration;

namespace Assets.Scripts.AI
{
    public readonly struct ActionInput
    {
        public readonly Transform playerTransform;
        public readonly MovementController movementController;
        public readonly LookController lookController;
        public readonly Animator animator;
        public readonly AIController aiController;
        public readonly Pathfinding<Vertex> pathfinder;

        public ActionInput(Transform playerTransform, MovementController movementController, LookController lookController, Animator animator, AIController aiController, Pathfinding<Vertex> pathfinder)
        {
            this.playerTransform = playerTransform;
            this.movementController = movementController;
            this.lookController = lookController;
            this.animator = animator;
            this.aiController = aiController;
            this.pathfinder = pathfinder;
        }
    }

    public abstract class Action
    {
        public int cost;
        public WorldState preconditions;
        public WorldState postconditions;

        public Action()
        {
            cost = 1;
            preconditions = new(new());
            postconditions = new(new());
        }

        public Action(int cost, WorldState preconditions, WorldState postconditions)
        {
            this.cost = cost;
            this.preconditions = preconditions;
            this.postconditions = postconditions;
        }

        public int GetCost()
        {
            return cost;
        }

        public WorldState GetPreconditions()
        {
            return preconditions;
        }

        public WorldState GetPostconditions()
        {
            return postconditions;
        }

        public abstract void Run(ActionInput input, System.Action OnFinish);
    }

    public enum TerminationType
    {
        Time,
        Condition,
    }


    public class StateNode
    {
        public WorldState state { get; private set; }
        public int gCost;
        public int hCost;
        public int fCost { get => gCost + hCost; }

        public Action parentAction;
        public StateNode parentNode;
        public List<StateNode> connections;
        public Vector3 currentPosition;

        public StateNode(WorldState state)
        {
            this.state = state;
            connections = new();
        }
    }

    // Actions
    public class Actions
    {
        public static AInvestigate Investigate;
        public static ASearch Search;

        public static ARangedAttack RangedAttack;
        public static AMeleeAttack MeleeAttack;
        public static AChase Chase;
        public static ARecover Recover;

        public static void CreateActions()
        {
            Investigate = new();
            Search = new();
            RangedAttack = new();
            MeleeAttack = new();
            Chase = new();
            Recover = new();
        }
    }

    public class AGoTo : Action
    {
        private Transform destination;
        private Vector3 start;

        private List<Node<Vertex>> path;
        private int current;

        public AGoTo(Transform destination, WorldState postconditions)
        {
            this.destination = destination;

            this.postconditions = postconditions;
        }

        public void SetStartPosition(Vector3 start)
        {
            this.start = start;
            CalculateCost();
        }

        public Vector3 GetDestination()
        {
            return destination.position;
        }

        private void CalculateCost()
        {
            cost = Mathf.RoundToInt((start - destination.position).magnitude);
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            if (path == null)
            {
                path = input.pathfinder.FindPath(Vertex.VectorToVertex(input.playerTransform.position), Vertex.VectorToVertex(destination.position));
                current = 0;

                if (path.Count < 1)
                {
                    OnFinish();
                    return;
                }
            }

            input.movementController.SetInput(0, 1);
            input.lookController.SetInput(new Vector3(path[current].position.x, 0, path[current].position.y));

            if ((input.playerTransform.position - new Vector3(path[current].position.x, 0, path[current].position.y)).sqrMagnitude < 0.01f)
            {
                current++;

                if (current >= path.Count)
                {
                    OnFinish();
                    return;
                }
            }
        }
    }

    public class AUseObject : Action
    {
        private string animation;
        private TerminationType terminationType;

        private float duration;
        private ConditionValuePair condition;

        private float timer;

        public AUseObject(WorldState preconditions, WorldState postconditions, string animation, float duration)
        {
            cost = 2;

            this.preconditions = preconditions;
            this.postconditions = postconditions;

            this.animation = animation;
            this.duration = duration;
            terminationType = TerminationType.Time;

            timer = 0;
        }

        public AUseObject(WorldState preconditions, WorldState postconditions, string animation, ConditionValuePair condition)
        {
            cost = 2;

            this.preconditions = preconditions;
            this.postconditions = postconditions;

            this.animation = animation;
            this.condition = condition;
            terminationType = TerminationType.Condition;

            timer = 0;
        }


        public override void Run(ActionInput input, System.Action OnFinish)
        {
            input.animator.CrossFade(animation, 0.1f);

            if (terminationType == TerminationType.Time)
            {
                timer += Time.deltaTime;

                if (timer < duration)
                    return;
            }
            else
            {
                if (!input.aiController.currentWorldState.MatchesState(condition))
                    return;
            }

            OnFinish();
        }
    }

    public class AInvestigate : Action
    {
        public AInvestigate()
        {
            preconditions.SetCondition(Condition.AwareOfSound, -1, true);
            preconditions.SetCondition(Condition.AwareOfPlayer, -1, false);

            postconditions.SetCondition(Condition.AwareOfSound, -1, false);
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            OnFinish();
        }
    }

    public class ASearch : Action
    {
        public ASearch()
        {
            cost = 2;
            preconditions.SetCondition(Condition.AwareOfPlayer, -1, true);
            preconditions.SetCondition(Condition.CanSeePlayer, -1, false);

            postconditions.SetCondition(Condition.CanSeePlayer, -1, true);
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            OnFinish();
        }
    }

    public class ARangedAttack : Action
    {
        public ARangedAttack()
        {
            preconditions.SetCondition(Condition.CanSeePlayer, -1, true);
            preconditions.SetCondition(Condition.HasRanged, -1, true);
            preconditions.SetCondition(Condition.HasAmmo, -1, true);

            postconditions.SetCondition(Condition.PlayerDead, -1, true);
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            OnFinish();
        }
    }

    public class AMeleeAttack : Action
    {
        public AMeleeAttack()
        {
            cost = 2;
            preconditions.SetCondition(Condition.CanSeePlayer, -1, true);
            preconditions.SetCondition(Condition.NearPlayer, -1, true);
            preconditions.SetCondition(Condition.HasMelee, -1, true);

            postconditions.SetCondition(Condition.PlayerDead, -1, true);
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            OnFinish();
        }
    }

    public class AChase : Action
    {
        public AChase()
        {
            cost = 2;
            preconditions.SetCondition(Condition.CanSeePlayer, -1, true);
            preconditions.SetCondition(Condition.NearPlayer, -1, false);

            postconditions.SetCondition(Condition.NearPlayer, -1, true);
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            OnFinish();
        }
    }

    public class ARecover : Action
    {
        public ARecover()
        {
            cost = 2;
            preconditions.SetCondition(Condition.LowHealth, -1, true);
            preconditions.SetCondition(Condition.NearHealth, -1, true);

            postconditions.SetCondition(Condition.LowHealth, -1, false);
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            OnFinish();
        }
    }
}