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
    public TileBase wallTile;
    //public TileBase doorTile;
    //public TileBase normalTile;

#if UNITY_EDITOR
    [Header("Demo")]
    public Optional<float> cameraSpeed;
    [ShowWhen("cameraSpeed.enabled", false)] public bool enablePlayerDemoMode;
    [ShowWhen("cameraSpeed.enabled", false)] public bool disableEnemies;
    private Vector3 startPos;
    private Vector3 endPos;
    private int currentDemoRoom = -1;
    private PlayerController player;
    private Camera main;
    static int seed;

    [EasyButtons.Button]
    void RestartLevel()
    {
        if (Application.isPlaying)
        {
            DungeonGenerator generator = FindObjectOfType<DungeonGenerator>();
            // NOTE: generator.seed is a quick fix for me to get the local random seed of the generator. Remeber to add this when updating Edgar.
            seed = generator.seed;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Awake()
    {
        if (seed != 0)
        {
            DungeonGenerator generator = FindObjectOfType<DungeonGenerator>();
            generator.UseRandomSeed = false;
            generator.RandomGeneratorSeed = seed;
            seed = 0;
        }
    }

    void NextRoom()
    {
        currentBoundsInt.value = EdgarHelper.GetRoomBoundsInt(rooms[++currentDemoRoom]);

        float aspect = (float)currentBoundsInt.value.size.x / currentBoundsInt.value.size.y;
        Vector3 offset;
        if (aspect >= main.aspect)
        {
            main.orthographicSize = currentBoundsInt.value.size.y / 2f;
        }
        else
        {
            main.orthographicSize = currentBoundsInt.value.size.x / main.aspect / 2f;
        }
        offset = new Vector3(main.orthographicSize * main.aspect, main.orthographicSize);
        startPos = currentBoundsInt.value.min + offset + Vector3.forward * -10;
        endPos = currentBoundsInt.value.max - offset + Vector3.forward * -10;
        Debug.DrawLine(startPos, endPos, Color.yellow, 1000);
    }

    System.Collections.IEnumerator UpdateDemo()
    {
        yield return null;
        NEXT_ROOM:
        NextRoom();
        float t = 0;
        bool toNextRoom = false;
        while (true)
        {
            if (t > 1)
            {
                if (toNextRoom)
                {
                    if (currentDemoRoom == rooms.Count - 1)
                        yield break;
                    goto NEXT_ROOM;
                }
                MathUtils.Swap(ref startPos, ref endPos);
                t = 0;
                toNextRoom = true;
            }
            main.transform.position = Vector3.Lerp(startPos, endPos, t);
            t += cameraSpeed * Time.deltaTime;
            yield return null;
        }
    }
#endif

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void EnterDemo(PlayerController controller)
    {
        if (cameraSpeed.enabled)
        {
            disableEnemies = true;
            main = Camera.main;
            Destroy(main.GetComponentInParent<CameraFollow2D>());
            if (controller)
                Destroy(controller.gameObject);
            StartCoroutine(UpdateDemo());
        }
        else if (enablePlayerDemoMode && controller)
        {
            player = controller;
            player.EnterDemo();
        }

        if (disableEnemies)
            foreach (var listener in GetComponents<GameEventListener>())
            {
                listener.Event.UnregisterListener(listener);
                Destroy(listener);
            }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(tilemap.cellSize == Vector3Int.one, "Tilemap's size isn't Vector3Int.one!");

        List<Vector3Int> removeTilesPos = new List<Vector3Int>();
        List<Vector3Int> doorTilesPos   = new List<Vector3Int>();
        List<Vector3Int> wallTilesPos   = new List<Vector3Int>();

        // Find starting room and remove all door tiles
        bool hasStartingRoom = false;
        foreach (var room in rooms)
        {
            if (room.RoomTemplatePrefab.name == startingRoom)
            {
                currentRoom.value = room;
                EnterDemo(currentRoom.value.RoomTemplateInstance.GetComponentInChildren<PlayerController>());
                hasStartingRoom = true;
            }

#if false
            Tilemap wallTilemap = room.RoomTemplateInstance.transform.Find("Tilemaps").GetChild(2).GetComponent<Tilemap>();
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
#endif

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

        if (!hasStartingRoom)
            EnterDemo(null);

        //tilemap.SetTiles(doorTilesPos.ToArray(), new TileBase[doorTilesPos.Count].Populate(doorTile));
        tilemap.SetTiles(removeTilesPos.ToArray(), new TileBase[removeTilesPos.Count].Populate(null));
        //tilemap.SetTiles(wallTilesPos.ToArray(), new TileBase[wallTilesPos.Count].Populate(normalTile));
        tilemap.RefreshAllTiles();
        currentBoundsInt.value = EdgarHelper.GetRoomBoundsInt(currentRoom.value);
    }

#if UNITY_EDITOR
    void Update()
    {
        foreach (var room in rooms)
        {
            Bounds bounds = EdgarHelper.GetRoomBoundsInt(room).ToBounds();
            ExtDebug.DrawBox(bounds.center, bounds.extents, Quaternion.identity, Color.cyan);
        }
    }
#endif

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
