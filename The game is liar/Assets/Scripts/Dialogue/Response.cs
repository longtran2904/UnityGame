using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Response
{
    public string text;
    public string dialogueID;

    public Response(string text, string dialogueID)
    {
        this.text = text;
        this.dialogueID = dialogueID;
    }
}
