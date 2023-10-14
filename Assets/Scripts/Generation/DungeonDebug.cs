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
        public bool drawFloorTiles = false;
        public bool hideWalls = false;
        public bool hideFloor = false;
        public bool drawEdges = false;
        public bool drawRooms = false;

        public float spacing = 0.1f;

        private static GameObject wallsContainer;
        private static GameObject floorContainer;

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
            if (!InstanceVariablesSet()) return;

            DrawWallsTiles();
            DrawFloorTiles();
            ToggleWalls();
            ToggleFloor();
            DrawEdges();
            DrawRooms();
        }

        public static void DrawWallsTiles()
        {
            if (!instance.drawWallTiles) return;

            IterateArea(0, 0, dungeon.GetWidth() - 1, dungeon.GetHeight() - 1, (int x, int y) =>
            {
                if (dungeon.GetTile(x, y) != TileType.Wall) return;

                DrawRectangle(x, y, 1, 1, 0, Color.magenta);
            });
        }

        public static void DrawFloorTiles()
        {
            if (!instance.drawFloorTiles) return;

            IterateArea(0, 0, dungeon.GetWidth() - 1, dungeon.GetHeight() - 1, (int x, int y) =>
            {
                if (dungeon.GetTile(x, y) != TileType.Floor) return;

                DrawRectangle(x, y, 1, 1, 0, Color.white);
            });
        }

        public static void ToggleWalls()
        {
            if (wallsContainer == null)
            {
                wallsContainer = GameObject.Find("Walls");

                if (wallsContainer == null) return;
            }

            if (instance.hideWalls == wallsContainer.activeInHierarchy)
                wallsContainer.SetActive(!instance.hideWalls);
        }

        public static void ToggleFloor()
        {
            if (floorContainer == null)
            {
                floorContainer = GameObject.Find("Floor");

                if (floorContainer == null) return;
            }

            if (instance.hideFloor == floorContainer.activeInHierarchy)
                floorContainer.SetActive(!instance.hideFloor);
        }

        public static void DrawEdges()
        {
            if (!instance.drawEdges) return;

            foreach (Edge edge in dungeon.edges)
            {
                Debug.DrawLine(edge.u.ToVector(), edge.v.ToVector(), Color.white, 0);
            }
        }

        public static void DrawRooms()
        {
            if (!instance.drawRooms) return;

            foreach (Room room in dungeon.rooms)
            {
                DrawRectangle(room.xPosition, room.yPosition, room.width, room.height, 0, Color.cyan);
            }
        }

        public static void DrawRectangle(float x, float y, float width, float height, float time, Color color)
        {
            Vector3 topLeft = new Vector3(x + instance.spacing, 0, y + height - instance.spacing);
            Vector3 topRight = new Vector3(x + width - instance.spacing, 0, y + height - instance.spacing);
            Vector3 bottomLeft = new Vector3(x + instance.spacing, 0, y + instance.spacing);
            Vector3 bottomRight = new Vector3(x + width - instance.spacing, 0, y + instance.spacing);

            Debug.DrawLine(topRight, bottomRight, color, time);
            Debug.DrawLine(bottomRight, bottomLeft, color, time);
            Debug.DrawLine(bottomLeft, topLeft, color, time);
            Debug.DrawLine(topLeft, topRight, color, time);
        }

        public static void DrawText(string text, Vertex position)
        {
            GameObject go = new("Text");
            go.transform.position = position.ToVector();
            go.transform.localScale = new(0.4f, 0.4f, 1f);
            go.transform.eulerAngles = new(90, 0, 0);

            TextMesh textComponent = go.AddComponent<TextMesh>();
            textComponent.text = text;
            textComponent.fontSize = 24;
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
