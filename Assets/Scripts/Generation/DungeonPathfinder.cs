using System;
using System.Collections.Generic;

namespace Assets.Scripts.Generation
{
    public class DungeonPathfinder
    {
        class Node
        {
            public Vertex position;
            public Node parent;
            public float gCost;
            public float hCost;
            public bool traversable;

            public float fCost
            {
                get { return gCost + hCost; }
            }

            public Node(Vertex position, bool traversable)
            {
                this.position = position;
                this.traversable = traversable;
            }
        }

        private Node[,] grid;
        private int gridWidth;
        private int gridHeight;

        private HashSet<Node> closed;
        private List<Node> open;

        public DungeonPathfinder(bool[,] tiles, int gridWidth, int gridHeight)
        {
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;

            grid = new Node[gridWidth, gridHeight];

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y] = new Node(new Vertex(x, y), true);
                }
            }
        }

        public List<Vertex> FindPath(Vertex start, Vertex end)
        {
            Node startNode = grid[(int)MathF.Round(start.x), (int)MathF.Round(start.y)];
            Node endNode = grid[(int)MathF.Round(end.x), (int)MathF.Round(end.y)];

            open = new List<Node>();
            closed = new HashSet<Node>();

            open.Add(startNode);

            while (open.Count > 0)
            {
                Node node = open[0];

                for (int i = 1; i < open.Count; i++)
                {
                    if (open[i].fCost < node.fCost || Math.Approximately(open[i].fCost, node.fCost))
                    {
                        if (open[i].hCost < node.hCost)
                        {
                            node = open[i];
                        }
                    }
                }

                open.Remove(node);
                closed.Add(node);

                if (node == endNode)
                {
                    return RetracePath(startNode, endNode);
                }

                foreach (Node neighbor in GetNeighborNodes(node))
                {
                    if (!neighbor.traversable || closed.Contains(neighbor))
                    {
                        continue;
                    }

                    float cost = node.gCost + Math.Distance(node.position, neighbor.position);

                    if (cost < neighbor.gCost || !open.Contains(neighbor))
                    {
                        neighbor.gCost = cost;
                        neighbor.hCost = Math.Distance(neighbor.position, endNode.position);
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

        private List<Vertex> RetracePath(Node startNode, Node endNode)
        {
            List<Vertex> path = new List<Vertex>();
            Node current = endNode;

            while (current != startNode)
            {
                path.Add(current.parent.position);
                current = current.parent;
            }

            path.Reverse();

            return path;
        }

        private List<Node> GetNeighborNodes(Node node)
        {
            List<Node> neighbors = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    int checkX = (int)MathF.Round(node.position.x) + x;
                    int checkY = (int)MathF.Round(node.position.y) + y;

                    if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                    {
                        neighbors.Add(grid[checkX, checkY]);
                    }
                }
            }

            return neighbors;
        }
    }
}