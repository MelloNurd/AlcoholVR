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
    public bool IsDialogueActive => currentDialogue != null;

    public Dialogue currentDialogue = null;

    public UnityEvent onStart;
    public UnityEvent onEnd;

    private Typewriter _typewriter;
    private TMP_Text _dialogueText;
    private GameObject _textBubble;
    private GameObject _headObj;
    Vector3 _textBubbleScale;

    private void Awake()
    {
        _headObj = transform.Find("Body/BSMC_CharacterBase/Root/Hips/Spine/Spine1/Spine2/Neck/Head").gameObject;
        _dialogueText = transform.Find("Body").GetComponentInChildren<TMP_Text>();
        _textBubble = _dialogueText.transform.parent.gameObject;
        _typewriter = GetComponentInChildren<Typewriter>();
    }

    private void Start()
    {
        _textBubbleScale = _textBubble.transform.localScale;
        _textBubble.SetActive(false);
    }

    private void Update()
    {
        if (_textBubble.activeSelf)
        {
            _textBubble.transform.position = _textBubble.transform.position.WithY(_headObj.transform.position.y + 0.25f);
            _textBubble.transform.parent.LookAt(Player.Instance.Position.WithY(_textBubble.transform.parent.position.y));
        }
    }

    public async void StartDialogue(Dialogue dialogue, int depth = 0)
    {
        DialogueButtons.Instance.ClearButtons();
        Player.Instance.IsInDialogue = true;

        if (depth == 0)
        {
            onStart?.Invoke();
        }

        if (dialogue == null || dialogue.options == null)
        {
            Player.Instance.IsInteractingWithNPC = false;
            EndCurrentDialogue();
            return;
        }

        if(currentDialogue != null)
        {
            currentDialogue.onDialogueEnd?.Invoke();
        }

        currentDialogue = dialogue;

        currentDialogue.onDialogueStart?.Invoke();

        await DisplayText(dialogue.dialogueText);

        if (dialogue.options.Count > 0)
        {
            if(DialogueButtons.Instance.TryCreateDialogueButtons(this, dialogue))
            {
                dialogue.onDialogueStart?.Invoke();
            }
            else
            {
                // failed to create buttons, end dialogue as fallback
                Debug.LogWarning("Failed to create dialogue buttons. Ending dialogue.");
                EndCurrentDialogue();
                Player.Instance.IsInteractingWithNPC = false;
                return;
            }
        }
        else
        {
            Player.Instance.EnableMovement();
            Player.Instance.IsInteractingWithNPC = false;
            await UniTask.Delay(3000); // Wait a bit before hiding text bubble
            EndCurrentDialogue();
        }
    }

    private async UniTask DisplayText(string text)
    {
        if (text.IsBlank()) return;

        if (useTypewriterEffect && _typewriter != null)
        {
            _textBubble.SetActive(true);
            Tween.CompleteAll(_textBubble.transform);
            _ = Tween.Scale(_textBubble.transform, _textBubbleScale, 0.2f, Ease.OutBack);
            await UniTask.Delay(100); // Slight delay for bubble animation
            await _typewriter.StartWritingAsync(text);
        }
        else
        {
            _textBubble.SetActive(true);
            Tween.CompleteAll(_textBubble.transform);
            _ = Tween.Scale(_textBubble.transform, _textBubbleScale, 0.2f, Ease.OutBack);
            await UniTask.Delay(100); // Slight delay for bubble animation
            _dialogueText.text = text;
        }

        await UniTask.Delay(_typewriter.DefaultWritingSpeedInMS); // Wait slightly before showing buttons
    }

    public async void EndCurrentDialogue()
    {
        if (TryGetComponent<InteractableNPC_SM>(out var interactableNPC) && interactableNPC.IsInState(NPC_SM.States.Interact))
        {
            interactableNPC.SwitchState(NPC_SM.States.Walk);
        }
        Player.Instance.IsInDialogue = false;
        DialogueButtons.Instance.ClearButtons();
        currentDialogue?.onDialogueEnd?.Invoke();
        currentDialogue = null;
        onEnd?.Invoke();
        _dialogueText.text = "";
        Tween.CompleteAll(_textBubble.transform);
        await Tween.Scale(_textBubble.transform, 0, 0.2f, Ease.InBack);
        _textBubble.SetActive(false);
    }
}
