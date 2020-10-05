using ProceduralLevelGenerator.Unity.Generators.Common;
using ProceduralLevelGenerator.Unity.Generators.DungeonGenerator.PipelineTasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon generator/CameraInfo", fileName = "CameraInfo")]
public class CameraInfoPostProcess : DungeonGeneratorPostProcessBase
{
    private Bounds[] roomBounds;

    public override void Run(GeneratedLevel level, LevelDescription levelDescription)
    {
        roomBounds = new Bounds[level.GetRoomInstances().Count];
        int x = 0;
        foreach (var roomInstance in level.GetRoomInstances())
        {
            if (roomInstance.IsCorridor)
            {
                continue;
            }
            Vector2Int[] points = roomInstance.OutlinePolygon.GetOutlinePoints().ToArray();  // The points' order are clockwide but the starting point's position is random

            // Bottom left
            int startIndex = 0;
            int opositeIndex = 2;
            if (points[0].x - points[1].x > 0) // Bottom right
            {
                startIndex = 1;
                opositeIndex = 3;
            }
            else if (points[0].x - points[1].x < 0) // Upper left
            {
                startIndex = 3;
                opositeIndex = 1;
            }
            else if (points[0].y - points[1].y > 0) // Upper right
            {
                startIndex = 2;
                opositeIndex = 0;
            }

            roomBounds[x] = MathUtils.CreateBounds(points[startIndex], points[opositeIndex] + Vector2Int.one); // Start from bottom left to upper right
            x++;
        }
        CameraFollow2D camera = GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<CameraFollow2D>();
        camera.roomsBounds = roomBounds;
    }
}
