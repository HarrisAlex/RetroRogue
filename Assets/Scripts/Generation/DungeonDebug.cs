
using UnityEngine;
using static Assets.Scripts.Generation.DungeonGeneration;

namespace Assets.Scripts.Generation
{
    public class DungeonDebug : MonoBehaviour
    {
#if UNITY_EDITOR
        private static Dungeon dungeon;
        public static DungeonDebug instance;

        public bool enableDebugging = false;
        public bool drawWallTiles = false;
        public bool hideWalls = false;

        private static GameObject wallsContainer;

        private void OnValidate()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(this);
        }

        public static void SetDungeon(Dungeon dungeon)
        {
            DungeonDebug.dungeon = dungeon;
        }

        private void Update()
        {
            if (!enableDebugging) return;

            DrawWallsTiles();
            ToggleWalls();
        }

        public static void DrawWallsTiles()
        {
            if (!InstanceVariablesSet()) return;
            if (!instance.drawWallTiles) return;

            IterateArea(0, 0, dungeon.GetWidth() - 1, dungeon.GetHeight() - 1, (int x, int y) =>
            {
                if (!IsWall(dungeon.GetTile(x, y))) return;

                DrawRectangle(x, y, 1, 1, 0, Color.green);
            });
        }

        public static void ToggleWalls()
        {
            if (!InstanceVariablesSet()) return;

            if (wallsContainer == null)
            {
                wallsContainer = GameObject.Find("Walls");

                if (wallsContainer == null) return;
            }

            if (instance.hideWalls == wallsContainer.activeInHierarchy)
                wallsContainer.SetActive(!instance.hideWalls);
        }

        public static void DrawRectangle(float x, float y, float width, float height, float time, Color color)
        {
            Vector3 topLeft = new Vector3(x, 0, y + height);
            Vector3 topRight = new Vector3(x + width, 0, y + height);
            Vector3 bottomLeft = new Vector3(x, 0, y);
            Vector3 bottomRight = new Vector3(x + width, 0, y);

            Debug.DrawLine(topRight, bottomRight, color, time);
            Debug.DrawLine(bottomRight, bottomLeft, color, time);
            Debug.DrawLine(bottomLeft, topLeft, color, time);
        }

        private static bool InstanceVariablesSet()
        {
            if (instance == null) return false;
            if (dungeon == null) return false;

            return true;
        }
#endif
    }
}
