using System.Collections.Generic;
using static Assets.Scripts.Generation.DungeonGeneration;

namespace Assets.Scripts.Generation
{
    public class NavigationTree
    {
        private readonly bool[,] grid;
        private readonly int width, height;

        public readonly List<Edge> edges;
        private readonly List<Vertex> vertices;

        private readonly List<Node> nodes;

        public NavigationTree(TileType[,] grid, List<Edge> edges)
        {
            this.edges = edges;
            nodes = new();

            vertices = new();
            foreach (Edge edge in edges)
            {
                if (!vertices.Contains(edge.u))
                    vertices.Add(edge.u);
                if (!vertices.Contains(edge.v))
                    vertices.Add(edge.v);
            }

            // Create a new node for each vertex
            foreach (Vertex vertex in vertices)
                nodes.Add(new(vertex.x, vertex.y));

            foreach (Node node in nodes)
                node.neighbors = FindNeigbors(node.position);

            width = grid.GetLength(0);
            height = grid.GetLength(1);

            this.grid = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    this.grid[x, y] = grid[x, y] != TileType.Void;
                }
            }
        }

        public List<Vertex> FindPath(Vertex start, Vertex end)
        {
            // Check variables are valid
            if (!IsValidPoint(start) || !IsValidPoint(end)) return null;
            if (!IsValidList(edges)) return null;
            if (!IsValidList(nodes)) return null;

            // Check that nodes can be found
            if (!TryFindNodeAtPoint(FindNearestVertex(start), out Node startNode)) return null;
            if (!TryFindNodeAtPoint(end, out Node endNode)) return null;

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

        /// <summary>
        /// Checks if a given position is on the grid.
        /// </summary>
        /// <param name="vertex">The position to be checked.</param>
        /// <returns>Returns whether or not the position is on the grid.</returns>
        private bool IsValidPoint(Vertex vertex)
        {
            if (vertex.x < 0 || vertex.x > width || vertex.y < 0 || vertex.y > height) return false;

            return grid[(int)vertex.x, (int)vertex.y];
        }

        private Vertex FindNearestVertex(Vertex vertex)
        {
            if (vertices == null) return Vertex.NegativeInfinity;
            if (vertices.Count < 1) return Vertex.NegativeInfinity;

            int closest = 0;
            float distance, closestDistance = Distance(vertex, vertices[0]);
            for (int i = 1; i < vertices.Count; i++)
            {
                distance = Distance(vertex, vertices[i]);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = i;
                }
            }

            return vertices[closest];
        }

        private bool IsValidList<T>(List<T> list)
        {
            if (list == null) return false;
            if (list.Count < 1) return false;

            return true;
        }

        /// <summary>
        /// Get list of Node.Connection structs that vertex is connected to.
        /// </summary>
        /// <param name="vertex">The vertex from which to find connections.</param>
        /// <returns>Returns a List of Node.Connection structs.</returns>
        private List<Node> FindNeigbors(Vertex vertex)
        {
            List<Node> neighbors = new();

            if (vertex == Vertex.NegativeInfinity) return neighbors;
            if (edges == null) return neighbors;

            // Get edges that include vertex and add other vertex as connection,
            // creating a new node for that other vertex if none exists
            Vertex tmpVertex;
            foreach (Edge edge in edges)
            {
                tmpVertex = Vertex.NegativeInfinity;

                if (edge.u == vertex)
                    tmpVertex = edge.v;
                else if (edge.v == vertex)
                    tmpVertex = edge.u;

                if (TryFindNodeAtPoint(tmpVertex, out Node node))
                {
                    neighbors.Add(node);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Attempts to retrieve the node at a given postion.
        /// </summary>
        /// <param name="vertex">The position to check.</param>
        /// <param name="result">The Node struct to be filled.</param>
        /// <returns>Returns whether a node exists at the position.</returns>
        private bool TryFindNodeAtPoint(Vertex vertex, out Node result)
        {
            result = null;

            if (vertex == Vertex.NegativeInfinity) return false;
            if (nodes == null) return false;

            foreach (Node node in nodes)
            {
                if (node.position == vertex)
                {
                    result = node;
                    return true;
                }
            }

            return false;
        }
    }
}