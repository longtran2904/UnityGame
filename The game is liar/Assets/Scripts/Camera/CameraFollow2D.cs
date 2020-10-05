using UnityEngine;
using System;

public class CameraFollow2D : MonoBehaviour
{
    public GameObject player;
    public float timeOffset;
    public Vector2 posOffset;

    public Vector2 leftAndBottomLimit;
    public Vector2 rightAndUpLimit;

    [HideInInspector] public Bounds[] roomsBounds;
    private Bounds lastRoomBounds;
    Camera main;

    public event Action<Bounds> hasPlayer;

    private void Start()
    {
        main = Camera.main;
    }

    void LateUpdate()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

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
        Mathf.Clamp(transform.position.x, leftAndBottomLimit.x, rightAndUpLimit.x),
        Mathf.Clamp(transform.position.y, leftAndBottomLimit.y, rightAndUpLimit.y),
        transform.position.z
        );

        ToNextRoom(roomsBounds);
    }

    public void ToNextRoom(Bounds[] _roomsBounds)
    {
        int x = 0;
        foreach (Bounds bounds in _roomsBounds)
        {
            ExtDebug.DrawBox(bounds.center, bounds.extents, Quaternion.identity, Color.cyan);
            if (bounds.Contains(player.transform.position) && bounds != lastRoomBounds)
            {
                Vector3 cameraOffset = new Vector3(main.orthographicSize * main.aspect, main.orthographicSize);
                leftAndBottomLimit = bounds.min + cameraOffset;
                rightAndUpLimit = bounds.max - cameraOffset;
                lastRoomBounds = bounds;
                hasPlayer?.Invoke(bounds);
                break;
            }
            x++;
        }
    }
}
