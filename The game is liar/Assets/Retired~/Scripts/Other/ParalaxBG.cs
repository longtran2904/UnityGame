using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParalaxBG : MonoBehaviour
{
    public float speed;

    public Transform other;

    public float distanceX;
    public float endX;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x <= endX)
        {
            Vector2 pos = new Vector2(distanceX + other.position.x, transform.position.y);
            transform.position = pos;
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }
    }
}
