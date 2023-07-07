using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable, ISmartObject
{
    public bool CurrentlyInteractable { get; set; }
    [SerializeField] private List<string> allowedTypes;
    [SerializeField] private List<Condition> preconditions;
    [SerializeField] private List<Condition> postconditions;

    public WorldState GetPostconditions()
    {
        return new(postconditions);
    }

    public WorldState GetPreconditions()
    {
        return new(preconditions);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public List<Type> GetAllowedTypes()
    {
        List<Type> result = new();

        foreach (string character in allowedTypes)
        {
            Type type = Type.GetType("Assets.Scripts.AI." + character);

            if (!result.Contains(type))
                result.Add(type);
        }

        return result;
    }

    public string GetAnimationName()
    {
        return "Pick Up";
    }

    public void Interact()
    {
        throw new System.NotImplementedException();
    }

    public float GetAnimationDuration()
    {
        return 0;
    }

    public Condition GetAnimationCondition()
    {
        return new Condition("", false);
    }
}
