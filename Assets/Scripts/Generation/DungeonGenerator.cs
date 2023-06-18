using System;
using System.Collections.Generic;
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

    public class DungeonGenerator
    {
        private Random random;

        private TileType[,] grid;
        public List<Room> rooms { get; private set; }
        private List<Edge> edges;
        private HashSet<MeasuredEdge> selectedEdges;

        public GenerationSettings Settings { get; private set; }

        public DungeonGenerator(GenerationSettings settings)
        {
            this.Settings = settings;
        }

        public TileType[,] Generate(int seed)
        {
            random = new Random(seed);

            grid = new TileType[Settings.gridWidth, Settings.gridHeight];
            rooms = new List<Room>();

            InitializeGrid();
            CreateRooms();
            Triangulate();
            CreateHallways();
            PathfindHallways();
            GenerateWalls();
            FindInsetCorners();
            AddLights();

            return grid;
        }

        private void InitializeGrid()
        {
            for (int x = 0; x < Settings.gridWidth; x++)
            {
                for (int y = 0; y < Settings.gridHeight; y++)
                {
                    grid[x, y] = TileType.Void;
                }
            }
        }

        private void CreateRooms()
        {
            int attempts;
            bool canAddRoom;

            for (int i = 0; i < Settings.roomCount; i++)
            {
                Room newRoom = new Room();

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
                            grid[x, y] = TileType.RoomFloor;
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
                if (random.Next(0, 100) < Settings.extraHallwayGenerationChance)
                {
                    selectedEdges.Add(new MeasuredEdge(edge));
                }
            }
        }

        private void PathfindHallways()
        {
            List<Coordinate> currentHallway;

            foreach (MeasuredEdge edge in selectedEdges)
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
            TileType type;

            for (int x = 0; x < Settings.gridWidth; x++)
            {
                for (int y = 0; y < Settings.gridHeight; y++)
                {
                    if (grid[x, y] != TileType.HallwayFloor) continue;

                    if (TryGetCornerType(x, y, out type))
                    {
                        SetTile(x, y, type);
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

            Coordinate left = new Coordinate(x - 1, y);
            Coordinate right = new Coordinate(x + 1, y);
            Coordinate top = new Coordinate(x, y + 1);
            Coordinate bottom = new Coordinate(x, y - 1);

            TileType tile1, tile2;

            // Top-left
            if (TryGetCoordinate(top, out tile1) && TryGetCoordinate(left, out tile2))
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
}