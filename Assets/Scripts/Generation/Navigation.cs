using System;
using System.Collections.Generic;
using static Assets.Scripts.Generation.DungeonGeneration;

namespace Assets.Scripts.Generation
{
    public class Node
    {
        public Vertex position;
        public List<Node> neighbors;
        public Node parent;
        public float gCost;
        public float hCost;

        public float FCost
        {
            get { return gCost + hCost; }
        }

        public Node(float x, float y)
        {
            position = new Vertex(x, y);
        }

        public Node(Vertex position)
        {
            this.position = position;
        }
    }

    public class Navigation
    {
        private List<Node> nodes;

        public Navigation(List<Node> nodes)
        {
            this.nodes = nodes;
        }

        public Navigation(Node[,] nodes)
        {
            if (nodes == null) return;
            if (nodes.Length < 1) return;

            List<Node> nodeList = new();

            IterateArea(0, 0, nodes.GetLength(0) - 1, nodes.GetLength(1) - 1, (int x, int y) =>
            {
                nodeList.Add(nodes[x, y]);
            });

            this.nodes = nodeList;
        }

        public List<Vertex> FindPath(Vertex start, Vertex end)
        {
            // Check that nodes can be found
            Node startNode = FindNearestNode(start);
            if (startNode == null) return null;

            Node endNode = FindNearestNode(end);
            if (endNode == null) return null;

            // Create open list for A*
            List<Node> open = new();
            open.Add(startNode);

            HashSet<Node> closed = new();

            while (open.Count > 0)
            {
                Node current = open[0];

                Node closest;
                for (int i = 1; i < open.Count; i++)
                {
                    closest = open[i];
                    if (closest.FCost < current.FCost || Approximately(closest.FCost, current.FCost))
                    {
                        if (closest.hCost < current.hCost)
                            current = closest;
                    }
                }

                open.Remove(current);
                closed.Add(current);

                // Return if path has been found
                if (current == endNode)
                {
                    List<Vertex> path = new();
                    Node tmp = endNode;

                    while (tmp != startNode)
                    {
                        path.Add(tmp.position);
                        tmp = tmp.parent;
                    }

                    path.Reverse();

                    return path;
                }

                // Iterate through neighbors
                foreach (Node neighbor in current.neighbors)
                {
                    if (closed.Contains(neighbor)) continue;

                    float cost = current.gCost + Distance(current.position, neighbor.position);

                    bool openContainsNeighbor = open.Contains(neighbor);

                    // Adds each neighbor cheaper than current node to open set
                    if (cost < neighbor.gCost || !openContainsNeighbor)
                    {
                        neighbor.gCost = cost;
                        neighbor.hCost = Distance(neighbor.position, endNode.position);
                        neighbor.parent = current;

                        if (!openContainsNeighbor)
                            open.Add(neighbor);
                    }
                }
            }

            return new();
        }

        private Node FindNearestNode(Vertex vertex)
        {
            if (nodes == null) return null;
            if (nodes.Count < 1) return null;

            int closest = 0;
            float distance, closestDistance = Distance(vertex, nodes[0].position);
            for (int i = 1; i < nodes.Count; i++)
            {
                distance = Distance(vertex, nodes[i].position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = i;
                }
            }

            return nodes[closest];
        }
    }
}