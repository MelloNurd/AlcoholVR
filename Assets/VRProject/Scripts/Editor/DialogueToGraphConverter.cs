using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DialogueToGraphConverter : EditorWindow
{
    private Dialogue _rootDialogue;
    private string _outputPath = "Assets/VRProject/ScriptableObjects/DialogueGraphs";
    private string _graphName = "ConvertedDialogue";
    
    private Vector2 _scrollPos;
    private List<string> _conversionLog = new();
    private bool _showPreview;
    private Dictionary<Dialogue, string> _previewNodes = new();

    [MenuItem("Tools/Dialogue/Convert Dialogue to Graph")]
    public static void ShowWindow()
    {
        var window = GetWindow<DialogueToGraphConverter>("Dialogue Converter");
        window.minSize = new Vector2(400, 300);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Convert Old Dialogue to DialogueGraph", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool converts recursive Dialogue ScriptableObjects into a single flat DialogueGraph asset.",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // Input section
        EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
        _rootDialogue = (Dialogue)EditorGUILayout.ObjectField("Root Dialogue", _rootDialogue, typeof(Dialogue), false);
        
        EditorGUILayout.Space(5);
        
        // Output section
        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
        _graphName = EditorGUILayout.TextField("Graph Name", _graphName);
        
        EditorGUILayout.BeginHorizontal();
        _outputPath = EditorGUILayout.TextField("Output Folder", _outputPath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                {
                    _outputPath = "Assets" + selected.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Preview button
        GUI.enabled = _rootDialogue != null;
        if (GUILayout.Button("Preview Conversion", GUILayout.Height(25)))
        {
            PreviewConversion();
        }
        
        // Convert button
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
        if (GUILayout.Button("Convert to DialogueGraph", GUILayout.Height(30)))
        {
            ConvertDialogue();
        }
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        // Preview/Log section
        if (_previewNodes.Count > 0 || _conversionLog.Count > 0)
        {
            EditorGUILayout.LabelField(_showPreview ? "Preview" : "Conversion Log", EditorStyles.boldLabel);
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, EditorStyles.helpBox, GUILayout.Height(200));
            
            if (_showPreview)
            {
                foreach (var kvp in _previewNodes)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Node: {kvp.Value}", EditorStyles.boldLabel);
                    
                    string preview = kvp.Key.dialogueText;
                    if (preview.Length > 100) preview = preview.Substring(0, 100) + "...";
                    EditorGUILayout.LabelField(preview, EditorStyles.wordWrappedLabel);
                    
                    if (kvp.Key.options != null && kvp.Key.options.Count > 0)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var opt in kvp.Key.options)
                        {
                            string target = opt.nextDialogue != null && _previewNodes.ContainsKey(opt.nextDialogue) 
                                ? _previewNodes[opt.nextDialogue] 
                                : "(End)";
                            EditorGUILayout.LabelField($"→ \"{opt.optionText}\" → {target}");
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                foreach (var log in _conversionLog)
                {
                    EditorGUILayout.LabelField(log);
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        EditorGUILayout.Space(10);
        
        // Batch conversion section
        EditorGUILayout.LabelField("Batch Conversion", EditorStyles.boldLabel);
        if (GUILayout.Button("Convert All Dialogue Assets in Project"))
        {
            ConvertAllDialogues();
        }
    }

    private void PreviewConversion()
    {
        _previewNodes.Clear();
        _showPreview = true;
        
        if (_rootDialogue == null) return;
        
        var visited = new HashSet<Dialogue>();
        var queue = new Queue<Dialogue>();
        queue.Enqueue(_rootDialogue);
        int nodeIndex = 0;
        
        while (queue.Count > 0)
        {
            var dialogue = queue.Dequeue();
            if (dialogue == null || visited.Contains(dialogue)) continue;
            
            visited.Add(dialogue);
            string nodeId = nodeIndex == 0 ? "start" : $"node_{nodeIndex}";
            _previewNodes[dialogue] = nodeId;
            nodeIndex++;
            
            if (dialogue.options != null)
            {
                foreach (var option in dialogue.options)
                {
                    if (option.nextDialogue != null && !visited.Contains(option.nextDialogue))
                    {
                        queue.Enqueue(option.nextDialogue);
                    }
                }
            }
        }
    }

    private void ConvertDialogue()
    {
        _conversionLog.Clear();
        _showPreview = false;
        
        if (_rootDialogue == null)
        {
            _conversionLog.Add("ERROR: No root dialogue selected.");
            return;
        }
        
        // Ensure output directory exists
        if (!AssetDatabase.IsValidFolder(_outputPath))
        {
            string[] folders = _outputPath.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                    _conversionLog.Add($"Created folder: {nextPath}");
                }
                currentPath = nextPath;
            }
        }
        
        // Create the DialogueGraph asset
        var graph = ScriptableObject.CreateInstance<DialogueGraph>();
        
        // Traverse the dialogue tree and build nodes
        var dialogueToNodeId = new Dictionary<Dialogue, string>();
        var visited = new HashSet<Dialogue>();
        var queue = new Queue<Dialogue>();
        var nodes = new List<DialogueNode>();
        
        queue.Enqueue(_rootDialogue);
        int nodeIndex = 0;
        
        while (queue.Count > 0)
        {
            var dialogue = queue.Dequeue();
            if (dialogue == null || visited.Contains(dialogue)) continue;
            
            visited.Add(dialogue);
            
            // Generate node ID
            string nodeId = nodeIndex == 0 ? "start" : GenerateNodeId(dialogue, nodeIndex);
            dialogueToNodeId[dialogue] = nodeId;
            
            _conversionLog.Add($"Processing: {dialogue.name} → {nodeId}");
            
            // Queue child dialogues
            if (dialogue.options != null)
            {
                foreach (var option in dialogue.options)
                {
                    if (option.nextDialogue != null && !visited.Contains(option.nextDialogue))
                    {
                        queue.Enqueue(option.nextDialogue);
                    }
                }
            }
            
            nodeIndex++;
        }
        
        // Second pass: create nodes with proper target IDs
        foreach (var kvp in dialogueToNodeId)
        {
            var dialogue = kvp.Key;
            var nodeId = kvp.Value;
            
            var node = new DialogueNode
            {
                nodeId = nodeId,
                dialogueText = dialogue.dialogueText,
                playedAudio = dialogue.playedAudio,
                choices = new List<DialogueChoice>()
            };
            
            // Note: UnityEvents cannot be directly copied between assets at edit time
            // They would need to be manually reconnected after conversion
            
            if (dialogue.options != null)
            {
                foreach (var option in dialogue.options)
                {
                    string targetId = "";
                    if (option.nextDialogue != null && dialogueToNodeId.ContainsKey(option.nextDialogue))
                    {
                        targetId = dialogueToNodeId[option.nextDialogue];
                    }
                    
                    var choice = new DialogueChoice
                    {
                        choiceText = option.optionText,
                        targetNodeId = targetId,
                        isDisabled = option.DisableButton
                    };
                    
                    node.choices.Add(choice);
                }
            }
            
            nodes.Add(node);
        }
        
        // Assign nodes to graph
        graph.nodes = nodes;
        graph.startNodeId = "start";
        
        // Save the asset
        string assetPath = $"{_outputPath}/{_graphName}.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(graph, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        _conversionLog.Add($"");
        _conversionLog.Add($"SUCCESS: Created {assetPath}");
        _conversionLog.Add($"Converted {nodes.Count} dialogue nodes.");
        _conversionLog.Add($"");
        _conversionLog.Add("NOTE: UnityEvents must be manually reconnected.");
        
        // Select the new asset
        Selection.activeObject = graph;
        EditorGUIUtility.PingObject(graph);
    }

    private string GenerateNodeId(Dialogue dialogue, int index)
    {
        // Try to create a meaningful ID from the dialogue name or text
        string baseName = dialogue.name;
        
        if (string.IsNullOrEmpty(baseName) || baseName == "New Dialogue")
        {
            // Fall back to first few words of dialogue text
            if (!string.IsNullOrEmpty(dialogue.dialogueText))
            {
                var words = dialogue.dialogueText.Split(' ').Take(3);
                baseName = string.Join("_", words);
            }
        }
        
        // Sanitize the name
        baseName = System.Text.RegularExpressions.Regex.Replace(baseName, @"[^a-zA-Z0-9_]", "_");
        baseName = baseName.ToLower().Trim('_');
        
        if (string.IsNullOrEmpty(baseName))
        {
            baseName = $"node_{index}";
        }
        
        return baseName;
    }

    private void ConvertAllDialogues()
    {
        _conversionLog.Clear();
        _showPreview = false;
        
        // Find all Dialogue assets
        string[] guids = AssetDatabase.FindAssets("t:Dialogue");
        
        if (guids.Length == 0)
        {
            _conversionLog.Add("No Dialogue assets found in project.");
            return;
        }
        
        _conversionLog.Add($"Found {guids.Length} Dialogue assets.");
        
        // Find root dialogues (dialogues not referenced by other dialogues)
        var allDialogues = new HashSet<Dialogue>();
        var referencedDialogues = new HashSet<Dialogue>();
        
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var dialogue = AssetDatabase.LoadAssetAtPath<Dialogue>(path);
            if (dialogue != null)
            {
                allDialogues.Add(dialogue);
                
                if (dialogue.options != null)
                {
                    foreach (var opt in dialogue.options)
                    {
                        if (opt.nextDialogue != null)
                        {
                            referencedDialogues.Add(opt.nextDialogue);
                        }
                    }
                }
            }
        }
        
        var rootDialogues = allDialogues.Except(referencedDialogues).ToList();
        
        _conversionLog.Add($"Found {rootDialogues.Count} root dialogue(s) to convert.");
        _conversionLog.Add("");
        
        int convertedCount = 0;
        foreach (var root in rootDialogues)
        {
            _rootDialogue = root;
            _graphName = root.name + "_Graph";
            
            _conversionLog.Add($"--- Converting: {root.name} ---");
            ConvertDialogue();
            convertedCount++;
        }
        
        _conversionLog.Add("");
        _conversionLog.Add($"Batch conversion complete. Converted {convertedCount} dialogue tree(s).");
    }
}