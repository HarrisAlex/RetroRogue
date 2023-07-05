using System;
using Assets.Scripts.AI;
using System.Collections.Generic;
using UnityEngine;

public interface ISmartObject
{
    public List<Type> GetAllowedTypes();
    public WorldState GetPreconditions();
    public WorldState GetPostconditions();
    public Transform GetTransform();
}