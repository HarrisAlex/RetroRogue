using System;
using System.Collections.Generic;
using static Assets.Scripts.Generation.Geometry;

namespace Assets.Scripts.Generation
{
    public class DungeonPathfinder
    {
        class Node
        {
            public Coordinate position;
            public Node parent;
            public float gCost;
            public float hCost;
            public bool traversable;

            public float fCost
            {
                get { return gCost + hCost; }
            }

            public Node(Coordinate position, bool traversable)
            {
                this.position = position;
                this.traversable = traversable;
            }
        }

        class Grid
        {
            public Node[,] nodes;
            private int width;
            private int height;

            public Grid(int width, int height)
            {
                nodes = new Node[width, height];

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        nodes[x, y] = new Node(new Coordinate(x, y), true);
                    }
                }

                this.width = width;
                this.height = height;
            }

            public Node GetNode(Vertex vertex)
            {
                return GetNode(vertex.x, vertex.y);
            }

            public Node GetNode(float x, float y)
            {
                int xCoord = FloatToInt(x);
                int yCoord = FloatToInt(y);

                if (!IsValidPosition(x, y)) return null;

                return nodes[xCoord, yCoord];
            }

            public List<Node> GetNeighbors(Node node)
            {
                List<Node> result = new();

                int checkX, checkY;
                for (int xi = -1; xi <= 1; xi++)
                {
                    for (int yi = -1; yi <= 1; yi++)
                    {
                        if (xi == 0 && yi == 0)
                        {
                            continue;
                        }

                        checkX = node.position.x + xi;
                        checkY = node.position.y + yi;

                        if (IsValidPosition(checkX, checkY))
                            result.Add(GetNode(checkX, checkY));
                    }
                }

                return result;
            }

            private int FloatToInt(float number)
            {
                return (int)MathF.Round(number);
            }

            private bool IsValidPosition(float x, float y)
            {
                if (x > width || x < 0 || y > height || y < 0)
                    return false;

                return true;
            }

            private bool IsValidPosition(int x, int y)
            {
                if (x > width || x < 0 || y > height || y < 0)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Finds the shortest path, if any, between two positions enclsoed within a grid.
        /// </summary>
        /// <param name="gridWidth">Width of the grid.</param>
        /// <param name="gridHeight">Height of the grid.</param>
        /// <param name="start">The starting position to calculate from.</param>
        /// <param name="end">The desintation of the path.</param>
        /// <returns></returns>
        public static List<Coordinate> FindPath(int gridWidth, int gridHeight, Vertex start, Vertex end)
        {
            Grid grid = new Grid(gridWidth, gridHeight);

            Node startNode = grid.GetNode(start);
            Node endNode = grid.GetNode(end);

            List<Node> open = new();
            open.Add(startNode);

            HashSet<Node> closed = new();

            // Iterate while all nodes have not been checked
            while (open.Count > 0)
            {
                Node node = open[0];

                // Get cheapest next node
                for (int i = 1; i < open.Count; i++)
                {
                    if (open[i].fCost < node.fCost || Approximately(open[i].fCost, node.fCost))
                    {
                        if (open[i].hCost < node.hCost)
                        {
                            node = open[i];
                        }
                    }
                }

                open.Remove(node);
                closed.Add(node);

                // Path has been found
                if (node == endNode)
                {
                    return RetracePath(startNode, endNode);
                }

                // Iterate through neighbors
                foreach (Node neighbor in grid.GetNeighbors(node))
                {
                    if (!neighbor.traversable || closed.Contains(neighbor))
                    {
                        continue;
                    }

                    float cost = node.gCost + Distance(node.position, neighbor.position);

                    // Adds each neighbor cheaper than current node to open set
                    if (cost < neighbor.gCost || !open.Contains(neighbor))
                    {
                        neighbor.gCost = cost;
                        neighbor.hCost = Distance(neighbor.position, endNode.position);
                        neighbor.parent = node;

                        if (!open.Contains(neighbor))
                        {
                            open.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        // Trace path from start to finish through parents
        private static List<Coordinate> RetracePath(Node startNode, Node endNode)
        {
            List<Coordinate> path = new List<Coordinate>();
            Node current = endNode;

            while (current != startNode)
            {
                path.Add(current.parent.position);
                current = current.parent;
            }

            path.Reverse();

            return path;
        }
    }
}