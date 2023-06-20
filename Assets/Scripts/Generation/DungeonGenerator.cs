using System;
using System.Collections.Generic;
using static Assets.Scripts.Generation.Geometry;
using Random = System.Random;

namespace Assets.Scripts.Generation
{
    public enum TileType
    {
        Void = 0,
        RoomFloor = 1,
        HallwayFloor = 2,
        Wall = 4,
        TopLeftCorner = 8,
        TopRightCorner = 16,
        BottomLeftCorner = 32,
        BottomRightCorner = 64,
        Floor = RoomFloor & HallwayFloor
    }

    class Room
    {
        public int xPosition;
        public int yPosition;

        public int width;
        public int height;

        public List<Vertex> lights;

        public Room() { }

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
            List<Vertex> vertices = new();

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

        public bool ContainsPoint(Vertex vertex)
        {
            return xPosition < vertex.x && (xPosition + width) > vertex.x
                && yPosition < vertex.y && (yPosition + height) > vertex.y;
        }
    }

    public class DungeonGenerator
    {
        private Random random;

        private TileType[,] grid;
        private List<Room> Rooms { get; set; }
        private List<Edge> edges;
        private HashSet<Edge> selectedEdges;

        public GenerationSettings Settings { get; private set; }

        public DungeonGenerator(GenerationSettings settings)
        {
            this.Settings = settings;
        }

        public TileType[,] Generate(int seed)
        {
            random = new Random(seed);

            grid = new TileType[Settings.gridWidth, Settings.gridHeight];
            Rooms = new List<Room>();

            // Initialize grid
            for (int x = 0; x < Settings.gridWidth; x++)
            {
                for (int y = 0; y < Settings.gridHeight; y++)
                {
                    grid[x, y] = TileType.Void;
                }
            }

            CreateRooms();
            Triangulate();
            CreateHallways();
            PathfindHallways();
            GenerateWalls();
            FindInsetCorners();
            AddLights();

            return grid;
        }

        private void CreateRooms()
        {
            int attempts;
            bool canAddRoom;

            for (int i = 0; i < Settings.roomCount; i++)
            {
                Room newRoom = new();

                canAddRoom = false;
                attempts = 0;

                // Allows for multiple iterations in case of intersections or reaching outside grid
                while (!canAddRoom && attempts < Settings.maxRoomAttempts)
                {
                    canAddRoom = true;

                    newRoom = new Room(
                        random.Next(Settings.minRoomWidth, Settings.gridWidth - Settings.maxRoomWidth),
                        random.Next(Settings.minRoomHeight, Settings.gridHeight - Settings.maxRoomHeight),
                        random.Next(Settings.minRoomWidth, Settings.maxRoomWidth + 1),
                        random.Next(Settings.minRoomHeight, Settings.maxRoomHeight + 1)
                        );

                    // Check if room is within grid
                    if (!newRoom.WithinGrid(Settings.gridWidth, Settings.gridHeight))
                    {
                        canAddRoom = false;
                        attempts++;

                        continue;
                    }

                    // Check for intersection with other rooms
                    foreach (Room room in Rooms)
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
                    Rooms.Add(newRoom);

                    // Mark grid squares that new room takes up
                    for (int x = newRoom.xPosition; x <= newRoom.xPosition + newRoom.width; x++)
                    {
                        for (int y = newRoom.yPosition; y <= newRoom.yPosition + newRoom.height; y++)
                        {
                            grid[x, y] = TileType.RoomFloor;
                        }
                    }
                }
            }
        }

