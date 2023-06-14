using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Generation
{
    public class DungeonRenderer
    {
        private static Transform root;

        public struct MeshDescription
        {
            public string name;
            public Vector3[] vertices;
            public int[] triangles;
            public Vector2[] uvs;
        }

        public static Vector3 RenderDungeon(GenerationSettings settings)
        {
            DungeonGenerator dungeonGenerator = new DungeonGenerator(settings);
            TileType[,] grid = dungeonGenerator.Generate((int)System.DateTime.Now.Ticks);

            for (int x = 0; x < settings.gridWidth; x++)
            {
                for (int y = 0; y < settings.gridHeight; y++)
                {
                    if (grid[x, y] == TileType.RoomWall || grid[x, y] == TileType.HallwayWall)
                    {
                        CreateWall(x, y, 0);
                    }
                }
            }

            Transform floor = GenerateMesh(CreatePlaneDescription(settings), settings.floorMaterial);
            floor.localScale = new Vector3(1, -1, 1);
            floor.gameObject.AddComponent<BoxCollider>();

            Transform ceiling = GenerateMesh(CreatePlaneDescription(settings), settings.ceilingMaterial);
            ceiling.position = new Vector3(0, 3, 0);

            Light lightComponent;
            foreach (Room room in dungeonGenerator.rooms)
            {
                Color color = Random.ColorHSV();

                foreach (Vertex light in room.lights)
                {
                    lightComponent = CreateLight((int)light.x, (int)light.y);
                    LightManager.AddLight(lightComponent);
                }
            }

            Vertex randomRoomCenter;
            if (dungeonGenerator.TryGetRandomRoomCenter(out randomRoomCenter))
            {
                return new Vector3(randomRoomCenter.x, 0, randomRoomCenter.y);
            }
            else
            {
                return Vector3.zero;
            }
        }

        private static void CreateWall(int x, int y, float angle)
        {
            if (root == null)
            {
                root = new GameObject("Dungeon").transform;
            }

            Transform go = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            go.position = new Vector3(x, 1.5f, y);
            go.localScale = new Vector3(1, 3, Mathf.Abs(angle) < 1 ? 1 : 1.415f);
            go.eulerAngles = new Vector3(0, angle, 0);
            go.parent = root;
        }

        private static MeshDescription CreatePlaneDescription(GenerationSettings settings)
        {
            MeshDescription description = new MeshDescription();

            description.name = "Floor";
            description.vertices = new Vector3[((settings.gridWidth / 2) + 1) * ((settings.gridHeight / 2) + 1)];
            description.triangles = new int[(settings.gridWidth / 2) * (settings.gridHeight / 2) * 6];
            description.uvs = new Vector2[description.vertices.Length];

            for (int x = 0, i = 0; x <= settings.gridWidth / 2; x++)
            {
                for (int y = 0; y <= settings.gridHeight / 2; y++, i++)
                {
                    description.vertices[i] = new Vector3(x * 2, 0, y * 2);
                    description.uvs[i] = new Vector2(x * 2, y * 2);
                }
            }

            for (int ti = 0, vi = 0, y = 0; y < settings.gridHeight / 2; y++, vi++)
            {
                for (int x = 0; x < settings.gridWidth / 2; x++, ti += 6, vi++)
                {
                    description.triangles[ti] = vi;
                    description.triangles[ti + 3] = description.triangles[ti + 2] = vi + 1;
                    description.triangles[ti + 4] = description.triangles[ti + 1] = vi + (settings.gridWidth / 2) + 1;
                    description.triangles[ti + 5] = vi + (settings.gridWidth / 2) + 2;
                }
            }

            return description;
        }

        private static Transform GenerateMesh(MeshDescription description, Material material)
        {
            Mesh mesh = new Mesh();
            GameObject go = new GameObject();
            mesh.name = go.name = description.name;

            go.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.material = material;

            mesh.vertices = description.vertices;
            mesh.triangles = description.triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.uv = description.uvs;

            return go.transform;
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