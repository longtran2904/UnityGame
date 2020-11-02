using ProceduralLevelGenerator.Unity.Generators.Common.Rooms;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    public Dictionary<RoomInstance, Bounds> rooms = new Dictionary<RoomInstance, Bounds>();
    public List<Vector2Int> allGroundTiles = new List<Vector2Int>();
    [HideInInspector] public Tilemap tilemap; // the tilemap of the current level
    private RoomInstance currentRoom;
    private RoomInstance lastRoom;
    bool canLock = true;

    public static RoomManager instance;
    public const string startingRoom = "Starting Room";
    public event Action<RoomInstance> hasPlayer;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Camera.main.GetComponentInParent<CameraFollow2D>().hasPlayer += UpdateCurrentRoom;
        foreach (var room in rooms.Keys)
        {
            if (room.RoomTemplatePrefab.name == startingRoom)
            {
                currentRoom = room;
            }
            RemoveBlockTile(room);
        }
        tilemap.RefreshAllTiles();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCurrentRoom();
        try
        {
            if (Enemies.numberOfEnemiesAlive > 0 && currentRoom.RoomTemplatePrefab.name != startingRoom && canLock)
            {
                LockRoom();
                canLock = false;
            }
            else if (Enemies.numberOfEnemiesAlive == 0 && currentRoom.RoomTemplatePrefab.name != startingRoom && !canLock)
            {
                UnLockRoom();
                canLock = true;
            }
        }
        catch (Exception e)
        {
            InternalDebug.Log(currentRoom);
            InternalDebug.LogError(e);
            throw;
        }
    }

    void UpdateCurrentRoom()
    {
        foreach (var bounds in rooms.Values)
        {
            ExtDebug.DrawBox(bounds.center, bounds.extents, Quaternion.identity, Color.cyan);
        }
        foreach (var room in rooms.Keys)
        {
            if (rooms[room].Contains(Player.player.transform.position) && room != lastRoom)
            {
                currentRoom = room;
                EnemySpawner currentSpawner = currentRoom.RoomTemplateInstance.transform.Find("Enemies").GetComponent<EnemySpawner>();
                currentSpawner.enabled = true;
                hasPlayer?.Invoke(room);
                lastRoom = room;
            }
        }
    }

    void LockRoom()
    {
        foreach (var doorInstance in currentRoom.Doors)
        {
            Door[] doors = doorInstance.ConnectedRoomInstance.RoomTemplateInstance.transform.Find("Doors").GetComponentsInChildren<Door>(true);
            if (doors.Length > 0)
            {
                if (doors.Length == 1)
                {
                    doors[0].gameObject.SetActive(true);
                    continue;
                }
                int furthest = 0; // We active the furthest door because the player had already moved over it
                Vector3 roomCenter = rooms[currentRoom].center;
                if (Mathf.Abs(roomCenter.x - doors[0].transform.position.x) < Mathf.Abs(roomCenter.x - doors[1].transform.position.x))
                {
                    furthest = 1;
                }
                doors[furthest].gameObject.SetActive(true);
            }
        }
    }

    void UnLockRoom()
    {
        foreach (var doorInstance in currentRoom.Doors)
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
                RemoveTile((Vector2Int)door.DoorLine.From + removeDir);
                RemoveTile((Vector2Int)door.DoorLine.To + removeDir);
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
