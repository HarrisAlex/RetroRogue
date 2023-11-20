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

    public interface IAction
    {
        public int Cost { get; }
        public WorldState Preconditions { get; }
        public WorldState Postconditions { get; }
        public System.Action OnFinish { get; }

        public void Initialize();
        public void Run();
    }

    [System.Serializable]
    public struct AnimationData
    {
        public enum ExitType
        {
            Time,
            Condition,
        }

        public readonly AnimationClip animation;
        public readonly ExitType exitType;
        public readonly float duration;
        public readonly ConditionValuePair exitCondition;

        public AnimationData(AnimationClip animation, ConditionValuePair exitCondition)
        {
            this.animation = animation;
            exitType = ExitType.Condition;
            duration = -1;
            this.exitCondition = exitCondition;
        }

        public AnimationData(AnimationClip animation, float duration)
        {
            this.animation = animation;
            exitType = ExitType.Time;
            this.duration = duration;
            exitCondition = default;
        }
    }

    public class StateNode
    {
        public readonly WorldState state;
        public int gCost;
        public int hCost;
        public int FCost { get => gCost + hCost; }

        public IAction parentAction;
        public StateNode parentNode;
        public List<StateNode> connections;
        public Vector3 currentPosition;

        public StateNode(WorldState state)
        {
            this.state = state;
            connections = new();
        }
    }

    public class GoTo : IAction
    {
        private Transform destination;
        private Vector3 start;

        private List<Node<Vertex>> path;
        private int current;

        public int Cost { get; private set; }
        public WorldState Preconditions { get; private set; }
        public WorldState Postconditions { get; private set; }
        public System.Action OnFinish { get; private set; }

        private MovementController movementController;
        private LookController lookController;

        public GoTo(Transform destination, WorldState postconditions, MovementController movementController, LookController lookController, System.Action onFinish)
        {
            this.destination = destination;

            Preconditions = new(new());
            Postconditions = postconditions;

            this.movementController = movementController;
            this.lookController = lookController;

            OnFinish = onFinish;
        }

        public void SetStartPosition(Vector3 start)
        {
            this.start = start;
            CalculateCost();
        }

        private void CalculateCost()
        {
            Cost = Mathf.RoundToInt((start - destination.position).magnitude);
        }

        public void Initialize() { }

        public void Run()
        {
            if (path == null)
            {
                path = GameManager.dungeon.pathfinding.FindPath(Vertex.VectorToVertex(movementController.transform.position), Vertex.VectorToVertex(destination.position));
                current = 0;

                if (path.Count < 1)
                {
                    OnFinish();
                    return;
                }
            }

            movementController.SetInput(0, 1);
            movementController.Run();

            lookController.SetInput(new Vector3(path[current].position.x, 0, path[current].position.y));
            lookController.Run();

            if ((movementController.transform.position - new Vector3(path[current].position.x, 0, path[current].position.y)).sqrMagnitude < 0.01f)
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

    public class UseObject : IAction
    {
        private AnimationData animationData;

        private float timer;

        public int Cost { get; private set; }

        public WorldState Preconditions { get; private set; }
        public WorldState Postconditions { get; private set; }
        public System.Action OnFinish { get; private set; }

        private AIController aiController;
        private Animator animator;

        public UseObject(WorldState preconditions, WorldState postconditions, AnimationData animationData, AIController aiController, Animator animator, System.Action onFinish)
        {
            Cost = 2;

            Preconditions = preconditions;
            Postconditions = postconditions;

            this.animationData = animationData;
            this.aiController = aiController;
            this.animator = animator;

            timer = 0;

            OnFinish = onFinish;
        }

        public void Initialize() { }

        public void Run()
        {
            if (animationData.animation != null)
                animator.CrossFade(animationData.animation.name, 0.1f);

            if (animationData.exitType == AnimationData.ExitType.Time)
            {
                timer += Time.deltaTime;

                if (timer < animationData.duration)
                    return;
            }
            else
            {
                if (!aiController.currentWorldState.MatchesState(animationData.exitCondition))
                    return;
            }

            OnFinish();
        }
    }

    public class Investigate : IAction
    {
        public int Cost { get; private set; }

        public WorldState Preconditions { get; private set; }
        public WorldState Postconditions { get; private set; }
        public System.Action OnFinish { get; private set; }

        public Investigate(System.Action onFinish)
        {
            Preconditions = new(new());
            Postconditions = new(new());

            Preconditions.SetCondition(Conditions.AwareOfPlayer, false);
            Preconditions.SetCondition(Conditions.AwareOfSound, true);

            Postconditions.SetCondition(Conditions.AwareOfSound, false);

            OnFinish = onFinish;
        }

        public void Initialize() { }

        public void Run()
        {
            OnFinish();
        }
    }

    public class Search : IAction
    {
        private struct SearchRoom
        {
            public Room room;
            public Stack<Vertex> searchPoints;

            public SearchRoom(Room room, float searchPointDensity)
            {
                this.room = room;
                searchPoints = new();

                int count = Mathf.RoundToInt(room.Area * (searchPointDensity / 16));
                for (int i = 0; i < count; i++)
                {
                    searchPoints.Push(new(room.xPosition + Random.Range(0, room.width), room.yPosition + Random.Range(0, room.height)));
                }
            }

            public bool TryFindNextPath(Vertex currentPosition, out List<Node<Vertex>> path)
            {
                path = new();

                Vertex nextPoint = Vertex.Invalid;
                while (nextPoint == Vertex.Invalid && searchPoints.Count == 0)
                {
                    nextPoint = searchPoints.Pop();
                }

                if (nextPoint == null)
                    return false;

                path = GameManager.dungeon.pathfinding.FindPath(currentPosition, nextPoint);
                return true;
            }
        }

        public float radius = 5;
        public float searchPointDensity = 8;

        private Stack<SearchRoom> searchRooms;
        private SearchRoom currentSearchRoom;
        private List<Node<Vertex>> path;
        private int currentPathIndex;

        public int Cost { get; private set; }

        public WorldState Preconditions { get; private set; }
        public WorldState Postconditions { get; private set; }
        public System.Action OnFinish { get; private set; }

        private AIController aiController;
        private MovementController movementController;
        private LookController lookController;

        public Search(System.Action onFinish, AIController aiController)
        {
            Preconditions = new(new());
            Postconditions = new(new());

            Cost = 2;
            Preconditions.SetCondition(Conditions.AwareOfPlayer, true);
            Preconditions.SetCondition(Conditions.CanSeePlayer, false);

            Postconditions.SetCondition(Conditions.CanSeePlayer, true);

            this.aiController = aiController;

            OnFinish = onFinish;
        }

        public void Initialize()
        {
            searchRooms = new();

            foreach (Room room in GameManager.dungeon.rooms)
            {
                if (SquareDistance(room.Center, Vertex.VectorToVertex(aiController.transform.position)) <= radius * radius)
                {
                    searchRooms.Push(new(room, searchPointDensity));
                }
            }

            // Check for valid rooms
            if (searchRooms.TryPop(out SearchRoom searchRoom))
            {
                currentSearchRoom = searchRoom;
            }
            else
            {
                OnFinish();
                return;
            }

            if (currentSearchRoom.TryFindNextPath()

            currentPathIndex = 0;

            GetNextRoom();
        }

        public void Run()
        {
            // Check if reached next search point
            if (SquareDistance(path[currentPathIndex].position, Vertex.VectorToVertex(aiController.transform.position)) < 0.01f)
            {
                FindNextPath();

                if (path == null)
                {
                    GetNextRoom();

                    if (currentRoom == null)
                    {
                        OnFinish();
                        return;
                    }
                    else
                    {
                        FindNextPath();
                    }
                }
            }

            // Check if reached next pathfinding node
            if (SquareDistance(path[currentPathIndex].position, Vertex.VectorToVertex(movementController.transform.position)) < 0.01f)
            {
                currentPathIndex++;

                if (currentPathIndex >= path.Count)
                {

                }
            }
        }
    }

    public class RangedAttack : IAction
    {
        public int Cost { get; private set; }

        public WorldState Preconditions { get; private set; }
        public WorldState Postconditions { get; private set; }
        public System.Action OnFinish { get; private set; }

        public RangedAttack(System.Action onFinish)
        {
            Preconditions = new(new());
            Postconditions = new(new());

            Preconditions.SetCondition(Conditions.CanSeePlayer, true);
            Preconditions.SetCondition(Conditions.HasRanged, true);
            Preconditions.SetCondition(Conditions.HasAmmo, true);

            Postconditions.SetCondition(Conditions.PlayerDead, true);

            OnFinish = onFinish;
        }

        public void Initialize() { }

        public void Run()
        {
            OnFinish();
        }
    }

    public class MeleeAttack : IAction
    {
        public int Cost { get; private set; }

        public WorldState Preconditions { get; private set; }
        public WorldState Postconditions { get; private set; }
        public System.Action OnFinish { get; private set; }

        public MeleeAttack(System.Action onFinish)
        {
            Preconditions = new(new());
            Postconditions = new(new());

            Cost = 2;
            Preconditions.SetCondition(Conditions.CanSeePlayer, true);
            Preconditions.SetCondition(Conditions.NearPlayer, true);
            Preconditions.SetCondition(Conditions.HasMelee, true);

            Postconditions.SetCondition(Conditions.PlayerDead, true);

            OnFinish = onFinish;
        }

        public void Initialize() { }

        public void Run()
        {
            OnFinish();
        }
    }

    public class Chase : IAction
    {
        public int Cost { get; private set; }

        public WorldState Preconditions { get; private set; }
        public WorldState Postconditions { get; private set; }
        public System.Action OnFinish { get; private set; }

        public Chase(System.Action onFinish)
        {
            Preconditions = new(new());
            Postconditions = new(new());

            Cost = 2;
            Preconditions.SetCondition(Conditions.CanSeePlayer, true);
            Preconditions.SetCondition(Conditions.NearPlayer, false);

            Postconditions.SetCondition(Conditions.NearPlayer, true);

            OnFinish = onFinish;
        }

        public void Initialize() { }

        public void Run()
        {
            OnFinish();
        }
    }

    public class Recover : IAction
    {
        public int Cost { get; private set; }

        public WorldState Preconditions { get; private set; }
        public WorldState Postconditions { get; private set; }
        public System.Action OnFinish { get; private set; }

        public Recover(System.Action onFinish)
        {
            Preconditions = new(new());
            Postconditions = new(new());

            Cost = 2;
            Preconditions.SetCondition(Conditions.LowHealth, true);
            Preconditions.SetCondition(Conditions.NearHealth, true);

            Postconditions.SetCondition(Conditions.LowHealth, false);

            OnFinish = onFinish;
        }

        public void Initialize() { }

        public void Run()
        {
            OnFinish();
        }
    }
}