        private void Triangulate()
        {
            List<Vertex> vertices = new();

            // Create midpoint for center of each room
            foreach (Room room in Rooms)
            {
                vertices.Add(room.GetCenter());
            }

            List<Triangle> triangles = new();
            edges = new();

            float minX = vertices[0].x;
            float minY = vertices[0].y;
            float maxX = minX;
            float maxY = minY;

            // Get bounding box for vertices
            foreach (Vertex vertex in vertices)
            {
                if (vertex.x < minX)
                {
                    minX = vertex.x;
                }

                if (vertex.y < minY)
                {
                    minY = vertex.y;
                }

                if (vertex.x > maxX)
                {
                    maxX = vertex.x;
                }

                if (vertex.y > maxY)
                {
                    maxY = vertex.y;
                }
            }

            float deltaX = maxX - minX;
            float deltaY = maxY - minY;
            float deltaMax = MathF.Max(deltaX, deltaY);

            // Create supra-triangle
            Vertex v1 = new(minX - 1, minY - 1);
            Vertex v2 = new(minX - 1, maxY + deltaMax);
            Vertex v3 = new(maxX + deltaMax, minY - 1);

            triangles.Add(new Triangle(v1, v2, v3));

            foreach (Vertex vertex in vertices)
            {
                List<Edge> polygon = new();


                foreach (Triangle triangle in triangles)
                {
                    if (triangle.CircumcircleContains(vertex))
                    {
                        triangle.isBad = true;

                        polygon.Add(new Edge(triangle.a, triangle.b));
                        polygon.Add(new Edge(triangle.b, triangle.c));
                        polygon.Add(new Edge(triangle.c, triangle.a));
                    }
                }

                triangles.RemoveAll((Triangle triangle) => triangle.isBad);

                for (int i = 0; i < polygon.Count; i++)
                {
                    for (int j = i + 1; j < polygon.Count; j++)
                    {
                        if (Edge.AlmostEqual(polygon[i], polygon[j]))
                        {
                            polygon[i].isBad = true;
                            polygon[j].isBad = true;
                        }
                    }
                }

                polygon.RemoveAll((Edge edge) => edge.isBad);

                foreach (Edge edge in polygon)
                {
                    triangles.Add(new Triangle(edge.u, edge.v, vertex));
                }
            }

            triangles.RemoveAll((Triangle triangle) => triangle.ContainsVertex(v1)
                || triangle.ContainsVertex(v2)
                || triangle.ContainsVertex(v3));

            HashSet<Edge> edgeSet = new();

            foreach (Triangle triangle in triangles)
            {
                Edge ab = new(triangle.a, triangle.b);
                Edge bc = new(triangle.b, triangle.c);
                Edge ca = new(triangle.c, triangle.a);

                if (edgeSet.Add(ab))
                {
                    edges.Add(ab);
                }

                if (edgeSet.Add(bc))
                {
                    edges.Add(bc);
                }

                if (edgeSet.Add(ca))
                {
                    edges.Add(ca);
                }
            }
        }

        private void CreateHallways()
        {
            // Get minimum spanning tree
            selectedEdges = new();

            HashSet<Vertex> openSet = new();
            HashSet<Vertex> closedSet = new();

            foreach (Edge edge in edges)
            {
                openSet.Add(edge.u);
                openSet.Add(edge.v);
            }

            closedSet.Add(edges[0].u);

            while (openSet.Count > 0)
            {
                bool chosen = false;
                Edge chosenEdge = null;
                float minWeight = float.PositiveInfinity;

                foreach (Edge edge in edges)
                {
                    int closedVertices = 0;

                    if (!closedSet.Contains(edge.u))
                        closedVertices++;

                    if (!closedSet.Contains(edge.v))
                        closedVertices++;

                    if (closedVertices != 1)
                        continue;

                    if (edge.Length < minWeight)
                    {
                        chosenEdge = edge;
                        chosen = true;
                        minWeight = edge.Length;
                    }
                }

                if (!chosen)
                    break;

                selectedEdges.Add(chosenEdge);
                openSet.Remove(chosenEdge.u);
                openSet.Remove(chosenEdge.v);
                closedSet.Add(chosenEdge.u);
                closedSet.Add(chosenEdge.v);
            }

            // Find all hallways not included in minimum spanning tree (all that are unecessary)
            HashSet<Edge> remainingEdges = new(edges);
            remainingEdges.ExceptWith(selectedEdges);

            // Randomly add unecessary hallways based on chance
            foreach (Edge edge in remainingEdges)
            {
                if (random.Next(0, 100) < Settings.extraHallwayGenerationChance)
                {
                    selectedEdges.Add(edge);
                }
            }
        }

        private void PathfindHallways()
        {
            List<Coordinate> currentHallway;

            foreach (Edge edge in selectedEdges)
            {
                currentHallway = DungeonPathfinder.FindPath(Settings.gridWidth, Settings.gridHeight, edge.u, edge.v);

                if (currentHallway != null)
                {
                    int xIndex;
                    int yIndex;
                    foreach (Coordinate coordinate in currentHallway)
                    {
                        for (int x = -Settings.hallwayExpansion; x <= Settings.hallwayExpansion; x++)
                        {
                            for (int y = -Settings.hallwayExpansion; y <= Settings.hallwayExpansion; y++)
                            {
                                xIndex = coordinate.x + x;
                                yIndex = coordinate.y + y;

                                if (!WithinGrid(xIndex, yIndex)) continue;
                                if (grid[xIndex, yIndex] != TileType.Void) continue;

                                SetTile(xIndex, yIndex, TileType.HallwayFloor);
                            }
                        }
                    }
                }
            }
        }

