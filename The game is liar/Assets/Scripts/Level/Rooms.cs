using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.IO;

public class Rooms : MonoBehaviour
{
    [Range(1, 5)]
    public int width = 1;
    [Range(1, 5)]
    public int height = 1;

    [HideInInspector, SerializeField] private int roomWidth;
    [HideInInspector, SerializeField] private int roomHeight;

    [HideInInspector, SerializeField] private int previousWidth;
    [HideInInspector, SerializeField] private int previousHeight;

    [HideInInspector] public int tilesPerUnit = 2;

    public Vector2 leftAndBottomLimit;
    public Vector2 rightAndUpLimit;

    [HideInInspector] public bool editTileMode;

    [HideInInspector] public bool[] exits = new bool[4];

    public Tile tile;
    private Tilemap tilemap;

    [HideInInspector] public const int size = 6;

    [HideInInspector] public SerializableTile[] serializableTiles;
        
    private BoundsInt bounds;
    private TileBase[] tiles;
    private int boundsSizeX;

    public void SaveTile(SerializableTile[] _tiles)
    {
        string jsonData = JsonHelper.ToJson<SerializableTile>(_tiles, true);

        string path = transform.parent.name + ".json";

        SaveSystem.SaveJsonData(jsonData, path);

        Debug.Log("Saving...");
    }

    SerializableTile[] LoadTile()
    {
        string fileName = transform.parent.name + ".json";

        string jsonData = SaveSystem.LoadJsonData(fileName);

        Debug.Log("Loading...");

        return JsonHelper.FromJson<SerializableTile>(jsonData);
    }

    public void SetBoundaries()
    {
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }
        
        roomWidth = width * size;

        roomHeight = height * size;

        bounds = tilemap.cellBounds;

        if (editTileMode)
        {
            previousWidth = roomWidth;

            previousHeight = roomHeight;

            bounds = tilemap.cellBounds;

            tiles = tilemap.GetTilesBlock(bounds);

            serializableTiles = new SerializableTile[bounds.size.x * bounds.size.y];
            boundsSizeX = bounds.size.x;

            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    TileBase tile = tiles[x + y * bounds.size.x];

                    if (tile != null)
                    {
                        serializableTiles[x + y * bounds.size.x] = new SerializableTile(tile, tilemap.GetTransformMatrix(new Vector3Int(x + bounds.position.x, y + bounds.position.y, 0)));
                    }
                }
            }

            return;
        }

        tilemap.ClearAllTiles();

        serializableTiles = LoadTile();
        Debug.Log(serializableTiles.Length);

        if (serializableTiles != null && serializableTiles.Length != 0)
        {
            for (int x = 0; x < boundsSizeX; x++)
            {
                for (int y = 0; y < serializableTiles.Length / boundsSizeX; y++)
                {
                    if (serializableTiles[x + y * boundsSizeX].tile != null)
                    {
                        Vector3Int cellPosition = new Vector3Int(x + bounds.position.x, y + bounds.position.y, 0);

                        tilemap.SetTile(cellPosition, serializableTiles[x + y * boundsSizeX].tile);

                        tilemap.SetTransformMatrix(cellPosition, serializableTiles[x + y * boundsSizeX].rotation);
                    }
                }
            }
        }

        if (previousWidth == roomWidth && previousHeight == roomHeight && serializableTiles != null)
        {
            return;
        }

        Debug.Log(serializableTiles);

        Vector3Int position = Vector3Int.zero;

        for (int x = 0; x < roomWidth; x++)
        {
            position.y = 0;

            position.x = x * (tilesPerUnit);

            SetTile(position);

            if (x > 0 && x < roomWidth - 1)
            {
                position.y = roomHeight * tilesPerUnit - tilesPerUnit;

                SetTile(position);

                continue;
            }
            for (int y = 1; y < roomHeight; y++)
            {
                position.y = y * (tilesPerUnit);

                SetTile(position);
            }
        }
    }

    void SetTile(Vector3Int _position)
    {
        Vector3Int _beginPosition = _position;

        for (int x = 0; x < tilesPerUnit; x++)
        {
            _position.x += x;

            tilemap.SetTile(_position, tile);

            for (int y = 1; y < tilesPerUnit; y++)
            {
                _position.y += y;

                tilemap.SetTile(_position, tile);

                _position.y = _beginPosition.y;
            }

            _position = _beginPosition;
        }
    }
}

[Serializable]
public class SerializableTile
{
    public TileBase tile;
    public Matrix4x4 rotation;

    public SerializableTile(TileBase _tile, Matrix4x4 _rotation)
    {
        tile = _tile;
        rotation = _rotation;
    }
}
