using System;
using UnityEngine;
using UnityEngine.Events;

public class TriggerCheck : MonoBehaviour
{
    public bool useTag;
    [ShowWhen("useTag")] public string compareTag;

    public UnityEvent collisionEvent;
    public Action<Collider2D> onTriggerEnterFunc;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (useTag && !collision.CompareTag(compareTag)) return;
        onTriggerEnterFunc?.Invoke(collision);
        collisionEvent?.Invoke();
    }
}