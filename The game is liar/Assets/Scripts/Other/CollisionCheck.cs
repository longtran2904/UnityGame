using System;
using UnityEngine;
using UnityEngine.Events;

public class CollisionCheck : MonoBehaviour
{
    public UnityEvent collisionEvent;
    public Action<Collider2D> onTriggerEnterFunc;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onTriggerEnterFunc?.Invoke(collision);
        collisionEvent?.Invoke();
    }
}