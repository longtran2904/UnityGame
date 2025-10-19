using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothDampTest : MonoBehaviour
{
    public float distance;
    public float smoothTime;

    private Transform position;
    private Rigidbody2D velocity;
    private Vector3 currentVelocity1;
    private Vector2 currentVelocity2;

    // Start is called before the first frame update
    void Start()
    {
        position = transform.GetChild(0);
        velocity = transform.GetChild(1).GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        position.position = Vector3.SmoothDamp(position.position, new Vector3(distance, position.position.y), ref currentVelocity1, smoothTime);
        Vector2.SmoothDamp(velocity.position, new Vector2(distance, velocity.position.y), ref currentVelocity2, smoothTime);
        velocity.velocity = currentVelocity2;
    }

    [EasyButtons.Button]
    void Restart()
    {
        position.position = new Vector3(0, position.position.y);
        velocity.position = new Vector2(0, velocity.position.y);
    }
}
