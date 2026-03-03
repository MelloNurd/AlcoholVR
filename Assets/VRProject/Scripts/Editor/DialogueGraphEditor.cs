using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(DialogueGraph))]
public class DialogueGraphEditor : Editor
{
    private SerializedProperty _startNodeIdProperty;
    private SerializedProperty _nodesProperty;
    
    private Dictionary<int, bool> _nodeFoldouts = new();
    private Dictionary<int, bool> _choicesFoldouts = new();
    private Dictionary<int, bool> _eventsFoldouts = new();
    private Dictionary<int, Dictionary<int, bool>> _choiceEventFoldouts = new();
    
    // Colors for visual feedback
    private readonly Color _startNodeColor = new(0.2f, 0.6f, 0.3f, 0.3f);
    private readonly Color _errorColor = new(0.8f, 0.2f, 0.2f, 0.3f);
    private readonly Color _warningColor = new(0.8f, 0.6f, 0.2f, 0.3f);
    private readonly Color _nodeHeaderColor = new(0.25f, 0.25f, 0.25f, 1f);
    
    private string _searchFilter = "";
    private Vector2 _scrollPosition;

    private void OnEnable()
    {
        _startNodeIdProperty = serializedObject.FindProperty("startNodeId");
        _nodesProperty = serializedObject.FindProperty("nodes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        DrawHeader();
        DrawToolbar();
        DrawValidationMessages();
        
        EditorGUILayout.Space(8);
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        DrawNodes();
        EditorGUILayout.EndScrollView();
        
        DrawAddNodeButton();
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(4);
        
        // Script field (disabled)
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        GUI.enabled = true;
        
        EditorGUILayout.Space(4);
        
        // Start node dropdown
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Start Node");
        
        var nodeIds = GetAllNodeIds();
        int currentIndex = nodeIds.IndexOf(_startNodeIdProperty.stringValue);
        if (currentIndex < 0 && nodeIds.Count > 0) currentIndex = 0;
        
        if (nodeIds.Count > 0)
        {
            int newIndex = EditorGUILayout.Popup(currentIndex, nodeIds.ToArray());
            if (newIndex >= 0 && newIndex < nodeIds.Count)
            {
                _startNodeIdProperty.stringValue = nodeIds[newIndex];
            }
        }
        else
        {
            EditorGUILayout.LabelField("(No nodes)", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // Search field
        GUILayout.Label("Search:", GUILayout.Width(50));
        _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(150));
        
        GUILayout.FlexibleSpace();
        
        // Expand/Collapse all
        if (GUILayout.Button("Expand All", EditorStyles.toolbarButton, GUILayout.Width(75)))
        {
            for (int i = 0; i < _nodesProperty.arraySize; i++)
                _nodeFoldouts[i] = true;
        }
        if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            _nodeFoldouts.Clear();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawValidationMessages()
    {
        var issues = ValidateGraph();
        foreach (var issue in issues)
        {
            EditorGUILayout.HelpBox(issue.message, issue.isError ? MessageType.Error : MessageType.Warning);
        }
    }

    private void DrawNodes()
    {
        if (_nodesProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No dialogue nodes. Click 'Add Node' to create your first node.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"Nodes ({_nodesProperty.arraySize})", EditorStyles.boldLabel);
        
        for (int i = 0; i < _nodesProperty.arraySize; i++)
        {
            var nodeProperty = _nodesProperty.GetArrayElementAtIndex(i);
            var nodeIdProperty = nodeProperty.FindPropertyRelative("nodeId");
            string nodeId = nodeIdProperty.stringValue;
            
            // Apply search filter
            if (!string.IsNullOrEmpty(_searchFilter))
            {
                var dialogueTextProp = nodeProperty.FindPropertyRelative("dialogueText");
                bool matchesId = nodeId.ToLower().Contains(_searchFilter.ToLower());
                bool matchesText = dialogueTextProp.stringValue.ToLower().Contains(_searchFilter.ToLower());
                if (!matchesId && !matchesText) continue;
            }
            
            DrawNodeItem(i, nodeProperty, nodeId);
        }
    }

    private void DrawNodeItem(int index, SerializedProperty nodeProperty, string nodeId)
    {
        if (!_nodeFoldouts.ContainsKey(index)) _nodeFoldouts[index] = false;
        if (!_choicesFoldouts.ContainsKey(index)) _choicesFoldouts[index] = true;
        if (!_eventsFoldouts.ContainsKey(index)) _eventsFoldouts[index] = false;
        if (!_choiceEventFoldouts.ContainsKey(index)) _choiceEventFoldouts[index] = new Dictionary<int, bool>();

        EditorGUILayout.Space(4);
        
        // Determine node status for coloring
        bool isStartNode = nodeId == _startNodeIdProperty.stringValue;
        bool hasError = HasBrokenLinks(nodeProperty);
        bool hasDuplicateId = IsDuplicateId(nodeId, index);
        
        // Draw node box
        var boxRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Color overlay
        if (isStartNode)
            EditorGUI.DrawRect(boxRect, _startNodeColor);
        else if (hasError || hasDuplicateId)
            EditorGUI.DrawRect(boxRect, _errorColor);
        
        // Header row
        EditorGUILayout.BeginHorizontal();
        
        // Foldout with node ID
        string displayName = string.IsNullOrEmpty(nodeId) ? "(unnamed)" : nodeId;
        if (isStartNode) displayName = "▶ " + displayName + " (START)";
        
        _nodeFoldouts[index] = EditorGUILayout.Foldout(_nodeFoldouts[index], displayName, true, EditorStyles.foldoutHeader);
        
        GUILayout.FlexibleSpace();
        
        // Move buttons
        GUI.enabled = index > 0;
        if (GUILayout.Button("↑", GUILayout.Width(22)))
        {
            _nodesProperty.MoveArrayElement(index, index - 1);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        GUI.enabled = index < _nodesProperty.arraySize - 1;
        if (GUILayout.Button("↓", GUILayout.Width(22)))
        {
            _nodesProperty.MoveArrayElement(index, index + 1);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        GUI.enabled = true;
        
        // Set as start button
        if (!isStartNode && GUILayout.Button("Set Start", GUILayout.Width(65)))
        {
            _startNodeIdProperty.stringValue = nodeId;
        }
        
        // Delete button
        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
        if (GUILayout.Button("X", GUILayout.Width(22)))
        {
            if (EditorUtility.DisplayDialog("Delete Node", $"Delete node '{nodeId}'?", "Delete", "Cancel"))
            {
                _nodesProperty.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                return;
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        // Node content (if expanded)
        if (_nodeFoldouts[index])
        {
            EditorGUI.indentLevel++;
            
            // Node ID
            EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("nodeId"), new GUIContent("Node ID"));
            if (hasDuplicateId)
            {
                EditorGUILayout.HelpBox("Duplicate node ID!", MessageType.Error);
            }
            
            // Dialogue text
            EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("dialogueText"), new GUIContent("Dialogue Text"));
            
            // Audio
            EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("playedAudio"), new GUIContent("Audio Clip"));
            
            EditorGUILayout.Space(4);
            
            // Choices section
            DrawChoicesSection(index, nodeProperty);
            
            // Events foldout
            _eventsFoldouts[index] = EditorGUILayout.Foldout(_eventsFoldouts[index], "Node Events", true);
            if (_eventsFoldouts[index])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("onNodeEnter"), new GUIContent("On Enter"));
                EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("onNodeExit"), new GUIContent("On Exit"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawChoicesSection(int nodeIndex, SerializedProperty nodeProperty)
    {
        var choicesProperty = nodeProperty.FindPropertyRelative("choices");
        
        EditorGUILayout.BeginHorizontal();
        _choicesFoldouts[nodeIndex] = EditorGUILayout.Foldout(_choicesFoldouts[nodeIndex], $"Choices ({choicesProperty.arraySize})", true);
        
        if (GUILayout.Button("+", GUILayout.Width(22)))
        {
            choicesProperty.arraySize++;
            var newChoice = choicesProperty.GetArrayElementAtIndex(choicesProperty.arraySize - 1);
            newChoice.FindPropertyRelative("choiceText").stringValue = "New Choice";
            newChoice.FindPropertyRelative("targetNodeId").stringValue = "";
            newChoice.FindPropertyRelative("isDisabled").boolValue = false;
        }
        EditorGUILayout.EndHorizontal();
        
        if (!_choicesFoldouts[nodeIndex]) return;
        
        if (choicesProperty.arraySize == 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("No choices = dialogue ends after this node.", MessageType.Info);
            EditorGUI.indentLevel--;
            return;
        }
        
        EditorGUI.indentLevel++;
        
        for (int i = 0; i < choicesProperty.arraySize; i++)
        {
            DrawChoiceItem(nodeIndex, i, choicesProperty);
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawChoiceItem(int nodeIndex, int choiceIndex, SerializedProperty choicesProperty)
    {
        var choiceProperty = choicesProperty.GetArrayElementAtIndex(choiceIndex);
        var choiceTextProp = choiceProperty.FindPropertyRelative("choiceText");
        var targetNodeIdProp = choiceProperty.FindPropertyRelative("targetNodeId");
        var isDisabledProp = choiceProperty.FindPropertyRelative("isDisabled");
        
        if (!_choiceEventFoldouts[nodeIndex].ContainsKey(choiceIndex))
            _choiceEventFoldouts[nodeIndex][choiceIndex] = false;
        
        // Check if target is valid
        bool isBrokenLink = !string.IsNullOrEmpty(targetNodeIdProp.stringValue) && 
                            !NodeExists(targetNodeIdProp.stringValue);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (isBrokenLink)
            EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), _errorColor);
        
        EditorGUILayout.BeginHorizontal();
        
        // Choice text (inline)
        EditorGUILayout.LabelField($"[{choiceIndex}]", GUILayout.Width(25));
        choiceTextProp.stringValue = EditorGUILayout.TextField(choiceTextProp.stringValue);
        
        // Delete choice button
        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            choicesProperty.DeleteArrayElementAtIndex(choiceIndex);
            return;
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        // Target node dropdown
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("→ Goes to");
        
        var nodeIds = GetAllNodeIds();
        nodeIds.Insert(0, "(End Dialogue)");
        
        string currentTarget = targetNodeIdProp.stringValue;
        int currentIndex = string.IsNullOrEmpty(currentTarget) ? 0 : nodeIds.IndexOf(currentTarget);
        if (currentIndex < 0) currentIndex = 0;
        
        int newIndex = EditorGUILayout.Popup(currentIndex, nodeIds.ToArray());
        targetNodeIdProp.stringValue = newIndex == 0 ? "" : nodeIds[newIndex];
        
        EditorGUILayout.EndHorizontal();
        
        if (isBrokenLink)
        {
            EditorGUILayout.HelpBox($"Target node '{targetNodeIdProp.stringValue}' doesn't exist!", MessageType.Error);
        }
        
        // Disabled toggle
        EditorGUILayout.PropertyField(isDisabledProp, new GUIContent("Disabled"));
        
        // Choice event foldout
        _choiceEventFoldouts[nodeIndex][choiceIndex] = EditorGUILayout.Foldout(
            _choiceEventFoldouts[nodeIndex][choiceIndex], "On Selected Event", true);
        
        if (_choiceEventFoldouts[nodeIndex][choiceIndex])
        {
            EditorGUILayout.PropertyField(choiceProperty.FindPropertyRelative("onChoiceSelected"), GUIContent.none);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawAddNodeButton()
    {
        EditorGUILayout.Space(8);
        
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
        if (GUILayout.Button("+ Add New Node", GUILayout.Height(30)))
        {
            _nodesProperty.arraySize++;
            var newNode = _nodesProperty.GetArrayElementAtIndex(_nodesProperty.arraySize - 1);
            
            // Generate unique ID
            string baseId = "node_" + _nodesProperty.arraySize;
            int suffix = 0;
            while (NodeExists(baseId + (suffix > 0 ? "_" + suffix : "")))
                suffix++;
            
            string newId = baseId + (suffix > 0 ? "_" + suffix : "");
            
            newNode.FindPropertyRelative("nodeId").stringValue = newId;
            newNode.FindPropertyRelative("dialogueText").stringValue = "";
            newNode.FindPropertyRelative("choices").ClearArray();
            
            // Set as start if first node
            if (_nodesProperty.arraySize == 1)
            {
                _startNodeIdProperty.stringValue = newId;
            }
            
            // Expand new node
            _nodeFoldouts[_nodesProperty.arraySize - 1] = true;
        }
        GUI.backgroundColor = Color.white;
    }

    private List<string> GetAllNodeIds()
    {
        var ids = new List<string>();
        for (int i = 0; i < _nodesProperty.arraySize; i++)
        {
            var nodeId = _nodesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("nodeId").stringValue;
            if (!string.IsNullOrEmpty(nodeId))
                ids.Add(nodeId);
        }
        return ids;
    }

    private bool NodeExists(string nodeId)
    {
        return GetAllNodeIds().Contains(nodeId);
    }

    private bool IsDuplicateId(string nodeId, int currentIndex)
    {
        if (string.IsNullOrEmpty(nodeId)) return false;
        
        for (int i = 0; i < _nodesProperty.arraySize; i++)
        {
            if (i == currentIndex) continue;
            var otherId = _nodesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("nodeId").stringValue;
            if (otherId == nodeId) return true;
        }
        return false;
    }

    private bool HasBrokenLinks(SerializedProperty nodeProperty)
    {
        var choicesProperty = nodeProperty.FindPropertyRelative("choices");
        for (int i = 0; i < choicesProperty.arraySize; i++)
        {
            var targetId = choicesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("targetNodeId").stringValue;
            if (!string.IsNullOrEmpty(targetId) && !NodeExists(targetId))
                return true;
        }
        return false;
    }

    private List<(string message, bool isError)> ValidateGraph()
    {
        var issues = new List<(string, bool)>();
        
        // Check start node exists
        if (!NodeExists(_startNodeIdProperty.stringValue) && _nodesProperty.arraySize > 0)
        {
            issues.Add(($"Start node '{_startNodeIdProperty.stringValue}' doesn't exist!", true));
        }
        
        // Check for orphaned nodes (unreachable)
        var reachable = new HashSet<string> { _startNodeIdProperty.stringValue };
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int i = 0; i < _nodesProperty.arraySize; i++)
            {
                var nodeId = _nodesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("nodeId").stringValue;
                if (!reachable.Contains(nodeId)) continue;
                
                var choices = _nodesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("choices");
                for (int j = 0; j < choices.arraySize; j++)
                {
                    var targetId = choices.GetArrayElementAtIndex(j).FindPropertyRelative("targetNodeId").stringValue;
                    if (!string.IsNullOrEmpty(targetId) && reachable.Add(targetId))
                        changed = true;
                }
            }
        }
        
        for (int i = 0; i < _nodesProperty.arraySize; i++)
        {
            var nodeId = _nodesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("nodeId").stringValue;
            if (!reachable.Contains(nodeId))
            {
                issues.Add(($"Node '{nodeId}' is unreachable from start.", false));
            }
        }
        
        return issues;
    }
}