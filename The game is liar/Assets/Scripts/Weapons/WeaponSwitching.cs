using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public int selectedWeapon;

    // Start is called before the first frame update
    void Start()
    {
        SelectWeapon();
    }

    // Update is called once per frame
    void Update()
    {
        int previousSelectedWeapon = selectedWeapon;
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (!transform.GetChild(selectedWeapon).GetComponent<Weapon>().canSwitch)
        {
            return;
        }

        if (scrollInput > 0)
        {
            if (selectedWeapon >= transform.childCount - 1)
                selectedWeapon = 0;
            else
                selectedWeapon++;
        }

        if (scrollInput < 0)
        {
            if (selectedWeapon <= 0)
                selectedWeapon = transform.childCount - 1;
            else
                selectedWeapon--;
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
                Player.player.currentWeapon = weapon.GetComponent<Weapon>();
            }
            else
                weapon.gameObject.SetActive(false);
            i++;
        }
    }
}
