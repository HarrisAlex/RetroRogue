using System.Collections.Generic;
using static Assets.Scripts.Generation.Geometry;

namespace Assets.Scripts.Generation
{
    public class NavigationTree
    {
        private readonly bool[,] grid;
        private readonly int width, height;

        public readonly List<Edge> edges;
        private readonly List<Vertex> vertices;

        private readonly List<Node> nodes;

        private class Node
        {
            public struct Connection
            {
                public Node node;
                public float distance;
            }

            public readonly Vertex position;
            public readonly List<Connection> connections;

            public float distance;
            public Node previous;

            public Node(Vertex position, List<Connection> connections)
            {
                this.position = position;
                this.connections = connections;

                distance = float.MaxValue;
                previous = null;
            }

            /// <summary>
            /// Sets distance to node if less than current shortest distance.
            /// </summary>
            /// <param name="distance">The distance to be set.</param>
            /// <returns>Returns whether distance was shorter than original distance.</returns>
            public bool SetDistance(float distance, Node previous)
            {
                if (this.distance > distance)
                {
                    this.distance = distance;
                    this.previous = previous;

                    return true;
                }

                return false;
            }

            public void ResetDistance()
            {
                distance = float.MaxValue;
            }
        }

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
            {
                nodes.Add(new(vertex, FindConnections(vertex)));
            }

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

            // Create open list for Dijkstra's and set distances to infinity
            List<Node> open = new(nodes);
            nodes.ForEach((Node n) => n.ResetDistance());

            // Initialize start node and current
            startNode.distance = 0;
            Node current = startNode;

            // Iterate through all nodes
            while (open.Count > 0)
            {
                // Set lowest distance for each connecting node
                current.connections.ForEach((Node.Connection c) =>
                    c.node.SetDistance(Distance(current.position, c.node.position), current));

                open.Remove(current);

                if (open.Count > 0)
                    current = open[0];
            }

            // Build path
            List<Node> nodePath = new();
            current = endNode;

            while (current != null)
            {
                nodePath.Add(current);
                current = current.previous;
            }

            nodePath.Reverse();

            // Convert path to List<Vertex>
            List<Vertex> path = new();
            nodePath.ForEach((Node n) => path.Add(n.position));

            return path;
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
        private List<Node.Connection> FindConnections(Vertex vertex)
        {
            if (vertex == Vertex.NegativeInfinity) return null;
            if (edges == null) return null;

            List<Node.Connection> connections = new();

            // Get edges that include vertex and add other vertex as connection,
            // creating a new node for that other vertex if none exists
            Node.Connection connection;
            foreach (Edge edge in edges)
            {
                if (edge.u == vertex)
                {
                    connection = new();
                    connection.distance = Distance(vertex, edge.v);

                    if (TryFindNodeAtPoint(edge.v, out Node node))
                        connection.node = node;
                    else
                        connection.node = new Node(edge.v, new());

                    connections.Add(connection);
                }
                else if (edge.v == vertex)
                {
                    connection = new();
                    connection.distance = Distance(vertex, edge.u);

                    if (TryFindNodeAtPoint(edge.u, out Node node))
                        connection.node = node;
                    else
                        connection.node = new Node(edge.u, new());

                    connections.Add(connection);
                }
            }

            return connections;
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