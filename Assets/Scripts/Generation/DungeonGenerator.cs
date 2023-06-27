using System;
using System.Collections.Generic;
using static Assets.Scripts.Generation.Geometry;
using Random = System.Random;

namespace Assets.Scripts.Generation
{
    public enum TileType
    {
        Void,
        RoomFloor,
        HallwayFloor,
        Wall,
        TopLeftCorner,
        TopRightCorner,
        BottomLeftCorner,
        BottomRightCorner,
    }

    public struct Dungeon
    {
        public TileType[,] grid;
        public NavigationTree navigationTree;
        public GenerationSettings settings;

        private readonly int seed;

        public Dungeon(TileType[,] grid, HashSet<Edge> edges, GenerationSettings settings, int seed)
        {
            this.grid = grid;

            navigationTree = new NavigationTree(grid, new(edges));

            this.settings = settings;
            this.seed = seed;
        }


        public bool TryGetRandomRoomCenter(out Vertex vertex)
        {
            vertex = Vertex.NegativeInfinity;
            if (navigationTree == null) return false;
            if (navigationTree.edges == null) return false;
            if (navigationTree.edges.Count < 1) return false;

            vertex = navigationTree.edges[new Random(seed).Next(0, navigationTree.edges.Count - 1)].u;
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
            return (vertex.x >= xPosition && vertex.x <= xPosition + width
                && vertex.y >= yPosition && vertex.y <= yPosition + height);
        }
    }

    class Node
    {
        public Coordinate position;
        public Node parent;
        public float gCost;
        public float hCost;

        public float FCost
        {
            get { return gCost + hCost; }
        }

        public Node(int x, int y)
        {
            position = new Coordinate(x, y);
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

        // Pathfinding
        private Node[,] allNodes;

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
                        newRoom.xPosition + newRoom.width, newRoom.yPosition + newRoom.height, TileType.RoomFloor);
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

            // Find minimum spanning tree
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
            allNodes = new Node[Settings.gridWidth, Settings.gridHeight];
            IterateArea(0, 0, Settings.gridWidth - 1, Settings.gridHeight - 1, (int x, int y) =>
            {
                if (TryGetCoordinate(x, y, out TileType type))
                    allNodes[x, y] = new Node(x, y);
                else
                    allNodes[x, y] = null;
            });

            // Pathfind and fill hallways
            foreach (Edge edge in selectedEdges)
            {
                if (TryFindPath(edge.u, edge.v, out List<Coordinate> currentHallway))
                {
                    foreach (Coordinate coordinate in currentHallway)
                    {
                        FillArea(coordinate.x - Settings.hallwayExpansion,
                            coordinate.y - Settings.hallwayExpansion,
                            coordinate.x + Settings.hallwayExpansion,
                            coordinate.y + Settings.hallwayExpansion,
                            TileType.HallwayFloor, IsVoid
                            );
                    }
                }
            }


            // Add walls
            FillArea(0, 0, Settings.gridWidth - 1, Settings.gridHeight - 1, TileType.Wall, ShouldPlaceWall);


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

            // Create dungeon struct
            return new(grid, selectedEdges, Settings, seed);
        }

