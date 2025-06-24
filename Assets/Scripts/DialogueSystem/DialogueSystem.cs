using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
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
    private GameObject _textBubble;

    private void Awake()
    {
        _dialogueText = transform.Find("Body").GetComponentInChildren<TMP_Text>();
        _textBubble = _dialogueText.transform.GetChild(0).gameObject;
        _typewriter = GetComponentInChildren<Typewriter>();

        _textBubble.SetActive(false);
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
            _textBubble.SetActive(true);
            Tween.CompleteAll(_textBubble.transform);
            Vector3 scale = _textBubble.transform.localScale;
            _ = Tween.Scale(_textBubble.transform, scale * 1.1f, scale, 0.2f, Ease.OutBack);
            await UniTask.Delay(100); // Slight delay for bubble animation
            await _typewriter.StartWritingAsync(currentDialogue.text);
        }
        else {
            _dialogueText.text = currentDialogue.text;
            _textBubble.SetActive(true);
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
        _textBubble.SetActive(false);
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
