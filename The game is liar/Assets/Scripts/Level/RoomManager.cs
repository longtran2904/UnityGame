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

    // Start is called before the first frame update
    void Start()
    {
        // Find starting room and remove all door blocks
        foreach (var room in rooms)
        {
            if (room.RoomTemplatePrefab.name == startingRoom)
            {
                currentRoom.value = room;
            }
            RemoveBlockTile(room);
        }
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

    public void UnLockRoom()
    {
        foreach (var doorInstance in currentRoom.value.Doors)
        {
            Door door = doorInstance.ConnectedRoomInstance.RoomTemplateInstance.transform.Find("Doors")?.GetComponentInChildren<Door>();
            if (door)
                door.canOpen = true;
        }
    }

    void RemoveBlockTile(RoomInstance room)
    {
        Vector2Int removeDir = Vector2Int.right;

        if (room == null)
        {
            return;
        }

        foreach (var door in room.Doors)
        {
            if (door.ConnectedRoomInstance != null)
            {
                removeDir = -door.FacingDirection;
                RemoveTile((Vector2Int)(door.DoorLine.From + room.Position) + removeDir);
                RemoveTile((Vector2Int)(door.DoorLine.To + room.Position) + removeDir);
            }
        }

        void RemoveTile(Vector2Int pos)
        {
            if (tilemap.GetTile((Vector3Int)pos))
            {
                tilemap.SetTile((Vector3Int)pos, null);
                RemoveTile(pos + removeDir);
            }
        }
    }
}
