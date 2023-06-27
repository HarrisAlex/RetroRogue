using UnityEngine;

namespace Assets.Scripts.Generation
{
    public class DungeonRenderer
    {
        private static Transform root;

        public static Vector3 RenderDungeon(Dungeon dungeon)
        {
            if (root != null)
            {
                Object.Destroy(root.gameObject);
                root = null;
            }

            for (int x = 0; x < dungeon.settings.gridWidth; x++)
            {
                for (int y = 0; y < dungeon.settings.gridHeight; y++)
                {
                    switch (dungeon.grid[x, y])
                    {
                        case TileType.Wall:
                            CreateWall(x, y, 0);
                            break;
                        case TileType.TopLeftCorner:
                            CreateCorner(x, y, TileType.TopLeftCorner);
                            break;
                        case TileType.TopRightCorner:
                            CreateCorner(x, y, TileType.TopRightCorner);
                            break;
                        case TileType.BottomLeftCorner:
                            CreateCorner(x, y, TileType.BottomLeftCorner);
                            break;
                        case TileType.BottomRightCorner:
                            CreateCorner(x, y, TileType.BottomRightCorner);
                            break;
                    }
                }
            }

            MeshDescription floorDescription = MeshGenerator.CreatePlaneDescription(dungeon.settings);
            Transform floor = MeshGenerator.GenerateMesh(floorDescription, dungeon.settings.floorMaterial);
            floor.localScale = new Vector3(1, -1, 1);
            floor.gameObject.AddComponent<BoxCollider>();
            floor.parent = root;

            MeshDescription ceilingDescription = MeshGenerator.CreatePlaneDescription(dungeon.settings);
            Transform ceiling = MeshGenerator.GenerateMesh(ceilingDescription, dungeon.settings.ceilingMaterial);
            ceiling.position = new Vector3(0, 3, 0);
            ceiling.parent = root;

            if (dungeon.TryGetRandomRoomCenter(out Geometry.Vertex randomRoomCenter))
                return new Vector3(randomRoomCenter.x, 0, randomRoomCenter.y);
            else
                return Vector3.zero;
        }

        private static void CreateWall(float x, float y, float angle)
        {
            if (root == null)
                root = new GameObject("Dungeon").transform;

            float scale = Mathf.Abs(angle) < 1 ? 1 : 1.415f;

            Transform go = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            go.position = new Vector3(x, 1.5f, y);
            go.localScale = new Vector3(scale, 3, scale);
            go.eulerAngles = new Vector3(0, angle, 0);
            go.parent = root;
        }

        private static void CreateCorner(int x, int y, TileType cornerType)
        {
            float angle = 0;
            float xPos = x, yPos = y;

            switch (cornerType)
            {
                case TileType.TopLeftCorner:
                    angle = 45;
                    xPos = x - 0.5f;
                    yPos = y + 0.5f;
                    break;
                case TileType.BottomRightCorner:
                    angle = 45;
                    xPos = x + 0.5f;
                    yPos = y - 0.5f;
                    break;
                case TileType.BottomLeftCorner:
                    angle = -45;
                    xPos = x - 0.5f;
                    yPos = y - 0.5f;
                    break;
                case TileType.TopRightCorner:
                    angle = -45;
                    xPos = x + 0.5f;
                    yPos = y + 0.5f;
                    break;
            }

            CreateWall(xPos, yPos, angle);
        }

        private static Light CreateLight(int x, int y)
        {
            if (root == null)
                root = new GameObject("Dungeon").transform;

            Transform go = new GameObject("Light").transform;
            go.position = new Vector3(x, 1.5f, y);
            go.parent = root;

            Light light = go.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8;
            light.intensity = 3;

            return light;
        }
    }
}