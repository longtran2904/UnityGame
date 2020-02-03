using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    Camera mainCamera;

    public float offset;

    public GameObject projectilePrefab;
    public Transform shotPos;

    private float timeBtwShots;
    public float startTimeBtwShots;

    PlayerController player;

    public float damage;
    private Projectile projectile;

    public GameObject muzzleFlash;
    public float muzzelFlashTime;
    private float muzzelFlashTimeValue;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        player = GetComponentInParent<PlayerController>();
        muzzelFlashTimeValue = muzzelFlashTime;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 difference = mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        FlipWeapon(rotationZ);

        ShootProjectile();
    }

    // flip the weapon correctly toward the mouse position
    void FlipWeapon(float rotZ)
    {
        if (player.top)
        {
            if (rotZ <= 90 && rotZ >= -90)
            {
                transform.rotation = Quaternion.Euler(180f, 0f, -(rotZ + offset));
            }
            else if (rotZ > 90 || rotZ < -90)
            {
                transform.rotation = Quaternion.Euler(180f, 180f, (rotZ + offset - 180));
            }
        }
        else
        {
            if (rotZ <= 90 && rotZ >= -90)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
            }
            else if (rotZ > 90 || rotZ < -90)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, -(rotZ + offset - 180));
            }
        }
    }


    // shoot projectile toward the mouse position
    void ShootProjectile()
    {
        if (muzzelFlashTimeValue <= 0)
        {
            muzzleFlash.SetActive(false);
            muzzelFlashTimeValue = muzzelFlashTime;
        }
        if (muzzleFlash.activeSelf == true)
        {
            muzzelFlashTimeValue -= Time.deltaTime;
        }
        if (timeBtwShots <= 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Instantiate(projectilePrefab, shotPos.position, transform.rotation);
                projectile = projectilePrefab.gameObject.GetComponent<Projectile>();
                projectile.damage = damage;
                timeBtwShots = startTimeBtwShots;
                muzzleFlash.SetActive(true);
                muzzelFlashTimeValue = muzzelFlashTime;
            }
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }
    }
}
