using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    /*public Dialogue[] dialogues;
    public bool notImmediate;
    private Dialogue lastDialogue;
    public GameObject textbox;
    bool startDialogue;
    bool inRange = true;

    void Start()
    {
        DialogueManager.instance.endDialogue += () => { startDialogue = false; };
    }

    private void Update()
    {
        if (startDialogue)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                DialogueManager.instance.DisplayNextSentence();
            }
        }
        else if (inRange)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                //textbox.SetActive(false);
                StartDialogue();
            }
        }
    }

    public void DisplayUI()
    {
        if (TextboxHandler.closestObj.GetComponent<NPC>() == this && !inRange)
        {
            inRange = true;
            textbox.SetActive(true);
        }
    }

    public void StartDialogue()
    {
        startDialogue = true;
        int randomIndex = Random.Range(0, dialogues.Length);
        if (notImmediate && dialogues.Length > 1)
        {
            lastDialogue = dialogues[randomIndex];
        }
        DialogueManager.instance.StartDialogue(dialogues[randomIndex]);
    }

    public void ResetUI()
    {
        if (TextboxHandler.lastObj.GetComponent<NPC>() == this)
        {
            inRange = false;
            textbox.SetActive(false);
        }
    }*/
}