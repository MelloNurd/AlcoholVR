using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueOption
{
    public string text;
    public Dialogue nextDialogue;

    public UnityEvent onOptionSelected = new();

    public Dialogue SelectDialogueOption()
    {
        onOptionSelected?.Invoke();
        return nextDialogue;
    }
}

[Serializable]
[CreateAssetMenu(fileName = "NewDialogue", menuName = "DialogueSystem/Dialogue")]
public class Dialogue : ScriptableObject
{
    [TextArea(3, 10)] public string text;

    public List<DialogueOption> options;

    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;
}
