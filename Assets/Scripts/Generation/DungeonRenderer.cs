using UnityEngine;
using static Assets.Scripts.Generation.DungeonGeneration;
using System.Collections.Generic;

namespace Assets.Scripts.Generation
{
    public class DungeonRenderer : MonoBehaviour
    {
        public Material floorMaterial;
        public Material ceilingMaterial;
        public Material wallMaterial;

        private Transform root;
        private Transform wallRoot;
        private readonly float diagonalScale = Mathf.Sqrt(2);

        private List<DungeonGeneration.Light> lights;

        public Vector3 RenderDungeon(Dungeon dungeon)
        {
            if (root != null)
            {
                Destroy(root.gameObject);
                root = null;
            }

            int width = dungeon.GetWidth();
            int height = dungeon.GetHeight();

            lights = dungeon.lights;

            // Wall vars
            IterateArea(0, 0, width - 1, height - 1, (int x, int y) =>
            {
                if (dungeon.GetTile(x, y) == TileType.Floor) return;
                if (dungeon.GetTile(x, y) == TileType.Void) return;

                // Top-Left
                if (dungeon.GetTile(x - 1, y) == TileType.Wall && dungeon.GetTile(x, y + 1) == TileType.Wall
                && dungeon.GetTile(x + 1, y) == TileType.Floor && dungeon.GetTile(x, y - 1) == TileType.Floor)
                {
                    CreateWall(diagonalScale, x + 1, y + 1, 135, "TL");
                    return;
                }

                // Top-Right
                if (dungeon.GetTile(x + 1, y) == TileType.Wall && dungeon.GetTile(x, y + 1) == TileType.Wall
                && dungeon.GetTile(x - 1, y) == TileType.Floor && dungeon.GetTile(x, y - 1) == TileType.Floor)
                {
                    CreateWall(diagonalScale, x + 1, y, -135, "TR");
                    return;
                }

                // Bottom-Left
                if (dungeon.GetTile(x - 1, y) == TileType.Wall && dungeon.GetTile(x, y - 1) == TileType.Wall
                && dungeon.GetTile(x + 1, y) == TileType.Floor && dungeon.GetTile(x, y + 1) == TileType.Floor)
                {
                    CreateWall(diagonalScale, x, y + 1, 45, "BL");
                    return;
                }

                // Bottom-Right
                if (dungeon.GetTile(x + 1, y) == TileType.Wall && dungeon.GetTile(x, y - 1) == TileType.Wall
                && dungeon.GetTile(x - 1, y) == TileType.Floor && dungeon.GetTile(x, y + 1) == TileType.Floor)
                {
                    CreateWall(diagonalScale, x, y, -45, "BR");
                    return;
                }

                // Right
                if (dungeon.GetTile(x + 1, y) == TileType.Floor)
                    CreateWall(1, x + 1, y + 1, 90, "R");

                // Left
                if (dungeon.GetTile(x - 1, y) == TileType.Floor)
                    CreateWall(1, x, y, 270, "L");

                // Top
                if (dungeon.GetTile(x, y + 1) == TileType.Floor)
                    CreateWall(1, x, y + 1, 0, "T");

                // Bottom
                if (dungeon.GetTile(x, y - 1) == TileType.Floor)
                    CreateWall(1, x + 1, y, 180, "B");
            });

            Transform floor = CreatePlane(Vector3.zero, Quaternion.Euler(0, 90, 180), width, height, floorMaterial, 120);
            floor.gameObject.AddComponent<BoxCollider>();
            floor.name = "Floor";

            Transform ceiling = CreatePlane(new Vector3(0, 3, 0), Quaternion.identity, width, height, ceilingMaterial, 120);

            return new Vector3(dungeon.spawn.x, 0, dungeon.spawn.y);
        }

        private void CreateWall(float scale, int x, int y, float angle, string suffix)
        {
            if (wallRoot == null)
            {
                wallRoot = new GameObject("Walls").transform;
                wallRoot.parent = root;
            }

            Transform wall = CreatePlane(new Vector3(x, 0, y), Quaternion.Euler(270, angle, 0), scale, 3, wallMaterial, 2);
            wall.parent = wallRoot;

#if UNITY_EDITOR
            SetWallName(wall, x, y, suffix);
#endif
        }

