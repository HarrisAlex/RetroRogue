using System;

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
            public static Vertex NegativeInfinity { get => new(float.MinValue, float.MinValue); }

            public Vertex(float x, float y)
            {
                this.x = x;
                this.y = y;
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
        }

        public class Triangle
        {
            public Vertex a;
            public Vertex b;
            public Vertex c;

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
    }
}