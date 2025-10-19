using UnityEngine;
using System;

public class CameraFollow2D : MonoBehaviour
{
    public bool disable;

    private Vector3Variable playerPos;
    public float viewDistance;

    private Vector2 smoothTime;
    private Vector2 velocity;
    private Vector2 leftAndBottomLimit;
    private Vector2 rightAndUpLimit;
    [HideInInspector] public Vector2 cameraOffset;

    private Vector3 startPos;
    private Vector3 endPos;

    private bool useSmoothDamp;
    private bool moveAutomatically;
    private float t;
    private int turn;
    private float waitTime;
    private bool waiting;
    private Action<CameraFollow2D> done;

    void LateUpdate()
    {
        if (disable)
            return;
        if (waiting)
            return;

        if (moveAutomatically)
        {
            if (MathUtils.InRange(transform.position, endPos, .01f))
            {
                transform.position = endPos;
                ++turn;
                waiting = true;

                this.InvokeAfter(waitTime, () =>
                {
                    waiting = false;
                    MathUtils.Swap(ref startPos, ref endPos);
                    if (turn % 2 == 0)
                        done?.Invoke(this);
                });
            }
        }
        else
        {
            endPos = playerPos.value;
            if (GameInput.IsMouseOnScreen())
                endPos += (Vector3)GameInput.GetMouseDir() * viewDistance;
        }

        if (useSmoothDamp)
            transform.position = MathUtils.SmoothDamp(transform.position, endPos, ref velocity, smoothTime, Time.deltaTime, -10);
        else
            transform.position = Vector2.Lerp(startPos, endPos, t += velocity.magnitude * Time.deltaTime);
        transform.position = MathUtils.Clamp(transform.position, leftAndBottomLimit, rightAndUpLimit, -10);
    }

    public void InitManual(Vector2 camOffset, bool useSmoothDamp, Vector2 value, Vector3Variable playerPos)
    {
        Init(camOffset, useSmoothDamp, value);
        this.playerPos = playerPos;
        GameInput.BindEvent(GameEventType.NextRoom, room => ToNextRoom(GameManager.GetBoundsFromRoom(room).ToBounds()));
    }

    public void InitAutomatic(Vector2 camOffset, bool useSmoothDamp, Vector2 value, float waitTime, Action<CameraFollow2D> done)
    {
        Init(camOffset, useSmoothDamp, value);
        moveAutomatically = true;
        this.waitTime = waitTime;
        this.done = done;
    }

    void Init(Vector2 camOffset, bool useSmoothDamp, Vector2 value)
    {
        cameraOffset = camOffset;
        this.useSmoothDamp = useSmoothDamp;
        if (useSmoothDamp)
            smoothTime = value;
        else
            velocity = value;
    }

    public void ToNextRoom(Bounds bounds)
    {
        velocity = Vector2.zero;
        leftAndBottomLimit = bounds.min + (Vector3)cameraOffset;
        rightAndUpLimit = bounds.max - (Vector3)cameraOffset;
        Debug.Assert((leftAndBottomLimit.x <= rightAndUpLimit.x) && (leftAndBottomLimit.y <= rightAndUpLimit.y),
            $"Camera's limit is wrong: Low: {leftAndBottomLimit}, High: {rightAndUpLimit}, Bounds: {bounds}");
        if (moveAutomatically)
        {
            startPos = leftAndBottomLimit;
            endPos = rightAndUpLimit;
            t = 0;
        }
    }
}