        private void GenerateWalls()
        {
            for (int x = 0; x < Settings.gridWidth; x++)
            {
                for (int y = 0; y < Settings.gridHeight; y++)
                {
                    if (grid[x, y] == TileType.Void)
                    {
                        if (IsWall(x, y))
                            SetTile(x, y, TileType.Wall);
                    }
                }
            }
        }

        private void FindInsetCorners()
        {
            for (int x = 0; x < Settings.gridWidth; x++)
            {
                for (int y = 0; y < Settings.gridHeight; y++)
                {
                    if (grid[x, y] != TileType.HallwayFloor) continue;

                    if (TryGetCornerType(x, y, out TileType type))
                    {
                        SetTile(x, y, type);
                    }
                }
            }
        }

        private void AddLights()
        {
            foreach (Room room in Rooms)
            {
                room.lights = new List<Vertex>();

                if (random.Next(0, 1) == 0)
                {
                    room.lights.Add(room.GetCenter());
                }
            }
        }

        private void SetTile(int x, int y, TileType tileType)
        {
            if (!WithinGrid(x, y)) return;

            grid[x, y] = tileType;
        }

        private bool IsWall(int x, int y)
        {
            if (!WithinGrid(x, y)) return false;

            for (int xi = -1; xi <= 1; xi++)
            {
                for (int yi = -1; yi <= 1; yi++)
                {
                    if (!WithinGrid(x + xi, y + yi)) continue;

                    if (xi == 0 && yi == 0) continue;

                    if (grid[x + xi, y + yi] == TileType.RoomFloor || grid[x + xi, y + yi] == TileType.HallwayFloor)
                        return true;
                }
            }

            return false;
        }

        private bool TryGetCornerType(int x, int y, out TileType type)
        {
            type = TileType.HallwayFloor;

            if (!WithinGrid(x, y)) return false;

            Coordinate left = new(x - 1, y);
            Coordinate right = new(x + 1, y);
            Coordinate top = new(x, y + 1);
            Coordinate bottom = new(x, y - 1);

            // Top-left
            if (TryGetCoordinate(top, out TileType tile1) && TryGetCoordinate(left, out TileType tile2))
            {
                if (tile1 == TileType.Wall && tile2 == TileType.Wall)
                {
                    type = TileType.TopLeftCorner;

                    return true;
                }
            }

            // Top-Right
            if (TryGetCoordinate(top, out tile1) && TryGetCoordinate(right, out tile2))
            {
                if (tile1 == TileType.Wall && tile2 == TileType.Wall)
                {
                    type = TileType.TopRightCorner;

                    return true;
                }
            }

            // Bottom-left
            if (TryGetCoordinate(bottom, out tile1) && TryGetCoordinate(left, out tile2))
            {
                if (tile1 == TileType.Wall && tile2 == TileType.Wall)
                {
                    type = TileType.BottomLeftCorner;

                    return true;
                }
            }

            // Bottom-right
            if (TryGetCoordinate(bottom, out tile1) && TryGetCoordinate(right, out tile2))
            {
                if (tile1 == TileType.Wall && tile2 == TileType.Wall)
                {
                    type = TileType.BottomRightCorner;

                    return true;
                }
            }


            return false;
        }

        private bool WithinGrid(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < Settings.gridWidth && y < Settings.gridHeight);
        }

        private bool WithinGrid(Coordinate coords)
        {
            return (coords.x >= 0 && coords.y >= 0 && coords.x < Settings.gridWidth && coords.y < Settings.gridHeight);
        }

        public TileType GetCoordinate(int x, int y)
        {
            if (WithinGrid(x, y))
            {
                return grid[x, y];
            }

            return TileType.Void;
        }

        public TileType GetCoordinate(Coordinate coordinate)
        {
            return GetCoordinate(coordinate.x, coordinate.y);
        }

        private bool TryGetCoordinate(Coordinate coordinate, out TileType type)
        {
            type = TileType.Void;

            if (WithinGrid(coordinate))
            {
                type = grid[coordinate.x, coordinate.y];

                return true;
            }

            return false;
        }

        public bool TryGetRandomRoomCenter(out Vertex center)
        {
            center = new Vertex(0, 0);

            if (Rooms == null)
            {
                return false;
            }

            if (Rooms.Count < 1)
            {
                return false;
            }

            center = Rooms[random.Next(0, Rooms.Count - 1)].GetCenter();
            return true;
        }
    }
}