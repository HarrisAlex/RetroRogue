using Assets.Scripts.Generation;
using System;
using System.Collections.Generic;
using Random = System.Random;

public class DungeonGenerator
{
    private Random random;

    // Bool = isRoom
    private bool[,] grid;
    public List<Room> rooms;
    private List<Edge> edges;
    private HashSet<MeasuredEdge> selectedEdges;

    public int roomCount = 20;
    public int gridWidth = 128;
    public int gridHeight = 128;
    public int maxRoomWidth = 25;
    public int maxRoomHeight = 25;
    public int minRoomWidth = 7;
    public int minRoomHeight = 7;
    public int maxRoomAttempts = 5;
    public float extraHallwayGenerationChance = 0;

    public bool[,] Generate(int seed)
    {
        random = new Random(seed);

        grid = new bool[gridWidth, gridHeight];
        rooms = new List<Room>();

        InitializeGrid();
        CreateRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();

        return grid;
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = false;
            }
        }
    }

    private void CreateRooms()
    {
        int attempts;
        bool canAddRoom;

        for (int i = 0; i < roomCount; i++)
        {
            Room newRoom = new Room();

            canAddRoom = false;
            attempts = 0;

            // Allows for multiple iterations in case of intersections or reaching outside grid
            while (!canAddRoom && attempts < maxRoomAttempts)
            {
                canAddRoom = true;

                newRoom = new Room(
                    random.Next(minRoomWidth, gridWidth - maxRoomWidth),
                    random.Next(minRoomHeight, gridHeight - maxRoomHeight),
                    random.Next(minRoomWidth, maxRoomWidth + 1),
                    random.Next(minRoomHeight, maxRoomHeight + 1)
                    );

                // Check if room is within grid
                if (!newRoom.WithinGrid(gridWidth, gridHeight))
                {
                    canAddRoom = false;
                    attempts++;

                    continue;
                }

                // Check for intersection with other rooms
                foreach (Room room in rooms)
                {
                    if (Room.RoomsIntersect(room, newRoom, 1))
                    {
                        canAddRoom = false;
                        attempts++;

                        break;
                    }
                }
            }

            if (canAddRoom)
            {
                rooms.Add(newRoom);

                // Mark grid squares that new room takes up as being Room
                for (int x = newRoom.xPosition; x < newRoom.xPosition + newRoom.width; x++)
                {
                    for (int y = newRoom.yPosition; y < newRoom.yPosition + newRoom.height; y++)
                    {
                        grid[x, y] = true;
                    }
                }
            }
        }
    }

    private void Triangulate()
    {
        List<Vertex> vertices = new List<Vertex>();

        // Create midpoint for center of each room
        foreach (Room room in rooms)
        {
            vertices.Add(room.GetCenter());
        }

        edges = DelaunayTriangulation.Triangulate(vertices);
    }

    private void CreateHallways()
    {
        List<MeasuredEdge> tree = new List<MeasuredEdge>();

        foreach (Edge edge in edges)
        {
            tree.Add(new MeasuredEdge(edge.u, edge.v));
        }

        tree = MinimumSpanningTree.GetTree(tree, tree[0].u);

        selectedEdges = new HashSet<MeasuredEdge>(tree);
        HashSet<Edge> remainingEdges = new HashSet<Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (Edge edge in remainingEdges)
        {
            if (random.Next(0, 100) < extraHallwayGenerationChance)
            {
                selectedEdges.Add(new MeasuredEdge(edge));
            }
        }
    }

    private void PathfindHallways()
    {
        DungeonPathfinder pathfinder = new DungeonPathfinder();
        pathfinder.InitializeGrid(grid, gridWidth, gridHeight);

        List<Vertex> currentHallway;

        foreach (MeasuredEdge edge in selectedEdges)
        {
            currentHallway = pathfinder.FindPath(edge.u, edge.v);

            if (currentHallway != null)
            {
                int xIndex;
                int yIndex;
                foreach (Vertex vertex in currentHallway)
                {
                    xIndex = (int)MathF.Round(vertex.x);
                    yIndex = (int)MathF.Round(vertex.y);

                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
                        {
                            SetTile(xIndex + x, yIndex + y, true);
                        }
                    }
                }
            }
        }
    }

    private void SetTile(int x, int y, bool isRoom)
    {
        if (x < 0 || y < 0 || x > gridWidth || y > gridHeight)
        {
            return;
        }

        grid[x, y] = isRoom;
    }

    public bool IsWall(int xIndex, int yIndex)
    {
        if (!grid[xIndex, yIndex])
        {
            return false;
        }

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (!grid[xIndex + x, yIndex + y])
                {
                    return true;
                }
            }
        }

        return false;
    }

    public int GetRotation(int xIndex, int yIndex)
    {
        // Left
        if (xIndex - 1 >= 0)
        {
            if (!grid[xIndex - 1, yIndex])
            {
                return 270;
            }
        }

        // Right
        if (xIndex + 1 < gridWidth)
        {
            if (!grid[xIndex + 1, yIndex])
            {
                return 90;
            }
        }

        // Bottom
        if (yIndex - 1 >= 0)
        {
            if (!grid[xIndex, yIndex - 1])
            {
                return 180;
            }
        }

        // Top
        if (yIndex + 1 < gridHeight)
        {
            if (!grid[xIndex, yIndex + 1])
            {
                return 0;
            }
        }

        return 0;
    }

    public int[] GetCornerRotation(int xIndex, int yIndex)
    {
        int[] rotations = new int[] { 0, 0 };
        int checkX, checkY;

        // Top-left
        checkX = xIndex - 1;
        checkY = yIndex + 1;

        if (checkX >= 0 && checkY < gridHeight)
        {
            if (grid[checkX, checkY])
            {
                rotations = new int[] { 90, 180 };
            }
        }

        // Top-right
        checkX = xIndex + 1;
        checkY = yIndex + 1;

        if (checkX < gridWidth && checkY < gridHeight)
        {
            if (grid[checkX, checkY])
            {
                rotations = new int[] { 180, 270 };
            }
        }

        // Bottom-left
        checkX = xIndex - 1;
        checkY = yIndex - 1;

        if (checkX >= 0 && checkY >= 0)
        {
            if (grid[checkX, checkY])
            {
                rotations = new int[] { 90, 0 };
            }
        }

        // Bottom-right
        checkX = xIndex + 1;
        checkY = yIndex - 1;

        if (checkX < gridWidth && checkY >= 0)
        {
            if (grid[checkX, checkY])
            {
                rotations = new int[] { 0, 270 };
            }
        }

        return rotations;
    }
}

