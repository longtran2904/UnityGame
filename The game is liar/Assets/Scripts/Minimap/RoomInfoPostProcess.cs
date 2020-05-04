using Assets.ProceduralLevelGenerator.Scripts.Generators.Common;
using Assets.ProceduralLevelGenerator.Scripts.Generators.DungeonGenerator.PipelineTasks;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Dungeon generator/RoomInfo", fileName = "RoomInfo")]
public class RoomInfoPostProcess : DungeonGeneratorPostProcessBase
{
    public override void Run(GeneratedLevel level, LevelDescription levelDescription)
    {
        Tilemap tilemap = level.GetSharedTilemaps()[2];

        Minimap.instance.bounds = tilemap.cellBounds;

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

                    bool isWall = false;
                    bool canShow = false;

                    if (room.RoomTemplateInstance.name == "Starting Room(Clone)")
                    {
                        canShow = true;
                    }

                    if (x == 0 || x == bounds.size.x - 1 || y == 0 || y == bounds.size.y - 1)
                    {
                        isWall = true;
                    }

                    Minimap.instance.tilesDictionary.Add(pos, new[] { isWall, canShow });
                }
            }
        }

        Minimap.instance.CreateTexture();
    }
}
