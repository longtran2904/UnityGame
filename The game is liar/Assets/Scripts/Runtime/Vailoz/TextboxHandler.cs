using UnityEngine;

public enum TextboxType
{
    NONE,
    DIALOGUE,
    CHEST,
    WEAPON,

    TEXTBOX_TYPE_COUNT
}

public class TextboxHandler : MonoBehaviour
{
    public GameObject textboxCanvas;
    private DialogueBox textbox;
    public float radius;
    public DropWeapon dropWeapon;

    private GameObject closestObj;
    private GameObject lastObj;

    private Collider2D[] overlapObjects = new Collider2D[10];
    private TextboxTrigger trigger;
    private System.Collections.Generic.Queue<string> sentences = new System.Collections.Generic.Queue<string>();
    private bool inDialogue;
    private Player player;
    private Weapon[] weaponTemplates;
    private ShootAndRotateGun shoot;

    private void Start()
    {
        textboxCanvas = Instantiate(textboxCanvas);
        textboxCanvas.SetActive(false);
        textbox = textboxCanvas.GetComponentInChildren<DialogueBox>();
        player = GetComponentInParent<Player>();
        weaponTemplates = GetComponent<WeaponSwitching>().startInventory.items.ToArray();
        shoot = GetComponent<ShootAndRotateGun>();
    }

    private void Update()
    {
        if (inDialogue)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (sentences.Count == 0)
                {
                    inDialogue = false;
                    lastObj = null; // This will make closestObj != lastObj and will show the "Press F to talk" textbox properly
                    textboxCanvas.SetActive(false);
                    player.EnableInput(true);
                    shoot.delayUntilNextMouseDown = true;
                }
                else
                    textbox.ShowDialogue(null, sentences.Dequeue());
            }
            return;
        }

        closestObj = GetClosestObject(overlapObjects);

        if (closestObj != lastObj)
        {
            if (closestObj)
            {
                trigger = closestObj.GetComponent<TextboxTrigger>();
                Vector3 textboxPos = trigger.transform.position;

                switch (trigger.textboxType)
                {
                    case TextboxType.NONE:
                        return;
                    case TextboxType.DIALOGUE:
                        {
                            textbox.ShowDialogue(null, "Press F to talk");
                        } break;
                    case TextboxType.CHEST:
                        {
                            textbox.ShowDialogue(null, "Press F to open");
                        } break;
                    case TextboxType.WEAPON:
                        {
                            WeaponStat stat = closestObj.GetComponent<DropWeapon>().template.stat;
                            System.Text.StringBuilder builder = new System.Text.StringBuilder();
                            builder.Append("Damage: ").Append(stat.damage).Append("\nCritical: ").Append(stat.critDamage)
                                .Append("\nFire rate:").Append(stat.fireRate).Append("\nPress F to change weapon");
                            textbox.ShowDialogue("Name: " + stat.weaponName, builder.ToString());
                            textboxPos = trigger.hitGroundPos;
                        } break;
                }

                textboxCanvas.transform.position = textboxPos + (Vector3)trigger.textboxOffset;
                textboxCanvas.SetActive(true);
            }
            else
            {
                trigger = null;
                textboxCanvas.SetActive(false);
            }
            lastObj = closestObj;
        }

        if (trigger)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                trigger.trigger?.Invoke();

                switch (trigger.textboxType)
                {
                    case TextboxType.DIALOGUE:
                        {
                            Dialogue dialogue = trigger.GetRandomDialogue();
                            foreach (var sentence in dialogue.dialogues)
                            {
                                sentences.Enqueue(sentence);
                            }
                            inDialogue = true;
                            textbox.ShowDialogue(null, sentences.Dequeue());
                            player.EnableInput(false);
                        } break;
                    case TextboxType.CHEST:
                        {
                            dropWeapon.Drop(trigger.dropData, trigger.transform.position);
                            closestObj.layer = 0;
                            Destroy(trigger);
                            textboxCanvas.SetActive(false);
                        } break;
                    case TextboxType.WEAPON:
                        {
                            // NOTE: One problem with the current take and drop system is that when the player pick up a new weapon,
                            // The weapon will automatically be added to the end of the list. Meaning the order of weapon switching will not be correct.
                            // Ex: Before: A, B, C -> After: A, C, B* (B* is the new weapon that replacing B).
                            dropWeapon.Drop(weaponTemplates[player.inventory.currentWeapon], player.transform.position, new Vector2(transform.right.x, 1) * 10);
                            Destroy(player.inventory.GetCurrent().gameObject);
                            player.inventory.items.RemoveAt(player.inventory.currentWeapon);

                            Weapon template = trigger.GetComponent<DropWeapon>().template;
                            Weapon weapon = Instantiate(template, transform.position, Quaternion.identity);
                            weapon.transform.parent = transform;
                            weapon.transform.localPosition = weapon.posOffset;
                            player.inventory.AddAndSetCurrent(weapon);
                            weaponTemplates[1] = template;

                            Destroy(trigger.gameObject);
                            textboxCanvas.SetActive(false);
                        } break;
                }
            }
        }
    }

    GameObject GetClosestObject(Collider2D[] colliders)
    {
        int length = Physics2D.OverlapCircleNonAlloc(transform.position, radius, colliders, LayerMask.GetMask("HasTextbox"));

        if (length == 0)
            return null;

        int closest = 0;
        for (int i = 0; i < length; i++)
            if ((transform.position - colliders[i].transform.position).sqrMagnitude < (transform.position - colliders[closest].transform.position).sqrMagnitude)
                closest = i;
        return colliders[closest].gameObject;
    }
}
