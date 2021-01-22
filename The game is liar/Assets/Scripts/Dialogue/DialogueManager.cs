using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // UI
    public GameObject dialogueBox;
    public GameObject responseBox;
    public Button buttonPrefab;
    private TextMeshProUGUI text;

    private Queue<string> sentences = new Queue<string>();
    private Dialogue currentDialogue;
    // Use for moving and resizing all the current button. Also check if we can display next sentence because there are no response options left.
    private List<Button> unUsedButtons = new List<Button>();

    public static DialogueManager instance;
    public event Action endDialogue;

    private Player player;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        responseBox.SetActive(false);
        foreach (var button in unUsedButtons)
        {
            Destroy(button.gameObject);
        }
        unUsedButtons.Clear();
        currentDialogue = dialogue;
        if (!currentDialogue) // This is when the response button you clicked didn't have any following dialogue
        {
            EndDialogue();
            return;
        }
        text = dialogueBox.GetComponentInChildren<TextMeshProUGUI>();
        sentences.Clear();
        foreach (var sentence in currentDialogue.dialogues)
        {
            sentences.Enqueue(sentence);
        }
        dialogueBox.SetActive(true);
        player.ActivePlayerInput(false);
        DisplayNextSentence();
    }

    // This return a bool because DialogueManager need it
    public void DisplayNextSentence()
    {
        if (sentences.Count == 0 && unUsedButtons.Count == 0)
        {
            DisplayResponseOptions();
            return;
        }
        else if (sentences.Count == 0) // This is when we pressed Enter or LM in the middle of a response option and then get called in DialogueManager
        {
            return;
        }
        text.text = sentences.Dequeue();
        return;
    }

    void DisplayResponseOptions()
    {
        if (currentDialogue.responses.Length == 0)
        {
            EndDialogue();
            return;
        }
        Vector3 pos = responseBox.transform.position;
        float distanceBtwButton = 5;
        float maxWidth = 0;
        float maxHeight = 0;
        int i = 0;
        float firstPosY = 0; // The heighest point of the first button
        RectTransform rt = responseBox.GetComponent<RectTransform>();
        foreach (var response in currentDialogue.responses)
        {
            Button button = Instantiate(buttonPrefab, pos, Quaternion.identity, transform);
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            RectTransform rect = text.GetComponent<RectTransform>();
            text.text = response.text;
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight); // Scale with the width of the text (The text.preferredWidth change here)
            buttonRect.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight); // Scale with the height of the text (The text.preferredHeight change here)
            button.onClick.AddListener(() => { StartDialogue(Dialogue.FindDialogueByID(response.dialogueID)); });
            if (i != 0)
            {
                pos.y -= text.preferredHeight / 2 * rect.localScale.y; // Move the pos down another half the height of the current text (This run second)
                button.transform.position = pos - new Vector3(0, distanceBtwButton);
                maxHeight += distanceBtwButton;
            }
            else
            {
                firstPosY = button.transform.position.y + text.preferredHeight / 2;
            }
            if (text.preferredWidth > maxWidth)
                maxWidth = text.preferredWidth;
            maxHeight += text.preferredHeight;
            pos -= new Vector3(0, text.preferredHeight / 2 * rect.localScale.y); // Move the pos down half the height of the previous text (This run first)
            unUsedButtons.Add(button);
            i++;
        }
        Vector2 offset = new Vector2(10, 10);
        rt.sizeDelta = new Vector2(maxWidth / rt.localScale.x, maxHeight / rt.localScale.y) + offset;
        rt.position = new Vector3(rt.position.x, firstPosY - maxHeight / 2);
        foreach (var button in unUsedButtons)
        {
            button.transform.SetParent(responseBox.transform, true);
        }
        rt.position += new Vector3(rt.sizeDelta.x / 2 * rt.localScale.x, 0);
        responseBox.SetActive(true);
    }

    void EndDialogue()
    {
        dialogueBox.SetActive(false);
        endDialogue?.Invoke();
        StartCoroutine(EnablePlayerInput());

        IEnumerator EnablePlayerInput()
        {
            yield return new WaitForEndOfFrame();
            player.ActivePlayerInput(true);
        }
    }
}
