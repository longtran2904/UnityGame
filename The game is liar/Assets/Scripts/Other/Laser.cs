using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour
{
    public int damage;
    public bool willTurnOff;

    public float timeOn;
    public float timeOff;
    public float offsetTime;

    public Transform from;
    public Transform to;
    private Animator laserAnim;
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        laserAnim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        SetupLaser();
        if (willTurnOff)
        {
            StartCoroutine(ControlLaser());
        }
    }

    IEnumerator ControlLaser()
    {
        yield return new WaitForSeconds(offsetTime);

        while (true)
        {
            yield return new WaitForSeconds(timeOn);
            gameObject.SetActive(false);
            yield return new WaitForSeconds(timeOff);
            gameObject.SetActive(true);
            laserAnim.Play("laser");
        }
    }

    void SetupLaser()
    {
        if (!from || !to)
            return;
        bool isRotated = false;
        Quaternion rotation = transform.rotation;
        if (transform.rotation != Quaternion.identity)
        {
            isRotated = true;
            transform.rotation = Quaternion.identity;
        }
        Vector3 pos = MathUtils.Average(from.position, to.position);
        Vector3 scale = new Vector3(5, Mathf.Abs(to.position.y - from.position.y) * BigBrain.distanceToSizeRatio, 1);
        transform.position = pos;
        transform.localScale = scale;
        if (isRotated)
        {
            transform.rotation = rotation;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.Hurt(damage);
        }
    }
}
