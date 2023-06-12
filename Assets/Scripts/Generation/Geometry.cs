using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Generation
{
    public struct Vertex
    {
        public float x;
        public float y;

        public static Vertex Zero { get { return new Vertex(0, 0); } }

        public Vertex(float x, float y)
        {
            this.x = x;
            this.y = y;
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
    }

    public class Edge
    {
        public Vertex u;
        public Vertex v;

        public bool isBad = false;

        public Edge(Vertex u, Vertex v)
        {
            this.u = u;
            this.v = v;
        }

        public static bool AlmostEqual(Edge left, Edge right)
        {
            return Math.Approximately(left.u, right.u) && Math.Approximately(left.v, right.v)
                || Math.Approximately(left.u, right.v) && Math.Approximately(left.v, right.u);
        }
    }

    public class MeasuredEdge : Edge
    {
        public float length;

        public MeasuredEdge(Edge edge) : base(edge.u, edge.v)
        {
            length = Math.Distance(u, v);
        }

        public MeasuredEdge(Vertex u, Vertex v) : base(u, v)
        {
            length = Math.Distance(u, v);
        }
    }

    class Triangle
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

            float aSquare = Math.SquareDistance(Vertex.Zero, a);
            float bSquare = Math.SquareDistance(Vertex.Zero, b);
            float cSquare = Math.SquareDistance(Vertex.Zero, c);

            float circleX = (aSquare * (b.y - c.y) + bSquare * (c.y - a.y) + cSquare * (a.y - b.y)) / diameter;

            float circleY = (aSquare * (c.x - b.x) + bSquare * (a.x - c.x) + cSquare * (b.x - a.x)) / diameter;


            // Check if vertex is inside circumcircle
            Vertex circleCenter = new Vertex(circleX, circleY);
            float circleRadius = Math.SquareDistance(a, circleCenter);
            float vertexDistance = Math.SquareDistance(vertex, circleCenter);

            return vertexDistance <= circleRadius;
        }

        public bool ContainsVertex(Vertex vertex)
        {
            return Math.Distance(vertex, a) < 0.01f
                || Math.Distance(vertex, b) < 0.01f
                || Math.Distance(vertex, c) < 0.01f;
        }
    }
}