        private bool TryCreateRoom(out Room newRoom)
        {
            int attempts = 0;
            newRoom = null;

            // Create new rooms until one is valid
            while (newRoom == null && attempts < Settings.maxRoomAttempts)
            {
                newRoom = new Room(
                    random.Next(Settings.minRoomWidth, Settings.gridWidth - Settings.maxRoomWidth),
                    random.Next(Settings.minRoomHeight, Settings.gridHeight - Settings.maxRoomHeight),
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
                    if (Room.RoomsIntersect(room, newRoom, 0))
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

        private bool TryFindPath(Vertex start, Vertex end, out List<Coordinate> path)
        {
            path = null;

            // Validate start and end positions
            if (!TryGetNode(start.ToCoordinate(), out Node startNode)) return false;
            if (!TryGetNode(end.ToCoordinate(), out Node endNode)) return false;

            List<Node> open = new();
            open.Add(startNode);

            HashSet<Node> closed = new();

            // Iterate while all nodes have not been checked
            while (open.Count > 0)
            {
                Node node = open[0];

                // Get cheapest next node
                Node current;
                for (int i = 1; i < open.Count; i++)
                {
                    current = open[i];
                    if (current.FCost < node.FCost || Approximately(current.FCost, node.FCost))
                    {
                        if (current.hCost < node.hCost)
                            node = current;
                    }
                }

                open.Remove(node);
                closed.Add(node);

                // Path has been found
                if (node == endNode)
                {
                    path = RetracePath(startNode, endNode);
                    break;
                }

                // Iterate through neighbors
                foreach (Node neighbor in GetNeighbors(node))
                {
                    if (closed.Contains(neighbor)) continue;

                    float cost = node.gCost + Distance(node.position, neighbor.position);

                    bool openContainsNeighbor = open.Contains(neighbor);

                    // Adds each neighbor cheaper than current node to open set
                    if (cost < neighbor.gCost || !openContainsNeighbor)
                    {
                        neighbor.gCost = cost;
                        neighbor.hCost = Distance(neighbor.position, endNode.position);
                        neighbor.parent = node;

                        if (!openContainsNeighbor)
                            open.Add(neighbor);
                    }
                }
            }

            return path != null;
        }

        // Trace path from start to finish through parents
        private static List<Coordinate> RetracePath(Node startNode, Node endNode)
        {
            List<Coordinate> path = new();
            Node current = endNode;

            while (current != startNode)
            {
                path.Add(current.parent.position);
                current = current.parent;
            }

            path.Reverse();

            return path;
        }

        private List<Node> GetNeighbors(Node node)
        {
            List<Node> result = new();

            int checkX, checkY;
            IterateArea(-1, -1, 1, 1, (int x, int y) =>
            {
                if (x == 0 && y == 0) return;

                checkX = node.position.x + x;
                checkY = node.position.y + y;

                if (TryGetNode(checkX, checkY, out Node neighbor))
                    result.Add(neighbor);
            });

            return result;
        }

        #region Functions for Getting Nodes

        private bool TryGetNode(Coordinate coordinate, out Node node)
        {
            return TryGetNode(coordinate.x, coordinate.y, out node);
        }

        private bool TryGetNode(int x, int y, out Node node)
        {
            if (WithinGrid(x, y))
                node = allNodes[x, y];
            else
                node = null;

            return node != null;
        }

        #endregion

        #region Functions for Setting Tiles

        private void SetTile(int x, int y, TileType tileType)
        {
            if (!WithinGrid(x, y)) return;

            grid[x, y] = tileType;
        }

        /// <summary>
        /// Fill an area as defined by coordinates with a certain TileType.
        /// </summary>
        /// <param name="x1">Bottom-left corner of area.</param>
        /// <param name="y1">Bottom-left corner of area.</param>
        /// <param name="x2">Top-right corner of area.</param>
        /// <param name="y2">Top-rightcorner of area.</param>
        /// <param name="tileType">The TileType of which to set tiles in the area.</param>
        private void FillArea(int x1, int y1, int x2, int y2, TileType tileType)
        {
            if (!WithinGrid(x1, y1) || !WithinGrid(x2, y2)) return;

            IterateArea(x1, y1, x2, y2, (int x, int y) => SetTile(x, y, tileType));
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
        private void FillArea(int x1, int y1, int x2, int y2, TileType tileType, Func<int, int, bool> conditional)
        {
            if (!WithinGrid(x1, y1) || !WithinGrid(x2, y2)) return;

            IterateArea(x1, y1, x2, y2, (int x, int y) =>
            {
                if (!WithinGrid(x, y)) return;

                if (conditional(x, y))
                    SetTile(x, y, tileType);
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

            Coordinate left = new(x - 1, y);
            Coordinate top = new(x, y + 1);

            if (TryGetCoordinate(top, out TileType tile1) && TryGetCoordinate(left, out TileType tile2))
            {
                if (tile1 == TileType.Wall && tile2 == TileType.Wall)
                    return true;
            }

            return false;
        }

        private bool IsTopRightCorner(int x, int y)
        {
            if (!IsCorner(x, y)) return false;

            Coordinate right = new(x + 1, y);
            Coordinate top = new(x, y + 1);

            if (TryGetCoordinate(top, out TileType tile1) && TryGetCoordinate(right, out TileType tile2))
            {
                if (tile1 == TileType.Wall && tile2 == TileType.Wall)
                    return true;
            }

            return false;
        }

        private bool IsBottomLeftCorner(int x, int y)
        {
            if (!IsCorner(x, y)) return false;

            Coordinate left = new(x - 1, y);
            Coordinate bottom = new(x, y - 1);

            if (TryGetCoordinate(bottom, out TileType tile1) && TryGetCoordinate(left, out TileType tile2))
            {
                if (tile1 == TileType.Wall && tile2 == TileType.Wall)
                    return true;
            }

            return false;
        }

        private bool IsBottomRightCorner(int x, int y)
        {
            if (!IsCorner(x, y)) return false;

            Coordinate right = new(x + 1, y);
            Coordinate bottom = new(x, y - 1);

            if (TryGetCoordinate(bottom, out TileType tile1) && TryGetCoordinate(right, out TileType tile2))
            {
                if (tile1 == TileType.Wall && tile2 == TileType.Wall)
                    return true;
            }

            return false;
        }

        #endregion

        private bool IsCorner(int x, int y)
        {
            if (!WithinGrid(x, y)) return false;
            if (GetCoordinate(x, y) != TileType.HallwayFloor) return false;

            return true;
        }

        private bool IsFloor(TileType type)
        {
            return type == TileType.HallwayFloor || type == TileType.RoomFloor;
        }

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

        private bool WithinGrid(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < Settings.gridWidth && y < Settings.gridHeight);
        }

        public void IterateArea(int x1, int y1, int x2, int y2, Action<int, int> function)
        {
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    function(x, y);
                }
            }
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