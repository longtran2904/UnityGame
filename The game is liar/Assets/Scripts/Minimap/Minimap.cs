using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public static Minimap instance;

    [HideInInspector] public Texture2D texture, fullTexture;
    private Texture2D cropTexture;
    private RawImage rawImage;
    private RectTransform rect;

    [HideInInspector] public BoundsInt bounds;
    public Dictionary<Vector2Int, bool[]> tilesDictionary = new Dictionary<Vector2Int, bool[]>();

    Player player;
    [HideInInspector] public Vector2Int playerPosition;

    public int minimapWidth;
    public int minimapHeight;
    public int pixelSize;
    [HideInInspector] public int textureX, textureY;

    public Color wallColor;
    public Color roomColor;
    public Color playerColor;
    public Color bossRoomColor;

    private void Awake()
    {
        instance = this;
        rawImage = GetComponent<RawImage>();
        rect = GetComponent<RectTransform>();
        cropTexture = new Texture2D(minimapWidth, minimapHeight);
        cropTexture.wrapMode = TextureWrapMode.Clamp;
        cropTexture.filterMode = FilterMode.Point;
    }

    private void LateUpdate()
    {
        CropTexture();
    }

    public void CreateTexture()
    {
        Color[] colors = new Color[bounds.size.x * bounds.size.y];

        // Reset texture color
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(.804f, .804f, .804f, .804f);
        }

        // Assign each tile to a color
        foreach (var tile in tilesDictionary.Keys)
        {
            if (!tilesDictionary[tile][1])
            {
                continue;
            }
            int i = tile.x + tile.y * bounds.size.x;
            if (tilesDictionary[tile][2])
            {
                colors[i] = bossRoomColor;
            }
            else
            {
                colors[i] = roomColor;
            }
            if (tilesDictionary[tile][0])
            {
                colors[i] = wallColor;
            }
        }

        texture.SetPixels(colors);
        texture.Apply(false);
    }

    public void SetupTexture()
    {
        // Map texture
        texture = new Texture2D(bounds.size.x, bounds.size.y);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        // Make a new texture which has the map texture surround with transparent tile
        fullTexture = new Texture2D(texture.width * 2, texture.height * 2);
        textureX = fullTexture.width / 2 - texture.width / 2;
        textureY = fullTexture.height / 2 - texture.height / 2;
    }

    void CropTexture()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        // The player position relative to the map position
        playerPosition = MathUtils.ToVector2Int(player.transform.position - bounds.position, true);

        // The position of the minimap
        int mapX = playerPosition.x - minimapWidth / 2;
        int mapY = playerPosition.y - minimapHeight / 2;

        Graphics.CopyTexture(texture, 0, 0, 0, 0, texture.width, texture.height, fullTexture, 0, 0, textureX, textureY);

        // Crop the minimap from the map texture
        Color[] colors = fullTexture.GetPixels(mapX + textureX, mapY + textureY, minimapWidth, minimapHeight);
        colors[minimapWidth / 2 + (minimapHeight / 2) * minimapWidth] = playerColor;

        cropTexture.SetPixels(colors);
        cropTexture.Apply();

        rect.sizeDelta = new Vector2(minimapWidth * pixelSize, minimapHeight * pixelSize);
        rawImage.texture = cropTexture;
    }

    public void AddRoomTexture(BoundsIntVariable currentRoom)
    {
        foreach (var tile in tilesDictionary.Keys)
        {
            if (currentRoom.value.Contains(MathUtils.ToVector3Int(tile + (Vector2)(Vector3)bounds.position)))
            {
                tilesDictionary[tile][1] = true;
            }
        }
        CreateTexture();
    }
}
