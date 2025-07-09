using System.IO;
using EditorAttributes;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Constructor", menuName = "Dialogue/Constructor", order = 0)]
public class DialogueConstructor : ScriptableObject
{
    public bool IsCreated => AssetDatabase.AssetPathExists(folderPath);

    [HelpBox(drawAbove: true, messageType: MessageMode.None, message: "Write out a dialogue tree and build all scriptable objects with the Construct Dialogue button.\n\nFormat your dialogue script as follows:\n\n• Each dialogue line should be on its own line\n• Use tabs to create dialogue options (1 tab = option for current dialogue)\n• You can nest multiple levels of dialogue this way\n\nExample:\nHello there, traveler!\n\tWho are you?\n\t\tI'm just a simple merchant passing through.\n\t\t\tA merchant? What do you sell?\n\t\t\t\tI sell rare artifacts and magical items.\n\tWhat brings you here?\n\t\tI'm looking for adventure.\n\t\t\tThen you've come to the right place!\n\nThis creates a branching conversation tree where the player can choose responses and the NPC can reply accordingly.")]
    public Void messageBox;

    [TextArea(10, 25)] public string dialogueScript;

    [Space] public Void spaceHolder;

    [HorizontalGroup(false, nameof(buttonHolder01), nameof(buttonHolder02))]
    [SerializeField] private Void groupHolder;

    [ButtonField(nameof(ConstructDialogue), "Construct Dialogue", buttonHeight: 30)]
    [SerializeField, HideInInspector] private Void buttonHolder01;

    [ButtonField(nameof(DeleteDialogueFolder), "Delete Dialogue Folder", buttonHeight: 30)]
    [SerializeField, HideInInspector] private Void buttonHolder02;

    private string folderPath;

    public void ConstructDialogue()
    {
        InitializeDialogueFolder();
        ParseScript();
    }

    private void DeleteDialogueFolder()
    {
        if (AssetDatabase.AssetPathExists(folderPath))
        {
            AssetDatabase.DeleteAsset(folderPath); // clear any existing data to rebuild
        }

        AssetDatabase.AssetPathExists(folderPath); // ensure the path is refreshed
    }

    private void InitializeDialogueFolder()
    {
        string path = AssetDatabase.GetAssetPath(this);
        folderPath = Path.GetDirectoryName(path) + "/" + this.name; // uses name of the scriptable object
        DeleteDialogueFolder();

        Directory.CreateDirectory(folderPath);
        AssetDatabase.Refresh();
    }

    private void ParseScript()
    {
        string[] lines = dialogueScript.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        int currentIndex = 0;
        RecursiveConstruction(lines, ref currentIndex);
    }

    private Dialogue RecursiveConstruction(string[] lines, ref int currentIndex, int expectedIndent = 0, string parentFolderPath = null)
    {
        if (currentIndex >= lines.Length) return null;
        
        Dialogue dialogueAsset = ScriptableObject.CreateInstance<Dialogue>();
        string dialogueText = lines[currentIndex].Trim();
        dialogueAsset.dialogueText = dialogueText;

        // determine the folder where this dialogue asset should be placed
        string currentFolderPath = parentFolderPath ?? folderPath;
        
        // create asset directly in the current folder (no subfolder for the dialogue itself)
        string dialogueFileName = dialogueText.FileNameFriendly();
        string assetPath = Path.Combine(currentFolderPath, dialogueFileName + ".asset");
        
        currentIndex++;
        int optionIndex = 1;
        
        while (currentIndex < lines.Length)
        {
            int indentCount = CountLeadingTabs(lines[currentIndex]);
            
            if (indentCount == expectedIndent + 1)
            {
                // this is an option for the current dialogue
                string optionText = lines[currentIndex].TrimStart('\t').Trim();
                dialogueAsset.options.Add(new DialogueOption(optionText));
                currentIndex++;
                
                // check if next line is a sub-dialogue for this option
                if (currentIndex < lines.Length && CountLeadingTabs(lines[currentIndex]) == expectedIndent + 2)
                {
                    // create option subfolder for the next dialogue
                    string optionFolderName = optionText.FileNameFriendly();
                    string optionFolderPath = Path.Combine(currentFolderPath, optionFolderName);
                    
                    if (!Directory.Exists(optionFolderPath))
                    {
                        Directory.CreateDirectory(optionFolderPath);
                        AssetDatabase.Refresh();
                    }
                    
                    Dialogue nextDialogue = RecursiveConstruction(lines, ref currentIndex, expectedIndent + 2, optionFolderPath);
                    dialogueAsset.options[dialogueAsset.options.Count - 1].nextDialogue = nextDialogue;
                }
                
                optionIndex++;
            }
            else if (indentCount <= expectedIndent)
            {
                break; // this line belongs to a parent level, stop processing
            }
            else
            {
                currentIndex++; // skip unexpected indentation
            }
        }
        
        AssetDatabase.CreateAsset(dialogueAsset, assetPath);
        AssetDatabase.SaveAssets();
        
        return dialogueAsset;
    }

    private int CountLeadingTabs(string line)
    {
        int count = 0;
        foreach (char c in line)
        {
            if (c == '\t') count++;
            else break;
        }
        return count;
    }
}
