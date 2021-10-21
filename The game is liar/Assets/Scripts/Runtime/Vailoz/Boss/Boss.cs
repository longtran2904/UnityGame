using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Boss : MonoBehaviour
{
    public int health;
    protected Player player;
    protected SpriteRenderer sr;
    protected Rigidbody2D rb;

    // Start is called before the first frame update
    protected abstract void Start();

    // Update is called once per frame
    protected abstract void Update();

    protected abstract void Die();

    public abstract void Hurt(int _damage);
}
