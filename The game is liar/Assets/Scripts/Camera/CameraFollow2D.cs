using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraFollow2D : MonoBehaviour
{
    public GameObject player;
    public float timeOffset;
    public Vector2 posOffset;

    public Vector2 leftAndBottomLimit;
    public Vector2 rightAndUpLimit;

    [HideInInspector] public CameraInfo[] cameraInfos;
    [HideInInspector] public Bounds[] roomsBounds;
    [HideInInspector] public Transform[] roomsPos;

    public event Action<Bounds> hasPlayer;

    // Update is called once per frame
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

        ToNextRoom(cameraInfos, roomsBounds, roomsPos);
    }

    public void ToNextRoom(CameraInfo[] _infos, Bounds[] _roomsBounds, Transform[] _roomsPos)
    {
        if (_infos == null)
        {
            return;
        }

        int x = 0;

        foreach (Bounds bounds in _roomsBounds)
        {
            ExtDebug.DrawBox(bounds.center, bounds.extents, Quaternion.identity, Color.cyan);
            if (bounds.Contains(player.transform.position))
            {
                leftAndBottomLimit = _infos[x].leftAndBottomLimit + (Vector2)_roomsPos[x].position;

                rightAndUpLimit = _infos[x].rightAndUpLimit + (Vector2)_roomsPos[x].position;

                _roomsPos[x].Find("Enemies").GetComponent<EnemySpawner>().active = true;

                hasPlayer?.Invoke(bounds);

                break;
            }

            x++;
        }
    }
}
