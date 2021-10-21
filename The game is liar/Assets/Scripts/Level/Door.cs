using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool canOpen;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canOpen)
        {
            anim.SetBool("Open", true);
        }
    }

    // Destroy the door after the animation. Get call in animation event
    void DisableDoor()
    {
        Destroy(gameObject);
    }
}
