using EditorAttributes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueText
{
    [TextArea(3, 10)] public string text;
    public List<DialogueResponse> options = new();

    public UnityEvent onOptionSelected = new();
}

[Serializable]
public class DialogueResponse
{
    public string text;
    public DialogueText dialogue;

    public UnityEvent onResponseSelected = new();
}

[Serializable]
[CreateAssetMenu(fileName = "NewDialogueTree", menuName = "DialogueTree")]
public class DialogueTree : ScriptableObject
{
    public DialogueText rootText;
    [HideProperty] public DialogueText currentDialogueText;

    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;
}
