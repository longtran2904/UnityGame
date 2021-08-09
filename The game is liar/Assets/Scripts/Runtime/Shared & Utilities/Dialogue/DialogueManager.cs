using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue Manager")]
public class DialogueManager : InputMiddleware
{
    public bool isInDialogue;
    private string currentSpeaker;
    private Queue<string> dialouges = new Queue<string>();

    public GameObject dialogueBoxCanvasObj; // only for assignment in the inspector
    private GameObject dialogueBoxCanvas;
    private static DialogueBox dialogueBox;

    private void OnEnable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += state => { if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode) Init(); };
#else
        if (Application.isPlaying) Init();
#endif
    }

    void Init()
    {
        // TODO: Load dialogueBox
        dialogueBoxCanvas = Instantiate(dialogueBoxCanvasObj);
        dialogueBox = dialogueBoxCanvas.GetComponentInChildren<DialogueBox>();
        dialogueBoxCanvas.SetActive(false);
    }

    public override bool Process(InputState input)
    {
        if (isInDialogue)
        {
            input.moveDir = Vector2.zero;
            input.canJump = false;
            return true;
        }
        return false;
    }

    public void StartDialogue(Dialogue dialogue, Vector3 position)
    {
        if (dialogue == null || dialogue.dialogues == null || isInDialogue)
        {
            return;
        }
        currentSpeaker = dialogue.speaker;
        isInDialogue = true;
        foreach (var sentence in dialogue.dialogues)
        {
            dialouges.Enqueue(sentence);
        }

        // UI
        dialogueBoxCanvas.transform.position = position;
        dialogueBoxCanvas.SetActive(true);
        dialogueBox.ShowDialogue(dialogue.speaker, dialouges.Dequeue());
    }

    public bool DisplayNextDialogue()
    {
        // TODO: check if the current dialogue has done (after vfx)
        InternalDebug.Log(dialouges.Count);

        if (dialouges.Count == 0)
        {
            EndDialogue();
            return false;
        }
        dialogueBox.ShowDialogue(currentSpeaker, dialouges.Dequeue());
        return true;
    }

    public void DisplayResponseOption(Response[] responses)
    {
        
    }

    void EndDialogue()
    {
        dialogueBoxCanvas.SetActive(false);
        dialouges.Clear();
        currentSpeaker = null;
        isInDialogue = false;
    }
}
