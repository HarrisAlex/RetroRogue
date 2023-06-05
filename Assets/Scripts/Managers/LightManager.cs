using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LightManager : MonoBehaviour
{
    private static Transform player;
    private static List<LightInfo> lights;
    private static LightManager instance;

    [SerializeField] private float lightCheckFrequency = 3;

    private struct LightInfo
    {
        public Light light;
        public float distance;
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        player = GameManager.player;
    }

    public static void Initialize()
    {
        player = GameManager.player;

        lights = new List<LightInfo>();
        Light[] allLights = FindObjectsOfType<Light>();

        for (int i = 0; i < allLights.Length; i++)
        {
            LightInfo tmp;
            tmp.light = allLights[i];
            tmp.distance = (allLights[i].transform.position - player.position).sqrMagnitude;

            lights.Add(tmp);
        }

        lights.Sort((a, b) => a.distance.CompareTo(b.distance));
    }

    private void Update()
    {
        if (lights == null) return;

        if (lights.Count > 8)
        {
            LightInfo tmp;
            for (int i = 0; i < lights.Count; i++)
            {
                tmp.light = lights[i].light;
                tmp.distance = (lights[i].light.transform.position - player.position).sqrMagnitude;

                lights[i] = tmp;
            }

            lights.Sort((a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].light.enabled = i < 8;
            }
        }
    }

    public static void AddLight(Light light)
    {
        if (HasLight(light))
        {
            return;
        }

        LightInfo tmp;
        tmp.light = light;
        tmp.distance = (light.transform.position - player.position).sqrMagnitude;

        lights.Add(tmp);
    }

    public static void RemoveLight(Light light)
    {
        if (!HasLight(light))
        {
            return;
        }

        //TODO remove light
    }

    public static bool HasLight(Light light)
    {
        for (int i = 0; i < lights.Count; i++)
        {
            if (lights[i].light == light)
            {
                return true;
            }
        }

        return false;
    }
}
