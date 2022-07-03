using System;
using System.Collections.Generic;
using UnityEngine;
using Edgar.Unity;
using UnityEngine.Tilemaps;

public enum GameMode
{
    None,
    Quit,
    Main,
    Play,
    // Cutscene mode

    Count
}

public class GameManager : MonoBehaviour
{
    [Header("Game Mode")]
    public GameMode startMode;

    public GameObject mainMode;
    public GameObject playMode;
    public LevelData[] levels;
    public int currentLevel;

    [Header("Camera")]
    public Vector3Variable playerPos;
    public Entity cameraEntity;
    private Camera main;

    [Header("UI")]
    public bool overrideGameUI;
    public GameUI gameUI;
    public GameMenu gameMenu;

    [Header("Other")]
    public Audio[] audios;
    public AudioType firstMusic;
    public int sourceCount;
    public Pool[] pools;

    private List<RoomInstance> rooms;
    private int currentRoom;
    private static Bounds defaultBounds;
    public static Entity player;

    // IMPORTANT: This function only gets called in LevelInfoPostProcess.cs and its job is to:
    // 1. Add 2 rule tiles at the 2 ends of a door line.
    // 2. Delete all the tiles that are at the door's tiles.
    // 3. Delete all the tiles that are next to the door's tiles.
    // Originally, I let Edgar's corridor system to handle the first 2 cases (I have to have a shared tilemap).
    // Now, I don't need the corridor system anymore but I still need a shared tilemap.
    // Obviously, these cases can be easily solved if Unity's rule tile system let you work with out-of-bounds cases, but it doesn't :(
    // If Unity ever allow this then I can remove most of this code and also don't need a shared tilemap anymore.
    public void InitTilemap(List<RoomInstance> rooms, Tilemap tilemap, TileBase ruleTile)
    {
        foreach (var room in rooms)
        {
            foreach (var door in room.Doors)
            {
                if (door.ConnectedRoomInstance != null)
                {
                    Vector3Int dir = door.DoorLine.GetDirectionVector();
                    tilemap.SetTile(door.DoorLine.From + room.Position - dir, ruleTile);
                    tilemap.SetTile(door.DoorLine.To + room.Position + dir, ruleTile);

                    foreach (Vector3Int doorTile in door.DoorLine.GetPoints())
                    {
                        Vector3Int pos = doorTile + room.Position;
                        tilemap.SetTile(pos, null);
                        Remove(tilemap, pos, -(Vector3Int)door.FacingDirection);

                        static void Remove(Tilemap tilemap, Vector3Int pos, Vector3Int removeDir)
                        {
                            pos += removeDir;
                            if (tilemap.GetTile(pos))
                            {
                                tilemap.SetTile(pos, null);
                                Remove(tilemap, pos, removeDir);
                            }
                        }
                    }
                }
            }
        }

        tilemap.RefreshAllTiles();
        this.rooms = rooms;
    }

    public static Bounds GetBoundsFromRoom(Transform roomTransform, bool compress = false)
    {
        Tilemap tilemap = roomTransform?.GetChild(0).GetChild(2).GetComponent<Tilemap>();
        if (tilemap)
        {
            if (compress)
            {
                tilemap.CompressBounds();
                tilemap.RefreshAllTiles();
            }
            Bounds bounds = tilemap.cellBounds.ToBounds();
            //bounds.min += roomTransform.position.ToVector3Int();
            bounds.center += roomTransform.position;
            return bounds;
        }
        return defaultBounds;
    }

    private void Start()
    {
        ObjectPooler.Init(gameObject, pools);
        AudioManager.Init(gameObject, audios, firstMusic, sourceCount);
        main = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Entity>();
        StartGameMode(startMode);
    }

    private void Update()
    {
        if (rooms != null)
        {
            if (levels[currentLevel].moveAutomatically)
            {
                //if (cameraEntity.HasProperty(EntityProperty.CompleteCycle))
                if (cameraEntity.CompleteCycle())
                {
                    if (currentRoom < rooms.Count)
                    {
                        float aspectRatio = 16f / 9f;
                        Transform roomTransform = rooms[currentRoom++].RoomTemplateInstance.transform;
                        Bounds bounds = GetBoundsFromRoom(roomTransform);
                        if (bounds.size.x / bounds.size.y <= aspectRatio)
                            main.orthographicSize = bounds.extents.x / aspectRatio;
                        else
                            main.orthographicSize = bounds.extents.y;
                        GameInput.TriggerEvent(GameEventType.NextRoom, roomTransform);
                    }
                }
            }
            else
            {
                ToNextRoom(rooms, ref currentRoom, playerPos);

                static void ToNextRoom(List<RoomInstance> roomInstances, ref int currentRoom, Vector3Variable playerPos)
                {
                    int i = 0;
                    foreach (var room in roomInstances)
                    {
                        if (i != currentRoom)
                        {
                            // NOTE: This will make sure the player has already moved pass the doors
                            Bounds roomBounds = GetBoundsFromRoom(room.RoomTemplateInstance.transform);
                            roomBounds.min += (Vector3Int)(Vector2Int.one * 2);
                            roomBounds.max -= (Vector3Int)(Vector2Int.one * 2);

                            if (roomBounds.Contains(playerPos.value.ToVector3Int()))
                            {
                                GameInput.TriggerEvent(GameEventType.NextRoom, room.RoomTemplateInstance.transform);
                                currentRoom = i;
                                break;
                            }
                        }

                        ++i;
                    }
                }
            }
        }
    }

