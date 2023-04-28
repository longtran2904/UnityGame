using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponStat stat;
    public GameObject muzzleFlash;
    public Vector2 posOffset;

    public int currentAmmo;
    [HideInInspector] public Transform shootPos;

    public bool moveUpAndDownWhenDrop;
    [ShowWhen("moveUpAndDownWhenDrop")] public float speed;
    [ShowWhen("moveUpAndDownWhenDrop")] public float halfDistance;
    [ShowWhen("moveUpAndDownWhenDrop")] public RangedFloat waitTime;

    private bool isMoving;
    private Rigidbody2D rb;
    private BoxCollider2D box;
    private TextboxTrigger trigger;

    // RANT: What I'm trying to do here is to just know when the collider is hitting the ground (meaning the tag/layer is "Ground" and the normal surface is Vector2.up).
    // Obviously, because Unity is f****** retarded so this little simple task is impossible.
    // Why I was trying to do this in the first place?
    // For example, I may have a grenade that bounces off the wall but when hit an enemy then get stuck to it at a "correct" and "precise" position.
    // Or I can have a molotov that bounces around walls but explode when hit the ground (doesn't count the roof or the diagonal tiles).
    // Or literally right this instance, I'm trying to have dropped guns to bounce back when hitting the wall but stop moving and change their bodyType when hit the ground.
    // Another example I can give is when I shoot a projectile and it hit a wall, I want the particle effect's rotation to be like the collided surface's normal.
    // So as anyone can see (why am I talking to myself?), the most basic and simple task is literally impossible for Unity. God, I hate it so much.
    //  ---------------------------------------
    // I tried to use ContactPoint2D but:
    //  - Sometimes it wasn't correct (the normal and the point). It's also pretty limited in what you can do with it.
    //  - The collision.contacts array always has several contact points (when I tested it had 4). Please don't ask me why it is and which contact should I use.
    //  - All the OnCollisionX2D always get called after Unity handles its collision so I can't get the correct position or stop it before it bounces back.
    // I tried to use FixedUpdate (or yield return new WaitForFixedUpdates() in coroutine) and manually cast the box.
    // It worked in some instances but overall it wasn't great:
    //  - Occasionally, it misses when the object moving or bouncing really fast.
    //  - What should the size.x be? Maybe it should be less than the object's bounds but sometimes when the object moves at high speed a large chunk of it will be inside the walls.
    //  - What should the size.y be? Maybe it should be extremely small but then sometimes there are situations where the box is too deep in the ground and you have custom physics material.
    // Unity can't understand that anybody needs the collision to happen differently on each dir/axis.
    // Another thing that Unity also f*** up is the ability to be triggered to some layers but normal to other layers.
    // Even the idea of splitting stuff into trigger and non-trigger is also pretty stupid.
    // In conclusion, Unity's collision system is quite limited and useless, and its API is f****** retarded. I probably should make my own physics system soon.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // NOTE: This still doesn't fucking work, sometimes the gun still gets stuck to the wall
        ContactPoint2D contact = collision.GetContact(0);
        if (contact.normal == Vector2.up && contact.collider.CompareTag("Ground"))
        {
            trigger.hitGroundPos = transform.position;
            trigger.textboxType = TextboxType.Weapon;
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            if (moveUpAndDownWhenDrop)
            {
                isMoving = true;
                trigger.hitGroundPos += new Vector3(0, halfDistance);
                StartCoroutine(UpAndDown(halfDistance, trigger.hitGroundPos.y));

                System.Collections.IEnumerator UpAndDown(float halfDistance, float center)
                {
                    yield return new WaitForSeconds(waitTime.randomValue);
                    Vector2 dir = Vector2.up;
                    while (isMoving)
                    {
                        if (transform.position.y >= center + halfDistance)
                            dir = Vector2.down;
                        else if (transform.position.y <= center - halfDistance)
                            dir = Vector2.up;
                        rb.velocity = dir * speed;
                        yield return null;
                    }
                }
            }
        }
    }

    public Weapon Init()
    {
        currentAmmo = stat.ammo;
        shootPos = transform.Find("ShootPos");
        muzzleFlash = transform.Find("MuzzleFlash").gameObject;
        Debug.Assert(transform.parent == null);
        rb = GetComponent<Rigidbody2D>();
        // NOTE: For some stupid reason, setting the bodyType to Static rather than Kinematic still make it move for the first 2 frames when the game starts.
        rb.bodyType = RigidbodyType2D.Kinematic;
        box = gameObject.AddComponent<BoxCollider2D>();
        trigger = GetComponent<TextboxTrigger>();
        trigger.weapon = this;

        return this;
    }

    public void Drop(Transform parent, Vector2 dropDir)
    {
        Prepare(true, parent);
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = dropDir;
        trigger.textboxType = TextboxType.None;
    }

    public void Pickup(Transform parent)
    {
        Prepare(false, parent);
        isMoving = false;
    }

    void Prepare(bool drop, Transform parent)
    {
        box.enabled = drop;
        if (parent)
            ResetPosToTransform(parent);
    }

    public void ResetPosToTransform(Transform holder)
    {
        transform.SetPositionAndRotation(GameUtils.GetDirectionalPos(holder, posOffset), holder.rotation);
    }
}