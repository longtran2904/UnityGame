using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public Item[] items = new Item[2];
    private Camera main;
    private bool[] isUsing = new bool[2];
    private float[] timer = new float[2];

    // Start is called before the first frame update
    void Start()
    {
        main = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Cooldown();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseItem(0);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            UseItem(1);
        }
    }

    private void Cooldown()
    {
        for (int i = 0; i < isUsing.Length; i++)
        {
            if (isUsing[i])
            {
                timer[i] -= Time.deltaTime;
            }
            if (timer[i] <= 0)
            {
                isUsing[i] = false;
            } 
        }
    }

    void UseItem(int slot)
    {
        if (isUsing[slot])
        {
            return;
        }
        if (items[slot] is Grenade)
        {
            if (items[slot] is FragGrenade)
            {
                ThrowGrenade<FragGrenade>(slot);
            }
            else if (items[slot] is FireGrenade)
            {
                ThrowGrenade<FireGrenade>(slot);
            }
        }
    }

    private void ThrowGrenade<T>(int slot) where T : Grenade
    {
        T item = (T)items[slot];
        StartCooldown(slot, item);
        Vector2 dir = main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        Vector3 offset = new Vector2(0.1f, 0) * transform.right;
        item.Throw(transform.position + offset, dir.normalized);
        item.Explode();
    }

    void StartCooldown(int slot, Item item)
    {
        isUsing[slot] = true;
        timer[slot] = item.cooldownTime;
    }

    public void AddItem(Item item, int slot)
    {
        if (item == items[0] || item == items[1])
        {
            Debug.LogWarning("Can't assign the same item!");
            return;
        }
        items[slot] = item;
    }

    public void RemoveItem(int slot)
    {
        items[slot] = null;
    }
}
