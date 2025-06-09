using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    // NOT a singleton

    public Dialogue currentDialogue;
    public bool IsDialogueActive => _dialogueText.text.Length > 0;

    private TMP_Text _dialogueText;

    private void Awake()
    {
        _dialogueText = transform.Find("Body").GetComponentInChildren<TMP_Text>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if(dialogue == null)
        {
            Debug.LogWarning("Dialogue is null. Cannot start dialogue.");
            return;
        }

        currentDialogue = dialogue;
        _dialogueText.text = currentDialogue.text;

        DialogueButtons.Instance.CreateDialogueButtons(this);
    }

    public void EndDialogue()
    {
        if (currentDialogue != null)
        {
            currentDialogue.onDialogueEnd?.Invoke();
            currentDialogue = null;
        }

        DialogueButtons.Instance.ClearButtons();
        _dialogueText.text = string.Empty;
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

            StartDialogue(nextDialogue);
        }
    }
}
