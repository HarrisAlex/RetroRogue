using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    private static Transform player;
    private static List<LightInfo> lights = new List<LightInfo>();
    private static LightManager instance;

    [SerializeField, Tooltip("Maximum distance from player at which a light can be visible")]
    private float maximumDistance = 50;

    public struct LightInfo
    {
        public Light light;
        public float distance;
    }

    private void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);

        player = GameManager.player;
    }

    public static void Initialize()
    {
        player = GameManager.player;
    }

    private void Update()
    {
        if (lights == null) return;

        if (lights.Count < 9) return;

        OrderLights();

        for (int i = 0; i < lights.Count; i++)
        {
            if (i >= 7 || lights[i].distance > (maximumDistance * maximumDistance))
                lights[i].light.enabled = false;
            else
                lights[i].light.enabled = true;
        }
    }

    public static void AddLight(Light light)
    {
        if (HasLight(light)) return;

        LightInfo tmp;
        tmp.light = light;
        tmp.distance = -1;

        lights.Add(tmp);
    }

    public static void RemoveLight(Light light)
    {
        LightInfo lightInfo;
        if (GetLight(light, out lightInfo))
        {
            lights.Remove(lightInfo);
        }
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

    public static bool GetLight(Light light, out LightInfo lightInfo)
    {
        lightInfo.light = null;
        lightInfo.distance = -1;

        if (!HasLight(light)) return false;

        for (int i = 0; i < lights.Count; i++)
        {
            if (lights[i].light == light)
            {
                lightInfo = lights[i];
                return true;
            }
        }

        return false;
    }

    private static void OrderLights()
    {
        LightInfo tmp;
        for (int i = 0; i < lights.Count; i++)
        {
            tmp.light = lights[i].light;
            tmp.distance = (lights[i].light.transform.position - player.position).sqrMagnitude;

            lights[i] = tmp;
        }

        lights.Sort((a, b) => a.distance.CompareTo(b.distance));
    }
}
