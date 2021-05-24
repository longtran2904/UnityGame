using Edgar.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    public static List<RoomInstance> rooms = new List<RoomInstance>();
    public static List<Vector2Int> allGroundTiles = new List<Vector2Int>();
    public static Tilemap tilemap; // the tilemap of the current level

    public GameEvent updateCurrentRoom;
    public RoomInstanceVariable currentRoom;
    public BoundsIntVariable currentBoundsInt;

    public const string startingRoom = "Starting Room";
    public TileBase doorTile;
    public TileBase normalTile;

    // Start is called before the first frame update
    void Start()
    {
        List<Vector3Int> removeTilesPos = new List<Vector3Int>();
        List<Vector3Int> doorTilesPos   = new List<Vector3Int>();
        List<Vector3Int> wallTilesPos   = new List<Vector3Int>();

        // Find starting room and remove all door tiles
        foreach (var room in rooms)
        {
            if (room.RoomTemplatePrefab.name == startingRoom)
                currentRoom.value = room;

            Tilemap wallTilemap = room.RoomTemplateInstance.transform.Find("tilemaps").GetChild(2).GetComponent<Tilemap>();
            wallTilemap.CompressBounds();
            BoundsInt roomBounds = wallTilemap.cellBounds;

            HandleTiles(room.Doors, null, pos => removeTilesPos.Add(pos));
            HandleTiles(EdgarHelper.GetUnusedDoors(room), pos => doorTilesPos.Add(pos), pos => wallTilesPos.Add(pos));

            /// <summary>
            /// Check to see if there are any tiles that will be set/removed when a door is opened/closed.
            /// </summary>
            /// <param name="doorPosFunc">
            /// function to be executed if the current tile position is a special tile and at door position.
            /// </param>
            /// <param name="otherPosFunc">
            /// function to be executed if the current tile position is a special tile and not at door position.
            /// </param>
            void HandleTiles(List<DoorInstance> doors, System.Action<Vector3Int> doorPosFunc, System.Action<Vector3Int> otherPosFunc)
            {
                foreach (var door in doors)
                {
                    TileBase tileAtDoor = tilemap.GetTile(door.DoorLine.From);
                    if (tileAtDoor != doorTile)
                    {
                        for (int x = roomBounds.xMin; x <= roomBounds.xMax; x++)
                        {
                            for (int y = roomBounds.yMin; y <= roomBounds.yMax; y++)
                            {
                                Vector3Int pos = new Vector3Int(x, y, 0);
                                if (pos == door.DoorLine.From || pos == door.DoorLine.To)
                                {
                                    doorPosFunc?.Invoke(pos);
                                }
                                else if (tilemap.GetTile(pos) == tileAtDoor)
                                {
                                    otherPosFunc?.Invoke(pos);
                                }
                            }
                        }
                    }
                }
            }

            Vector3Int removeDir = Vector3Int.right;
            foreach (var door in room.Doors)
            {
                if (door.ConnectedRoomInstance != null)
                {
                    removeDir = -(Vector3Int)door.FacingDirection;
                    Remove(door.DoorLine.From + room.Position + removeDir);
                    Remove(door.DoorLine.To + room.Position + removeDir);
                }
            }

            void Remove(Vector3Int pos)
            {
                if (tilemap.GetTile(pos))
                {
                    removeTilesPos.Add(pos);
                    Remove(pos + removeDir);
                }
            }
        }

        tilemap.SetTiles(doorTilesPos.ToArray(), new TileBase[doorTilesPos.Count].Populate(doorTile));
        tilemap.SetTiles(removeTilesPos.ToArray(), null);
        tilemap.SetTiles(wallTilesPos.ToArray(), new TileBase[wallTilesPos.Count].Populate(normalTile));
        tilemap.RefreshAllTiles();
        currentBoundsInt.value = EdgarHelper.GetRoomBoundsInt(currentRoom.value);
    }

    void Update()
    {
        foreach (var room in rooms)
        {
            Bounds bounds = EdgarHelper.GetRoomBoundsInt(room).ToBounds();
            ExtDebug.DrawBox(bounds.center, bounds.extents, Quaternion.identity, Color.cyan);
        }
    }

    // Called by the custom Game Event
    public void LockRoom()
    {
        EnemySpawner currentSpawner = currentRoom.value.RoomTemplateInstance.transform.Find("Enemies")?.GetComponent<EnemySpawner>();
        if (currentSpawner) currentSpawner.enabled = true;
        else return;

        foreach (var doorInstance in currentRoom.value.Doors)
        {
            Door[] doors = doorInstance.ConnectedRoomInstance.RoomTemplateInstance.transform.Find("Doors").GetComponentsInChildren<Door>(true);
            if (doors.Length > 0)
            {
                if (doors.Length == 1)
                {
                    doors[0].gameObject.SetActive(true);
                    continue;
                }
                int nearest = 0;
                Vector3 roomCenter = EdgarHelper.GetRoomBoundsInt(currentRoom.value).center;
                if (Mathf.Abs(roomCenter.x - doors[0].transform.position.x) > Mathf.Abs(roomCenter.x - doors[1].transform.position.x))
                {
                    nearest = 1;
                }
                doors[nearest].gameObject.SetActive(true);
            }
        }
    }

    // Called by the custom Game Event
    public void UnlockRoom()
    {
        foreach (var doorInstance in currentRoom.value.Doors)
        {
            Door door = doorInstance.ConnectedRoomInstance.RoomTemplateInstance.transform.Find("Doors")?.GetComponentInChildren<Door>();
            if (door)
                door.canOpen = true;
        }
    }
}
