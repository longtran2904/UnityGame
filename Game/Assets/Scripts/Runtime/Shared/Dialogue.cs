using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Dialogue")]
public class Dialogue : ScriptableObject
{
    public string DialogueID;
    public string speaker;
    public string[] dialogues;
    public Response[] responses;

    private static Dictionary<string, Dialogue> allDialogues = new Dictionary<string, Dialogue>();

    public void OnEnable()
    {
        if (DialogueID != null && !allDialogues.ContainsKey(DialogueID))
            allDialogues[DialogueID] = this;
    }
}
