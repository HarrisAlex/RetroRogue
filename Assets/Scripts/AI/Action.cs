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

        public ActionInput(Transform playerTransform, MovementController movementController, LookController lookController, Animator animator, AIController aiController)
        {
            this.playerTransform = playerTransform;
            this.movementController = movementController;
            this.lookController = lookController;
            this.animator = animator;
            this.aiController = aiController;
        }
    }

    public abstract class Action
    {
        public int Cost { get; protected set; }
        public WorldState Preconditions { get; protected set; }
        public WorldState Postconditions { get; protected set; }

        public Action()
        {
            Cost = 1;
            Preconditions = new(new());
            Postconditions = new(new());
        }

        public Action(int cost, WorldState preconditions, WorldState postconditions)
        {
            Cost = cost;
            Preconditions = preconditions;
            Postconditions = postconditions;
        }

        public abstract void Initialize();

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

            Postconditions = postconditions;
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
            Cost = Mathf.RoundToInt((start - destination.position).magnitude);
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            if (path == null)
            {
                path = GameManager.dungeon.pathfinding.FindPath(Vertex.VectorToVertex(input.playerTransform.position), Vertex.VectorToVertex(destination.position));
                current = 0;

                if (path.Count < 1)
                {
                    OnFinish();
                    return;
                }
            }

            input.movementController.SetInput(0, 1);
            input.movementController.Run();

            input.lookController.SetInput(new Vector3(path[current].position.x, 0, path[current].position.y));
            input.lookController.Run();

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
            Cost = 2;

            Preconditions = preconditions;
            Postconditions = postconditions;

            this.animation = animation;
            this.duration = duration;
            terminationType = TerminationType.Time;

            timer = 0;
        }

        public AUseObject(WorldState preconditions, WorldState postconditions, string animation, ConditionValuePair condition)
        {
            Cost = 2;

            Preconditions = preconditions;
            Postconditions = postconditions;

            this.animation = animation;
            this.condition = condition;
            terminationType = TerminationType.Condition;

            timer = 0;
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
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
            Preconditions.SetCondition(Conditions.AwareOfPlayer, false);
            Preconditions.SetCondition(Conditions.AwareOfSound, true);
            
            Postconditions.SetCondition(Conditions.AwareOfSound, false);
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            OnFinish();
        }
    }

    public class ASearch : Action
    {
        public float radius = 5;
        private Stack<Room> searchRooms;
        private bool currentRoomSearched;

        public ASearch()
        {
            Cost = 2;
            Preconditions.SetCondition(Conditions.AwareOfPlayer, true);
            Preconditions.SetCondition(Conditions.CanSeePlayer, false);

            Postconditions.SetCondition(Conditions.CanSeePlayer, true);
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            searchRooms = new();
            currentRoomSearched = false;

            foreach (Room room in GameManager.dungeon.rooms)
            {
                if (SquareDistance(room.Center, Vertex.VectorToVertex(input.aiController.transform.position)) <= radius * radius)
                {
                    searchRooms.Push(room);
                }
            }

            

            OnFinish();
        }
    }

    public class ARangedAttack : Action
    {
        public ARangedAttack()
        {
            Preconditions.SetCondition(Conditions.CanSeePlayer, true);
            Preconditions.SetCondition(Conditions.HasRanged, true);
            Preconditions.SetCondition(Conditions.HasAmmo, true);
            
            Postconditions.SetCondition(Conditions.PlayerDead, true);
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
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
            Cost = 2;
            Preconditions.SetCondition(Conditions.CanSeePlayer, true);
            Preconditions.SetCondition(Conditions.NearPlayer, true);
            Preconditions.SetCondition(Conditions.HasMelee, true);

            Postconditions.SetCondition(Conditions.PlayerDead, true);
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
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
            Cost = 2;
            Preconditions.SetCondition(Conditions.CanSeePlayer, true);
            Preconditions.SetCondition(Conditions.NearPlayer, false);
            
            Postconditions.SetCondition(Conditions.NearPlayer, true);
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
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
            Cost = 2;
            Preconditions.SetCondition(Conditions.LowHealth, true);
            Preconditions.SetCondition(Conditions.NearHealth, true);
            
            Postconditions.SetCondition(Conditions.LowHealth, false);
        }

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public override void Run(ActionInput input, System.Action OnFinish)
        {
            OnFinish();
        }
    }
}