public class Room
{
    public int xPosition;
    public int yPosition;

    public int width;
    public int height;

    public Room()
    {

    }

    public Room(int xPosition, int yPosition, int width, int height)
    {
        this.xPosition = xPosition;
        this.yPosition = yPosition;

        this.width = width;
        this.height = height;
    }

    public Vertex GetCenter()
    {
        return new Vertex(xPosition + (width / 2), yPosition + (height / 2));
    }

    public List<Vertex> GetCorners()
    {
        List<Vertex> vertices = new List<Vertex>();

        vertices.Add(new Vertex(xPosition, yPosition));
        vertices.Add(new Vertex(xPosition, yPosition + height));
        vertices.Add(new Vertex(xPosition + width, yPosition + height));
        vertices.Add(new Vertex(xPosition + width, yPosition));

        return vertices;
    }

    public bool WithinGrid(int gridWidth, int gridHeight)
    {
        return xPosition + width < gridWidth || yPosition + height < gridHeight;
    }

    public static bool RoomsIntersect(Room room1, Room room2, int spacing)
    {
        return !((room1.xPosition - spacing >= (room2.xPosition + room2.width + spacing))
            || ((room1.xPosition + room1.width + spacing) <= room2.xPosition - spacing)
            || (room1.yPosition - spacing >= (room2.yPosition + room2.height + spacing))
            || ((room1.yPosition + room1.height + spacing) <= room2.yPosition - spacing));
    }
}