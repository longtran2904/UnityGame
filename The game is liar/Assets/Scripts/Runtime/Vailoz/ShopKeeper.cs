using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    [MinMax(0, 10)] public RangedInt sellCount;
    public Transform[] sellPos;
    public WeaponInventory inventory;
    public BoxCollider2D table;

    // Start is called before the first frame update
    void Start()
    {
        int length = Mathf.Min(sellCount.randomValue, sellPos.Length, inventory.items.Count);
        List<int> weapons = new List<int>(length);
        for (int i = 0; i < length; i++)
        {
            int weapon;
            do
            {
                weapon = Random.Range(0, inventory.items.Count);
            } while (weapons.Contains(weapon));
            weapons.Add(weapon);
            inventory.SpawnAndDropWeapon(weapon, sellPos[i].position, Vector2.zero);
        }

        if (table)
            Destroy(table, 2f);
    }
}
