namespace Assets.Scripts.Generation
{
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

            float aSquare = Math.SquareDistance(Vertex.zero, a);
            float bSquare = Math.SquareDistance(Vertex.zero, b);
            float cSquare = Math.SquareDistance(Vertex.zero, c);

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