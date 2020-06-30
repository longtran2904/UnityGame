using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public int damage;
    public bool willTurnOff;
    public float timeOn;
    public float timeOff;
    public float offsetTimer;
    public GameObject laserBeam;

    private float timeOnValue;
    private float timeOffValue;
    private bool isOn = false;
    private bool canStart = false;
    private Animator laserAnim;

    // Start is called before the first frame update
    void Start()
    {
        isOn = true;
        timeOnValue = timeOn;
        timeOffValue = timeOff;
        laserAnim = laserBeam.GetComponent<Animator>();
        Invoke("Setup", offsetTimer);
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

    void Setup()
    {
        canStart = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().Hurt(damage);
        }
    }
}
