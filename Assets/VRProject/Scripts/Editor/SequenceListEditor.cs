using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine.Events;

[CustomEditor(typeof(NPC))]
public class SequenceListEditor : Editor
{
    private SerializedProperty _sequencesProperty;
    private SerializedProperty _currentSequenceIndexProperty;
    private Type[] _sequenceTypes;
    private string[] _sequenceTypeNames;
    private bool _showAddSequenceMenu = false;
    private Dictionary<int, bool> _foldoutStates = new Dictionary<int, bool>();
    private Dictionary<int, bool> _eventsFoldoutStates = new Dictionary<int, bool>();

    private void OnEnable()
    {
        _sequencesProperty = serializedObject.FindProperty("Sequences");
        _currentSequenceIndexProperty = serializedObject.FindProperty("CurrentSequenceIndex");
        
        if (_sequencesProperty == null)
        {
            Debug.LogError("Could not find 'Sequences' property on NPC.");
            return;
        }
        
        RefreshSequenceTypes();
    }

    private void RefreshSequenceTypes()
    {
        try
        {
            var sequenceBaseType = typeof(Sequence);
            _sequenceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => sequenceBaseType.IsAssignableFrom(p) && 
                           p != sequenceBaseType && 
                           !p.IsAbstract &&
                           !p.IsInterface)
                .OrderBy(p => p.Name)
                .ToArray();

            // Convert PascalCase to spaced words (e.g., "WaitSequence" -> "Wait Sequence")
            _sequenceTypeNames = _sequenceTypes
                .Select(type => Regex.Replace(type.Name, "(?<!^)([A-Z])", " $1"))
                .ToArray();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error discovering Sequence types: {ex.Message}");
        }
    }

    public override void OnInspectorGUI()
    {
        if (_sequencesProperty == null)
        {
            EditorGUILayout.HelpBox("Sequences property not found.", MessageType.Error);
            return;
        }

        serializedObject.Update();

        // Draw default inspector but skip Sequences field
        DrawPropertiesExcluding(serializedObject, "Sequences", "m_Script");
        
        // Draw script field manually at top
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        GUI.enabled = true;

        EditorGUILayout.Space(4);
        
        // Draw sequences header with count
        EditorGUILayout.LabelField($"Sequences ({_sequencesProperty.arraySize})", EditorStyles.boldLabel);

        if (_sequencesProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No sequences added yet.", MessageType.Info);
        }

        // Draw each sequence
        for (int i = 0; i < _sequencesProperty.arraySize; i++)
        {
            DrawSequenceItem(i);
        }

        // Add sequence button/menu
        DrawAddSequenceSection();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSequenceItem(int index)
    {
        // Consistent vertical padding for all items
        EditorGUILayout.Space(4);

        // Draw separator line between sequences (after the padding)
        if (index > 0)
        {
            var lineRect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(lineRect, new Color(0.3f, 0.3f, 0.3f, 1f));
            EditorGUILayout.Space(4);
        }

        var sequenceProperty = _sequencesProperty.GetArrayElementAtIndex(index);
        var managedRef = sequenceProperty.managedReferenceValue as Sequence;

        if (!_foldoutStates.ContainsKey(index))
            _foldoutStates[index] = false;
        
        if (!_eventsFoldoutStates.ContainsKey(index))
            _eventsFoldoutStates[index] = false;

        string typeName = managedRef != null 
            ? Regex.Replace(managedRef.GetType().Name, "(?<!^)([A-Z])", " $1") 
            : "None";

        // Check if this is the active sequence
        bool isActive = _currentSequenceIndexProperty.intValue == index;

        // Use consistent row height for all elements
        float rowHeight = EditorGUIUtility.singleLineHeight;

        // Reserve space for the row and draw highlight behind it
        var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(rowHeight));
        
        if (isActive)
        {
            // Expand highlight to full width with padding
            var highlightRect = rowRect;
            highlightRect.x = 0;
            highlightRect.width = EditorGUIUtility.currentViewWidth;
            highlightRect.yMin -= 2;
            highlightRect.yMax += 2;
            EditorGUI.DrawRect(highlightRect, new Color(1f, 1f, 1f, 0.05f));
        }

        // Foldout arrow inline with label
        _foldoutStates[index] = EditorGUILayout.Foldout(_foldoutStates[index], $"[{index}] {typeName}", true);

        // Move up button
        GUI.enabled = index > 0;
        if (GUILayout.Button("↑", GUILayout.Width(22), GUILayout.Height(rowHeight)))
        {
            _sequencesProperty.MoveArrayElement(index, index - 1);
            if (isActive)
                _currentSequenceIndexProperty.intValue = index - 1;
            else if (_currentSequenceIndexProperty.intValue == index - 1)
                _currentSequenceIndexProperty.intValue = index;
            serializedObject.ApplyModifiedProperties();
        }
        GUI.enabled = true;

        // Move down button
        GUI.enabled = index < _sequencesProperty.arraySize - 1;
        if (GUILayout.Button("↓", GUILayout.Width(22), GUILayout.Height(rowHeight)))
        {
            _sequencesProperty.MoveArrayElement(index, index + 1);
            if (isActive)
                _currentSequenceIndexProperty.intValue = index + 1;
            else if (_currentSequenceIndexProperty.intValue == index + 1)
                _currentSequenceIndexProperty.intValue = index;
            serializedObject.ApplyModifiedProperties();
        }
        GUI.enabled = true;

        // Remove button
        if (GUILayout.Button("X", GUILayout.Width(22), GUILayout.Height(rowHeight)))
        {
            _sequencesProperty.DeleteArrayElementAtIndex(index);
            if (_currentSequenceIndexProperty.intValue >= _sequencesProperty.arraySize)
                _currentSequenceIndexProperty.intValue = Mathf.Max(0, _sequencesProperty.arraySize - 1);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
            return;
        }

        EditorGUILayout.EndHorizontal();

        // Draw properties if expanded
        if (_foldoutStates[index] && managedRef != null)
        {
            EditorGUI.indentLevel++;

            // Get all event field names to skip them in the main property loop
            var eventFieldNames = managedRef.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => typeof(UnityEventBase).IsAssignableFrom(f.FieldType))
                .Select(f => f.Name)
                .ToHashSet();

            var iterator = sequenceProperty.Copy();
            var endProperty = sequenceProperty.GetEndProperty();
            
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(iterator, endProperty))
                        break;

                    // Skip all event properties - we'll draw them in foldout
                    if (eventFieldNames.Contains(iterator.name))
                        continue;

                    EditorGUILayout.PropertyField(iterator, true);
                }
                while (iterator.NextVisible(false));
            }

            // Events foldout
            _eventsFoldoutStates[index] = EditorGUILayout.Foldout(_eventsFoldoutStates[index], "Events", true);
            if (_eventsFoldoutStates[index])
            {
                EditorGUI.indentLevel++;
                
                // Get all UnityEvent fields from the sequence type using reflection
                var sequenceType = managedRef.GetType();
                var eventFields = sequenceType
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => typeof(UnityEventBase).IsAssignableFrom(f.FieldType))
                    .ToArray();
                
                foreach (var eventField in eventFields)
                {
                    var eventProp = sequenceProperty.FindPropertyRelative(eventField.Name);
                    if (eventProp != null)
                    {
                        EditorGUILayout.PropertyField(eventProp);
                    }
                }
                
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }
    }

    private void DrawAddSequenceSection()
    {
        EditorGUILayout.Space(4);

        // Main "Add Sequence" button with dropdown arrow indicator
        var buttonRect = GUILayoutUtility.GetRect(new GUIContent("+ Add Sequence"), GUI.skin.button, GUILayout.Height(24));
        if (GUI.Button(buttonRect, _showAddSequenceMenu ? "− Add Sequence" : "+ Add Sequence"))
        {
            _showAddSequenceMenu = !_showAddSequenceMenu;
        }

        if (_showAddSequenceMenu && _sequenceTypes.Length > 0)
        {
            // Indented, smaller buttons for sequence types
            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel++;
            
            for (int i = 0; i < _sequenceTypes.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 15);
                
                var style = new GUIStyle(EditorStyles.miniButton)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 6, 6),
                    fixedHeight = 0 // Allow height to be controlled by GUILayout
                };
                
                if (GUILayout.Button("→ " + _sequenceTypeNames[i], style, GUILayout.Height(26), GUILayout.ExpandWidth(true)))
                {
                    AddSequenceOfType(_sequenceTypes[i]);
                    _showAddSequenceMenu = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }

    private void AddSequenceOfType(Type sequenceType)
    {
        try
        {
            var newSequence = (Sequence)Activator.CreateInstance(sequenceType);
            _sequencesProperty.arraySize++;
            int newIndex = _sequencesProperty.arraySize - 1;
            var newElement = _sequencesProperty.GetArrayElementAtIndex(newIndex);
            newElement.managedReferenceValue = newSequence;
            serializedObject.ApplyModifiedProperties();
            
            // Automatically expand the foldout for the new sequence
            _foldoutStates[newIndex] = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create {sequenceType.Name}: {ex.Message}");
        }
    }
}