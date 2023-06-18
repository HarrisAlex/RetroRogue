using UnityEngine;

namespace Assets.Scripts.Generation
{
    public struct MeshDescription
    {
        public string name;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
    }

    public class MeshGenerator
    {
        public static MeshDescription CreatePlaneDescription(GenerationSettings settings)
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

        public static Transform GenerateMesh(MeshDescription description, Material material)
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