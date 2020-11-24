using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class WeaponManager : MonoBehaviour
{
    Weapon weapon;
    Weapon lastWeapon;
    public GameObject textBox;
    private GameObject textboxObj;
    Vector3 offset;
    float delay;
    Player player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (weapon)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeWeapon();
            }

            // Delay the textbox .5f seconds for the drop gun to falling
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                textboxObj.SetActive(false);
                return;
            }

            if (weapon != lastWeapon)
            {
                Destroy(textboxObj);
                DisplayUI();
            }
            else if (!textboxObj.activeSelf)
            {
                textboxObj.transform.position = weapon.transform.position + offset;
                textboxObj.SetActive(true);
            }
        }
        else if (textboxObj)
            textboxObj.SetActive(false);
        weapon = null;
    }

    public void UpdateWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }

    void DisplayUI()
    {
        offset = new Vector3(0, 1.75f);
        textboxObj = Instantiate(textBox, weapon.transform.position + offset, Quaternion.identity);
        TextMeshProUGUI text = textboxObj.GetComponentInChildren<TextMeshProUGUI>();
        StringBuilder builder = new StringBuilder();
        builder.Append(weapon.stat.weaponName).Append('\n').Append(weapon.stat.description);
        text.text = builder.ToString();
        lastWeapon = weapon;
    }

    void ChangeWeapon()
    {
        if (weapon.CompareTag("SellWeapon"))
        {
            if (weapon.stat.price > GetComponentInParent<Player>().money)
            {
                return;
            }
            else
            {
                AudioManager.instance.Play("Buy");
                GetComponentInParent<Player>().money -= weapon.stat.price;
            }
        }
        int current = GetComponent<WeaponSwitching>().selectedWeapon;
        Transform last = transform.GetChild(current);
        weapon.transform.position = last.position;
        weapon.transform.localScale = last.lossyScale;
        Destroy(weapon.GetComponent<BoxCollider2D>());
        Destroy(weapon.GetComponent<Rigidbody2D>());
        DropWeapon(current);
        weapon.transform.SetParent(transform, true);
        weapon.transform.SetSiblingIndex(current);
        weapon.tag = "Weapon";
        foreach (var component in weapon.GetComponents<MonoBehaviour>())
        {
            component.enabled = true;
        }
        player.currentWeapon = weapon;
        delay = .5f;
    }

    void DropWeapon(int index)
    {
        GameObject dropWeapon = transform.GetChild(index).gameObject;
        dropWeapon.transform.parent = null;
        dropWeapon.AddComponent<BoxCollider2D>();
        Rigidbody2D rb = dropWeapon.AddComponent<Rigidbody2D>();
        rb.gravityScale = 5;
        rb.transform.rotation = transform.rotation;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = new Vector2(transform.right.x, 1) * Vector2.one.normalized * 10;
        foreach (var behaviour in dropWeapon.GetComponents<MonoBehaviour>())
        {
            behaviour.enabled = false;
        }
    }
}
