using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TextType = TMPro.TextMeshProUGUI;

public class GameUI : MonoBehaviour
{
    public bool displayHealthBar;
    public bool displayMoneyCount;
    public bool displayWaveCount;
    public bool displayWeaponUI;
    public bool displayMinimap;
    public bool displayTextbox;
    
    [Header("Health Bar")]
    [ShowWhen("displayHealthBar")] public Slider healthSlider;
    private TextType healthText;
    
    [Header("Money Count")]
    [ShowWhen("displayMoneyCount")] public IntVariable money;
    [ShowWhen("displayMoneyCount")] public TextType moneyText;
    
    [Header("Wave Count")]
    [ShowWhen("displayWaveCount")] public TextType waveText;
    [ShowWhen("displayWaveCount")] public TextType enemyText;
    
    [Header("Weapon UI")]
    [ShowWhen("displayWeaponUI")] public Image weaponImage;
    private WeaponController weaponController;
    private TextType ammoText;
    
    [Header("Reload UI")]
    public RangedFloat perfectBarPos;
    public Color failedColor;
    public Color perfectColor;
    public GameObject reloadCanvas;
    
    private Slider reloadSlider;
    private Image reloadHandle;
    private RectTransform perfectBar;
    
    [Header("Minimap")]
    [ShowWhen("displayMinimap")] public RawImage minimap;
    [ShowWhen("displayMinimap")] public RawImage map;
    
    [Header("Textbox")]
    [ShowWhen("displayTextbox")] public GameObject textboxCanvas;
    [ShowWhen("displayTextbox")] public float radius;
    [ShowWhen("displayTextbox")] public WeaponInventory inventory;
    
    private GameObject closestObj;
    private GameObject lastObj;
    
    private Transform weaponHolder;
    private DialogueBox textbox;
    private Collider2D[] overlapObjects = new Collider2D[10];
    private TextboxTrigger trigger;
    private Queue<string> sentences = new Queue<string>();
    private bool inDialogue;
    
    // Start is called before the first frame update
    void Start()
    {
        healthSlider.gameObject.SetActive(displayHealthBar);
        healthText = healthSlider.GetComponentInChildren<TextType>();
        
        moneyText.gameObject.SetActive(displayMoneyCount);
        
        waveText.gameObject.SetActive(false);
        enemyText.gameObject.SetActive(false);
        
        weaponImage.gameObject.SetActive(displayWeaponUI);
        if (displayWeaponUI)
        {
            weaponController = FindObjectOfType<WeaponController>();
            ammoText = weaponImage.GetComponentInChildren<TextType>();
        }
        
        reloadCanvas = Instantiate(reloadCanvas);
        reloadCanvas.SetActive(false);
        reloadSlider = reloadCanvas.GetComponentInChildren<Slider>(true);
        reloadHandle = reloadSlider.transform.Find("Handle").GetComponent<Image>();
        perfectBar = (RectTransform)reloadSlider.transform.Find("Perfect");
        
        minimap.gameObject.SetActive(displayMinimap);
        map.gameObject.SetActive(false);
        GameInput.BindEvent(GameEventType.NextRoom, room => room.GetChild(0).FindChildWithLayer("LevelMap").SetActive(true));
        
        textboxCanvas = Instantiate(textboxCanvas);
        textboxCanvas.SetActive(false);
        textbox = textboxCanvas.GetComponentInChildren<DialogueBox>();
        //weaponHolder = GameObject.FindGameObjectWithTag("Player")?.transform.GetChild(0);
        weaponHolder = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (displayHealthBar)
        {
            int health = (int)Mathf.Clamp(GameManager.player.health, 0, healthSlider.maxValue);
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
            weaponImage.sprite = weaponController.inventory.current.stat.icon;
            ammoText.text = $"{weaponController.inventory.current.currentAmmo}/{weaponController.inventory.current.stat.ammo}";
        }
        
        if (GameManager.player)
        {
            reloadCanvas.transform.position = GameManager.player.transform.position;
            if (GameManager.player.transform.up != reloadCanvas.transform.up)
                reloadCanvas.transform.Rotate(180, 0, 0);
        }
        
        if (displayMinimap && GameInput.GetInput(InputType.Map))
        {
            minimap.gameObject.SetActive(!minimap.gameObject.activeSelf);
            map.gameObject.SetActive(!map.gameObject.activeSelf);
        }
        
        if (displayTextbox)
        {
            if (inDialogue)
            {
                if (GameInput.GetRawInput(InputType.NextDialogue))
                {
                    if (sentences.Count == 0)
                    {
                        inDialogue = false;
                        lastObj = null; // This will make closestObj != lastObj and will show the "Press F to talk" textbox properly
                        textboxCanvas.SetActive(false);
                        GameInput.EnableAllInputs(true);
                        return;
                    }
                    textbox.ShowDialogue(null, sentences.Dequeue());
                }
                return;
            }
            
            closestObj = GameUtils.GetClosestCollider(weaponHolder.position, radius, overlapObjects, LayerMask.GetMask("HasTextbox"))?.gameObject;
            if (closestObj != lastObj)
                if (ShowTextbox(trigger = closestObj?.GetComponent<TextboxTrigger>(), textbox, textboxCanvas))
                    lastObj = closestObj;
            
            if (trigger && GameInput.GetInput(InputType.Interact))
            {
                trigger.trigger?.Invoke();
                
                switch (trigger.textboxType)
                {
                    case TextboxType.Dialogue:
                    {
                        Dialogue dialogue = trigger.GetRandomDialogue();
                        foreach (var sentence in dialogue.dialogues)
                            sentences.Enqueue(sentence);
                        inDialogue = true;
                        textbox.ShowDialogue(null, sentences.Dequeue());
                        GameInput.EnableAllInputs(false);
                    } break;
                    case TextboxType.Chest:
                    {
                        int randomWeapon = Random.Range(0, trigger.chestData.inventory.items.Count);
                        trigger.chestData.inventory.SpawnAndDropWeapon(randomWeapon, trigger.transform.position, trigger.GetRandomDir(), weaponHolder);
                        closestObj.layer = 0;
                        Destroy(trigger);
                        textboxCanvas.SetActive(false);
                    } break;
                    case TextboxType.Weapon:
                    {
                        inventory.SwapCurrent(new Vector2(weaponHolder.right.x, 1) * 10, trigger.weapon, weaponHolder);
                        textboxCanvas.SetActive(false);
                    } break;
                }
            }
        }
    }
    
