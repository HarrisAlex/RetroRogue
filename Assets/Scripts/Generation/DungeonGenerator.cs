using UnityEngine;
using System.Collections.Generic;
using static Assets.Scripts.Generation.DungeonGeneration;
using Random = System.Random;

namespace Assets.Scripts.Generation
{
    public enum TileType
    {
        Void,
        Floor,
        Wall
    }

    public class DungeonGenerator
    {
        public GenerationSettings GenerationSettings { get; private set; }

        // Generation
        private Random random;

        private TileType[,] grid;
        private List<Room> rooms;
        private List<Edge> edges;
        private HashSet<Edge> selectedEdges;
        private List<LightData> lights;

        public DungeonGenerator(GenerationSettings generationSettings)
        {
            GenerationSettings = generationSettings;
        }

        public Dungeon Generate()
        {
            int seed = GenerationSettings.seed;

            if (seed == 0)
            {
                System.TimeSpan time = (System.DateTime.UtcNow - new System.DateTime(2003, 8, 8));
                seed = (int)time.TotalSeconds;

                seed = 631086620;
            }

            // Initialization of variables
            random = new Random(seed);
            grid = new TileType[GenerationSettings.gridWidth, GenerationSettings.gridHeight];
            rooms = new List<Room>();


            // Initialize grid
            FillArea(0, 0, GenerationSettings.gridWidth - 1, GenerationSettings.gridHeight - 1, TileType.Void);


            // Create rooms
            for (int i = 0; i < GenerationSettings.roomCount; i++)
            {
                Room newRoom = new();

                if (TryCreateRoom(out newRoom))
                {
                    rooms.Add(newRoom);

                    FillArea(newRoom.xPosition, newRoom.yPosition,
                        newRoom.xPosition + newRoom.width, newRoom.yPosition + newRoom.height, TileType.Floor);
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
                if (random.Next(0, 100) < GenerationSettings.extraHallwayGenerationChance)
                    selectedEdges.Add(edge);
            }

            // Hallway creation
            List<Hallway> hallways = new();
            foreach (Edge edge in selectedEdges)
            {
                int expansion = random.Next(1, GenerationSettings.maxHallwayExpansion);
                hallways.Add(new(edge, expansion));
            }

            foreach (Hallway hallway in hallways)
            {
                IterateArea((int)hallway.BoundingBox.xPosition, (int)hallway.BoundingBox.yPosition,
                    (int)hallway.BoundingBox.xPosition + (int)hallway.BoundingBox.width, (int)hallway.BoundingBox.yPosition + (int)hallway.BoundingBox.height,
                    (int x, int y) =>
                    {
                        if (!WithinGrid(x, y)) return;

                        if (hallway.ContainsPoint(new(x, y)))
                        {
                            SetTile(x, y, TileType.Floor);
                        }
                    });
            }

            // Add lights
            lights = new();
            UnityEngine.Color tmpColor;
            foreach (Room room in rooms)
            {
                tmpColor = new(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));

                lights.Add(new LightData(new(room.Center.x, 1.5f, room.Center.y), 5, tmpColor, 5));
            }

            // Add walls
            FillArea(0, 0, GenerationSettings.gridWidth - 1, GenerationSettings.gridHeight - 1, TileType.Wall, ShouldPlaceWall);

            Dungeon dungeon = new(grid, selectedEdges, GenerationSettings, rooms, lights);

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
            while (newRoom == null && attempts < GenerationSettings.maxRoomAttempts)
            {
                newRoom = new Room(
                    random.Next(1, GenerationSettings.gridWidth - GenerationSettings.maxRoomWidth - 1),
                    random.Next(1, GenerationSettings.gridHeight - GenerationSettings.maxRoomHeight - 1),
                    random.Next(GenerationSettings.minRoomWidth, GenerationSettings.maxRoomWidth + 1),
                    random.Next(GenerationSettings.minRoomHeight, GenerationSettings.maxRoomHeight + 1)
                    );

                // Check if room is within grid
                if (!newRoom.WithinGrid(GenerationSettings.gridWidth, GenerationSettings.gridHeight))
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
                vertices.Add(room.Center);
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
            float deltaMax = Mathf.Max(deltaX, deltaY) * 2;

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

            foreach (Triangle triangle in triangles)
            {
                int supraVerticesContained = 0;

                if (triangle.ContainsVertex(v1)) supraVerticesContained++;
                if (triangle.ContainsVertex(v2)) supraVerticesContained++;
                if (triangle.ContainsVertex(v3)) supraVerticesContained++;

                if (supraVerticesContained > 1) continue;

                Edge ab = new(triangle.a, triangle.b);
                Edge bc = new(triangle.b, triangle.c);
                Edge ca = new(triangle.c, triangle.a);

                if (!edges.Contains(ab) && WithinGrid((int)ab.u.x, (int)ab.u.y) && WithinGrid((int)ab.v.x, (int)ab.v.y))
                    edges.Add(ab);

                if (!edges.Contains(bc) && WithinGrid((int)bc.u.x, (int)bc.u.y) && WithinGrid((int)bc.v.x, (int)bc.v.y))
                    edges.Add(bc);

                if (!edges.Contains(ca) && WithinGrid((int)ca.u.x, (int)ca.u.y) && WithinGrid((int)ca.v.x, (int)ca.v.y))
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
        private void FillArea(int x1, int y1, int x2, int y2, TileType tileType, System.Func<int, int, bool> conditional)
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
                        if (type == TileType.Floor)
                            return true;
                    }
                }
            }

            return false;
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

        private bool WithinGrid(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < GenerationSettings.gridWidth && y < GenerationSettings.gridHeight);
        }

        public bool TryGetRandomRoomCenter(out Vertex center)
        {
            center = new Vertex(0, 0);

            if (rooms == null)
                return false;

            if (rooms.Count < 1)
                return false;

            center = rooms[random.Next(0, rooms.Count - 1)].Center;
            return true;
        }
    }
}