using System.Collections.Generic;

namespace Assets.Scripts.AI
{
    public struct WorldState
    {
        public List<Condition> Conditions { get; private set; }

        public WorldState(List<Condition> conditions)
        {
            Conditions = conditions;
        }

        public static WorldState CombineStates(WorldState current, WorldState addition)
        {
            WorldState result = new(new());

            foreach (Condition condition in current.Conditions)
                result.SetCondition(condition.name, condition.value);

            foreach (Condition condition in addition.Conditions)
                result.SetCondition(condition.name, condition.value);

            return result;
        }

        public bool ContainsCondition(Condition condition)
        {
            foreach (Condition tmpCond in Conditions)
            {
                if (condition == tmpCond)
                    return true;
            }

            return false;
        }

        public bool GetCondition(Condition condition)
        {
            foreach (Condition tmpCond in Conditions)
            {
                if (condition == tmpCond)
                    return tmpCond.value;
            }

            return false;
        }

        public void SetCondition(string condition, bool value)
        {
            Condition tmpCond = new(condition, value);
            if (ContainsCondition(tmpCond))
            {
                for (int i = 0; i < Conditions.Count; i++)
                {
                    if (Conditions[i].name == condition)
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
            foreach (Condition condition in state.Conditions)
            {
                if (!ContainsCondition(condition)) return false;

                if (GetCondition(condition) != state.GetCondition(condition)) return false;
            }

            return true;
        }

        public bool MatchesState(Condition condition)
        {
            if (!ContainsCondition(condition)) return false;

            if (GetCondition(condition) != condition.value) return false;

            return true;
        }

        public override bool Equals(object o)
        {
            if (o.GetType() != typeof(WorldState))
                return false;

            return this == (WorldState)o;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(WorldState a, WorldState b)
        {
            if (a.Conditions == null || b.Conditions == null)
                return false;

            foreach (Condition condition in a.Conditions)
            {
                if (b.ContainsCondition(condition))
                {
                    if (a.GetCondition(condition) != b.GetCondition(condition))
                        return false;
                }
                else
                    return false;
            }

            return true;
        }

        public static bool operator !=(WorldState a, WorldState b)
        {
            if (a.Conditions == null || b.Conditions == null)
                return true;

            foreach (Condition condition in a.Conditions)
            {
                if (b.ContainsCondition(condition))
                {
                    if (a.GetCondition(condition) != b.GetCondition(condition))
                        return true;
                }
            }

            return false;
        }
    }

    [System.Serializable]
    public struct Condition
    {
        public string name;

        public bool value;

        public Condition(string name, bool value)
        {
            this.name = name;
            this.value = value;
        }

        public static bool operator ==(Condition a, Condition b)
        {
            return a.name == b.name;
        }

        public static bool operator !=(Condition a, Condition b)
        {
            return a.name != b.name;
        }

        public override bool Equals(object o)
        {
            if (o.GetType() != typeof(Condition))
                return false;

            return this == (Condition)o;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Conditions
    {
        public const string HAS_AMMO = "hasAmmo";
        public const string HAS_RANGED = "hasRanged";
        public const string HAS_MELEE = "hasMelee";
        public const string NEAR_PLAYER = "nearPlayer";
        public const string PLAYER_DEAD = "playerDead";
        public const string TIRED = "tired";
        public const string NEAR_ALLY = "nearAlly";
        public const string BORED = "bored";
        public const string HUNGRY = "hungry";
        public const string NEAR_BED = "nearBed";
        public const string LOW_HEALTH = "lowHealth";
        public const string NEAR_HEALTH = "nearHealth";

        public const string AWARE_OF_PLAYER = "awareOfPlayer";
        public const string AWARE_OF_SOUND = "awareOfSound";

        public const string CAN_SEE_PLAYER = "canSeePlayer";
    }
}