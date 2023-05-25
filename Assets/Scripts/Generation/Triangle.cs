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

            float aSquare = MathUtils.SquareDistance(Vertex.zero, a);
            float bSquare = MathUtils.SquareDistance(Vertex.zero, b);
            float cSquare = MathUtils.SquareDistance(Vertex.zero, c);

            float circleX = (aSquare * (b.y - c.y) + bSquare * (c.y - a.y) + cSquare * (a.y - b.y)) / diameter;

            float circleY = (aSquare * (c.x - b.x) + bSquare * (a.x - c.x) + cSquare * (b.x - a.x)) / diameter;


            // Check if vertex is inside circumcircle
            Vertex circleCenter = new Vertex(circleX, circleY);
            float circleRadius = MathUtils.SquareDistance(a, circleCenter);
            float vertexDistance = MathUtils.SquareDistance(vertex, circleCenter);

            return vertexDistance <= circleRadius;
        }

        public bool ContainsVertex(Vertex vertex)
        {
            return MathUtils.Distance(vertex, a) < 0.01f
                || MathUtils.Distance(vertex, b) < 0.01f
                || MathUtils.Distance(vertex, c) < 0.01f;
        }
    }
}