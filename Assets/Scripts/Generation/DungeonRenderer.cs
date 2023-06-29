using UnityEngine;

namespace Assets.Scripts.Generation
{
    public class DungeonRenderer : MonoBehaviour
    {
        public Material floorMaterial;
        public Material ceilingMaterial;
        public Material wallMaterial;

        private static Transform root;

        public Vector3 RenderDungeon(Dungeon dungeon)
        {
            if (root != null)
            {
                Destroy(root.gameObject);
                root = null;
            }

            int width = dungeon.GetWidth();
            int height = dungeon.GetHeight();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    switch (dungeon.GetTile(x, y))
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

            Transform floor = CreatePlane(width, height, floorMaterial, 1);
            floor.localScale = new Vector3(1, -1, 1);
            floor.gameObject.AddComponent<BoxCollider>();
            floor.parent = root;

            Transform ceiling = CreatePlane(width, height, ceilingMaterial, 1);
            ceiling.position = new Vector3(0, 3, 0);
            ceiling.parent = root;

            return new Vector3(dungeon.spawn.x, 0, dungeon.spawn.y);
        }

        private void CreateWall(float x, float y, float angle)
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

        private void CreateCorner(int x, int y, TileType cornerType)
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

        private Light CreateLight(int x, int y)
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

        // Mesh generation
        private struct MeshDescription
        {
            public string name;
            public Vector3[] vertices;
            public int[] triangles;
            public Vector2[] uvs;
        }

        private int Square(int number)
        {
            return number * number;
        }

        private Transform CreatePlane(int width, int height, Material material, int subvision = 0)
        {
            MeshDescription description = new MeshDescription();

            description.name = "Floor";
            description.vertices = new Vector3[Square(subvision + 2)];
            description.triangles = new int[2 * Square(subvision + 1) * 3];
            description.uvs = new Vector2[description.vertices.Length];

            int vertexIterations = (description.vertices.Length / 2) - 1;

            for (int x = 0, i = 0; x < vertexIterations; x++)
            {
                for (int y = 0; y < vertexIterations; y++, i++)
                {
                    description.vertices[i] = new Vector3(Mathf.Lerp(0, width, x), 0, Mathf.Lerp(0, height, y));
                    description.uvs[i] = new Vector2(x, y);
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

            return GenerateMesh(description, material);
        }

        private Transform GenerateMesh(MeshDescription description, Material material)
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
    }
}