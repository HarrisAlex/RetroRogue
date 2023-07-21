using System;
using System.Collections.Generic;
using static Assets.Scripts.Generation.DungeonGeneration;

public class Node
{
    public Coordinate position;
    public Node parent;
    public float gCost;
    public float hCost;

    public float FCost
    {
        get { return gCost + hCost; }
    }

    public Node(int x, int y)
    {
        position = new Coordinate(x, y);
    }
}

public class Navigation
{
    private Node[,] nodes;

    public Navigation(Node[,] nodes)
    {
        this.nodes = nodes;
    }

    public List<Vertex> FindPath(Vertex start, Vertex end)
    {
        // Check that nodes can be found
        Node startNode = FindNearestNode(start);
        if (startNode == null) return new();

        Node endNode = FindNearestNode(end);
        if (endNode == null) return new();

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
                    path.Add(new(tmp.position.x + 0.5f, tmp.position.y + 0.5f));
                    tmp = tmp.parent;
                }

                path.Reverse();

                return path;
            }

            int checkX, checkY;
            IterateArea(-1, -1, 1, 1, (int x, int y) =>
            {
                checkX = current.position.x + x;
                checkY = current.position.y + y;

                if (checkX < 0 || checkY < 0 || checkX >= nodes.GetLength(0) || checkY >= nodes.GetLength(1)) return;
                if (x == 0 && y == 0) return;
                if (nodes[checkX, checkY] == null) return;

                Node neighbor = nodes[checkX, checkY];

                if (closed.Contains(neighbor)) return;

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
            });
        }

        return new();
    }

    private Node FindNearestNode(Vertex vertex)
    {
        if (nodes == null) return null;
        if (nodes.Length < 1) return null;

        if (vertex.x < 0 || vertex.y < 0 || vertex.x >= nodes.GetLength(0) || vertex.y >= nodes.GetLength(1)) return null;

        return nodes[(int)vertex.x, (int)vertex.y];
    }
}