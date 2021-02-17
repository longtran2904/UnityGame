using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    private Player player;
    private Image weaponImage;
    public TextMeshProUGUI ammoText;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        weaponImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        weaponImage.sprite = player.inventory.GetCurrent().stat.icon;
        ammoText.SetText("{0}/{1}", player.inventory.GetCurrent().currentAmmo, player.inventory.GetCurrent().stat.ammo);
    }
}
