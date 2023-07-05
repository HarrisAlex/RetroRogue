using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Assets.Scripts.AI;

public class SmartObjectManager : MonoBehaviour
{
    private static Dictionary<Type, List<ISmartObject>> smartObjects;
    private static SmartObjectManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);

        // Get all smart objects
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        smartObjects = new();

        foreach (GameObject rootObject in rootObjects)
        {
            ISmartObject[] children = rootObject.GetComponentsInChildren<ISmartObject>();

            foreach (ISmartObject smartObject in children)
            {
                List<Type> validAgents = smartObject.GetAllowedTypes();
                foreach (Type type in validAgents)
                {
                    if (type == null) continue;

                    if (smartObjects.ContainsKey(type))
                        smartObjects[type].Add(smartObject);
                    else
                    {
                        List<ISmartObject> tmp = new();
                        tmp.Add(smartObject);

                        smartObjects.Add(type, tmp);
                    }
                }
            }
        }
    }

    public static List<ISmartObject> GetSmartObjects(Character character)
    {
        if (smartObjects == null) return new();

        Type type = character.GetType();
        if (smartObjects.ContainsKey(type))
            return smartObjects[type];

        return new();
    }
}