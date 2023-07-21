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
        public readonly Navigation navigation;

        public ActionInput(Transform playerTransform, MovementController movementController, LookController lookController, Animator animator, AIController aiController, Navigation navigation)
        {
            this.playerTransform = playerTransform;
            this.movementController = movementController;
            this.lookController = lookController;
            this.animator = animator;
            this.aiController = aiController;
            this.navigation = navigation;
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

        private List<Vertex> path;
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
                path = input.navigation.FindPath(Vertex.VectorToVertex(input.playerTransform.position), Vertex.VectorToVertex(destination.position));
                current = 0;

                if (path.Count < 1)
                {
                    OnFinish();
                    return;
                }
            }

            input.movementController.SetInput(0, 1);
            input.lookController.SetInput(new Vector3(path[current].x, 0, path[current].y));

            if ((input.playerTransform.position - new Vector3(path[current].x, 0, path[current].y)).sqrMagnitude < 0.01f)
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
        private Condition condition;

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

        public AUseObject(WorldState preconditions, WorldState postconditions, string animation, Condition condition)
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
            preconditions.SetCondition(Conditions.AWARE_OF_SOUND, true);
            preconditions.SetCondition(Conditions.AWARE_OF_PLAYER, false);

            postconditions.SetCondition(Conditions.AWARE_OF_SOUND, false);
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
            preconditions.SetCondition(Conditions.AWARE_OF_PLAYER, true);
            preconditions.SetCondition(Conditions.CAN_SEE_PLAYER, false);

            postconditions.SetCondition(Conditions.CAN_SEE_PLAYER, true);
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
            preconditions.SetCondition(Conditions.CAN_SEE_PLAYER, true);
            preconditions.SetCondition(Conditions.HAS_RANGED, true);
            preconditions.SetCondition(Conditions.HAS_AMMO, true);

            postconditions.SetCondition(Conditions.PLAYER_DEAD, true);
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
            preconditions.SetCondition(Conditions.CAN_SEE_PLAYER, true);
            preconditions.SetCondition(Conditions.NEAR_PLAYER, true);
            preconditions.SetCondition(Conditions.HAS_MELEE, true);

            postconditions.SetCondition(Conditions.PLAYER_DEAD, true);
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
            preconditions.SetCondition(Conditions.CAN_SEE_PLAYER, true);
            preconditions.SetCondition(Conditions.NEAR_PLAYER, false);

            postconditions.SetCondition(Conditions.NEAR_PLAYER, true);
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
            preconditions.SetCondition(Conditions.LOW_HEALTH, true);
            preconditions.SetCondition(Conditions.NEAR_HEALTH, true);

            postconditions.SetCondition(Conditions.LOW_HEALTH, false);
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            OnFinish();
        }
    }
}