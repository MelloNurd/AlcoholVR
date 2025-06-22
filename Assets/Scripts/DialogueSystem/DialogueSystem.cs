using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    // NOT a singleton

    public Dialogue currentDialogue;
    [SerializeField] private bool _useTypewriterEffect = true;
    public bool IsDialogueActive => _dialogueText.text.Length > 0;

    private Typewriter _typewriter;
    private TMP_Text _dialogueText;

    private void Awake()
    {
        _dialogueText = transform.Find("Body").GetComponentInChildren<TMP_Text>();
        _typewriter = GetComponentInChildren<Typewriter>();
    }

    public async void StartDialogue(Dialogue dialogue)
    {
        if(dialogue == null)
        {
            Debug.LogWarning("Dialogue is null. Cannot start dialogue.");
            return;
        }

        DialogueButtons.Instance.ClearButtons();

        currentDialogue = dialogue;
        if(_useTypewriterEffect && _typewriter != null) 
        {
            await _typewriter.StartWritingAsync(currentDialogue.text);
        }
        else {
            _dialogueText.text = currentDialogue.text;
        }

        await UniTask.Delay(_typewriter.DefaultWritingSpeedInMS); // Wait slightly before showing buttons

        DialogueButtons.Instance.TryCreateDialogueButtons(this);
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
