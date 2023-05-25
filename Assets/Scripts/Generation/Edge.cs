using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Generation
{
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
}