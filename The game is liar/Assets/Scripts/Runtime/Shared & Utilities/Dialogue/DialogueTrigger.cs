using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue[] dialogues;
    public Vector3 dialogueBoxPos;
    private int index;

    public Dialogue GetDialogue()
    {
        if (index >= dialogues.Length) index = 0;
        return dialogues[index++];
    }
}
