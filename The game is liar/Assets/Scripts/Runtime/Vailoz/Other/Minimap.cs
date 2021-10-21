using Edgar.Unity;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Collections.Generic;

public class Minimap : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase wallTile;
    public TileBase backgroundTile;
    public RoomInstanceVariable startingRoom;

    void Start()
    {
        AddRoomToLevelMap(startingRoom.value);
    }

    public void UnlockRoomInLevlMap(RoomInstanceVariable room)
    {
        AddRoomToLevelMap(room.value);
    }

    /*
     * Background:
     * First resize the bounds to not contain the door lines (horizontal line if the door's facing dir is left or right and vertical when it's up or down).
     * Then copy all the level background tiles that fill the resize bounds.
     * 
     * Wall:
     * First resize all the outline wall tiles to the resized bounds from the background function (Only draw the outline tiles to the minimap)
     * Then remove wall tiles at door position
     * Finally Add surrounding wall tiles around door position
     */
    void AddRoomToLevelMap(RoomInstance room)
    {
        BoundsInt bounds = EdgarHelper.GetRoomBoundsInt(room);
        SetBackgroundTiles(room, ref bounds);
        SetWallTiles(room, bounds);
    }

    // Also resize the bounds so take in as ref
    void SetBackgroundTiles(RoomInstance room, ref BoundsInt bounds)
    {
        BoundsInt startBounds = bounds;
        List<Vector3Int> corridorTiles = new List<Vector3Int>();

        foreach (var door in room.Doors)
        {
            // Add 2 background tiles at door position
            corridorTiles.Add(door.DoorLine.From + room.Position);
            corridorTiles.Add(door.DoorLine.To + room.Position);

            // Resize the bounds
            if (door.FacingDirection == Vector2Int.left && bounds.min.x == startBounds.min.x)
            {
                bounds.min += Vector3Int.right;
            }
            else if (door.FacingDirection == Vector2Int.right && bounds.max.x == startBounds.max.x)
            {
                bounds.max += Vector3Int.left;
            }
            else if (door.FacingDirection == Vector2Int.down && bounds.min.y == startBounds.min.y)
            {
                bounds.min += Vector3Int.up;
            }
            else if (bounds.max.y == startBounds.max.y)
            {
                bounds.max += Vector3Int.down;
            }
        }

        List<Vector3Int> backgroundPos = new List<Vector3Int>(bounds.size.x * bounds.size.y);
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                backgroundPos.Add(new Vector3Int(x, y, 0) + bounds.position);
            }
        }
        backgroundPos.AddRange(corridorTiles);

        tilemap.SetTiles(backgroundPos.ToArray(), new TileBase[backgroundPos.Count].Populate(backgroundTile));
    }

    void SetWallTiles(RoomInstance room, BoundsInt bounds)
    {
        List<Vector2Int> wallsPos = room.OutlinePolygon.GetOutlinePoints().ToList();
        List<Vector2Int> corridorWalls = new List<Vector2Int>();

        // Resize the outline wall tiles to the bounds
        for (int i = 0; i < wallsPos.Count; i++)
        {
            if (wallsPos[i].x < bounds.min.x)
            {
                wallsPos[i] += Vector2Int.right;
            }
            else if (wallsPos[i].y < bounds.min.y)
            {
                wallsPos[i] += Vector2Int.up;
            }
            else if (wallsPos[i].x > bounds.max.x - 1)
            {
                wallsPos[i] += Vector2Int.left;
            }
            else if (wallsPos[i].y > bounds.max.y - 1)
            {
                wallsPos[i] += Vector2Int.down;
            }
        }

        foreach (var door in room.Doors)
        {
            Vector2Int from = door.DoorLine.From.ToVector2Int() + room.Position.ToVector2Int();
            Vector2Int to = door.DoorLine.To.ToVector2Int() + room.Position.ToVector2Int();

            // Remove wall tiles next to corridor's entrance
            InternalDebug.Log(wallsPos.Remove(from - door.FacingDirection) + " " + (from - door.FacingDirection));
            InternalDebug.Log(wallsPos.Remove(to - door.FacingDirection) + " " + (to - door.FacingDirection));

            if (door.FacingDirection == Vector2Int.left || door.FacingDirection == Vector2Int.right)
            {
                // Add surrounding wall tiles above and below
                corridorWalls.Add(from + Vector2Int.down);
                corridorWalls.Add(to + Vector2Int.up);
            }
            else
            {
                // Add surrounding wall tiles left and right
                corridorWalls.Add(from + Vector2Int.left);
                corridorWalls.Add(to + Vector2Int.right);
            }
        }
        wallsPos.AddRange(corridorWalls);

        tilemap.SetTiles(wallsPos.ToArray().ToVector3Int(), new TileBase[wallsPos.ToArray().Length].Populate(wallTile));
    }
}