        private void SetWallName(Transform transform, int x, int y, string suffix)
        {
            if (suffix == "R" || suffix == "B")
                x--;

            if (suffix == "T" || suffix == "R")
                y--;

            transform.name = string.Concat("(", x, ", ", y, ") ", suffix);
        }

        // Mesh generation
        private struct MeshDescription
        {
            public string name;
            public Vector3[] vertices;
            public int[] triangles;
            public Vector2[] uvs;
            public Color32[] colors;
        }

        private int Square(int number)
        {
            return number * number;
        }

        private Transform CreatePlane(Vector3 position, Quaternion rotation, float width, float height, Material material, int subvision = 0)
        {
            if (root == null)
                root = new GameObject("Dungeon").transform;

            if (subvision > 254)
                subvision = 254;

            MeshDescription description = new MeshDescription();

            description.name = "Floor";
            description.vertices = new Vector3[Square(subvision + 2)];
            description.triangles = new int[2 * Square(subvision + 1) * 3];
            description.uvs = new Vector2[description.vertices.Length];
            description.colors = new Color32[description.vertices.Length];

            int vertexIterations = subvision + 1;
            Color currentColor;
            float distance;

            for (int x = 0, i = 0; x <= vertexIterations; x++)
            {
                for (int y = 0; y <= vertexIterations; y++, i++)
                {
                    description.vertices[i] = new Vector3(Mathf.Lerp(0, width, (float)x / vertexIterations) + position.x, position.y, Mathf.Lerp(0, height, (float)y / vertexIterations) + position.z);
                    description.vertices[i] = rotation * (description.vertices[i] - position) + position;
                    description.uvs[i] = new Vector2(x * (width / vertexIterations), y * (height / vertexIterations));

                    currentColor = Color.black;
                    foreach (DungeonGeneration.Light light in lights)
                    {
                        distance = (new Vector3(light.position.x, 1.5f, light.position.y) - description.vertices[i]).sqrMagnitude;

                        currentColor.r = Mathf.Max(Mathf.Sqrt((Mathf.Pow(currentColor.r, 2) + Mathf.Pow((1 / distance) * light.intensity, 2)) / 2) * (light.color.r / 255), currentColor.r);
                        currentColor.g = Mathf.Max(Mathf.Sqrt((Mathf.Pow(currentColor.g, 2) + Mathf.Pow((1 / distance) * light.intensity, 2)) / 2) * (light.color.g / 255), currentColor.g);
                        currentColor.b = Mathf.Max(Mathf.Sqrt((Mathf.Pow(currentColor.b, 2) + Mathf.Pow((1 / distance) * light.intensity, 2)) / 2) * (light.color.b / 255), currentColor.b);
                    }
                    description.colors[i] = currentColor;
                }
            }

            int triangleIterations = subvision + 1;

            for (int ti = 0, vi = 0, y = 0; y < triangleIterations; y++, vi++)
            {
                for (int x = 0; x < triangleIterations; x++, ti += 6, vi++)
                {
                    description.triangles[ti] = vi;
                    description.triangles[ti + 3] = description.triangles[ti + 2] = vi + 1;
                    description.triangles[ti + 4] = description.triangles[ti + 1] = vi + triangleIterations + 1;
                    description.triangles[ti + 5] = vi + triangleIterations + 2;
                }
            }

            return GenerateMesh(description, material, root);
        }

        private Transform GenerateMesh(MeshDescription description, Material material, Transform root)
        {
            Mesh mesh = new Mesh();
            GameObject go = new GameObject();
            go.transform.parent = root;
            mesh.name = go.name = description.name;

            go.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.material = material;

            mesh.vertices = description.vertices;
            mesh.triangles = description.triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.uv = description.uvs;
            mesh.colors32 = description.colors;

            return go.transform;
        }

        //private Vector3 GetClosestPointOnRectangle(Vector3 position, Rectangle rectangle, Vector3 point)
        //{
        //    rectangle
        //}
    }
}