using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TileBlob : MonoBehaviour
{
    public Tile[] topLeftQuadTiles;
    public Tile[] topRightQuadTiles;
    public Tile[] botLeftQuadTiles;
    public Tile[] botRightQuadTiles;
    public Tilemap input;
    public Tilemap output;
    public bool outOfBounds;

    void Update()
    {
        foreach (Vector3Int tilePos in input.cellBounds.allPositionsWithin)
        {
            if (GetTile(tilePos) == 0)
            {
                SetTile(tilePos, null);
                continue;
            }

            int topLeft  = GetTile(tilePos + new Vector3Int(-1, 1, 0));
            int left     = GetTile(tilePos + new Vector3Int(-1, 0, 0));
            int botLeft  = GetTile(tilePos + new Vector3Int(-1,-1, 0));
            int bot      = GetTile(tilePos + new Vector3Int( 0,-1, 0));
            int botRight = GetTile(tilePos + new Vector3Int( 1,-1, 0));
            int right    = GetTile(tilePos + new Vector3Int( 1, 0, 0));
            int topRight = GetTile(tilePos + new Vector3Int( 1, 1, 0));
            int top      = GetTile(tilePos + new Vector3Int( 0, 1, 0));

            SetTile(tilePos, new TileBase[4]
            {
                botLeftQuadTiles [GetQuadTileIndex(left , bot, botLeft )],
                botRightQuadTiles[GetQuadTileIndex(right, bot, botRight)],
                topLeftQuadTiles [GetQuadTileIndex(left , top, topLeft )],
                topRightQuadTiles[GetQuadTileIndex(right, top, topRight)],
            });
        }

        input.CompressAndRefresh();
        output.ResizeAndRefresh(input.origin, output.origin, 2);
    }

    int GetTile(Vector3Int pos)
    {
        if (input.cellBounds.Contains(pos))
            return input.GetTile(pos) ? 1 : 0;
        return outOfBounds ? 1 : 0;
    }

    // RANT: What I want to do here is very simple: I want to create a new tile base on smaller tiles (e.g 16x16 tiles can be created from 4 8x8 tiles).
    // Unity doesn't have any built-in way to do this. They don't have a sub-tilemap system or any API to quickly create new tiles based on smaller ones.
    // The only sane way to do it currently is to just create another tilemap with a smaller size and manually set the correct tiles.
    // This is expensive (both in memory and instructions), error-prone, and annoying.
    void SetTile(Vector3Int pos, TileBase[] tiles)
    {
        if (tiles == null)
            tiles = new TileBase[4].Populate((TileBase)null);
        output.SetTilesBlock(new BoundsInt(pos * 2, new Vector3Int(2, 2, 1)), tiles);
    }

    int GetQuadTileIndex(int horizontal, int vertical, int diagonal)
    {
        int quadTileIndex = horizontal + 2 * vertical;
        if (horizontal == 1 && vertical == 1 && diagonal == 1)
            quadTileIndex = 4;
        return quadTileIndex;
    }
}
