using System;
using System.Collections.Generic;
using static Assets.Scripts.Generation.Dungeon3D;

namespace Assets.Scripts.Generation
{
    public class DungeonGeneration
    {
        // ----------------------------
        //      Classes & structs      
        // ----------------------------
        public struct Vertex
        {
            public float x;
            public float y;

            public static Vertex Zero { get => new(0, 0); }
            public static Vertex Invalid { get => new(float.MinValue, float.MinValue); }

            public Vertex(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public static Vertex VectorToVertex(UnityEngine.Vector3 vector)
            {
                return new Vertex(vector.x, vector.z);
            }

            public UnityEngine.Vector3 ToVector(float yOffset = 0)
            {
                return new UnityEngine.Vector3(x, yOffset, y);
            }

            public Coordinate ToCoordinate()
            {
                return new Coordinate((int)x, (int)y);
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !GetType().Equals(obj.GetType()))
                    return false;

                Vertex vertex = (Vertex)obj;
                return vertex.x == x && vertex.y == y;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator ==(Vertex a, Vertex b)
            {
                return Approximately(a, b);
            }

            public static bool operator !=(Vertex a, Vertex b)
            {
                return !Approximately(a, b);
            }

            public static Vertex operator +(Vertex a, Vertex b)
            {
                return new Vertex(a.x + b.x, a.y + b.y);
            }

            public static Vertex operator -(Vertex a, Vertex b)
            {
                return new Vertex(a.x - b.x, a.y - b.y);
            }

            public static Vertex operator /(Vertex a, float f)
            {
                return new Vertex(a.x / f, a.y / f);
            }
        }

        public struct Coordinate
        {
            public int x;
            public int y;

            public Coordinate(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public Vertex ToVertex()
            {
                return new Vertex(x, y);
            }

            [Flags]
            public enum Neighbor
            {
                Up = 1,
                Down = 2,
                Left = 4,
                Right = 8
            }

            public Coordinate GetNeighborCoordinate(Neighbor neighbor)
            {
                int neighborX = x, neighborY = y;

                if (neighbor.HasFlag(Neighbor.Up))
                    neighborY++;

                if (neighbor.HasFlag(Neighbor.Down))
                    neighborY--;

                if (neighbor.HasFlag(Neighbor.Left))
                    neighborX--;

                if (neighbor.HasFlag(Neighbor.Right))
                    neighborX++;

                return new(neighborX, neighborY);
            }
        }

        public class Edge
        {
            public Vertex u;
            public Vertex v;

            public bool isBad = false;

            public float Length
            {
                get
                {
                    return Distance(u, v);
                }
            }

            public float Slope
            {
                get
                {
                    return (v.y - u.y) / (v.x - u.x);
                }
            }

            public Edge Normalized
            {
                get
                {
                    float divisor = MathF.Sqrt((v.x - u.x) + (v.y - u.y));
                    return new Edge(u, u + (v / divisor));
                }
            }

            public Edge(Vertex u, Vertex v)
            {
                this.u = u;
                this.v = v;
            }

            public static bool AlmostEqual(Edge left, Edge right)
            {
                return Approximately(left.u, right.u) && Approximately(left.v, right.v)
                    || Approximately(left.u, right.v) && Approximately(left.v, right.u);
            }

            public Vertex FindIntersection(Edge edge)
            {
                // Parallel lines
                if (Approximately(Slope, edge.Slope))
                    return Vertex.Invalid;

                float xIntersection = (edge.u.x - u.x) / (Slope - edge.Slope);
                float yIntersection = Slope * xIntersection + u.x;

                return new(xIntersection, yIntersection);
            }

            public bool PointIsOnEdge(Vertex vertex)
            {
                return Approximately(Distance(vertex, u) + Distance(vertex, v), Length);
            }
        }

        public class Triangle
        {
            public Vertex a;
            public Vertex b;
            public Vertex c;

            public float Area
            {
                get
                {
                    return MathF.Abs((a.x * (b.y - c.y)) + (b.x * (c.y - a.y)) + (c.x * (a.y - b.y))) * 0.5f;
                }
            }

            public bool isBad = false;

