using UnityEngine;

public class TextboxTrigger : MonoBehaviour
{
    public TextboxType textboxType;
    public Vector2 textboxOffset;
    public UnityEngine.Events.UnityEvent trigger;

    [ShowWhen("textboxType", TextboxType.DIALOGUE)] public Dialogue[] dialogues;
    private int currentDialogue;

    [ShowWhen("textboxType", TextboxType.CHEST)] public DropData dropData;

    [HideInInspector] public Vector3 hitGroundPos; // This is for dropped weapon

    public Dialogue GetRandomDialogue()
    {
        if (currentDialogue == dialogues.Length)
        {
            currentDialogue = 0;
            MathUtils.Shuffle(dialogues);
        }
        return dialogues[currentDialogue++];
    }
}