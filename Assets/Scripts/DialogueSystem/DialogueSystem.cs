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
    [HideProperty] public DialogueTree currentTree;

    public bool useTypewriterEffect = true;
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

    public void BeginDialogueTree(DialogueTree tree) 
    {
        if (tree == null)
        {
            Debug.LogWarning("DialogueTree is null. Cannot start dialogue.");
            return;
        }

        currentTree = tree;
        currentTree.onDialogueStart.Invoke();
        currentTree.currentDialogueText = currentTree.rootText;
        InitiateDialogue(currentTree.currentDialogueText).Forget();
    }

    public async UniTask InitiateDialogue(DialogueText option)
    {
        if (currentTree == null)
        {
            Debug.LogWarning("No current tree to associate.");
            return;
        }
        if (option == null)
        {
            Debug.LogWarning("DialogueOption is null. Cannot initiate dialogue.");
            return;
        }

        currentTree.currentDialogueText = option;
        option.onOptionSelected?.Invoke();

        DialogueButtons.Instance.ClearButtons();
        
        await DisplayText(option.text);
        
        if (option.options != null && option.options.Count > 0)
        {
            DialogueButtons.Instance.TryCreateDialogueButtons(this);
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
        if(currentTree == null)
        {
            Debug.LogWarning("No current dialogue to end.");
            return;
        }

        _textBubble.SetActive(false);
        _dialogueText.text = "";

        currentTree.onDialogueEnd?.Invoke();
    }
}
