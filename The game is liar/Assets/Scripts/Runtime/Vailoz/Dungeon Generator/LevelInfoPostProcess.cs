using Edgar.Unity;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dungeon generator/LevelInfo", fileName = "LevelInfo")]
public class LevelInfoPostProcess : DungeonGeneratorPostProcessBase
{
    public TileBase wallTile;
    public TileBase backgroundTile;
    public TileBase ruleTile;
    private TileBase[] tiles;

    public GameObject door;

    public override void Run(GeneratedLevel level, LevelDescription levelDescription)
    {
        List<RoomInstance> roomInstances = new List<RoomInstance>();
        tiles = new TileBase[256 * 256];
        foreach (RoomInstance room in level.GetRoomInstances())
        {
            roomInstances.Add(room);
            AddLevelMap(room, tiles, wallTile, backgroundTile);
            if (!room.RoomTemplateInstance.transform.FindChildWithLayer("Player"))
                AddDoor(room, door);
        }

        GameManager manager = FindObjectOfType<GameManager>();
        manager.InitTilemap(roomInstances, level.GetSharedTilemaps()[2].CompressAndRefresh(), ruleTile);
    }

    static void AddLevelMap(RoomInstance room, TileBase[] tiles, TileBase wallTile, TileBase backgroundTile)
    {
        Tilemap roomTilemap = GameManager.GetTilemapFromRoom(room.RoomTemplateInstance.transform).CompressAndRefresh();
        BoundsInt currentBounds = GameManager.GetBoundsFromTilemap(roomTilemap).ToBoundsInt();
        currentBounds.min += (Vector3Int)Vector2Int.one;
        currentBounds.max -= (Vector3Int)Vector2Int.one;
        List<Vector3Int> wallTiles = MathUtils.GetOutlinePoints(currentBounds);
        Tilemap tilemap = CreateGameObject(room.RoomTemplateInstance.transform.GetChild(0).transform,
            "Level Map", "LevelMap", true, typeof(Tilemap), typeof(TilemapRenderer)).GetComponent<Tilemap>();
        GameDebug.DrawBox(currentBounds.ToBounds().ToRect(), Color.cyan);

        foreach (DoorInstance door in room.Doors)
        {
            Vector3Int doorOrigin = room.Position - door.FacingDirection.ToVector3Int();
            Vector3Int doorFrom = doorOrigin + door.DoorLine.From;
            Vector3Int doorTo = doorOrigin + door.DoorLine.To;
            bool successFrom = wallTiles.Remove(doorFrom);
            bool successTo = wallTiles.Remove(doorTo);
            GameDebug.DrawBoxMinMax(doorFrom.ToVector2Int(), doorTo.ToVector2Int(), Color.red);
            Debug.Assert(successFrom, doorFrom + " " + door.FacingDirection + " " + room.RoomTemplateInstance.name);
            Debug.Assert(successTo, doorTo + " " + door.FacingDirection + " " + room.RoomTemplateInstance.name);
            Debug.Assert((door.DoorLine.From.x <= door.DoorLine.To.x) && (door.DoorLine.From.y <= door.DoorLine.To.y),
                "From: " + door.DoorLine.From + " To: " + door.DoorLine.To);

            Vector3Int dir = door.DoorLine.To - door.DoorLine.From;
            wallTiles.Add(room.Position + door.DoorLine.From - dir);
            wallTiles.Add(room.Position + door.DoorLine.To + dir);
        }
        tilemap.SetTiles(wallTiles.ToArray(), tiles.Populate(wallTile));

        currentBounds.min += (Vector3Int)Vector2Int.one;
        currentBounds.max -= (Vector3Int)Vector2Int.one;
        tilemap.SetTilesBlock(currentBounds, tiles.Populate(backgroundTile));

        // NOTE: There will be some duplications with the previous room's doors, but I don't care.
        foreach (DoorInstance door in room.Doors)
        {
            foreach (var doorTilePos in door.DoorLine.GetPoints())
            {
                tilemap.SetTile(room.Position + doorTilePos, backgroundTile);
                tilemap.SetTile(room.Position + doorTilePos - door.FacingDirection.ToVector3Int(), backgroundTile);
            }
        }

        tilemap.transform.position = Vector3.zero;
        tilemap.gameObject.SetActive(false);
    }

    static void AddDoor(RoomInstance room, GameObject doorObj)
    {
        Transform doorHolder = CreateGameObject(room.RoomTemplateInstance.transform, "Doors", null, false).transform;
        foreach (DoorInstance door in room.Doors)
        {
            Vector3 pos = room.Position;
            pos += (Vector3)MathUtils.Average((Vector3)door.DoorLine.From, (Vector3)door.DoorLine.To); // Center
            pos += (door.DoorLine.GetDirectionVector() + (door.IsHorizontal ? Vector3.up : Vector3.right)) * .5f; // Add .5 offset
            pos += -door.FacingDirection.ToVector3Int(); // Move 1 unit before the door's position

            Quaternion rot = door.FacingDirection.x == 0 ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
            Instantiate(doorObj, pos, rot, doorHolder);
        }
    }

    static GameObject CreateGameObject(Transform parent, string name, string layer, bool active, params System.Type[] components)
    {
        GameObject obj = new GameObject(name, components);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        if (layer != null && layer != "")
            obj.layer = LayerMask.NameToLayer(layer);
        obj.SetActive(active);

        return obj;
    }
}