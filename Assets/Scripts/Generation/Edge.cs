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
            return MathUtils.Approximately(left.u, right.u) && MathUtils.Approximately(left.v, right.v)
                || MathUtils.Approximately(left.u, right.v) && MathUtils.Approximately(left.v, right.u);
        }
    }

    public class MeasuredEdge : Edge
    {
        public float length;

        public MeasuredEdge(Edge edge) : base(edge.u, edge.v)
        {
            length = MathUtils.Distance(u, v);
        }

        public MeasuredEdge(Vertex u, Vertex v) : base(u, v)
        {
            length = MathUtils.Distance(u, v);
        }
    }
}