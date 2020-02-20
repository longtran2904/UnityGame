using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public GameObject player;
    public float timeOffset;
    public Vector2 posOffset;

    public Vector2 leftAndUpLimit;
    public Vector2 rightAndBottomLimit;

    // Update is called once per frame
    void Update()
    {
        // Camera current position
        Vector3 starPos = transform.position;

        // Player current position with offset
        Vector3 endPos = player.transform.position;
        endPos.x += posOffset.x;
        endPos.y += posOffset.y;
        endPos.z = -10;
        
        // Smoothly move the camera to the player position
        transform.position = Vector3.Lerp(starPos, endPos, timeOffset * Time.deltaTime);

        transform.position = new Vector3
        (
        Mathf.Clamp(transform.position.x, leftAndUpLimit.x, rightAndBottomLimit.x),
        Mathf.Clamp(transform.position.y, rightAndBottomLimit.y, leftAndUpLimit.y),
        transform.position.z
        );
    }
}
