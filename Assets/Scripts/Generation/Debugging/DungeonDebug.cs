using UnityEngine;
using static Assets.Scripts.Generation.DungeonGeneration;

namespace Assets.Scripts.Generation
{
    public class DungeonDebug
    {
        public static void DrawTriangle(object triangle, float time, Color color)
        {
            if (triangle.GetType() != typeof(Triangle))
                return;

            Triangle tmpTriangle = (Triangle)triangle;

            Debug.DrawLine(new Vector3(tmpTriangle.a.x, 0, tmpTriangle.a.y), new Vector3(tmpTriangle.b.x, 0, tmpTriangle.b.y), color, time);
            Debug.DrawLine(new Vector3(tmpTriangle.b.x, 0, tmpTriangle.b.y), new Vector3(tmpTriangle.c.x, 0, tmpTriangle.c.y), color, time);
            Debug.DrawLine(new Vector3(tmpTriangle.c.x, 0, tmpTriangle.c.y), new Vector3(tmpTriangle.a.x, 0, tmpTriangle.a.y), color, time);
        }

        public static void DrawEdge(object edge, float time, Color color)
        {
            if (edge.GetType() != typeof(Edge))
                return;

            Edge tmpEdge = (Edge)edge;

            Debug.DrawLine(new Vector3(tmpEdge.u.x, 0, tmpEdge.u.y), new Vector3(tmpEdge.v.x, 0, tmpEdge.v.y), color, time);
        }

        public static void DrawRoom(object room, float time, Color color)
        {
            if (room.GetType() != typeof(Room))
                return;

            Room tmpRoom = (Room)room;

            Vector3 topLeft = new Vector3(tmpRoom.xPosition, 0, tmpRoom.yPosition + tmpRoom.height);
            Vector3 topRight = new Vector3(tmpRoom.xPosition + tmpRoom.width, 0, tmpRoom.yPosition + tmpRoom.height);
            Vector3 bottomLeft = new Vector3(tmpRoom.xPosition, 0, tmpRoom.yPosition);
            Vector3 bottomRight = new Vector3(tmpRoom.xPosition + tmpRoom.width, 0, tmpRoom.yPosition);

            Debug.DrawLine(topLeft, topRight, color, time);
            Debug.DrawLine(topRight, bottomRight, color, time);
            Debug.DrawLine(bottomRight, bottomLeft, color, time);
            Debug.DrawLine(bottomLeft, topLeft, color, time);
        }
    }
}