using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public GameObject player;
    public float timeOffset;
    public Vector2 posOffset;

    private Vector2 leftAndBottomLimit;
    private Vector2 rightAndUpLimit;

    Camera main;
    public BoundsIntReference bounds;

    private void Start()
    {
        main = Camera.main;
        ToNextRoom();
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
    }

    public void ToNextRoom()
    {
        Vector3 cameraOffset = new Vector3(main.orthographicSize * main.aspect, main.orthographicSize);
        leftAndBottomLimit = bounds.value.min + cameraOffset;
        rightAndUpLimit = bounds.value.max - cameraOffset;
    }
}
