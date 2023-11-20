using System;
using Assets.Scripts.AI;
using System.Collections.Generic;
using UnityEngine;

public interface ISmartObject
{
    public List<Type> AllowedTypes { get; }
    public WorldState Preconditions { get; }
    public WorldState Postconditions { get; }
    public Transform Transform { get; }
    public AnimationData AnimationData { get; }
}