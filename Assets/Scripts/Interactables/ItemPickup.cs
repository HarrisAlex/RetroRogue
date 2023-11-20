using Assets.Scripts.AI;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable, ISmartObject
{
    public bool CurrentlyInteractable { get; set; }
    [SerializeField] private List<string> allowedTypes;
    [SerializeField] private List<ConditionValuePair> preconditions;
    [SerializeField] private List<ConditionValuePair> postconditions;

    [Header("Animation settings")]
    [SerializeField] private new AnimationClip animation;
    [SerializeField] private AnimationData.ExitType animationExitType;
    [SerializeField] private float animationDuration;
    [SerializeField] private bool useAnimationClipDuration;
    [SerializeField] private ConditionValuePair animationExitCondition;

    public List<System.Type> AllowedTypes
    {
        get
        {
            List<System.Type> result = new();

            foreach (string character in allowedTypes)
            {
                System.Type type = System.Type.GetType("Assets.Scripts.AI." + character);

                if (!result.Contains(type))
                    result.Add(type);
            }

            return result;
        }
    }
    public WorldState Preconditions => new(preconditions);
    public WorldState Postconditions => new(postconditions);
    public Transform Transform => transform;
    public AnimationData AnimationData
    {
        get
        {
            float duration = (useAnimationClipDuration && animation != null) ? animation.length : animationDuration;

            if (animationExitType == AnimationData.ExitType.Time)
            {
                return new(animation, duration);
            }
            else
                return new(animation, animationExitCondition);
        }
    }

    public void Interact()
    {
        throw new System.NotImplementedException();
    }
}
