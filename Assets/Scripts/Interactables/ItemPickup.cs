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

    [Header("Animation settings")]
    [SerializeField] private TerminationType terminationType;
    [SerializeField] private string animationName;
    [SerializeField] private float animationDuration;
    [SerializeField] private Condition terminationCondition;

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


    public void Interact()
    {
        throw new System.NotImplementedException();
    }

    public TerminationType GetAnimationTerminationType()
    {
        return terminationType;
    }

    public string GetAnimationName()
    {
        return animationName;
    }

    public float GetAnimationDuration()
    {
        return animationDuration;
    }

    public Condition GetAnimationCondition()
    {
        return terminationCondition;
    }
}
