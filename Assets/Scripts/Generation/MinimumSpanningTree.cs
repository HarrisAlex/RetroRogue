using System.Collections.Generic;

namespace Assets.Scripts.Generation
{
    public class MinimumSpanningTree
    {
        public static List<MeasuredEdge> GetTree(List<MeasuredEdge> edges, Vertex start)
        {
            HashSet<Vertex> openSet = new HashSet<Vertex>();
            HashSet<Vertex> closedSet = new HashSet<Vertex>();

            foreach (MeasuredEdge edge in edges)
            {
                openSet.Add(edge.u);
                openSet.Add(edge.v);
            }

            closedSet.Add(start);

            List<MeasuredEdge> results = new List<MeasuredEdge>();

            while (openSet.Count > 0)
            {
                bool chosen = false;
                MeasuredEdge chosenEdge = null;
                float minWeight = float.PositiveInfinity;

                foreach (MeasuredEdge edge in edges)
                {
                    int closedVertices = 0;

                    if (!closedSet.Contains(edge.u))
                    {
                        closedVertices++;
                    }

                    if (!closedSet.Contains(edge.v))
                    {
                        closedVertices++;
                    }

                    if (closedVertices != 1)
                    {
                        continue;
                    }

                    if (edge.length < minWeight)
                    {
                        chosenEdge = edge;
                        chosen = true;
                        minWeight = edge.length;
                    }
                }

                if (!chosen)
                {
                    break;
                }

                results.Add(chosenEdge);
                openSet.Remove(chosenEdge.u);
                openSet.Remove(chosenEdge.v);
                closedSet.Add(chosenEdge.u);
                closedSet.Add(chosenEdge.v);
            }

            return results;
        }
    }
}