using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;

    private RawImage image;

    private RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<RawImage>();
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (image.enabled == false)
            {
                rect.sizeDelta = new Vector3(mapWidth * Minimap.instance.pixelSize, mapHeight * Minimap.instance.pixelSize);
                image.enabled = true;

                Vector2Int mapPos = Minimap.instance.playerPosition - new Vector2Int(mapWidth/2, mapHeight/2) + new Vector2Int(Minimap.instance.textureX, Minimap.instance.textureY);

                Texture2D cropTexture = new Texture2D(mapWidth, mapHeight);
                
                Graphics.CopyTexture(Minimap.instance.fullTexture, 0, 0, mapPos.x, mapPos.y, mapWidth, mapHeight, cropTexture, 0, 0, 0, 0);

                cropTexture.SetPixel(mapWidth/2, mapHeight/2, Minimap.instance.playerColor);
                cropTexture.Apply();

                cropTexture.wrapMode = TextureWrapMode.Clamp;
                cropTexture.filterMode = FilterMode.Point;
                image.texture = cropTexture;
            }
            else
            {
                image.enabled = false;
            }
        }
    }
}
