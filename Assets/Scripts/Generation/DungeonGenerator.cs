using System;
using System.Collections.Generic;
using static Assets.Scripts.Generation.DungeonGeneration;
using Random = System.Random;

namespace Assets.Scripts.Generation
{
    [Flags]
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

        public bool WithinGrid(int gridWidth, int gridHeight)
        {
            return xPosition + width < gridWidth || yPosition + height < gridHeight;
        }

        public static bool RoomsIntersect(Room room1, Room room2)
        {
            return !((room1.xPosition >= (room2.xPosition + room2.width))
                || ((room1.xPosition + room1.width) <= room2.xPosition)
                || (room1.yPosition >= (room2.yPosition + room2.height))
                || ((room1.yPosition + room1.height) <= room2.yPosition));
        }

        public bool ContainsPoint(Vertex vertex)
        {
            return (vertex.x >= xPosition && vertex.x <= xPosition + width
                && vertex.y >= yPosition && vertex.y <= yPosition + height);
        }

        public List<Edge> GetEdges()
        {
            Vertex bottomLeft = new(xPosition, yPosition);
            Vertex bottomRight = new(xPosition + width, yPosition);
            Vertex topLeft = new(xPosition, yPosition + height);
            Vertex topRight = new(xPosition + width, yPosition + height);

            List<Edge> edges = new();
            edges.Add(new(bottomLeft, topLeft));
            edges.Add(new(topLeft, topRight));
            edges.Add(new(topRight, bottomRight));
            edges.Add(new(bottomRight, bottomLeft));

            return edges;
        }
    }

    public class DungeonGenerator
    {
        public GenerationSettings Settings { get; private set; }

        // Generation
        private Random random;

        private TileType[,] grid;
        private List<Room> rooms;
        private List<Edge> edges;
        private HashSet<Edge> selectedEdges;

        public DungeonGenerator(GenerationSettings settings)
        {
            Settings = settings;
        }

        public Dungeon Generate(int seed)
        {
            // Initialization of variables
            random = new Random(seed);
            grid = new TileType[Settings.gridWidth, Settings.gridHeight];
            rooms = new List<Room>();


            // Initialize grid
            FillArea(0, 0, Settings.gridWidth - 1, Settings.gridHeight - 1, TileType.Void);


            // Create rooms
            for (int i = 0; i < Settings.roomCount; i++)
            {
                Room newRoom = new();

                if (TryCreateRoom(out newRoom))
                {
                    rooms.Add(newRoom);

                    FillArea(newRoom.xPosition, newRoom.yPosition,
                        newRoom.xPosition + newRoom.width, newRoom.yPosition + newRoom.height, TileType.RoomFloor, true);
                }
            }

            Triangulate();

            // Create hallways
            selectedEdges = new();

            HashSet<Vertex> openSet = new();
            HashSet<Vertex> closedSet = new();

            foreach (Edge edge in edges)
            {
                openSet.Add(edge.u);
                openSet.Add(edge.v);
            }

            closedSet.Add(edges[0].u);

            // Find minimum spanning tree (Kruskal's)
            Edge chosenEdge;
            float minWeight;

            while (openSet.Count > 0)
            {
                chosenEdge = null;
                minWeight = float.PositiveInfinity;

                // Find shortest unexplored edge that shares a vertex with an explored edge and another vertex with an unexplored edge
                foreach (Edge edge in edges)
                {
                    int exploredVertices = 0;

                    if (!closedSet.Contains(edge.u))
                        exploredVertices++;

                    if (!closedSet.Contains(edge.v))
                        exploredVertices++;

                    if (exploredVertices != 1)
                        continue;

                    if (edge.Length < minWeight)
                    {
                        chosenEdge = edge;
                        minWeight = edge.Length;
                    }
                }

                if (chosenEdge == null)
                    break;

                selectedEdges.Add(chosenEdge);
                openSet.Remove(chosenEdge.u);
                openSet.Remove(chosenEdge.v);
                closedSet.Add(chosenEdge.u);
                closedSet.Add(chosenEdge.v);
            }

            // Get all hallways not included in minimum spanning tree (all that are unecessary)
            HashSet<Edge> remainingEdges = new(edges);
            remainingEdges.ExceptWith(selectedEdges);

            // Randomly add unecessary hallways based on chance
            foreach (Edge edge in remainingEdges)
            {
                if (random.Next(0, 100) < Settings.extraHallwayGenerationChance)
                    selectedEdges.Add(edge);
            }

            // Initialize nodes for pathfinding hallways
            Node<Coordinate>[,] nodesArray = new Node<Coordinate>[Settings.gridWidth, Settings.gridHeight];
            IterateArea(0, 0, Settings.gridWidth - 1, Settings.gridHeight - 1, (int x, int y) =>
            {
                if (TryGetCoordinate(x, y, out TileType type))
                    nodesArray[x, y] = new(new Coordinate(x, y));
                else
                    nodesArray[x, y] = null;
            });

            // Find neighbors
            foreach (Node<Coordinate> node in nodesArray)
            {
                List<Node<Coordinate>> neighbors = new();

                int checkX, checkY;
                IterateArea(-1, -1, 1, 1, (int x, int y) =>
                {
                    if (x == 0 && y == 0) return;

                    checkX = node.position.x + x;
                    checkY = node.position.y + y;

                    if (TryGetNode(nodesArray, checkX, checkY, out Node<Coordinate> neighbor))
                        neighbors.Add(neighbor);
                });

                node.neighbors = neighbors;
            }

            List<Node<Coordinate>> nodes = new();

            foreach (Node<Coordinate> node in nodesArray)
                nodes.Add(node);

            Pathfinding<Coordinate> hallwayPathfinder = new(new List<Node<Coordinate>>(nodes));

            // Pathfind and fill hallways
            foreach (Edge edge in selectedEdges)
            {
                List<Node<Coordinate>> path = hallwayPathfinder.FindPath(edge.u.ToCoordinate(), edge.v.ToCoordinate());

                if (path == null)
                    continue;

                int expansion = random.Next(0, Settings.maxHallwayExpansion);

                foreach (Node<Coordinate> coordinate in path)
                {
                    FillArea(coordinate.position.x - expansion,
                        coordinate.position.y - expansion,
                        coordinate.position.x + expansion,
                        coordinate.position.y + expansion,
                        TileType.HallwayFloor, IsVoid
                        );
                }

                var customers = new Dictionary<float, int>();
                Dictionary<float, int> customerb = new();
            }

            // Add walls
            FillArea(0, 0, Settings.gridWidth - 1, Settings.gridHeight - 1, TileType.Wall, ShouldPlaceWall, true);


            // Add inset corners
            FillArea(0, 0, Settings.gridWidth - 1, Settings.gridHeight - 1, TileType.TopLeftCorner, IsTopLeftCorner);
            FillArea(0, 0, Settings.gridWidth - 1, Settings.gridHeight - 1, TileType.TopRightCorner, IsTopRightCorner);
            FillArea(0, 0, Settings.gridWidth - 1, Settings.gridHeight - 1, TileType.BottomRightCorner, IsBottomRightCorner);
            FillArea(0, 0, Settings.gridWidth - 1, Settings.gridHeight - 1, TileType.BottomLeftCorner, IsBottomLeftCorner);


            // Add lights
            foreach (Room room in rooms)
            {
                room.lights = new List<Vertex>();

                if (random.Next(0, 1) == 0)
                    room.lights.Add(room.GetCenter());
            }

            Dungeon dungeon = new(grid, selectedEdges, Settings, random, rooms);

#if UNITY_EDITOR
            if (DungeonDebug.instance != null)
                DungeonDebug.SetDungeon(dungeon);
#endif

            // Create dungeon struct
            return dungeon;
        }

        private bool TryCreateRoom(out Room newRoom)
        {
            int attempts = 0;
            newRoom = null;

            // Create new rooms until one is valid
            while (newRoom == null && attempts < Settings.maxRoomAttempts)
            {
                newRoom = new Room(
                    random.Next(0, Settings.gridWidth - Settings.maxRoomWidth),
                    random.Next(0, Settings.gridHeight - Settings.maxRoomHeight),
                    random.Next(Settings.minRoomWidth, Settings.maxRoomWidth + 1),
                    random.Next(Settings.minRoomHeight, Settings.maxRoomHeight + 1)
                    );

                // Check if room is within grid
                if (!newRoom.WithinGrid(Settings.gridWidth, Settings.gridHeight))
                {
                    attempts++;
                    newRoom = null;

                    continue;
                }

                // Check for intersection with other rooms
                foreach (Room room in rooms)
                {
                    if (Room.RoomsIntersect(room, newRoom))
                    {
                        attempts++;
                        newRoom = null;

                        break;
                    }
                }
            }

            return newRoom != null;
        }

        private void Triangulate()
        {
            List<Vertex> vertices = new();

            // Create midpoint for center of each room
            foreach (Room room in rooms)
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
                    minX = vertex.x;

                if (vertex.y < minY)
                    minY = vertex.y;

                if (vertex.x > maxX)
                    maxX = vertex.x;

                if (vertex.y > maxY)
                    maxY = vertex.y;
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

                // Check for duplicate edges?
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

                // Remove duplicates?
                polygon.RemoveAll((Edge edge) => edge.isBad);

                foreach (Edge edge in polygon)
                    triangles.Add(new Triangle(edge.u, edge.v, vertex));
            }

            triangles.RemoveAll((Triangle triangle) => triangle.ContainsVertex(v1)
                || triangle.ContainsVertex(v2)
                || triangle.ContainsVertex(v3));

            foreach (Triangle triangle in triangles)
            {
                Edge ab = new(triangle.a, triangle.b);
                Edge bc = new(triangle.b, triangle.c);
                Edge ca = new(triangle.c, triangle.a);

                if (!edges.Contains(ab))
                    edges.Add(ab);

                if (!edges.Contains(bc))
                    edges.Add(bc);

                if (!edges.Contains(ca))
                    edges.Add(ca);
            }
        }

        #region Functions for Getting Nodes

        private bool TryGetNode(Node<Coordinate>[,] nodes, int x, int y, out Node<Coordinate> node)
        {
            if (WithinGrid(x, y))
                node = nodes[x, y];
            else
                node = null;

            return node != null;
        }

        #endregion

        #region Functions for Setting Tiles

        private void SetTile(int x, int y, TileType tileType, bool overwrite = false)
        {
            if (!WithinGrid(x, y)) return;

            if (overwrite)
                grid[x, y] = tileType;
            else
                grid[x, y] |= tileType;
        }

        /// <summary>
        /// Fill an area as defined by coordinates with a certain TileType.
        /// </summary>
        /// <param name="x1">Bottom-left corner of area.</param>
        /// <param name="y1">Bottom-left corner of area.</param>
        /// <param name="x2">Top-right corner of area.</param>
        /// <param name="y2">Top-rightcorner of area.</param>
        /// <param name="tileType">The TileType of which to set tiles in the area.</param>
        private void FillArea(int x1, int y1, int x2, int y2, TileType tileType, bool overwrite = false)
        {
            if (!WithinGrid(x1, y1) || !WithinGrid(x2, y2)) return;

            IterateArea(x1, y1, x2, y2, (int x, int y) => SetTile(x, y, tileType, overwrite));
        }

        /// <summary>
        /// Fill an area as defined by coordinates with a certain TileType based on a condition.
        /// </summary>
        /// <param name="x1">Bottom-left corner of area.</param>
        /// <param name="y1">Bottom-left corner of area.</param>
        /// <param name="x2">Top-right corner of area.</param>
        /// <param name="y2">Top-rightcorner of area.</param>
        /// <param name="tileType">The TileType of which to set tiles in the area.</param>
        /// <param name="conditional">The function by which to determine whether a tile should be set.</param>
        private void FillArea(int x1, int y1, int x2, int y2, TileType tileType, Func<int, int, bool> conditional, bool overwrite = false)
        {
            if (!WithinGrid(x1, y1) || !WithinGrid(x2, y2)) return;

            IterateArea(x1, y1, x2, y2, (int x, int y) =>
            {
                if (!WithinGrid(x, y)) return;

                if (conditional(x, y))
                    SetTile(x, y, tileType, overwrite);
            });
        }

        #endregion

        #region Conditional Function Presets (for FillArea())

        private bool IsVoid(int x, int y)
        {
            return GetCoordinate(x, y) == TileType.Void;
        }

        private bool ShouldPlaceWall(int x, int y)
        {
            if (!WithinGrid(x, y)) return false;
            if (GetCoordinate(x, y) != TileType.Void) return false;

            for (int xi = -1; xi <= 1; xi++)
            {
                for (int yi = -1; yi <= 1; yi++)
                {
                    if (xi == 0 && yi == 0) continue;

                    if (TryGetCoordinate(x + xi, y + yi, out TileType type))
                    {
                        if (IsFloor(type))
                            return true;
                    }
                }
            }

            return false;
        }

        private bool IsTopLeftCorner(int x, int y)
        {
            if (!IsCorner(x, y)) return false;

            Coordinate coords = new(x, y);

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Left), out TileType tile1))
            {
                if (IsFloor(tile1))
                    return false;
            }

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Down), out TileType tile5))
            {
                if (!tile5.HasFlag(TileType.HallwayFloor))
                    return false;
            }

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Right), out TileType tile4))
            {
                if (!tile4.HasFlag(TileType.HallwayFloor))
                    return false;
            }

            //if (TryGetCoordinate(bottomRight, out TileType tile3))
            //{
            //    if (IsFloor(tile3))
            //        return false;
            //}

            return true;
        }

        private bool IsTopRightCorner(int x, int y)
        {
            if (!IsCorner(x, y)) return false;

            Coordinate coords = new(x, y);

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Right), out TileType tile1))
            {
                if (IsFloor(tile1))
                    return false;
            }

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Left), out TileType tile4))
            {
                if (!tile4.HasFlag(TileType.HallwayFloor))
                    return false;
            }

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Down), out TileType tile5))
            {
                if (!tile5.HasFlag(TileType.HallwayFloor))
                    return false;
            }

            //if (TryGetCoordinate(bottomLeft, out TileType tile3))
            //{
            //    if (IsFloor(tile3))
            //        return false;
            //}

            return true;
        }

        private bool IsBottomLeftCorner(int x, int y)
        {
            if (!IsCorner(x, y)) return false;

            Coordinate coords = new(x, y);

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Left), out TileType tile2))
            {
                if (IsFloor(tile2))
                    return false;
            }

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Right), out TileType tile4))
            {
                if (!tile4.HasFlag(TileType.HallwayFloor))
                    return false;
            }

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Up), out TileType tile5))
            {
                if (!tile5.HasFlag(TileType.HallwayFloor))
                    return false;
            }

            //if (TryGetCoordinate(topRight, out TileType tile3))
            //{
            //    if (IsFloor(tile3))
            //        return false;
            //}

            return true;
        }

        private bool IsBottomRightCorner(int x, int y)
        {
            if (!IsCorner(x, y)) return false;

            Coordinate coords = new(x, y);

            TileType tile;

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Right), out tile))
            {
                if (IsFloor(tile))
                    return false;
            }

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Left), out tile))
            {
                if (!IsFloor(tile))
                    return false;
            }

            if (TryGetCoordinate(coords.GetNeighborCoordinate(Coordinate.Neighbor.Up), out tile))
            {
                if (!IsFloor(tile))
                    return false;
            }


            //if (TryGetCoordinate(topLeft, out TileType tile3))
            //{
            //    if (IsFloor(tile3))
            //        return false;
            //}

            return true;
        }

        #endregion

        #region Functions for Getting Tiles

        public TileType GetCoordinate(int x, int y)
        {
            if (WithinGrid(x, y))
                return grid[x, y];

            return TileType.Void;
        }

        private bool TryGetCoordinate(int x, int y, out TileType type)
        {
            type = TileType.Void;

            if (WithinGrid(x, y))
            {
                type = grid[x, y];

                return true;
            }

            return false;
        }

        private bool TryGetCoordinate(Coordinate coordinate, out TileType type)
        {
            return TryGetCoordinate(coordinate.x, coordinate.y, out type);
        }

        #endregion

        private bool IsCorner(int x, int y)
        {
            if (!WithinGrid(x, y)) return false;
            if (!IsWall(GetCoordinate(x, y))) return false;

            return true;
        }

        private bool WithinGrid(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < Settings.gridWidth && y < Settings.gridHeight);
        }

        public bool TryGetRandomRoomCenter(out Vertex center)
        {
            center = new Vertex(0, 0);

            if (rooms == null)
                return false;

            if (rooms.Count < 1)
                return false;

            center = rooms[random.Next(0, rooms.Count - 1)].GetCenter();
            return true;
        }
    }
}