using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// There is only one TextboxHandler and is on the player
public class TextboxHandler : MonoBehaviour
{
    public GameObject textbox;
    public DialogueManager dialogueManager;
    public float distanceAboveObject;
    public float radius;

    public static GameObject closestObj;
    public static GameObject lastObj;

    private static Collider2D[] overlapObjects = new Collider2D[10];
    private static DialogueTrigger trigger;

    private static InputHandlerState state;
    private enum InputHandlerState
    {
        None,
        TextBox,
        Dialogue,
    }
    private enum TextBoxType
    {
        DialogueBox,
    }

    private void Start()
    {
        textbox = Instantiate(textbox);
        textbox.SetActive(false);
    }

    private void Update()
    {
        /*
         * Job:
         * 1. Find closest object that has textbox
         * 2. Display it
         * 3. Handle input
         */

        switch (state)
        {
            case InputHandlerState.None:
                {
                    int mask = LayerMask.GetMask("HasTextbox");
                    int length = Physics2D.OverlapCircleNonAlloc(transform.position, radius, overlapObjects, mask);
                    closestObj = GetClosestObject(overlapObjects, length);
                    //ExtDebug.DrawCircle(transform.position, size / 2, Quaternion.identity, Color.green);
                    if (lastObj != closestObj && closestObj)
                    {
                        DisplayTextbox(TextBoxType.DialogueBox);
                        state = InputHandlerState.TextBox;
                    }
                    lastObj = closestObj;
                } break;
            case InputHandlerState.TextBox:
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        textbox.SetActive(false);
                        HandleInput(TextBoxType.DialogueBox);
                    }
                } break;
            case InputHandlerState.Dialogue:
                {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
                    {
                        if (!dialogueManager.DisplayNextDialogue())
                        {
                            state = InputHandlerState.None;
                        }
                    }
                } break;
        }
    }

    GameObject GetClosestObject(Collider2D[] colliders, int length)
    {
        if (length == 0)
        {
            return null;
        }
        int closest = 0;
        for (int i = 0; i < length; i++)
        {
            if ((transform.position - colliders[i].transform.position).sqrMagnitude < (transform.position - colliders[closest].transform.position).sqrMagnitude)
            {
                closest = i;
            }
        }
        return colliders[closest].gameObject;
    }

    void DisplayTextbox(TextBoxType type)
    {
        switch (type)
        {
            case TextBoxType.DialogueBox:
                {
                    // Spawn text box: "Press F to talk"
                    trigger = closestObj.GetComponent<DialogueTrigger>();
                    textbox.transform.position = trigger.transform.position + trigger.dialogueBoxPos;
                    textbox.gameObject.SetActive(true);
                } break;
        }
    }

    void HandleInput(TextBoxType type)
    {
        switch (type)
        {
            case TextBoxType.DialogueBox:
                {
                    // TODO: Disable input

                    dialogueManager.StartDialogue(trigger.GetDialogue(), trigger.transform.position + trigger.dialogueBoxPos);
                    state = InputHandlerState.Dialogue;
                } break;
        }
    }
}
