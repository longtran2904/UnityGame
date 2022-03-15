using UnityEngine;
using UnityEngine.UI;
using TextType = TMPro.TextMeshProUGUI;

public class GameUI : MonoBehaviour
{
    // Game menu:

    public bool displayHealthBar;
    public bool displayMoneyCount;
    public bool displayWaveCount;
    public bool displayWeaponUI;
    public bool displayMinimap;

    [Header("Health Bar")]
    [ShowWhen("displayHealthBar")] public IntVariable playerHealth;
    [ShowWhen("displayHealthBar")] public Slider healthSlider;
    private TextType healthText;

    [Header("Money Count")]
    [ShowWhen("displayMoneyCount")] public IntVariable money;
    [ShowWhen("displayMoneyCount")] public TextType moneyText;

    [Header("Wave Count")]
    [ShowWhen("displayWaveCount")] public TextType waveText;
    [ShowWhen("displayWaveCount")] public TextType enemyText;

    [Header("Weapon UI")]
    [ShowWhen("displayWeaponUI")] public WeaponInventory inventory;
    [ShowWhen("displayWeaponUI")] public Image weaponImage;
    private TextType ammoText;

    [Header("Minimap")]
    [ShowWhen("displayMinimap")] public RawImage minimap;
    [ShowWhen("displayMinimap")] public RawImage map;

    // Start is called before the first frame update
    void Start()
    {
        healthSlider.gameObject.SetActive(displayHealthBar);
        if (displayHealthBar)
        {
            healthText = healthSlider.GetComponentInChildren<TextType>();
            healthSlider.maxValue = playerHealth.value;
        }

        moneyText.gameObject.SetActive(displayMoneyCount);

        waveText.gameObject.SetActive(false);
        enemyText.gameObject.SetActive(false);

        weaponImage.gameObject.SetActive(displayWeaponUI);
        if (displayWeaponUI)
            ammoText = weaponImage.GetComponentInChildren<TextType>();

        minimap.gameObject.SetActive(displayMinimap);
        map.gameObject.SetActive(false);
        GameInput.BindEvent(GameEventType.NextRoom, room => room.GetChild(0).FindChildWithLayer("LevelMap").SetActive(true));
    }

    // Update is called once per frame
    void Update()
    {
        if (displayHealthBar)
        {
            int health = (int)Mathf.Clamp(playerHealth.value, 0, healthSlider.maxValue);
            healthSlider.value = health;
            healthText.text = health.ToString();
        }

        if (displayMoneyCount)
            moneyText.text = money.value.ToString();

        if (displayWaveCount)
        {
            if (EnemySpawner.totalWaves > 0)
            {
                waveText.gameObject.SetActive(true);
                enemyText.gameObject.SetActive(true);
                waveText.text = $"Waves: {EnemySpawner.waveCount}/{EnemySpawner.totalWaves}";
                enemyText.text = $"Number of enemies left: {Enemy.numberOfEnemiesAlive}";
            }
            else
            {
                waveText.gameObject.SetActive(false);
                enemyText.gameObject.SetActive(false);
            }
        }

        if (displayWeaponUI)
        {
            weaponImage.sprite = inventory.GetCurrent().stat.icon;
            ammoText.text = $"{inventory.GetCurrent().currentAmmo}/{inventory.GetCurrent().stat.ammo}";
        }

        if (displayMinimap && GameInput.GetInput(InputType.Map))
        {
            minimap.gameObject.SetActive(!minimap.gameObject.activeSelf);
            map.gameObject.SetActive(!map.gameObject.activeSelf);
        }
    }
}
