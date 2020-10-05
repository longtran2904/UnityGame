using UnityEngine;

public class Laser : MonoBehaviour
{
    public int damage;
    public bool willTurnOff;
    public float timeOn;
    public float timeOff;
    public float offsetTimer;
    public GameObject laserBeam;
    public Transform from;
    public Transform to;

    private float timeOnValue;
    private float timeOffValue;
    private bool isOn = false;
    private bool canStart = false;
    private Animator laserAnim;
    private BoxCollider2D laserCollider;

    // Start is called before the first frame update
    void Start()
    {
        isOn = true;
        timeOnValue = timeOn;
        timeOffValue = timeOff;
        laserAnim = laserBeam.GetComponent<Animator>();
        laserCollider = GetComponentInChildren<BoxCollider2D>();
        Invoke("CanStart", offsetTimer);
        SetupLaser();
    }

    // Update is called once per frame
    void Update()
    {
        if (!willTurnOff)
        {
            return;
        }

        if (!canStart)
        {
            return;
        }

        if (isOn)
        {
            if (Time.time > timeOnValue)
            {
                isOn = false;
                timeOffValue = Time.time + timeOff;
                laserBeam.SetActive(false);
            }
        }
        else
        {
            if (Time.time > timeOffValue)
            {
                isOn = true;
                timeOnValue = Time.time + timeOn;
                laserBeam.SetActive(true);
                laserAnim.Play("laser");
            }
        }
    }

    void CanStart()
    {
        canStart = true;
    }

    void SetupLaser()
    {
        bool isRotated = false;
        Quaternion rotation = transform.rotation;
        if (transform.rotation != Quaternion.identity)
        {
            isRotated = true;
            transform.rotation = Quaternion.identity;
        }
        Vector3 pos = MathUtils.Average(from.position, to.position);
        Vector3 scale = new Vector3(5, Mathf.Abs(to.position.y - from.position.y) * BigBrain.distanceToSizeRatio, 1);
        laserBeam.transform.position = pos;
        laserBeam.transform.localScale = scale;
        if (isRotated)
        {
            transform.rotation = rotation;
        }
    }
}
