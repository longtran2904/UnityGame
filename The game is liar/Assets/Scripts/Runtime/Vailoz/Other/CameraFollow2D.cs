using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public UnityEngine.Tilemaps.Tilemap tilemap;
    public BoundsIntReference bounds;
    public Vector3Reference playerPos;
    public float smoothTime;

    private Vector3 velocity;
    private Vector2 leftAndBottomLimit;
    private Vector2 rightAndUpLimit;

    Camera main;

    private void Start()
    {
        main = Camera.main;
        if (bounds.useConstant && tilemap)
        {
            tilemap.RefreshAllTiles();
            tilemap.CompressBounds();
            bounds.value = tilemap.cellBounds;
        }
        ToNextRoom();
    }

    void LateUpdate()
    {
        Vector3 endPos = playerPos.value;
        endPos.z = -10;

        transform.position = Vector3.SmoothDamp(transform.position, endPos, ref velocity, smoothTime);        
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
        velocity = Vector3.zero;
        Debug.Assert((leftAndBottomLimit.x <= rightAndUpLimit.x) && (leftAndBottomLimit.y <= rightAndUpLimit.y),
            $"Camera's limit is wrong: Low: {leftAndBottomLimit}, High: {rightAndUpLimit}");
    }
}