            public Triangle(Vertex a, Vertex b, Vertex c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public bool CircumcircleContains(Vertex vertex)
            {
                // Terrifying equations for circumcircle 
                float diameter = (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) * 2;

                float aSquare = SquareDistance(Vertex.Zero, a);
                float bSquare = SquareDistance(Vertex.Zero, b);
                float cSquare = SquareDistance(Vertex.Zero, c);

                float circleX = (aSquare * (b.y - c.y) + bSquare * (c.y - a.y) + cSquare * (a.y - b.y)) / diameter;

                float circleY = (aSquare * (c.x - b.x) + bSquare * (a.x - c.x) + cSquare * (b.x - a.x)) / diameter;


                // Check if vertex is inside circumcircle
                Vertex circleCenter = new(circleX, circleY);
                float circleRadius = SquareDistance(a, circleCenter);
                float vertexDistance = SquareDistance(vertex, circleCenter);

                return vertexDistance <= circleRadius;
            }

            public bool ContainsVertex(Vertex vertex)
            {
                return Distance(vertex, a) < 0.01f
                    || Distance(vertex, b) < 0.01f
                    || Distance(vertex, c) < 0.01f;
            }
        }

        public class Hallway
        {
            public Vertex a, b, c, d;

            public Rectangle BoundingBox
            {
                get
                {
                    float xMin = GetExtrema(new() { a, b, c, d }, Extrema.xMinimum);
                    float yMin = GetExtrema(new() { a, b, c, d }, Extrema.yMinimum);
                    float xMax = GetExtrema(new() { a, b, c, d }, Extrema.xMaximum);
                    float yMax = GetExtrema(new() { a, b, c, d }, Extrema.yMaximum);

                    return new(xMin, yMin, xMax - xMin, yMax - yMin);
                }
            }

            public float Area
            {
                get
                {
                    return Distance(a, c) * Distance(a, b);
                }
            }

            public Hallway(Edge edge, float expansion)
            {
                float edgeAngle = MathF.Asin(MathF.Abs(edge.u.x - edge.v.x) / edge.Length);

                float triangleAngle = ((MathF.PI / 180) * 90) - edgeAngle;

                float dx = MathF.Sin(triangleAngle) * expansion;
                float dy = MathF.Cos(triangleAngle) * expansion;

                if (edge.u.x > edge.v.x)
                    dx *= -1;

                if (edge.u.y < edge.v.y)
                    dy *= -1;

                a = new Vertex(edge.u.x - dx, edge.u.y - dy);
                b = new Vertex(edge.u.x + dx, edge.u.y + dy);
                c = new Vertex(edge.v.x - dx, edge.v.y - dy);
                d = new Vertex(edge.v.x + dx, edge.v.y + dy);
            }

            public bool ContainsPoint(Vertex vertex)
            {
                float triangleAVC = new Triangle(a, vertex, d).Area;
                float triangleDVC = new Triangle(d, vertex, c).Area;
                float triangleBVD = new Triangle(b, vertex, d).Area;
                float triangleAVB = new Triangle(a, vertex, b).Area;

                return Area > triangleAVC + triangleDVC + triangleBVD + triangleAVB;
            }
        }

        public class Room
        {
            public int xPosition;
            public int yPosition;

            public int width;
            public int height;

            public Vertex Center => new(xPosition + (width / 2), yPosition + (height / 2));

            public Room() { }

            public Room(int xPosition, int yPosition, int width, int height)
            {
                this.xPosition = xPosition;
                this.yPosition = yPosition;

                this.width = width;
                this.height = height;
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

        public struct Rectangle
        {
            public float xPosition, yPosition, width, height;

            public Rectangle(float xPosition, float yPosition, float width, float height)
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

            public Vertex[] GetCornerVertices()
            {
                Vertex[] vertices = new Vertex[4];

                vertices[0] = new(xPosition, yPosition);
                vertices[1] = new(xPosition, yPosition + height);
                vertices[2] = new(xPosition + width, yPosition + height);
                vertices[3] = new(xPosition + width, height);

                return vertices;
            }
        }

        public class Light
        {
            public Vertex3D position;
            public float intensity;

            public Light() { }

            public Light(Vertex3D position, float intensity)
            {
                this.position = position;
                this.intensity = intensity;
            }
        }

        public class Dungeon
        {
            public readonly Vertex spawn;
            public readonly Pathfinding<Vertex> pathfinding;

            public readonly TileType[,] grid;
            public readonly GenerationSettings settings;
            public readonly HashSet<Edge> edges;
            public readonly List<Room> rooms;
            public readonly List<Light> lights;

            public Dungeon(TileType[,] grid, HashSet<Edge> edges, GenerationSettings settings, Random random, List<Room> rooms, List<Light> lights)
            {
                this.grid = grid;
                this.edges = edges;
                this.rooms = rooms;
                this.lights = lights;

                List<Vertex> vertices = new();
                foreach (Edge edge in edges)
                {
                    if (!vertices.Contains(edge.u))
                        vertices.Add(edge.u);
                    if (!vertices.Contains(edge.v))
                        vertices.Add(edge.v);
                }

                spawn = vertices[random.Next(0, vertices.Count - 1)];

                // Create nodes for floor tiles
                List<Node<Vertex>> nodes = new();

                Vertex intersection;
                foreach (Edge edge in edges)
                {
                    foreach (Room room in rooms)
                    {
                        foreach (Edge roomEdge in room.GetEdges())
                        {
                            intersection = edge.FindIntersection(roomEdge);
                            if (Approximately(intersection, Vertex.Invalid)) continue;

                            nodes.Add(new(intersection));
                        }
                    }
                }

                pathfinding = new(nodes);

                this.settings = settings;
            }

            public TileType GetTile(int x, int y)
            {
                if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1)) return TileType.Void;

                return grid[x, y];
            }

