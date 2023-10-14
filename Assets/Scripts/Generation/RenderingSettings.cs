using UnityEngine;

namespace Assets.Scripts.Generation
{
    [CreateAssetMenu(menuName = "Dungeon/Settings/Rendering")]
    public class RenderingSettings : ScriptableObject
    {
        public int areaLightSamples = 5;
        public Color ambientColor = Color.black;
        public Color defaultLightColor = Color.white;
    }
}