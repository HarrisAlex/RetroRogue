using System.Collections.Generic;
using static Assets.Scripts.Generation.DungeonGeneration;

public class Node<T>
{
    public T position;
    public List<Node<T>> neighbors;
    public Node<T> parent;
    public float gCost;
    public float hCost;

    public float FCost
    {
        get { return gCost + hCost; }
    }

    public Node(T position)
    {
        this.position = position;
        neighbors = new();
    }
}

public class Pathfinding<T>
{
    private List<Node<T>> nodes;

    public Pathfinding(List<Node<T>> nodes)
    {
        this.nodes = nodes;
    }

    public List<Node<T>> FindPath(T start, T end)
    {
        if (typeof(T) != typeof(Vertex) && typeof(T) != typeof(Coordinate)) return new();

        // Check that nodes can be found
        Node<T> startNode = FindNearestNode(start);
        if (startNode == null) return new();

        Node<T> endNode = FindNearestNode(end);
        if (endNode == null) return new();

        // Create open list for A*
        List<Node<T>> open = new();
        open.Add(startNode);

        HashSet<Node<T>> closed = new();

        while (open.Count > 0)
        {
            Node<T> current = open[0];

            Node<T> closest;
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
                List<Node<T>> path = new();
                Node<T> tmp = endNode;

                while (tmp != startNode)
                {
                    path.Add(tmp);
                    tmp = tmp.parent;
                }

                path.Reverse();

                return path;
            }

            foreach (Node<T> neighbor in current.neighbors)
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

    private Node<T> FindNearestNode(T position)
    {
        if (nodes == null) return null;
        if (nodes.Count < 1) return null;

        int closest = -1;
        float distance, closestDistance = float.MaxValue;
        for (int i = 0; i < nodes.Count; i++)
        {
            distance = Distance(position, nodes[i].position);

            if (distance < 0)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = i;
            }
        }

        if (closest < 0)
            return null;

        return nodes[closest];
    }
}