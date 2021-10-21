using Edgar.Unity;
using Edgar.Geometry;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public static List<DoorInstance> GetUnusedDoors(RoomInstance roomInstance)
    {
        var doorMode = roomInstance.RoomTemplateInstance.GetComponent<Doors>().GetDoorMode();

        var polygon = RoomTemplatesLoader.GetPolygonFromRoomTemplate(roomInstance.RoomTemplateInstance);
        var doors = doorMode
            .GetDoors(polygon)
            .SelectMany(x =>
                x.Line
                    .GetPoints()
                    .Select(point =>
                        new OrthogonalLineGrid2D(point, point + x.Length * x.Line.GetDirectionVector(), x.Line.GetDirection()
            ))).ToList();
        var unusedDoors = doors.ToList();

        foreach (var door in roomInstance.Doors)
        {
            var from = door.DoorLine.From;
            var to = door.DoorLine.To;

            foreach (var otherDoor in unusedDoors.ToList())
            {
                var otherFrom = otherDoor.From.ToUnityIntVector3();
                var otherTo = otherDoor.To.ToUnityIntVector3();

                if ((from == otherFrom && to == otherTo) || (from == otherTo && to == otherFrom))
                {
                    unusedDoors.Remove(otherDoor);
                }
            }
        }

        var result = new List<DoorInstance>();

        foreach (var door in unusedDoors)
        {
            result.Add(CreateDoor(door));
        }

        return result;
    }

    public static DoorInstance CreateDoor(OrthogonalLineGrid2D doorLine)
    {
        switch (doorLine.GetDirection())
        {
            case OrthogonalLineGrid2D.Direction.Right:
                return new DoorInstance(new OrthogonalLine(doorLine.From.ToUnityIntVector3(), doorLine.To.ToUnityIntVector3()), Vector2Int.up,
                    null, null);

            case OrthogonalLineGrid2D.Direction.Left:
                return new DoorInstance(new OrthogonalLine(doorLine.To.ToUnityIntVector3(), doorLine.From.ToUnityIntVector3()), Vector2Int.down,
                    null, null);

            case OrthogonalLineGrid2D.Direction.Top:
                return new DoorInstance(new OrthogonalLine(doorLine.From.ToUnityIntVector3(), doorLine.To.ToUnityIntVector3()), Vector2Int.left,
                    null, null);

            case OrthogonalLineGrid2D.Direction.Bottom:
                return new DoorInstance(new OrthogonalLine(doorLine.To.ToUnityIntVector3(), doorLine.From.ToUnityIntVector3()), Vector2Int.right,
                    null, null);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
