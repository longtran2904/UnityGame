using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Edgar.Unity;
using UnityEngine.UI;

// TODO: Move minimap to RoomInfoPostProcess
public class Minimap : MonoBehaviour
{
    [HideInInspector] public Tilemap tilemap;
    public TileBase wallTile;
    public TileBase backgroundTile;
    public RoomInstanceVariable currentRoom;
    public GameObject map;

    private TileBase[] tiles;
    private RawImage image;

#if UNITY_EDITOR
    public bool unlockAllTiles;
    [ShowWhen("unlockAllTiles")] public TileBase normalWallTile;
    [ShowWhen("unlockAllTiles")] public TileBase normalSiblingTile;
    [ShowWhen("unlockAllTiles")] public Camera mapCam;
    private bool hasUnlocked;

    void InitCamera()
    {
        float aspect = 16f / 9f;
        BoundsInt bounds = tilemap.transform.parent.GetChild(2).GetComponent<Tilemap>().cellBounds;
        Vector2Int texSize;
        if ((bounds.size.x / (float)bounds.size.y) <= aspect)
        {
            mapCam.orthographicSize = bounds.size.y / 2f;
            texSize = new Vector2Int((int)(bounds.size.y * aspect), bounds.size.y);
        }
        else
        {
            mapCam.orthographicSize = (bounds.size.x / aspect) / 2f;
            texSize = new Vector2Int(bounds.size.x, (int)(bounds.size.x / aspect));
        }
        mapCam.transform.position = (Vector3)(Vector2)bounds.center + new Vector3(0, 0, -10);
    }

    [EasyButtons.Button]
    void UnlockAllRooms()
    {
        if (hasUnlocked)
            return;
        foreach (RoomInfo room in FindObjectsOfType<RoomInfo>())
        {
            if (room.RoomInstance.IsCorridor)
                continue;
            AddRoomToLevelMap(room.RoomInstance);
        }

        InitCamera();
        hasUnlocked = true;
    }
#endif
    void Start()
    {
        if (tilemap)
        {
            tiles = new TileBase[256 * 256];
            image = GetComponent<RawImage>();
#if UNITY_EDITOR
            if (unlockAllTiles)
            {
                UnlockAllRooms();
                return;
            }
#endif
            AddRoomToLevelMap(currentRoom.value);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            map.SetActive(!map.activeSelf);
            image.enabled = !image.enabled;
        }
    }

    public void AddRoomToLevelMap(RoomInstance room)
    {
        BoundsInt currentBounds = EdgarHelper.GetRoomBoundsInt(room);

        currentBounds.min += Vector3Int.one;
        currentBounds.max -= Vector3Int.one;
        List<Vector3Int> wallTiles = MathUtils.GetOutlinePoints(currentBounds);
        foreach (var door in room.Doors)
        {
            Debug.Assert(wallTiles.Remove(room.Position + door.DoorLine.From - door.FacingDirection.ToVector3Int()));
            Debug.Assert(wallTiles.Remove(room.Position + door.DoorLine.To - door.FacingDirection.ToVector3Int()));
            Debug.Assert((door.DoorLine.From.x <= door.DoorLine.To.x) && (door.DoorLine.From.y <= door.DoorLine.To.y), "From: " + door.DoorLine.From + " To: " + door.DoorLine.To);

            Vector3Int dir = door.DoorLine.To - door.DoorLine.From;
            wallTiles.Add(room.Position + door.DoorLine.From - dir);
            wallTiles.Add(room.Position + door.DoorLine.To + dir);
        }
        tilemap.SetTiles(wallTiles.ToArray(), tiles.Populate(wallTile));

        currentBounds.min += Vector3Int.one;
        currentBounds.max -= Vector3Int.one;
        tilemap.SetTilesBlock(currentBounds, tiles.Populate(backgroundTile));

        // NOTE: There will be some duplications with the previous room's doors, but I don't care.
        foreach (var door in room.Doors)
        {
            foreach (var doorTilePos in door.DoorLine.GetPoints())
            {
                tilemap.SetTile(room.Position + doorTilePos, backgroundTile);
                tilemap.SetTile(room.Position + doorTilePos - door.FacingDirection.ToVector3Int(), backgroundTile);
            }
        }
    }
}
