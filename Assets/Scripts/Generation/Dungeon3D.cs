using System.Collections;
using System.Collections.Generic;
using System;
using static Assets.Scripts.Generation.DungeonGeneration;

namespace Assets.Scripts.Generation
{
    public class Dungeon3D
    {
        public struct Vertex3D
        {
            public float x, y, z;

            public static Vertex3D Zero { get => new(0, 0, 0); }
            public static Vertex3D Invalid { get => new(float.MinValue, float.MinValue, float.MinValue); }

            public Vertex3D(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public static Vertex3D VectorToVertex(UnityEngine.Vector3 vector)
            {
                return new Vertex3D(vector.x, vector.y, vector.z);
            }

            public UnityEngine.Vector3 ToVector()
            {
                return new UnityEngine.Vector3(x, y, z);
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !GetType().Equals(obj.GetType()))
                    return false;

                Vertex3D vertex = (Vertex3D)obj;
                return vertex.x == x && vertex.y == y;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator ==(Vertex3D a, Vertex3D b)
            {
                return Approximately(a, b);
            }

            public static bool operator !=(Vertex3D a, Vertex3D b)
            {
                return !Approximately(a, b);
            }

            public static Vertex3D operator +(Vertex3D a, Vertex3D b)
            {
                return new Vertex3D(a.x + b.x, a.y + b.y, a.z + b.z);
            }

            public static Vertex3D operator -(Vertex3D a, Vertex3D b)
            {
                return new Vertex3D(a.x - b.x, a.y - b.y, a.z - b.z);
            }

            public static Vertex3D operator /(Vertex3D a, float f)
            {
                return new Vertex3D(a.x / f, a.y / f, a.z / f);
            }

            public static float operator *(Vertex3D a, Vertex3D b)
            {
                return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
            }
        }

        public class Edge3D
        {
            public Vertex3D u;
            public Vertex3D v;

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

            public Edge3D Normalized
            {
                get
                {
                    float divisor = MathF.Sqrt((v.x - u.x) + (v.y - u.y));
                    return new Edge3D(u, u + (v / divisor));
                }
            }

            public Edge3D(Vertex3D u, Vertex3D v)
            {
                this.u = u;
                this.v = v;
            }

            public static bool AlmostEqual(Edge3D left, Edge3D right)
            {
                return Approximately(left.u, right.u) && Approximately(left.v, right.v)
                    || Approximately(left.u, right.v) && Approximately(left.v, right.u);
            }

            public Vertex FindIntersection(Edge3D edge)
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

        public struct Rectangle3D
        {
            public List<Vertex3D> vertices;

            public Vertex3D Center => FindAverage(vertices.ToArray());
            public Vertex3D Normal
            {
                get
                {
                    Edge3D edgeAB = new(vertices[0], vertices[1]);
                    Edge3D edgeBC = new(vertices[1], vertices[2]);


                    Vertex3D product = FindCrossProduct(edgeAB, edgeBC);
                    return product / MathF.Sqrt((product.x * product.x) + (product.y * product.y) + (product.z * product.z));
                }
            }

            public Rectangle3D(Vertex3D position, float width, float height, float angle)
            {
                vertices = new();

                var tmpVerts = new Vertex3D[4];
                tmpVerts[0] = new(-width / 2, height / 2, 0);
                tmpVerts[1] = new(width / 2, height / 2, 0);
                tmpVerts[2] = new(width / 2, -height / 2, 0);
                tmpVerts[3] = new(-width / 2, -height / 2, 0);

                foreach (Vertex3D vertex in tmpVerts)
                {
                    float x = vertex.x * MathF.Cos(angle) + vertex.z * MathF.Sin(angle);
                    float y = vertex.y;
                    float z = -vertex.x * MathF.Sin(angle) + vertex.z * MathF.Cos(angle);

                    vertices.Add(new Vertex3D(x, y, z) + position);
                }
            }
        }

        public static Vertex3D FindAverage(Vertex3D[] vertices)
        {
            if (vertices == null) return default;
            if (vertices.Length < 1) return default;

            float x = 0, y = 0, z = 0;
            int count = vertices.Length;
            foreach (Vertex3D vertex in vertices)
            {
                x += vertex.x / count;
                y += vertex.y / count;
                z += vertex.z / count;
            }

            return new(x, y, z);
        }

        public static Vertex3D FindCrossProduct(Edge3D a, Edge3D b)
        {
            Vertex3D aVector = a.v - a.u;
            Vertex3D bVector = b.v - b.u;

            float x = aVector.y * bVector.z - aVector.z * bVector.y;
            float y = aVector.z * bVector.x - aVector.x * bVector.z;
            float z = aVector.x * bVector.y - aVector.y * bVector.x;

            return new(x, y, z);
        }
    }
}
