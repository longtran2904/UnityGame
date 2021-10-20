<<<<<<< Updated upstream
﻿using System.Collections;
using System.Collections.Generic;
=======
﻿using System.Collections.Generic;
using System.Linq;
>>>>>>> Stashed changes
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
<<<<<<< Updated upstream
    public Transform[] sellPos;
    public GameObject[] shopItems;
    private List<GameObject> usedItems = new List<GameObject>();
    public const float scaleMutiplier = 1.75f;
=======
    [MinMax(0, 10)] public RangedInt sellCount;
    public DropWeapon dropWeapon;
    public Transform[] sellPos;
    public WeaponInventory inventory;
    public BoxCollider2D table;
>>>>>>> Stashed changes

    // Start is called before the first frame update
    void Start()
    {
<<<<<<< Updated upstream
        for (int i = 0; i < sellPos.Length; i++)
        {
            int random = Random.Range(0, shopItems.Length);
            while (usedItems.Contains(shopItems[random]))
            {
                random = Random.Range(0, shopItems.Length);
            }
            usedItems.Add(shopItems[random]);
            GameObject currentItem = Instantiate(shopItems[random], sellPos[i].position, Quaternion.identity);
            foreach (var behaviour in currentItem.GetComponents<MonoBehaviour>())
            {
                behaviour.enabled = false;
            }
            currentItem.AddComponent<BoxCollider2D>();
            currentItem.tag = "SellWeapon";
            currentItem.layer = LayerMask.NameToLayer("HasTextbox");
            currentItem.transform.localScale *= scaleMutiplier;
        }
=======
        List<Weapon> weapons = inventory.items.ToList();
        int length = Mathf.Min(sellCount.randomValue, sellPos.Length, weapons.Count);
        for (int i = 0; i < length; i++)
        {
            dropWeapon.Drop(weapons.PopRandom(), sellPos[i].position, Vector2.zero);
        }

        if (table)
            Destroy(table, 2f);
>>>>>>> Stashed changes
    }
}
