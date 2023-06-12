using System;
using System.Collections.Generic;
using Random = System.Random;

namespace Assets.Scripts.Generation
{
    public enum TileType
    {
        Void,
        Floor,
        Wall,
    }

    public class DungeonGenerator
    {
        private Random random;

        // bool = isRoom
        private TileType[,] grid;
        public List<Room> rooms { get; private set; }
        private List<Edge> edges;
        private HashSet<MeasuredEdge> selectedEdges;
        //public List<Vertex> lights { get; private set; }

        private GenerationSettings settings;

        public DungeonGenerator(GenerationSettings settings)
        {
            this.settings = settings;
        }

        public TileType[,] Generate(int seed)
        {
            random = new Random(seed);

            grid = new TileType[settings.gridWidth, settings.gridHeight];
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
                    grid[x, y] = TileType.Void;
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

                    // Mark grid squares that new room takes up
                    for (int x = newRoom.xPosition; x <= newRoom.xPosition + newRoom.width; x++)
                    {
                        for (int y = newRoom.yPosition; y <= newRoom.yPosition + newRoom.height; y++)
                        {
                            if (x == newRoom.xPosition || x == newRoom.xPosition + newRoom.width
                                || y == newRoom.yPosition || y == newRoom.yPosition + newRoom.height)
                            {
                                grid[x, y] = TileType.Wall;
                            }
                            else
                            {
                                grid[x, y] = TileType.Floor;
                            }
                        }
                    }

                    DungeonDebug.DrawRoom(newRoom, 500, UnityEngine.Color.red);
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
            List<Coordinate> currentHallway;

            bool once = false;
            foreach (MeasuredEdge edge in selectedEdges)
            {
                currentHallway = DungeonPathfinder.FindPath(settings.gridWidth, settings.gridHeight, edge.u, edge.v);

                if (currentHallway != null)
                {
                    if (!once)
                        DungeonDebug.DrawEdge(edge, 500, UnityEngine.Color.green);

                    int xIndex;
                    int yIndex;
                    foreach (Coordinate coordinate in currentHallway)
                    {
                        xIndex = coordinate.x;
                        yIndex = coordinate.y;

                        for (int x = -2; x <= 2; x++)
                        {
                            for (int y = -2; y <= 2; y++)
                            {
                                if (WithinGrid(xIndex + x, yIndex + y))
                                {
                                    if (grid[xIndex + x, yIndex + y] != TileType.Void) continue;

                                    if (!once)
                                    {
                                        UnityEngine.Debug.Log(GetNeighborCount(xIndex + x, yIndex + y));
                                    }

                                    // LISTEN HERE:
                                    // The problem with this is that we are checking if things should be walls
                                    // before we've assigned all the tiles a value, meaning they are void, which
                                    // causes the GetNeighborCount() function to return incorrect values, marking
                                    // everything as walls

                                    SetTile(xIndex + x, yIndex + y, TileType.Floor);

                                    if (GetNeighborCount(xIndex + x, yIndex + y) < 8)
                                    {
                                        SetTile(xIndex + x, yIndex + y, TileType.Floor);
                                    }
                                    else
                                    {
                                        SetTile(xIndex + x, yIndex + y, TileType.Wall);
                                    }
                                }
                            }
                        }
                    }

                    once = true;
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

        private void SetTile(int x, int y, TileType tileType)
        {
            if (!WithinGrid(x, y)) return;

            grid[x, y] = tileType;
        }

        private int GetNeighborCount(int x, int y)
        {
            if (!WithinGrid(x, y)) return 0;

            int count = -1;
            for (int xi = -1; xi <= 1; xi++)
            {
                for (int yi = -1; yi <= 1; yi++)
                {
                    if (grid[x + xi, y + yi] != TileType.Void)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private bool WithinGrid(int x, int y)
        {
            return !(x < 0 || y < 0 || x > settings.gridWidth || y > settings.gridHeight);
        }

        public bool TryGetRandomRoomCenter(out Vertex center)
        {
            center = new Vertex(0, 0);

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