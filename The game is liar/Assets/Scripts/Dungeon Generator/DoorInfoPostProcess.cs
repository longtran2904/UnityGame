using ProceduralLevelGenerator.Unity.Generators.Common;
using ProceduralLevelGenerator.Unity.Generators.DungeonGenerator.PipelineTasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Dungeon generator/DoorInfo", fileName = "DoorInfo")]
public class DoorInfoPostProcess : DungeonGeneratorPostProcessBase
{
    public override void Run(GeneratedLevel level, LevelDescription levelDescription)
    {
        RoomManager.instance.tilemap = level.GetSharedTilemaps()[2];
        RoomManager.instance.tilemap.RefreshAllTiles();
        RoomManager.instance.rooms = level.GetRoomInstances();
    }
}
