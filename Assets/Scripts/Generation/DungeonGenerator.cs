using System;
using System.Collections.Generic;
using Random = System.Random;

namespace Assets.Scripts.Generation
{
    public class DungeonGenerator
    {
        private Random random;

        // Bool = isRoom
        private bool[,] grid;
        public List<Room> rooms { get; private set; }
        private List<Edge> edges;
        private HashSet<MeasuredEdge> selectedEdges;
        //public List<Vertex> lights { get; private set; }

        private GenerationSettings settings;

        public DungeonGenerator(GenerationSettings settings)
        {
            this.settings = settings;
        }

        public bool[,] Generate(int seed)
        {
            random = new Random(seed);

            grid = new bool[settings.gridWidth, settings.gridHeight];
            rooms = new List<Room>();

            InitializeGrid();
            CreateRooms();
            Triangulate();
            CreateHallways();
            PathfindHallways();
            AddLights();

            return grid;
        }

        private void InitializeGrid()
        {
            for (int x = 0; x < settings.gridWidth; x++)
            {
                for (int y = 0; y < settings.gridHeight; y++)
                {
                    grid[x, y] = false;
                }
            }
        }

        private void CreateRooms()
        {
            int attempts;
            bool canAddRoom;

            for (int i = 0; i < settings.roomCount; i++)
            {
                Room newRoom = new Room();

                canAddRoom = false;
                attempts = 0;

                // Allows for multiple iterations in case of intersections or reaching outside grid
                while (!canAddRoom && attempts < settings.maxRoomAttempts)
                {
                    canAddRoom = true;

                    newRoom = new Room(
                        random.Next(settings.minRoomWidth, settings.gridWidth - settings.maxRoomWidth),
                        random.Next(settings.minRoomHeight, settings.gridHeight - settings.maxRoomHeight),
                        random.Next(settings.minRoomWidth, settings.maxRoomWidth + 1),
                        random.Next(settings.minRoomHeight, settings.maxRoomHeight + 1)
                        );

                    // Check if room is within grid
                    if (!newRoom.WithinGrid(settings.gridWidth, settings.gridHeight))
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
                if (random.Next(0, 100) < settings.extraHallwayGenerationChance)
                {
                    selectedEdges.Add(new MeasuredEdge(edge));
                }
            }
        }

        private void PathfindHallways()
        {
            DungeonPathfinder pathfinder = new DungeonPathfinder(grid, settings.gridWidth, settings.gridHeight);

            List<Vertex> currentHallway;

            foreach (MeasuredEdge edge in selectedEdges)
            {
                currentHallway = pathfinder.FindPath(edge.u, edge.v);

                if (currentHallway != null)
                {
                    Room firstRoom = null;

                    foreach (Room room in rooms)
                    {
                        if (room.ContainsPoint(edge.u))
                        {
                            firstRoom = room;
                            break;
                        }
                    }

                    firstRoom.connectedRooms = new List<Room>();

                    if (firstRoom != null)
                    {
                        foreach (Room room in rooms)
                        {
                            if (room.ContainsPoint(edge.v))
                            {
                                if (room != firstRoom)
                                {
                                    firstRoom.connectedRooms.Add(room);
                                }
                            }
                        }
                    }

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

        private void AddLights()
        {
            foreach (Room room in rooms)
            {
                room.lights = new List<Vertex>();

                if (random.Next(0, 1) == 0)
                {
                    room.lights.Add(room.GetCenter());
                }
            }
        }

        private void SetTile(int x, int y, bool isRoom)
        {
            if (x < 0 || y < 0 || x > settings.gridWidth || y > settings.gridHeight)
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

        public bool TryGetRandomRoomCenter(out Vertex center)
        {
            center = null;

            if (rooms == null)
            {
                return false;
            }

            if (rooms.Count < 1)
            {
                return false;
            }

            center = rooms[random.Next(0, rooms.Count - 1)].GetCenter();
            return true;
        }
    }

    public class Room
    {
        public int xPosition;
        public int yPosition;

        public int width;
        public int height;

        public List<Vertex> lights;
        public List<Room> connectedRooms;

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

        public bool ContainsPoint(Vertex vertex)
        {
            return xPosition < vertex.x && (xPosition + width) > vertex.x
                && yPosition < vertex.y && (yPosition + height) > vertex.y;
        }
    }
}