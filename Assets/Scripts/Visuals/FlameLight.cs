using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlameLight : MonoBehaviour
{
    [SerializeField] private Color[] colors;
    [SerializeField] private float animateSpeed = 2;

    private new Light light;

    private Color targetColor;

    private float targetIntensity;
    private float originalIntensity;

    private void Awake()
    {
        light = GetComponent<Light>();
        originalIntensity = light.intensity;

        StartCoroutine(AnimateFlame());
    }

    private void Update()
    {
        light.color = Color.LerpUnclamped(light.color, targetColor, animateSpeed * Time.deltaTime);
        light.intensity = Mathf.Lerp(light.intensity, targetIntensity, animateSpeed * Time.smoothDeltaTime);
    }

    IEnumerator AnimateFlame()
    {
        while (true)
        {
            targetColor = colors[Random.Range(0, colors.Length - 1)];
            targetIntensity = Random.Range(originalIntensity * 0.75f, originalIntensity * 1.5f);

            yield return new WaitForSeconds(Random.Range(0.3f, 0.7f) / animateSpeed);
        }
    }
}
