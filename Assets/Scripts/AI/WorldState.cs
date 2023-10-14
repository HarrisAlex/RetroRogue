using System.Collections.Generic;

namespace Assets.Scripts.AI
{
    public struct WorldState
    {
        public List<ConditionValuePair> Conditions { get; private set; }

        public WorldState(List<ConditionValuePair> conditions)
        {
            Conditions = conditions;
        }

        public static WorldState CombineStates(WorldState current, WorldState addition)
        {
            WorldState result = new(new());

            foreach (ConditionValuePair condition in current.Conditions)
                result.SetCondition(condition.condition, condition.specification, condition.value);

            foreach (ConditionValuePair condition in addition.Conditions)
                result.SetCondition(condition.condition, condition.specification, condition.value);

            return result;
        }

        public bool ContainsCondition(Condition condition, int specification)
        {
            foreach (ConditionValuePair tmpCond in Conditions)
            {
                if (condition == tmpCond.condition && specification == tmpCond.specification)
                    return true;
            }

            return false;
        }

        public bool GetCondition(Condition condition, int specification)
        {
            foreach (ConditionValuePair tmpCond in Conditions)
            {
                if (condition == tmpCond.condition && specification == tmpCond.specification)
                    return tmpCond.value;
            }

            return false;
        }

        public void SetCondition(Condition condition, int specification, bool value)
        {
            ConditionValuePair tmpCond = new(condition, specification, value);
            if (ContainsCondition(condition, specification))
            {
                for (int i = 0; i < Conditions.Count; i++)
                {
                    if (Conditions[i].condition == condition && Conditions[i].specification == specification)
                    {
                        Conditions[i] = tmpCond;
                        return;
                    }
                }
            }

            Conditions.Add(tmpCond);
        }

        public bool MatchesState(WorldState state)
        {
            foreach (ConditionValuePair condition in state.Conditions)
            {
                if (!ContainsCondition(condition.condition, condition.specification)) return false;

                if (GetCondition(condition.condition, condition.specification) != state.GetCondition(condition.condition, condition.specification)) return false;
            }

            return true;
        }

        public bool MatchesState(ConditionValuePair condition)
        {
            if (!ContainsCondition(condition.condition, condition.specification)) return false;

            if (GetCondition(condition.condition, condition.specification) != condition.value) return false;

            return true;
        }

        public override bool Equals(object o)
        {
            if (o is WorldState)
                return false;

            return this == (WorldState)o;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(WorldState left, WorldState right)
        {
            if (left.Conditions == null || right.Conditions == null)
                return false;

            foreach (ConditionValuePair condition in left.Conditions)
            {
                if (right.ContainsCondition(condition.condition, condition.specification))
                {
                    if (left.GetCondition(condition.condition, condition.specification) != right.GetCondition(condition.condition, condition.specification))
                        return false;
                }
                else
                    return false;
            }

            return true;
        }

        public static bool operator !=(WorldState a, WorldState b)
        {
            return !(a == b);
        }
    }

    [System.Serializable]
    public struct ConditionValuePair
    {
        public Condition condition;
        public int specification;
        public bool value;

        public ConditionValuePair(Condition condition, int specification, bool value)
        {
            this.condition = condition;
            this.specification = specification;
            this.value = value;
        }

        public static bool operator ==(ConditionValuePair a, ConditionValuePair b)
        {
            return (a.condition == b.condition) && (a.specification == b.specification);
        }

        public static bool operator !=(ConditionValuePair a, ConditionValuePair b)
        {
            return (a.condition != b.condition) || (a.specification != b.specification);
        }

        public override bool Equals(object o)
        {
            if (o is ConditionValuePair)
                return false;

            return this == (ConditionValuePair)o;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum Condition
    {
        HasAmmo,
        HasRanged,
        HasMelee,
        
        NearPlayer,
        PlayerDead,
        NearAlly,
        NearBed,
        NearHealth,
        NearObject,

        Tired,
        Bored,
        Hungry,

        LowHealth,
        
        AwareOfPlayer,
        AwareOfSound,
        CanSeePlayer
    }
}