using UnityEngine.UI;
using UnityEngine;

public class WeaponUI : MonoBehaviour
{
    Player player;
    Image weaponImage;

    // Start is called before the first frame update
    void Start()
    {
        weaponImage = GetComponent<Image>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        weaponImage.sprite = player.inventory.GetCurrent().stat.icon;
    }
}
