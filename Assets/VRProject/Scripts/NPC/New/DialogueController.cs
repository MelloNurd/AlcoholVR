using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public Dialogue nextDialogue;

    // Component References
    private AudioSource _audioSource;
    private TMP_Text _dialogueText;
    private GameObject _textBubble;
    private GameObject _headObj;

    // Misc
    private Vector3 _textBubbleScale;

    private void Awake()
    {
        _headObj = transform.Find("Body/BSMC_CharacterBase/BodyRig/armature/root/pelvis/spine_01/spine_02/spine_03/spine_04/spine_05/neck_01/neck_02/head").gameObject;

        _dialogueText = transform.Find("Body").GetComponentInChildren<TMP_Text>();
        if (_dialogueText != null)
            _textBubble = _dialogueText.transform.parent.gameObject;

        _audioSource = GetComponentInChildren<AudioSource>();
    }

    private void Start()
    {
        _textBubbleScale = _textBubble.transform.localScale;
        _textBubble.SetActive(false);
    }

    private void Update()
    {
        UpdateSpeechBubblePositionAndRotation();
    }

    private void UpdateSpeechBubblePositionAndRotation()
    {
        if (_textBubble != null && _textBubble.activeSelf)
        {
            _textBubble.transform.position = _textBubble.transform.position.WithY(_headObj.transform.position.y + 0.25f);
            _textBubble.transform.parent.LookAt(Player.Instance.CamPosition.WithY(_textBubble.transform.parent.position.y));
        }
    }

    public void SetNextDialogue(Dialogue dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("No dialogue assigned to DialogueSequence, skipping.");
            return;
        }
        nextDialogue = dialogue;
    }

    //public async void StartDialogue(Dialogue dialogue, int depth = 0)
    //{
    //    DialogueButtons.Instance.ClearButtons();
    //    Player.Instance.IsInDialogue = true;


    //    /*
    //    if (depth == 0)
    //    {
    //        // This function calls recursively, so only at the topmost level (depth 0) do we run onStart
    //        onStart?.Invoke();
    //    }

    //    */
    //    if (dialogue == null || dialogue.options == null)
    //    {
    //        Player.Instance.IsInteractingWithNPC = false;
    //        EndCurrentDialogue();
    //        return;
    //    }

    //    if (currentDialogue != null)
    //    {
    //        currentDialogue.onDialogueEnd?.Invoke();
    //    }

    //    currentDialogue = dialogue;

    //    currentDialogue.onDialogueStart?.Invoke();

    //    // Play dialogue audio
    //    PlayDialogueAudio(dialogue);

    //    // Display dialogue text (takes time, so await)
    //    await DisplayText(dialogue.dialogueText);

    //    if (dialogue.options.Count > 0)
    //    {
    //        if (DialogueButtons.Instance.TryCreateDialogueButtons(this, dialogue))
    //        {
    //            dialogue.onDialogueStart?.Invoke();
    //        }
    //        else
    //        {
    //            // failed to create buttons, end dialogue as fallback
    //            Debug.LogWarning("Failed to create dialogue buttons. Ending dialogue.");
    //            EndCurrentDialogue();
    //            Player.Instance.IsInteractingWithNPC = false;
    //            return;
    //        }
    //    }
    //    else
    //    {
    //        Player.Instance.EnableMovement();
    //        Player.Instance.IsInteractingWithNPC = false;
    //        await UniTask.Delay(3000); // Wait a bit before hiding text bubble
    //        EndCurrentDialogue();
    //    }
    //}
}