using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// There is only one TextboxHandler and is on the player
public class TextboxHandler : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent resetEvent;
    public UnityEngine.Events.UnityEvent updateEvent;

    public static GameObject closestObj;
    public static GameObject lastObj;

    private void Start()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("HasTextbox"));
    }

    private void Update()
    {
        Vector2 size = new Vector2(5, 5);
        closestObj = GetClosestObject(Physics2D.OverlapBoxAll(transform.position, size, 0, LayerMask.GetMask("HasTextbox")));
        ExtDebug.DrawBox(transform.position, size / 2, Quaternion.identity, Color.green);
        if (lastObj != closestObj)
        {
            resetEvent.Invoke();
            if (closestObj)
            {
                updateEvent.Invoke();
                lastObj = closestObj;
            }
        }
    }

    GameObject GetClosestObject(Collider2D[] colliders)
    {
        if (colliders.Length == 0)
        {
            return null;
        }
        int closest = 0;
        for (int i = 0; i < colliders.Length; i++)
        {
            bool closer = (transform.position + colliders[i].transform.position).sqrMagnitude < (transform.position + colliders[closest].transform.position).sqrMagnitude;
            if (closer)
            {
                closest = i;
            }
        }
        return colliders[closest].gameObject;
    }
}