            public int GetWidth()
            {
                return settings.gridWidth;
            }

            public int GetHeight()
            {
                return settings.gridHeight;
            }
        }

        // ----------------------------
        //          Functions          
        // ----------------------------

        public static float Distance(Coordinate c1, Coordinate c2)
        {
            return MathF.Sqrt(MathF.Pow(c1.x - c2.x, 2) + MathF.Pow(c1.y - c2.y, 2));
        }

        public static float Distance(Vertex v1, Vertex v2)
        {
            return MathF.Sqrt(MathF.Pow(v1.x - v2.x, 2) + MathF.Pow(v1.y - v2.y, 2));
        }

        public static float Distance(object a, object b)
        {
            Type aType = a.GetType();
            Type bType = b.GetType();

            if (aType == typeof(Vertex) && bType == typeof(Vertex))
            {
                return Distance((Vertex)a, (Vertex)b);
            }

            if (aType == typeof(Coordinate) && bType == typeof(Coordinate))
            {
                return Distance((Coordinate)a, (Coordinate)b);
            }

            if (aType == typeof(Vertex) && bType == typeof(Coordinate))
            {
                Coordinate bCoord = (Coordinate)b;
                return Distance(a, new Vertex(bCoord.x, bCoord.y));
            }

            if (aType == typeof(Coordinate) && bType == typeof(Vertex))
            {
                Coordinate aCoord = (Coordinate)a;
                return Distance(new Vertex(aCoord.x, aCoord.y), b);
            }

            return -1;
        }

        public static float SquareDistance(Vertex v1, Vertex v2)
        {
            return MathF.Pow(v1.x - v2.x, 2) + MathF.Pow(v1.y - v2.y, 2);
        }

        public static bool Approximately(float a, float b)
        {
            return MathF.Abs(a - b) <= float.Epsilon * MathF.Abs(a + b) * 2 || MathF.Abs(a - b) < float.MinValue;
        }

        public static bool Approximately(Vertex a, Vertex b)
        {
            return Approximately(a.x, b.x) && Approximately(a.y, b.y);
        }

        public static bool Approximately(Vertex3D a, Vertex3D b)
        {
            return Approximately(a.x, b.x) && Approximately(a.y, b.y) && Approximately(a.z, b.z);
        }

        public static void IterateArea(int x1, int y1, int x2, int y2, Action<int, int> function)
        {
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    function(x, y);
                }
            }
        }

        public enum Extrema
        {
            xMinimum,
            yMinimum,
            xMaximum,
            yMaximum
        }

        public static float GetExtrema(List<Vertex> vertices, Extrema extremaType)
        {
            if (vertices == null) return float.MinValue;

            if (vertices.Count < 1) return float.MinValue;

            float greatestExtrema = float.MinValue;

            if (extremaType == Extrema.xMinimum || extremaType == Extrema.yMinimum)
                greatestExtrema = float.MaxValue;

            switch (extremaType)
            {
                case Extrema.xMinimum:
                    foreach (Vertex vertex in vertices)
                    {
                        if (vertex.x < greatestExtrema)
                            greatestExtrema = vertex.x;
                    }
                    break;
                case Extrema.yMinimum:
                    foreach (Vertex vertex in vertices)
                    {
                        if (vertex.y < greatestExtrema)
                            greatestExtrema = vertex.y;
                    }
                    break;
                case Extrema.xMaximum:
                    foreach (Vertex vertex in vertices)
                    {
                        if (vertex.x > greatestExtrema)
                            greatestExtrema = vertex.x;
                    }
                    break;
                case Extrema.yMaximum:
                    foreach (Vertex vertex in vertices)
                    {
                        if (vertex.y > greatestExtrema)
                            greatestExtrema = vertex.y;
                    }
                    break;
            }

            return greatestExtrema;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}