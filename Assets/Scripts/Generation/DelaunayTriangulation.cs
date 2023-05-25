using System.Collections.Generic;
using System;

namespace Assets.Scripts.Generation
{
    public class DelaunayTriangulation
    {
        private static List<Triangle> triangles = new List<Triangle>();

        public static List<Edge> Triangulate(List<Vertex> vertices)
        {
            triangles = new List<Triangle>();

            List<Edge> results = new List<Edge>();

            float minX = vertices[0].x;
            float minY = vertices[0].y;
            float maxX = minX;
            float maxY = minY;

            // Get bounding box for vertices
            foreach (Vertex vertex in vertices)
            {
                if (vertex.x < minX)
                {
                    minX = vertex.x;
                }

                if (vertex.y < minY)
                {
                    minY = vertex.y;
                }

                if (vertex.x > maxX)
                {
                    maxX = vertex.x;
                }

                if (vertex.y > maxY)
                {
                    maxY = vertex.y;
                }
            }

            float deltaX = maxX - minX;
            float deltaY = maxY - minY;
            float deltaMax = MathF.Max(deltaX, deltaY);

            // Create supra-triangle
            Vertex v1 = new Vertex(minX - 1, minY - 1);
            Vertex v2 = new Vertex(minX - 1, maxY + deltaMax);
            Vertex v3 = new Vertex(maxX + deltaMax, minY - 1);

            triangles.Add(new Triangle(v1, v2, v3));

            foreach (Vertex vertex in vertices)
            {
                List<Edge> polygon = new List<Edge>();


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

                polygon.RemoveAll((Edge edge) => edge.isBad);

                foreach (Edge edge in polygon)
                {
                    triangles.Add(new Triangle(edge.u, edge.v, vertex));
                }
            }

            triangles.RemoveAll((Triangle triangle) => triangle.ContainsVertex(v1)
                || triangle.ContainsVertex(v2)
                || triangle.ContainsVertex(v3));

            HashSet<Edge> edgeSet = new HashSet<Edge>();

            foreach (Triangle triangle in triangles)
            {
                Edge ab = new Edge(triangle.a, triangle.b);
                Edge bc = new Edge(triangle.b, triangle.c);
                Edge ca = new Edge(triangle.c, triangle.a);

                if (edgeSet.Add(ab))
                {
                    results.Add(ab);
                }

                if (edgeSet.Add(bc))
                {
                    results.Add(bc);
                }

                if (edgeSet.Add(ca))
                {
                    results.Add(ca);
                }
            }

            return results;
        }
    }
}