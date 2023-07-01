using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldState : ScriptableObject
{
    public List<ConditionValuePair> conditions = new();

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

    public bool ContainsCondition(ConditionValuePair condition)
    {
        foreach (ConditionValuePair pair in conditions)
        {
            if (pair.condition == condition.condition)
                return true;
        }

        return false;
    }

    public bool GetConditionValue(ConditionValuePair condition)
    {
        if (!ContainsCondition(condition)) return false;

        foreach (ConditionValuePair pair in conditions)
        {
            if (pair.condition == condition.condition)
                return pair.value;
        }

        return false;
    }

    public static bool operator ==(WorldState a, WorldState b)
    {
        if (a.conditions == null || b.conditions == null)
            return false;

        foreach (ConditionValuePair condition in a.conditions)
        {
            if (b.ContainsCondition(condition))
            {
                if (a.GetConditionValue(condition) != b.GetConditionValue(condition))
                    return false;
            }
        }

        return true;
    }

    public static bool operator !=(WorldState a, WorldState b)
    {
        foreach (ConditionValuePair condition in a.conditions)
        {
            if (b.ContainsCondition(condition))
            {
                if (a.GetConditionValue(condition) != b.GetConditionValue(condition))
                    return true;
            }
        }

        return false;
    }

    public static bool operator ==(List<ConditionValuePair> conditions, WorldState state)
    {
        foreach (ConditionValuePair condition in conditions)
        {
            if (state.ContainsCondition(condition))
            {
                if (condition.value != state.GetConditionValue(condition))
                    return false;
            }
        }

        return true;
    }

    public static bool operator !=(List<ConditionValuePair> conditions, WorldState state)
    {
        foreach (ConditionValuePair condition in conditions)
        {
            if (state.ContainsCondition(condition))
            {
                if (condition.value != state.GetConditionValue(condition))
                    return true;
            }
        }

        return false;
    }
}