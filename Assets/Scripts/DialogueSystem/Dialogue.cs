using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueOption // What the player can choose in a dialogue
{
    public string optionText;

    public Dialogue nextDialogue;

    public bool DisableButton = false; // If true, the button will be disabled, making the option unselectable

    public UnityEvent onOptionSelected;

    public DialogueOption(string optionText, Dialogue nextDialogue = null)
    {
        this.optionText = optionText;
        this.nextDialogue = nextDialogue;
        DisableButton = false;
    }
}

[Serializable]
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject // Dialogue node in a dialogue tree (spoken by npc)
{
    [TextArea(3, 15)] public string dialogueText;

    public List<DialogueOption> options = new();

    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;
}
