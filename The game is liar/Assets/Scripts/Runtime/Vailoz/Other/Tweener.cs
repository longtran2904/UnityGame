using UnityEngine;

public enum TweenType
{
    Move,
    MoveX,
    MoveY,
    Scale,
    ScaleX,
    ScaleY,
    Rotate,
}

public class Tweener : MonoBehaviour
{
    public TweenType tweenType;
    public LeanTweenType easeType;
    public float duration;
    public float delay;

    public bool loop;
    public bool pingpong;

    [ShowWhen("tweenType", new object[] { TweenType.MoveX, TweenType.ScaleX })]
    public float toX;
    [ShowWhen("tweenType", new object[] { TweenType.MoveY, TweenType.ScaleY })]
    public float toY;
    [ShowWhen("tweenType", new object[] { TweenType.Scale, TweenType.Move })]
    public Vector2 to;
    [ShowWhen("tweenType", TweenType.Rotate)]
    public float rotation;

    public UnityEngine.Events.UnityEvent onComplete;

    private LTDescr descr;

    // Start is called before the first frame update
    void Start()
    {
        switch (tweenType)
        {
            case TweenType.Move:
                {
                    descr = LeanTween.move(gameObject, to, duration);
                } break;
            case TweenType.MoveX:
                {
                    descr = LeanTween.moveX(gameObject, toX, duration);
                } break;
            case TweenType.MoveY:
                {
                    descr = LeanTween.moveY(gameObject, toY, duration);
                } break;
            case TweenType.Scale:
                {
                    descr = LeanTween.scale(gameObject, to, duration);
                } break;
            case TweenType.ScaleX:
                {
                    descr = LeanTween.scaleX(gameObject, toX, duration);
                } break;
            case TweenType.ScaleY:
                {
                    descr = LeanTween.scaleY(gameObject, toY, duration);
                } break;
            case TweenType.Rotate:
                {
                    descr = LeanTween.rotate(gameObject, new Vector3(0, 0, rotation), duration);
                } break;
        }

        descr.setDelay(delay).setEase(easeType).setOnComplete(() => onComplete?.Invoke());
        if (loop)
            descr.loopCount = int.MaxValue;
        if (pingpong)
            descr.setLoopPingPong();
    }
}
