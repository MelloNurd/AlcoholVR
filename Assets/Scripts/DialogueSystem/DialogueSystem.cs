using System;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public bool useTypewriterEffect = true;
    public bool IsDialogueActive => _dialogueText.text.Length > 0;

    public UnityEvent onDialogueStart = new();
    public UnityEvent onDialogueEnd = new();

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
        if (dialogue == null)
        {
            Debug.LogWarning("DialogueOption is null. Cannot initiate dialogue.");
            return;
        }

        dialogue.onDialogueStart.Invoke();

        DialogueButtons.Instance.ClearButtons();
        
        await DisplayText(dialogue.dialogueText);

        if (dialogue.options != null && dialogue.options.Count > 0)
        {
            if (DialogueButtons.Instance.TryCreateDialogueButtons(this, dialogue))
            {
                onDialogueStart.Invoke();
            }
            else
            {
                EndCurrentDialogue();
            }
        }
        else
        {
            EndCurrentDialogue();
        }
    }

    private async UniTask DisplayText(string text)
    {
        if (useTypewriterEffect && _typewriter != null)
        {
            _textBubble.SetActive(true);
            Tween.CompleteAll(_textBubble.transform);
            Vector3 scale = _textBubble.transform.localScale;
            _ = Tween.Scale(_textBubble.transform, scale * 1.1f, scale, 0.2f, Ease.OutBack);
            await UniTask.Delay(100); // Slight delay for bubble animation
            await _typewriter.StartWritingAsync(text);
        }
        else
        {
            _dialogueText.text = text;
            _textBubble.SetActive(true);
        }

        await UniTask.Delay(_typewriter.DefaultWritingSpeedInMS); // Wait slightly before showing buttons
    }

    public void EndCurrentDialogue()
    {
        onDialogueEnd.Invoke();
        _textBubble.SetActive(false);
        _dialogueText.text = "";
    }
}
