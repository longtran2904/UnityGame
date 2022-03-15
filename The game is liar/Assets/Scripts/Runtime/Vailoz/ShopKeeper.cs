using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    [MinMax(0, 10)] public RangedInt sellCount;
    public DropWeapon dropWeapon;
    public Transform[] sellPos;
    public WeaponInventory inventory;
    public BoxCollider2D table;

    // Start is called before the first frame update
    void Start()
    {
        List<Weapon> weapons = inventory.items.ToList();
        int length = Mathf.Min(sellCount.randomValue, sellPos.Length, weapons.Count);
        for (int i = 0; i < length; i++)
        {
            dropWeapon.Drop(weapons.PopRandom(), sellPos[i].position, Vector2.zero);
        }

        if (table)
            Destroy(table, 2f);
    }
}
