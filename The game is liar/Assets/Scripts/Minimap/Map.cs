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
                /*rect.sizeDelta = (Vector3)Minimap.instance.bounds.size * Minimap.instance.pixelSize;
                image.enabled = true;
                image.texture = Minimap.instance.texture;*/
                rect.sizeDelta = new Vector3(mapWidth, mapHeight);

                image.enabled = true;

                Vector2Int offset = new Vector2Int(mapWidth / Minimap.instance.pixelSize, mapHeight / Minimap.instance.pixelSize);
                Vector2Int mapPos = MathUtils.Clamp(Minimap.instance.playerPosition - offset / 2, Vector2Int.zero, 
                    new Vector2Int(Minimap.instance.texture.width - Minimap.instance.minimapWidth, Minimap.instance.texture.height - Minimap.instance.minimapHeight));

                Texture2D cropTexture = new Texture2D(mapWidth / Minimap.instance.pixelSize, mapHeight / Minimap.instance.pixelSize);

                Debug.Log("Map position: " + mapPos);
                Debug.Log("source's bounds: " + Minimap.instance.bounds);

                Graphics.CopyTexture(Minimap.instance.texture, 0, 0, mapPos.x, mapPos.y, mapWidth / Minimap.instance.pixelSize, mapHeight / Minimap.instance.pixelSize, 
                    cropTexture, 0, 0, 0, 0);

                //cropTexture.SetPixel(Minimap.instance.playerPosition.x, Minimap.instance.playerPosition.y, Minimap.instance.playerColor);

                cropTexture.wrapMode = TextureWrapMode.Clamp;

                cropTexture.filterMode = FilterMode.Point;

                cropTexture.Apply();

                image.texture = cropTexture;
            }
            else
            {
                image.enabled = false;
            }
        }
    }
}
