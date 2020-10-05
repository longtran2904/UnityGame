using ProceduralLevelGenerator.Unity.Generators.Common.Rooms;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    public List<RoomInstance> rooms;
    [HideInInspector] public Tilemap tilemap; // the tilemap of the current level
    private RoomInstance currentRoom;
    bool canLock = true;

    public static RoomManager instance;
    public const string startingRoom = "Starting Room";

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.GetComponentInParent<CameraFollow2D>().hasPlayer += UpdateCurrentRoom;
        foreach (var room in rooms)
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
        try
        {
            if (EnemySpawner.numberOfEnemiesAlive > 0 && currentRoom.RoomTemplatePrefab.name != startingRoom && canLock)
            {
                LockRoom();
                canLock = false;
            }
            else if (EnemySpawner.numberOfEnemiesAlive == 0 && currentRoom.RoomTemplatePrefab.name != startingRoom && !canLock)
            {
                UnLockRoom();
                canLock = true;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(currentRoom);
            Debug.LogError(e);
            throw;
        }
    }

    void UpdateCurrentRoom(Bounds bounds)
    {
        foreach (var room in rooms)
        {
            if (MathUtils.CreateBounds(room.OutlinePolygon.GetOutlinePoints()[0], room.OutlinePolygon.GetOutlinePoints()[2]) == bounds)
            {
                currentRoom = room;
                currentRoom.RoomTemplateInstance.transform.Find("Enemies").GetComponent<EnemySpawner>().enabled = true;
                if (currentRoom.RoomTemplatePrefab.name != startingRoom) WavesUI.instance.UpdateCurrentSpawner(currentRoom); // Update the waves of enemies UI
                return;
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
                    return;
                }
                int furthest = 0; // We active the furthest door because the player had already moved over it
                if (Mathf.Abs(currentRoom.Position.x - doors[0].transform.position.x) < Mathf.Abs(currentRoom.Position.x - doors[1].transform.position.x))
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
