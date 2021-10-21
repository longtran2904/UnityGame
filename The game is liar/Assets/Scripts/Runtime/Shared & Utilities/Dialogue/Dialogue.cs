using System.Collections;
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

    public static void AddDialogue(Dialogue dialogue)
    {
        if (!allDialogues.ContainsKey(dialogue.DialogueID))
            allDialogues[dialogue.DialogueID] = dialogue;
    }

    public void OnEnable()
    {
        AddDialogue(this);
    }

    public static Dialogue FindDialogueByID(string ID)
    {
        try
        {
            if (ID == "")
            {
                return null;
            }
            return allDialogues[ID];
        }
        catch (System.Exception e)
        {
            InternalDebug.LogError($"Couldn't find dialogue by ID: {ID}!");
            InternalDebug.LogError(e);
            throw;
        }
    }
}
