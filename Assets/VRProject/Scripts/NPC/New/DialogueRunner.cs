using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class DialogueRunner : MonoBehaviour
{
    public DialogueGraph currentGraph;
    public DialogueNode currentNode;
    
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;
    
    public bool IsActive => currentNode != null;
    
    // Reference your existing display components here
    // (Typewriter, AudioSource, TextBubble, etc.)
    
    public void StartDialogue(DialogueGraph graph)
    {
        currentGraph = graph;
        onDialogueStart?.Invoke();
        GoToNode(graph.startNodeId);
    }
    
    public async void GoToNode(string nodeId)
    {
        // Exit previous node
        currentNode?.onNodeExit?.Invoke();
        
        if (string.IsNullOrEmpty(nodeId))
        {
            EndDialogue();
            return;
        }
        
        currentNode = currentGraph.GetNode(nodeId);
        
        if (currentNode == null)
        {
            Debug.LogWarning($"Node '{nodeId}' not found in graph.");
            EndDialogue();
            return;
        }
        
        currentNode.onNodeEnter?.Invoke();
        
        // Display text, play audio, create buttons...
        await DisplayNodeAsync(currentNode);
    }
    
    private async UniTask DisplayNodeAsync(DialogueNode node)
    {
        // Your existing display logic here
        // After text displays, create choice buttons that call:
        // GoToNode(choice.targetNodeId)
    }
    
    public void EndDialogue()
    {
        currentNode?.onNodeExit?.Invoke();
        currentNode = null;
        currentGraph = null;
        onDialogueEnd?.Invoke();
    }
}