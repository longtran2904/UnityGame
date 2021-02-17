using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class WeaponManager : MonoBehaviour
{
    public GameObject textBox;
    private GameObject textboxObj;

    private Weapon weapon;
    private Weapon lastWeapon;

    public float delay = .5f;
    public IntReference playerMoney;
    public WeaponInventory startInventory;

    private Player player;
    private Vector3 offset;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        foreach (Weapon item in startInventory.items)
        {
            Weapon gun = Instantiate(item, transform.position, Quaternion.identity);
            gun.transform.parent = transform;
            gun.transform.localPosition = item.posOffset;
            player.inventory.items.Add(gun);
        }
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
            }
            else if (weapon != lastWeapon)
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
    }

    // Listen to the TextboxHandler event
    public void UpdateWeapon()
    {
        if (TextboxHandler.closestObj.CompareTag("Weapon") || TextboxHandler.closestObj.CompareTag("SellWeapon"))
            weapon = TextboxHandler.closestObj.GetComponent<Weapon>();
        else
            weapon = null;
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
            if (weapon.stat.price > playerMoney.value)
            {
                return;
            }
            else
            {
                AudioManager.instance.PlaySfx("Buy");
                playerMoney.value -= weapon.stat.price;
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
        player.inventory.AddAndSetCurrent(weapon);
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

        Weapon drop = dropWeapon.GetComponent<Weapon>();
        drop.enabled = false;
        player.inventory.Remove(drop);
    }
}
