using Edgar.Unity;
using UnityEngine;

public static class EdgarHelper
{
    public static BoundsInt GetRoomBoundsInt(RoomInstance room)
    {
        // The points' order are clockwide but the starting point's position is random
        Vector2Int[] points = room.OutlinePolygon.GetPoints().ToArray();

        // Bottom left
        int startIndex = 0;

        if (points[0].x - points[1].x > 0) // Bottom right
        {
            startIndex = 1;
        }
        else if (points[0].x - points[1].x < 0) // Upper left
        {
            startIndex = 3;
        }
        else if (points[0].y - points[1].y > 0) // Upper right
        {
            startIndex = 2;
        }

        int opositeIndex = (int)Mathf.Repeat(startIndex + 2, 4);

        return MathUtils.CreateBoundsInt(points[startIndex], points[opositeIndex] + Vector2Int.one);
    }
}
