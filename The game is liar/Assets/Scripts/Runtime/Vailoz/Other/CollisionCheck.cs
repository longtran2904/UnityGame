using UnityEngine.Events;
using UnityEngine;
using System;

public class CollisionCheck : MonoBehaviour
{
    public bool useTag;
    [ShowWhen("useTag")] public string compareTag;

    public UnityEvent collisionEvent;
    public Action<Collider2D> onTriggerEnterFunc;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTag && !collision.collider.CompareTag(compareTag)) return;
        onTriggerEnterFunc?.Invoke(collision.collider);
        collisionEvent?.Invoke();
    }
}
