using Assets.ProceduralLevelGenerator.Scripts.Generators.Common;
using Assets.ProceduralLevelGenerator.Scripts.Generators.DungeonGenerator.PipelineTasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon generator/CameraInfo", fileName = "CameraInfo")]
public class CameraInfoPostProcess : DungeonGeneratorPostProcessBase
{
    private CameraInfo[] info;
    private Bounds[] roomBounds;
    private Transform[] roomsPos;

    public override void Run(GeneratedLevel level, LevelDescription levelDescription)
    {
        info = new CameraInfo[level.GetRoomInstances().Count];

        roomBounds = new Bounds[level.GetRoomInstances().Count];

        roomsPos = new Transform[level.GetRoomInstances().Count];

        int x = 0;

        foreach (var roomInstance in level.GetRoomInstances())
        {
            if (roomInstance.IsCorridor)
            {
                continue;
            }

            info[x] = roomInstance.RoomTemplateInstance.transform.Find("Camera Info").GetComponent<CameraInfo>();

            roomBounds[x] = MathUtils.CreateBounds(roomInstance.OutlinePolygon.GetOutlinePoints()[0], roomInstance.OutlinePolygon.GetOutlinePoints()[2]);

            roomsPos[x] = roomInstance.RoomTemplateInstance.transform;

            x++;
        }

        CameraFollow2D camera = GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<CameraFollow2D>();

        camera.cameraInfos = info;
        camera.roomsBounds = roomBounds;
        camera.roomsPos = roomsPos;
    }
}
