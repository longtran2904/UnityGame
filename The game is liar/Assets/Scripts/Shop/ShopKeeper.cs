using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    public Transform[] sellPos;
    public GameObject[] shopItems;
    private List<GameObject> usedItems = new List<GameObject>();
    public const float scaleMutiplier = 1.75f;

    // Start is called before the first frame update
    void Start()
    {
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
    }
}
