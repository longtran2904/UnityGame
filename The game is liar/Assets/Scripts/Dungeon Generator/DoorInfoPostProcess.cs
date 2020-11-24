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
        RoomManager.tilemap = tilemap;

        foreach (RoomInstance room in level.GetRoomInstances())
        {
            if (room.IsCorridor)
            {
                continue;
            }

            RoomManager.rooms.Add(room);
        }

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] tiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                if (tiles[x + y * bounds.size.x])
                {
                    RoomManager.allGroundTiles.Add(new Vector2Int(x, y));
                }                
            }
        }
    }
}
