using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class DialogueBox : MonoBehaviour
{
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public LayoutElement layoutElement;
    public int charWrapLimit;

    public void ShowDialogue(string speaker, string dialogue)
    {
        if (string.IsNullOrEmpty(speaker))
        {
            speakerText.gameObject.SetActive(false);
        }
        else
        {
            speakerText.gameObject.SetActive(true);
            speakerText.text = speaker;
        }
        dialogueText.text = dialogue;
        ResizeLayout();
    }

    void ResizeLayout()
    {
        int headerLength = speakerText.text.Length;
        int dialogueLength = dialogueText.text.Length;
        layoutElement.enabled = (headerLength > charWrapLimit) || (dialogueLength > charWrapLimit);
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Application.isEditor)
            ResizeLayout();
    }
#endif
}
