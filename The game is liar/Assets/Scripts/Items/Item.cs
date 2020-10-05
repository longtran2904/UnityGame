using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : ScriptableObject
{
    public enum ItemType { ACTIVE, PASSIVE }

    public string itemName;
    public Sprite icon;
    public string description;
    public float cooldownTime;

    public ItemType type;
}
