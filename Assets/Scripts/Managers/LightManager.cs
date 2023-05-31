using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    private Transform player;
    private static List<Light> lights;
    private List<Light> activeLights;

    [SerializeField] private float lightCheckFrequency = 3;

    private void Start()
    {
        player = GameManager.player;

        lights = new List<Light>(FindObjectsOfType<Light>());

        StartCoroutine(CheckLights());
    }

    private void Update()
    {

    }

    IEnumerator CheckLights()
    {
        List<float> distances = new List<float>();
        activeLights = new List<Light>();

        while (true)
        {
            foreach (Light light in lights)
            {
                distances.Add((transform.position - light.transform.position).sqrMagnitude);
            }

            

            for (int i = 0; i < 8; i++)
            {
                activeLights.Add(distances[i])
            }

            yield return new WaitForSeconds(1 / lightCheckFrequency);
        }
    }

    float GetMaximumDistance()
    {
        float maxDistance = (lights[0].transform.position - player.position).sqrMagnitude;
        float distance;

        foreach (Light light in lights)
        {
            distance = (light.transform.position - player.position).sqrMagnitude;
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }

        return maxDistance;
    }

    public static void AddLight(Light light)
    {
        if (lights.Contains(light))
        {
            return;
        }

        lights.Add(light);
    }

    public static void RemoveLight(Light light)
    {
        if (lights.Contains(light))
        {
            lights.Remove(light);
        }
    }
}
