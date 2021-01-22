using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public GameObject spawnItem;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void UnlockChest()
    {
        if (TextboxHandler.closestObj.GetComponent<Chest>() == this && Input.GetKeyDown(KeyCode.F))
        {
            GameObject dropItem = Instantiate(spawnItem, transform.position, Quaternion.identity);
            dropItem.GetComponent<Rigidbody2D>().velocity = MathUtils.RandomVector2().normalized * 10;
            AudioManager.instance.Play("");
            anim.Play("");
        }
    }
}
