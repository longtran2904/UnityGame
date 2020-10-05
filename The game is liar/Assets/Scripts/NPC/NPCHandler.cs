using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHandler : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector2 size = new Vector2(5, 5);
        Physics2D.OverlapBox(transform.position, size, 0, LayerMask.GetMask("NPC"));
    }
}
