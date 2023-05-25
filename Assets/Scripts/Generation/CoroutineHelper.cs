using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Generation
{
    public class CoroutineHelper : MonoBehaviour
    {
        public IEnumerator DelayDrawTriangle(object triangle, float drawTime, Color color, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            
            DungeonDebug.DrawTriangle(triangle, drawTime, color);
        }
    }
}