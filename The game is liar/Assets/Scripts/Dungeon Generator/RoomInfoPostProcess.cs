using ProceduralLevelGenerator.Unity.Generators.Common;
using ProceduralLevelGenerator.Unity.Generators.DungeonGenerator.PipelineTasks;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Dungeon generator/RoomInfo", fileName = "RoomInfo")]
public class RoomInfoPostProcess : DungeonGeneratorPostProcessBase
{
    public override void Run(GeneratedLevel level, LevelDescription levelDescription)
    {
        Tilemap tilemap = level.GetSharedTilemaps()[2];
        Minimap.instance.bounds = tilemap.cellBounds;
        Minimap.instance.tilesDictionary.Clear();
        foreach (var room in level.GetRoomInstances())
        {
            if (room.IsCorridor)
            {
                continue;
            }
            BoundsInt bounds = MathUtils.CreateBoundsInt(room.OutlinePolygon.GetOutlinePoints()[0], room.OutlinePolygon.GetOutlinePoints()[2]);
            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x + bounds.position.x - tilemap.cellBounds.position.x, y + bounds.position.y - tilemap.cellBounds.position.y);
                    if (pos.x < 0 || pos.y < 0) InternalDebug.LogError(pos);
                    bool isWall = false;
                    bool canShow = false;
                    bool isBossRoom = false;

                    if (room.RoomTemplatePrefab.name == "Starting Room")
                    {
                        canShow = true;
                    }
                    if (x == 0 || x == bounds.size.x - 1 || y == 0 || y == bounds.size.y - 1)
                    {
                        isWall = true;
                    }
                    if (room.RoomTemplatePrefab.name == "Boss Room")
                    {
                        isBossRoom = true;
                    }
                    // TODO: When publish game have try catch and reload scene + send a bug report if catch a "already has key" exception (But I don't think this bug will appear again)
                    Minimap.instance.tilesDictionary.Add(pos, new[] { isWall, canShow, isBossRoom });
                }
            }
        }
        Minimap.instance.SetupTexture();
        Minimap.instance.CreateTexture();
    }
}
