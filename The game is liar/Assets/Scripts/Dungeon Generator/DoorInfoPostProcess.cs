using ProceduralLevelGenerator.Unity.Generators.Common;
using ProceduralLevelGenerator.Unity.Generators.Common.Rooms;
using ProceduralLevelGenerator.Unity.Generators.DungeonGenerator.PipelineTasks;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Dungeon generator/DoorInfo", fileName = "DoorInfo")]
public class DoorInfoPostProcess : DungeonGeneratorPostProcessBase
{
    public override void Run(GeneratedLevel level, LevelDescription levelDescription)
    {
        Tilemap tilemap = level.GetSharedTilemaps()[2];
        tilemap.RefreshAllTiles();
        RoomManager.instance.tilemap = tilemap;

        foreach (RoomInstance room in level.GetRoomInstances())
        {
            if (room.IsCorridor)
            {
                continue;
            }
            Vector2Int[] points = room.OutlinePolygon.GetOutlinePoints().ToArray();  // The points' order are clockwide but the starting point's position is random
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

            Bounds roomBounds = MathUtils.CreateBounds(points[startIndex], points[opositeIndex] + Vector2Int.one);
            RoomManager.instance.rooms.Add(room, roomBounds);
        }

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] tiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                if (tiles[x + y * bounds.size.x])
                {
                    RoomManager.instance.allGroundTiles.Add(new Vector2Int(x, y));
                }                
            }
        }
    }
}
