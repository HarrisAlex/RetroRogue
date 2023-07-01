using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Condition"), System.Serializable]
public class Condition : ScriptableObject
{
    public bool defaultValue;
}

[System.Serializable]
public struct ConditionValuePair
{
    public Condition condition;
    public bool value;

    public ConditionValuePair(Condition condition, bool value)
    {
        this.condition = condition;
        this.value = value;
    }
}
