using System.Collections.Generic;
using UnityEngine;

public class SystemsManager : MonoBehaviour
{
    private Dictionary<System.Type, List<Object>> systems;

    public static SystemsManager instance;



    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }

    private bool GetAllOfType(System.Type type, out Object[] objectArray)
    {
        objectArray = FindObjectsOfType(type);

        if (objectArray.Length < 1)
        {
            return false;
        }

        return true;
    }

    private void InitializeSystems()
    {
        Object[] objectArray;

        foreach (System.Type type in SystemsData.systemTypes)
        {
            if (GetAllOfType(type, out objectArray))
            {
                systems.Add(type, new List<Object>(objectArray));
            }
        }
    }

    /// <summary>
    /// Get all objects in a particular type of system and return whether or not any exist.
    /// </summary>
    /// <typeparam name="T">The type of system.</typeparam>
    /// <param name="objects">The object array to be filled.</param>
    /// <returns></returns>
    public bool GetObjectsInSystem<T>(out Object[] objects)
    {
        if (!systems.ContainsKey(typeof(T)))
        {
            objects = null;

            return false;
        }

        objects = systems[typeof(T)].ToArray();

        return true;
    }
}

class SystemsData
{
    public static System.Type[] systemTypes =
    {
        typeof(IElectrifiable),
        typeof (IFlammable),
        typeof(IDamagable),
        typeof(IInteractable),
        typeof(ILock)
    };
}