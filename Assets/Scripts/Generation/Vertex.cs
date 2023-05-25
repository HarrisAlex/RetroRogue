using System;

namespace Assets.Scripts.Generation
{
    public class Vertex
    {
        public float x;
        public float y;

        public static Vertex zero { get { return new Vertex(0, 0); } }

        public Vertex(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}