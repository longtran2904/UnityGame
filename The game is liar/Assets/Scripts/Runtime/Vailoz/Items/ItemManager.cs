using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GameObject[] items = new GameObject[2];
    private bool[] isUsing = new bool[2];
    private float[] timer = new float[2];

    // Update is called once per frame
    void Update()
    {
        Cooldown();

        if (GameInput.GetInput(InputType.LeftItem))
            UseItem(0);
        else if (GameInput.GetInput(InputType.RightItem))
            UseItem(1);
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
        Item item = Instantiate(items[slot], transform.position + new Vector3(1 * transform.right.x, 1 * transform.up.y, 0), Quaternion.identity).GetComponent<Item>();
        item.Init();
        isUsing[slot] = true;
        timer[slot] = item.cooldownTime;
    }

    public void AddItem(GameObject item, int slot)
    {
        if (item == items[0] || item == items[1])
        {
            InternalDebug.LogWarning("Can't assign the same item!");
            return;
        }
        items[slot] = item;
    }

    public void RemoveItem(int slot)
    {
        items[slot] = null;
    }
}