    static bool ShowTextbox(TextboxTrigger trigger, DialogueBox textbox, GameObject textboxCanvas)
    {
        if (trigger)
        {
            Vector3 textboxPos = trigger.transform.position;
            switch (trigger.textboxType)
            {
                case TextboxType.None:
                return false; // NOTE: We never update the lastObj if the type is None
                case TextboxType.Dialogue:
                {
                    textbox.ShowDialogue(null, "Press F to talk");
                } break;
                case TextboxType.Chest:
                {
                    textbox.ShowDialogue(null, "Press F to open");
                } break;
                case TextboxType.Weapon:
                {
                    WeaponStat stat = trigger.weapon.stat;
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    builder.Append("Damage: ").Append(stat.damage).Append("\nCritical: ").Append(stat.critDamage)
                        .Append("\nFire rate:").Append(stat.fireRate).Append("\nPress F to change weapon");
                    textbox.ShowDialogue("Name: " + stat.weaponName, builder.ToString());
                    textboxPos = trigger.hitGroundPos;
                } break;
            }
            textboxCanvas.transform.position = textboxPos + (Vector3)trigger.textboxOffset;
        }
        
        textboxCanvas.SetActive(trigger);
        return true;
    }
    
    public void EnableReload(bool enable, float reloadTime)
    {
        reloadCanvas.gameObject.SetActive(enable);
        if (enable)
        {
            reloadSlider.maxValue = reloadTime;
            reloadSlider.value = 0;
            reloadHandle.color = Color.white;
            perfectBar.anchoredPosition = new Vector2(perfectBarPos.randomValue * ((RectTransform)reloadSlider.transform).sizeDelta.x, 0);
        }
    }
    
    public bool UpdateReload(float value, bool hasReloaded)
    {
        reloadSlider.value = value;
        bool isPerfect = false;
        if (hasReloaded)
        {
            float t = reloadSlider.normalizedValue * ((RectTransform)reloadSlider.transform).sizeDelta.x;
            isPerfect = MathUtils.RangeInRange(perfectBar.anchoredPosition.x, perfectBar.sizeDelta.x, t, reloadHandle.rectTransform.sizeDelta.x);
            reloadHandle.color = isPerfect ? perfectColor : failedColor;
        }
        return isPerfect;
    }
}
