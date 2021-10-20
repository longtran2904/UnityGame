using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Minimap : MonoBehaviour
{
    [HideInInspector] public Tilemap tilemap;
    public TileBase wallTile;
    public TileBase backgroundTile;
    public TileBase emptyTile;
    public RoomInstanceVariable startingRoom;

    private TileBase[] tiles;

    void Start()
    {
        if (tilemap)
        {
            tiles = new TileBase[256 * 256];
            AddRoomToLevelMap(startingRoom);
        }
    }

    public void AddRoomToLevelMap(RoomInstanceVariable room)
    {
        BoundsInt currentBounds = EdgarHelper.GetRoomBoundsInt(room.value);

        currentBounds.min += Vector3Int.one;
        currentBounds.max -= Vector3Int.one;
        List<Vector3Int> wallTiles = MathUtils.GetOutlinePoints(currentBounds);
        foreach (var door in room.value.Doors)
        {
            Debug.Assert(wallTiles.Remove(room.value.Position + door.DoorLine.From - door.FacingDirection.ToVector3Int()));
            Debug.Assert(wallTiles.Remove(room.value.Position + door.DoorLine.To - door.FacingDirection.ToVector3Int()));
            Debug.Assert((door.DoorLine.From.x <= door.DoorLine.To.x) && (door.DoorLine.From.y <= door.DoorLine.To.y), "From: " + door.DoorLine.From + " To: " + door.DoorLine.To);

            Vector3Int dir = door.DoorLine.To - door.DoorLine.From;
            wallTiles.Add(room.value.Position + door.DoorLine.From - dir);
            wallTiles.Add(room.value.Position + door.DoorLine.To + dir);
        }
        tilemap.SetTiles(wallTiles.ToArray(), tiles.Populate(wallTile));

        currentBounds.min += Vector3Int.one;
        currentBounds.max -= Vector3Int.one;
        tilemap.SetTilesBlock(currentBounds, tiles.Populate(backgroundTile));

        // NOTE: There will be some duplications with the previous room's doors, but I don't care.
        foreach (var door in room.value.Doors)
        {
            foreach (var doorTilePos in door.DoorLine.GetPoints())
            {
                tilemap.SetTile(room.value.Position + doorTilePos, backgroundTile);
                tilemap.SetTile(room.value.Position + doorTilePos - door.FacingDirection.ToVector3Int(), backgroundTile);
            }
        }
    }
}
