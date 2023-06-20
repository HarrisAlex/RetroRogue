using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Generation
{
    public class DungeonRenderer
    {
        private static Transform root;

        public static Vector3 RenderDungeon(DungeonGenerator dungeon)
        {
            for (int x = 0; x < dungeon.Settings.gridWidth; x++)
            {
                for (int y = 0; y < dungeon.Settings.gridHeight; y++)
                {
                    switch (dungeon.GetCoordinate(x, y))
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

            MeshDescription floorDescription = MeshGenerator.CreatePlaneDescription(dungeon.Settings);
            Transform floor = MeshGenerator.GenerateMesh(floorDescription, dungeon.Settings.floorMaterial);
            floor.localScale = new Vector3(1, -1, 1);
            floor.gameObject.AddComponent<BoxCollider>();

            MeshDescription ceilingDescription = MeshGenerator.CreatePlaneDescription(dungeon.Settings);
            Transform ceiling = MeshGenerator.GenerateMesh(ceilingDescription, dungeon.Settings.ceilingMaterial);
            ceiling.position = new Vector3(0, 3, 0);

            if (dungeon.TryGetRandomRoomCenter(out Geometry.Vertex randomRoomCenter))
            {
                return new Vector3(randomRoomCenter.x, 0, randomRoomCenter.y);
            }
            else
            {
                return Vector3.zero;
            }
        }

        private static void CreateWall(float x, float y, float angle)
        {
            if (root == null)
            {
                root = new GameObject("Dungeon").transform;
            }

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
            {
                root = new GameObject("Dungeon").transform;
            }

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