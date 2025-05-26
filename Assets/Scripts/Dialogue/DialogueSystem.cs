using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    // NOT a singleton

    public Dialogue currentDialogue;

    [SerializeField] private GameObject _dialogueOptionRow;
    [SerializeField] private GameObject _dialogueOptionButton;

    private TMP_Text _dialogueText;
    private GameObject _dialogueOptionsHolder;

    private void Awake()
    {
        _dialogueText = transform.Find("Chat").GetComponentInChildren<TMP_Text>();
        _dialogueOptionsHolder = transform.Find("Options").GetChild(0).gameObject;
    }

    private void Start()
    {
        if (currentDialogue == null)
        {
            Debug.LogError("Current dialogue is not set. Please assign a dialogue to the DialogueSystem.");
            return;
        }

        if (_dialogueText == null || _dialogueOptionsHolder == null)
        {
            Debug.LogError("Dialogue UI components are not properly set up. Please check the hierarchy.");
            return;
        }

        UpdateDialogueUI();
    }

    public void UpdateDialogueUI()
    {
        _dialogueText.text = currentDialogue.text;

        CreateDialogueButtons();
    }

    private void CreateDialogueButtons()
    {
        foreach (Transform child in _dialogueOptionsHolder.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject currentRow = null;

        for(int i = 0; i < currentDialogue.options.Count; i++)
        {
            if(i % 3 == 0)
            {
                currentRow = Instantiate(_dialogueOptionRow, _dialogueOptionsHolder.transform);
                currentRow.name = "DialogueOptionRow_" + (i / 3);
            }


            Button optionButton = Instantiate(_dialogueOptionButton, currentRow.transform).GetComponent<Button>();
            optionButton.name = "DialogueOptionButton_" + i;

            int index = i; // weird behavior needed with lambda function, called a closure
            optionButton.onClick.AddListener(() => SwitchDialogue(index));

            optionButton.GetComponentInChildren<TMP_Text>().text = currentDialogue.options[i].text;
        }
    }

    public void SwitchDialogue(int optionIndex)
    {
        Dialogue nextDialogue = currentDialogue.options[optionIndex].SelectDialogueOption();
        if (nextDialogue != null)
        {
            if (currentDialogue != null)
            {
                currentDialogue.onDialogueEnd?.Invoke();
            }

            currentDialogue = nextDialogue;

            if (currentDialogue != null)
            {
                currentDialogue.onDialogueStart?.Invoke();
            }

            UpdateDialogueUI();
        }
    }
}
