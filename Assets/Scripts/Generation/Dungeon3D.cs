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

            public UnityEngine.Vector3 ToVector(float yOffset = 0)
            {
                return new UnityEngine.Vector3(x, yOffset, y);
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
    }
}
