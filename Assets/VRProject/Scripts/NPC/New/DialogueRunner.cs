using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using AYellowpaper.SerializedCollections;

public class DialogueRunner : MonoBehaviour
{
    public NPC npc { get; private set; }

    public DialogueGraph currentGraph { get; private set; }
    public DialogueNode currentNode { get; private set; }
    public DialogueGraph QueuedDialogue { get; private set; }

    public bool IsQueued => QueuedDialogue != null;
    public bool IsActive => currentNode != null;

    [Header("Events")]
    public UnityEvent onDialogueStart = new();
    public UnityEvent onDialogueEnd = new();

    [Tooltip("Map specific Node IDs to Scene Events. When the dialogue enters a node with a matching ID, the corresponding event will fire.")]
    [SerializedDictionary("Node ID", "Scene Event")]
    public SerializedDictionary<string, UnityEvent> SceneNodeEvents = new();

    private Typewriter _typewriter;
    private AudioSource _audioSource;

    private void Awake()
    {
        _typewriter = GetComponentInChildren<Typewriter>();
        _audioSource = GetComponentInChildren<AudioSource>();

        npc = GetComponent<NPC>();
    }

    private void Start()
    {
        _typewriter.HideTextBubble();
    }

    public void QueueDialogue(DialogueGraph graph)
    {
        if (graph != null && graph.startNodeId.IsBlank())
        {
            Debug.LogWarning("Dialogue graph has no start node defined.");
            return;
        }

        QueuedDialogue = graph;
    }

    public async UniTask StartDialogueAsync(DialogueGraph graph)
    {
        if (graph == null || graph.startNodeId.IsBlank())
        {
            Debug.LogWarning("Dialogue graph has no start node defined.");
            EndDialogue();
            return;
        }

        QueuedDialogue = null;
        currentGraph = graph;
        onDialogueStart?.Invoke();
        await ContinueDialogue(graph.startNodeId);
    }

    public async UniTask ContinueDialogue(string nodeId)
    {
        DialogueButtons.Instance.ClearButtons();

        // Exit previous node
        currentNode?.onNodeExit?.Invoke();
        currentNode = currentGraph.GetNode(nodeId);
        if (currentNode == null)
        {
            Debug.LogWarning($"Node '{nodeId}' not found in graph.");
            EndDialogue();
            return;
        }
        
        // Enter new node
        currentNode.onNodeEnter?.Invoke();
        if (SceneNodeEvents.TryGetValue(nodeId, out var sceneEvent))
        {
            sceneEvent.Invoke();
        }

        PlayDialogueAudio(currentNode);
        await DisplayText(currentNode.dialogueText);

        if (currentNode.choices.Count == 0)
        {
            await UniTask.Delay(3000); // Wait a bit before hiding text bubble
            EndDialogue();
            return;
        }

        DialogueChoice chosenChoice = await DialogueButtons.Instance.PromptPlayerForDialogueChoisesAsync(currentNode);

        // Continue through tree if applicable, otherwise end dialogue
        if (chosenChoice != null && !chosenChoice.targetNodeId.IsBlank())
        {
            chosenChoice.onChoiceSelected?.Invoke();
            await ContinueDialogue(chosenChoice.targetNodeId);
        }
        else
        {
            EndDialogue();
        }
    }
    
    public void EndDialogue()
    {
        currentNode?.onNodeExit?.Invoke();
        currentNode = null;
        currentGraph = null;
        onDialogueEnd?.Invoke();
        DialogueButtons.Instance.ClearButtons();
        _typewriter.HideTextBubble();
    }

    private async UniTask DisplayText(string text)
    {
        if (text.IsBlank()) return;

        await _typewriter.ShowTextBubble();
        await _typewriter.StartWritingAsync(text);
    }

    private async void PlayDialogueAudio(DialogueNode dialogue)
    {
        if (_audioSource == null) return;

        _audioSource.enabled = true;

        if (_audioSource.isPlaying) _audioSource.Stop();

        if (dialogue.playedAudio != null)
        {
            _audioSource.clip = dialogue.playedAudio;
            _audioSource.Play();
            await UniTask.Delay(dialogue.playedAudio.length.ToMS());
        }

        await UniTask.Yield(); // just to be safe
        _audioSource.enabled = false;
    }
}