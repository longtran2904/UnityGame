using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
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
                rect.sizeDelta = (Vector3)Minimap.instance.bounds.size * Minimap.instance.pixelSize;
                image.enabled = true;
                image.texture = Minimap.instance.texture;
            }
            else
            {
                image.enabled = false;
            }
        }
    }
}
