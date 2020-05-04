using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public static Minimap instance;

    [HideInInspector] public Texture2D texture;
    private Texture2D cropTexture;
    private RawImage rawImage;
    private RectTransform rect;

    [HideInInspector] public BoundsInt bounds;
    public Dictionary<Vector2Int, bool[]> tilesDictionary = new Dictionary<Vector2Int, bool[]>();

    Vector2Int previousPos;

    Player player;

    public int minimapWidth;
    public int minimapHeight;
    public int pixelSize;

    public Color wallColor;
    public Color roomColor;
    public Color playerColor;

    private void Awake()
    {
        instance = this;

        rawImage = GetComponent<RawImage>();

        rect = GetComponent<RectTransform>();

        cropTexture = new Texture2D(minimapWidth, minimapHeight);

        GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<CameraFollow2D>().hasPlayer += AddRoomTexture;
    }

    private void LateUpdate()
    {
        CropTexture();
    }

    public void CreateTexture()
    {
        texture = new Texture2D(bounds.size.x, bounds.size.y);

        Color[] colors = new Color[bounds.size.x * bounds.size.y];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(.804f, .804f, .804f, .804f);
        }

        foreach (var tile in tilesDictionary.Keys)
        {
            if (!tilesDictionary[tile][1])
            {
                continue;
            }

            int i = tile.x + tile.y * bounds.size.x;

            colors[i] = roomColor;

            if (tilesDictionary[tile][0])
            {
                colors[i] = wallColor;
            }
        }

        texture.SetPixels(colors);

        texture.wrapMode = TextureWrapMode.Clamp;

        texture.filterMode = FilterMode.Point;

        texture.Apply();
    }

    void CropTexture()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        Vector2Int playerPos = MathUtils.ToVector2Int(player.transform.position - bounds.position, true);
        int x = (int)(playerPos.x - minimapWidth / 2);
        int y = (int)(playerPos.y - minimapHeight / 2);

        x = Mathf.Clamp(x, 0, texture.width - minimapWidth);
        y = Mathf.Clamp(y, 0, texture.height - minimapHeight);

        Color[] colors = texture.GetPixels(x, y, minimapWidth, minimapHeight);
        colors[minimapWidth / 2 + (minimapHeight / 2) * minimapWidth] = playerColor;

        cropTexture.SetPixels(colors);

        cropTexture.wrapMode = TextureWrapMode.Clamp;
        cropTexture.filterMode = FilterMode.Point;

        cropTexture.Apply();

        rect.sizeDelta = new Vector2(minimapWidth * pixelSize, minimapHeight * pixelSize);

        rawImage.texture = cropTexture;
    }

    void AddRoomTexture(Bounds _bounds)
    {
        Debug.Log("Add room texture to minimap!");

        foreach (var tile in tilesDictionary.Keys)
        {
            if (_bounds.Contains((Vector2)tile + (Vector2)(Vector3)bounds.position))
            {
                tilesDictionary[tile][1] = true;
            }
        }

        CreateTexture();
    }
}
