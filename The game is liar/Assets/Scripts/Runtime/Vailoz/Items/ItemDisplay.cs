using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDisplay : MonoBehaviour
{
    public Item[] item;
    public Image[] icon;
    public TextMeshProUGUI[] text;

    public static ItemDisplay instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        DisplayIcon();
    }

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            DisplayDescription();
        }
    }*/

    public void DisplayIcon()
    {
        for (int i = 0; i < icon.Length; i++)
        {
            if (item == null || !item[i].icon)
            {
                icon[i].gameObject.SetActive(false);
                continue;
            }
            icon[i].sprite = item[i].icon;
        }
    }

    public void DisplayDescription()
    {
        for (int i = 0; i < text.Length; i++)
        {
            text[i].text = item[i].description;
        }
    }
}
