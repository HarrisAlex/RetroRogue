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
                result.SetCondition(condition.condition, condition.value);

            foreach (ConditionValuePair condition in addition.Conditions)
                result.SetCondition(condition.condition, condition.value);

            return result;
        }

        public bool ContainsCondition(ConditionValuePair condition)
        {
            foreach (ConditionValuePair tmpCond in Conditions)
            {
                if (condition == tmpCond)
                    return true;
            }

            return false;
        }

        public bool GetCondition(ConditionValuePair condition)
        {
            foreach (ConditionValuePair tmpCond in Conditions)
            {
                if (condition == tmpCond)
                    return tmpCond.value;
            }

            return false;
        }

        public void SetCondition(string condition, bool value)
        {
            ConditionValuePair tmpCond = new(condition, value);
            if (ContainsCondition(tmpCond))
            {
                for (int i = 0; i < Conditions.Count; i++)
                {
                    if (Conditions[i].condition == condition)
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
                if (!ContainsCondition(condition)) return false;

                if (GetCondition(condition) != state.GetCondition(condition)) return false;
            }

            return true;
        }

        public bool MatchesState(ConditionValuePair condition)
        {
            if (!ContainsCondition(condition)) return false;

            if (GetCondition(condition) != condition.value) return false;

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
                if (right.ContainsCondition(condition))
                {
                    if (left.GetCondition(condition) != right.GetCondition(condition))
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
        public string condition;
        public bool value;

        public ConditionValuePair(string condition, bool value)
        {
            this.condition = condition;
            this.value = value;
        }

        public static bool operator ==(ConditionValuePair a, ConditionValuePair b)
        {
            return (a.condition == b.condition);
        }

        public static bool operator !=(ConditionValuePair a, ConditionValuePair b)
        {
            return (a.condition != b.condition);
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

    public class Conditions
    {
        public const string HasAmmo = "hasAmmo";
        public const string HasRanged = "hasRanged";
        public const string HasMelee = "hasMelee";
        public const string NearPlayer = "nearPlayer";
        public const string PlayerDead = "playerDead";
        public const string Tired = "tired";
        public const string NearAlly = "nearAlly";
        public const string Bored = "bored";
        public const string Hungry = "hungry";
        public const string NearBed = "nearBed";
        public const string LowHealth = "lowHealth";
        public const string NearHealth = "nearHealth";

        public const string AwareOfPlayer = "awareOfPlayer";
        public const string AwareOfSound = "awareOfSound";

        public const string CanSeePlayer = "canSeePlayer";

        public const string NearObject = "nearObject";
    }
}