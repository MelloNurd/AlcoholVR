using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueNode
{
    public string nodeId = "new_node";
    
    [TextArea(3, 10)] 
    public string dialogueText;
    
    public AudioClip playedAudio;
    
    public List<DialogueChoice> choices = new();
    
    public UnityEvent onNodeEnter;
    public UnityEvent onNodeExit;
}

[Serializable]
public class DialogueChoice
{
    public string choiceText = "Continue";
    public string targetNodeId = "";  // Empty = end dialogue
    public bool isDisabled;
    public UnityEvent onChoiceSelected;
}

[CreateAssetMenu(fileName = "New DialogueGraph", menuName = "Dialogue/Dialogue Graph")]
public class DialogueGraph : ScriptableObject
{
    public string startNodeId = "start";
    
    [SerializeField]
    public List<DialogueNode> nodes = new();
    
    // Runtime lookup cache
    [NonSerialized]
    private Dictionary<string, DialogueNode> _nodeLookup;
    
    public DialogueNode GetNode(string nodeId)
    {
        if (_nodeLookup == null || _nodeLookup.Count != nodes.Count)
        {
            BuildLookup();
        }
        
        return _nodeLookup.TryGetValue(nodeId, out var node) ? node : null;
    }
    
    public DialogueNode GetStartNode() => GetNode(startNodeId);
    
    public bool HasNode(string nodeId)
    {
        if (_nodeLookup == null) BuildLookup();
        return _nodeLookup.ContainsKey(nodeId);
    }
    
    public List<string> GetAllNodeIds()
    {
        var ids = new List<string>();
        foreach (var node in nodes)
        {
            if (!string.IsNullOrEmpty(node.nodeId))
                ids.Add(node.nodeId);
        }
        return ids;
    }
    
    private void BuildLookup()
    {
        _nodeLookup = new Dictionary<string, DialogueNode>();
        foreach (var node in nodes)
        {
            if (!string.IsNullOrEmpty(node.nodeId) && !_nodeLookup.ContainsKey(node.nodeId))
                _nodeLookup[node.nodeId] = node;
        }
    }
    
    private void OnEnable() => BuildLookup();
    
    private void OnValidate() => BuildLookup();
}