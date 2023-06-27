using Assets.Scripts.Generation;
using System.Collections.Generic;
using static Assets.Scripts.Generation.Geometry;

public class NavigationTree
{
    private readonly bool[,] grid;
    private readonly int width, height;

    private readonly List<Room> Rooms;
    public readonly List<Edge> Edges;

    public NavigationTree(TileType[,] grid, List<Room> rooms, List<Edge> edges)
    {
        Rooms = rooms;
        Edges = edges;

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

    public Vertex[] FindPath(Vertex start, Vertex end)
    {
        if (!ValidPoint(start) || !ValidPoint(end)) return null;
        if (Edges == null) return null;
        if (Edges.Count < 1) return null;

        // Get unique vertices from edges
        List<Vertex> vertices = new();
        foreach (Edge edge in Edges)
        {
            if (!vertices.Contains(edge.u))
                vertices.Add(edge.u);

            if (!vertices.Contains(edge.v))
                vertices.Add(edge.v);
        }

        // Get closest node
        int closestIndex = 0;
        float distance, closestDistance = Distance(start, vertices[0]);

        for (int i = 1; i < vertices.Count; i++)
        {
            distance = Distance(start, vertices[i]);

            if (distance >= closestDistance) continue;

            closestIndex = i;
            closestDistance = distance;
        }

        Edge startingEdge = null;

        // Get an edge attached to closest node
        for (int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].u == vertices[closestIndex] || Edges[i].v == vertices[closestIndex])
            {
                startingEdge = Edges[i];
                break;
            }
        }

        if (startingEdge == null) return null;


    }

    private bool ValidPoint(Vertex vertex)
    {
        if (vertex.x < 0 || vertex.x > width || vertex.y < 0 || vertex.y > height) return false;

        return grid[(int)vertex.x, (int)vertex.y];
    }
}