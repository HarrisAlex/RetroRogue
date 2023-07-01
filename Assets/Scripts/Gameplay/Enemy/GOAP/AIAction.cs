using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Action")]
public class AIAction : ScriptableObject
{
    public int cost;
    public List<ConditionValuePair> preConditions;
    public List<ConditionValuePair> postConditions;
}

public class AIActionWrapper
{
    public AIAction action;
    public List<AIActionWrapper> connections;

    public float hCost;
    public float FCost { get => action.cost + hCost; }

    public AIActionWrapper(AIAction action)
    {
        this.action = action;
        connections = new();
    }

    public int CanReachState(HashSet<AIActionWrapper> closed, WorldState state , int safety = 0)
    {
        if (safety > 5000)
            return -1;

        if (action.postConditions == state)
        {
            return action.cost;
        }
            

        foreach (AIActionWrapper action in connections)
        {
            closed.Add(action);

            safety++;

            return action.action.cost + CanReachState(closed, state, safety);
        }

        return -1;
    }
}