    public void StartGameMode(GameMode mode)
    {
        if (mode == GameMode.None || mode == GameMode.Count)
            return;
        if (mode == GameMode.Quit)
        {
            Application.Quit();
            return;
        }

        // Reset the game
        {
            // TODO:
            //  - Reset all the scriptable objects
            //  - Reset all game input's events
            rooms?.Clear();
            gameMenu.gameObject.SetActive(false);
            if (overrideGameUI)
            {
                gameUI.displayHealthBar = gameUI.displayMoneyCount = gameUI.displayWaveCount = gameUI.displayWeaponUI = gameUI.displayMinimap = true;
                gameUI.enabled = false;
            }

            bool isMainMode = mode == GameMode.Main;
            // NOTE: I didn't use the ?. operator because Unity's GameObject has a custom == operator but doesn't have a custom ?. operator
            if (mainMode)
                mainMode.SetActive(isMainMode);
            playMode.SetActive(!isMainMode);
        }

        switch (mode)
        {
            case GameMode.Main:
                {
                    AudioManager.PlayAudio(AudioType.Music_Main);
                } break;
            case GameMode.Play:
                {
                    {
                        for (int i = 0; i < levels.Length; i++)
                            levels[i].gameObject.SetActive(i == currentLevel);
                        if (overrideGameUI)
                        {
                            gameUI.enabled = levels[currentLevel].enableUI;
                            gameUI.displayMinimap = levels[currentLevel].enableMinimap;
                        }
                    }

                    switch (levels[currentLevel].type)
                    {
                        case BoundsType.Generator:
                            {
                                {
                                    bool levelGenerated = false;
                                    int count = levels[currentLevel].maxGenerateTry;
                                    DungeonGenerator generator = levels[currentLevel].generator;
                                    generator.transform.Clear();

                                    while (!levelGenerated)
                                    {
                                        try
                                        {
                                            if (count == 0)
                                            {
                                                Debug.LogError("Level couldn't be generated!");
                                                break;
                                            }
                                            generator.Generate();
                                            levelGenerated = true;
                                        }
                                        catch (InvalidOperationException)
                                        {
                                            Debug.LogError("Timeout encountered");
                                            count--;
                                        }
                                    }
                                }

                                if (!levels[currentLevel].moveAutomatically)
                                {
                                    currentRoom = -1;
                                    GameInput.BindEvent(GameEventType.EndRoom, room => LockRoom(room, false, false));
                                    GameInput.BindEvent(GameEventType.NextRoom, room => LockRoom(room, true, levels[currentLevel].disableEnemies));
                                }
                                else
                                {
                                    currentRoom = 0;
                                    PlayerController player = FindObjectOfType<PlayerController>();
                                    if (player)
                                        Destroy(player.gameObject);
                                }
                            } break;
                    }

                    InitLevel(levels[currentLevel], main.HalfSize());

                    void InitLevel(LevelData level, Vector2 camHalfSize)
                    {
                        cameraEntity = main.GetComponentInParent<Entity>();
                        cameraEntity.InitCamera(level.moveAutomatically, level.useSmoothDamp, level.cameraValue, level.waitTime);

                        switch (level.type)
                        {
                            case BoundsType.Tilemap:
                                {
                                    level.tilemap.CompressBounds();
                                    level.tilemap.RefreshAllTiles();
                                    defaultBounds = level.tilemap.cellBounds.ToBounds();
                                    GameInput.TriggerEvent(GameEventType.NextRoom, null);
                                } break;
                            case BoundsType.Custom:
                                {
                                    defaultBounds = new Bounds(Vector3.zero, level.boundsSize + camHalfSize * 2);
                                    GameInput.TriggerEvent(GameEventType.NextRoom, null);
                                } break;
                        }
                    }
                } break;
        }
    }

    static void LockRoom(Transform room, bool lockRoom, bool disableEnemies)
    {
        if (lockRoom && !disableEnemies)
        {
            EnemySpawner spawner = room?.GetComponentInChildren<EnemySpawner>(true);
            if (!spawner)
                return;
            spawner.enabled = true;
        }

        Transform doorHolder = room?.Find("Doors");
        if (doorHolder)
        {
            if (lockRoom)
                doorHolder.gameObject.SetActive(true);
            else
                Destroy(doorHolder.gameObject, 1f);

            foreach (Transform door in doorHolder)
            {
                Animator doorAnim = door.GetComponent<Animator>();
                doorAnim.Play(lockRoom ? "Lock" : "Unlock");
            }
        }
    }
}
