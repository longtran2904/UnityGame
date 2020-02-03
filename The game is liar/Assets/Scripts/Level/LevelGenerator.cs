using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    public ColorToPrefab[] colorMappings;

    public Texture2D map;

    public GameObject grid;

    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        GenerateLevel();        
    }

    void GenerateOgmoLevel()
    {
        string levelFolder = "Levels";

        Object[] rooms = Resources.LoadAll(levelFolder + "/Generated Assets", typeof(TextAsset));

        List<OgmoLevel> levels = new List<OgmoLevel>();

        for (int ii = 0; ii < rooms.Length; ii++)
        {
            levels.Add(new OgmoLevel(rooms[ii] as TextAsset));
        }
        int[,] tiles = levels[0].layers["Grounds"].tiles;
        OgmoEntity playerEntity = levels[0].layers["Objects"].entities[0];
        Vector2 position = new Vector2(playerEntity.x / 32, -playerEntity.y / 32);
        if (playerEntity.name == "Player")
        {
            Instantiate(player, position, Quaternion.identity, transform);
        }
        for (int ii = 0; ii < tiles.GetLength(0); ii++)
        {
            for (int jj = 0; jj < tiles.GetLength(1); jj++)
            {
                if (tiles[ii, jj] == 49)
                {
                    Instantiate(grid, new Vector2(ii, -jj), Quaternion.identity, transform);
                }
            }
        }
        Debug.Log("Level has been generated!");
    }
    void GenerateLevel()
    {
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                GenerateTile(x, y);
            }
        }
    }

    void GenerateTile(int x, int y)
    {
        Color pixelColor = map.GetPixel(x, y);

        // This pixel is transparent so ignore it!
        if (pixelColor.a == 0)
        {
            return;
        }

        foreach (ColorToPrefab colorMapping in colorMappings)
        {
            if (colorMapping.color.Equals(pixelColor))
            {
                Vector2 position = new Vector2(x, y);

                if (colorMapping.prefab != null)
                {
                    Instantiate(colorMapping.prefab, position, colorMapping.prefab.transform.rotation, transform);
                }
            }
        }
    }
}
