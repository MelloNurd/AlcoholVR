using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializeReferenceDropdownAttribute))]
public class SerializeReferenceDropdownDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string sequenceName = GetTypeName(property).Replace("Sequence", ""); // Get type name, removing "Sequence" suffix

        // Rename the label to show the current type or prompt to select
        if (property.managedReferenceValue != null)
        {
            int index = int.Parse(label.text.Replace("Element ", ""));
            label.text = $"[{index}] {sequenceName.AsSentence()}";
        }
        else
        {
            label.text = "Select Type: ";
        }

        EditorGUI.BeginProperty(position, label, property);

        var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        // Draw the PropertyField normally, but now passing in our modified 'label'
        EditorGUI.PropertyField(position, property, label, true);

        // Handle the Right-Click Menu
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 1 && labelRect.Contains(e.mousePosition))
        {
            ShowTypeSelectorMenu(property);
            e.Use();
        }

        // Handle the "Null" Button
        if (property.managedReferenceValue == null)
        {
            var buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, "Select Sequence Type..."))
            {
                ShowTypeSelectorMenu(property);
            }
        }

        EditorGUI.EndProperty();
    }

    private void ShowTypeSelectorMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();

        // Get the type of the list element (Sequence)
        Type baseType = GetFieldType(property);

        // Find all non-abstract classes inheriting from baseType
        List<Type> types = TypeCache.GetTypesDerivedFrom(baseType)
            .Where(p => !p.IsAbstract && !p.IsInterface)
            .ToList();

        // Add "None" option
        menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () =>
        {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        });

        foreach (var type in types)
        {
            menu.AddItem(new GUIContent(type.Name.AsSentence()), typeName == type.Name, () =>
            {
                // Create instance and assign
                property.managedReferenceValue = Activator.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
            });
        }
        menu.ShowAsContext();
    }

    private string GetTypeName(SerializedProperty property)
    {
        if (property.managedReferenceValue == null) return "None";
        return property.managedReferenceValue.GetType().Name;
    }

    private Type GetFieldType(SerializedProperty property)
    {
        // Reflection magic to get the actual type of the field
        // This is a simplified version; for complex nested types, you might need a deeper reflection utility.
        // For a generic List<T>, this usually works:
        var fieldType = fieldInfo.FieldType;
        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
        {
            return fieldType.GetGenericArguments()[0];
        }
        return fieldType.IsArray ? fieldType.GetElementType() : fieldType;
    }

    // Helper to get current string name for comparison
    private string typeName = "";
}