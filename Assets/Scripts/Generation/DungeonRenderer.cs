using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Generation
{
    public class DungeonRenderer : MonoBehaviour
    {
        Transform root;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                DungeonGenerator dungeonGenerator = new DungeonGenerator();

                bool[,] grid = dungeonGenerator.Generate((int)System.DateTime.Now.Ticks);

                for (int x = 0; x < dungeonGenerator.gridWidth; x++)
                {
                    for (int y = 0; y < dungeonGenerator.gridHeight; y++)
                    {
                        if (grid[x, y])
                        {
                            CreateFloor(x, y);
                        }

                        if (dungeonGenerator.IsWall(x, y))
                        {
                            CreateWall(x, y);
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    Destroy(root.GetChild(i).gameObject);
                }
            }
        }

        void CreateWall(int x, int y)
        {
            if (root == null)
            {
                root = new GameObject("Dungeon").transform;
            }

            Transform go = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            go.position = new Vector3(x, 1, y);
            go.localScale = new Vector3(1, 2, 1);
            go.parent = root;
        }

        void CreateFloor(int x, int y)
        {
            if (root == null)
            {
                root = new GameObject("Dungeon").transform;
            }

            Transform go = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            go.position = new Vector3(x, 0, y);
            go.eulerAngles = new Vector3(90, 0, 0);
            go.parent = root;
        }
    }
}