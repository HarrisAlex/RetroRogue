using System.Collections.Generic;
using Assets.Scripts.AI;
using UnityEngine;

namespace Assets.Scripts.AI
{
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
    }

    public enum TerminationType
    {
        Time,
        Condition,
        Animation
    }

    public interface IAnimatedAction
    {
        public int GetAnimationHash();
        public TerminationType GetTerminationType();
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
    }

    public class AUseObject : Action, IAnimatedAction
    {
        private int animationHash;
        private TerminationType terminationType;

        public AUseObject(WorldState preconditions, WorldState postconditions, string animationName, TerminationType terminationType)
        {
            cost = 2;

            this.preconditions = preconditions;
            this.postconditions = postconditions;

            animationHash = Animator.StringToHash(animationName);
            this.terminationType = terminationType;
        }

        public int GetAnimationHash()
        {
            return animationHash;
        }

        public TerminationType GetTerminationType()
        {
            return terminationType;
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
    }
}