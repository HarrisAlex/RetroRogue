using System;

namespace Assets.Scripts.Generation
{
    public class MathUtils
    {
        public static float Distance(Vertex v1, Vertex v2)
        {
            return MathF.Sqrt(MathF.Pow(v1.x - v2.x, 2) + MathF.Pow(v1.y - v2.y, 2));
        }

        public static float SquareDistance(Vertex v1, Vertex v2)
        {
            return MathF.Pow(v1.x - v2.x, 2) + MathF.Pow(v1.y - v2.y, 2);
        }

        public static bool Approximately(float a, float b)
        {
            return MathF.Abs(a - b) <= float.Epsilon * MathF.Abs(a + b) * 2 || MathF.Abs(a - b) < float.MinValue;
        }

        public static bool Approximately(Vertex a, Vertex b)
        {
            return Approximately(a.x, b.x) && Approximately(a.y, b.y);
        }
    }
}