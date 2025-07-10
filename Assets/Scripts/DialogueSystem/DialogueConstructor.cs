using System.IO;
using EditorAttributes;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Constructor", menuName = "Dialogue/Constructor", order = 0)]
public class DialogueConstructor : ScriptableObject
{
    public bool IsCreated => AssetDatabase.AssetPathExists(folderPath);

    [HelpBox(drawAbove: true, messageType: MessageMode.None, message: "Write out a dialogue tree and build all scriptable objects with the Construct Dialogue button.\n\nFormat your dialogue script as follows:\n\n• Each dialogue line should be on its own line\n• Use tabs to create branching dialogue (even # of tabs = NPC dialogue, odd # of tabs = player dialogue options)\n\nExample:\nHello there, traveler!\n\tWho are you?\n\t\tI'm just a simple merchant passing through.\n\t\t\tA merchant? What do you sell?\n\t\t\t\tI sell rare artifacts and magical items.\n\tWhat brings you here?\n\t\tI'm looking for adventure.\n\t\t\tThen you've come to the right place!\n\nThis creates a branching conversation tree where the player can choose responses and the NPC can reply accordingly.")]
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
        if(folderPath.IsBlank())
        {
            string path = AssetDatabase.GetAssetPath(this);
            folderPath = Path.GetDirectoryName(path) + "/" + this.name.FileNameFriendly(); // uses name of the scriptable object
        }

        if (AssetDatabase.AssetPathExists(folderPath))
        {
            AssetDatabase.DeleteAsset(folderPath); // clear any existing data to rebuild
        }
    }

    private void InitializeDialogueFolder()
    {
        DeleteDialogueFolder(); // folderPath initialized here

        string parentDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
        string folderName = this.name.FileNameFriendly();
        
        AssetDatabase.CreateFolder(parentDirectory, folderName);
        AssetDatabase.Refresh();
    }

    private void ParseScript()
    {
        string[] lines = dialogueScript.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length == 0) return;
        
        int currentIndex = 0;
        
        // Check if the first line is indented
        int firstLineIndent = CountLeadingTabs(lines[0]);
        
        if (firstLineIndent > 0)
        {
            // Create an empty dialogue and start processing from the expected indent level
            RecursiveConstruction(lines, ref currentIndex, firstLineIndent - 1, null, true);
        }
        else
        {
            // Normal processing starting with NPC dialogue
            RecursiveConstruction(lines, ref currentIndex);
        }
    }

    private Dialogue RecursiveConstruction(string[] lines, ref int currentIndex, int expectedIndent = 0, string parentFolderPath = null, bool createEmptyDialogue = false)
    {
        if (currentIndex >= lines.Length) return null;
        
        Dialogue dialogueAsset = ScriptableObject.CreateInstance<Dialogue>();
        
        string dialogueText = "";
        string dialogueFileName = this.name.FileNameFriendly();
        
        if (createEmptyDialogue)
        {
            // Create empty dialogue with just options
            dialogueAsset.dialogueText = "";
            dialogueFileName = this.name.FileNameFriendly();
        }
        else
        {
            // Normal dialogue processing
            dialogueText = lines[currentIndex].Trim();
            dialogueAsset.dialogueText = dialogueText;
            dialogueFileName = dialogueText.FileNameFriendly();
            if (dialogueFileName.IsBlank()) dialogueFileName = this.name.FileNameFriendly();
            currentIndex++;
        }

        // determine the folder where this dialogue asset should be placed
        string currentFolderPath = parentFolderPath ?? folderPath;
        
        // create asset directly in the current folder (no subfolder for the dialogue itself)
        string assetPath = Path.Combine(currentFolderPath, dialogueFileName + ".asset");
        
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
                    
                    if (!AssetDatabase.AssetPathExists(optionFolderPath))
                    {
                        // Create the folder using proper parent/child relationship
                        string parentDir = Path.GetDirectoryName(optionFolderPath);
                        string folderName = Path.GetFileName(optionFolderPath);
                        
                        // Ensure parent directory exists
                        if (!AssetDatabase.AssetPathExists(parentDir))
                        {
                            CreateDirectoryRecursively(parentDir);
                        }
                        
                        AssetDatabase.CreateFolder(parentDir, folderName);
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
        
        // Ensure the directory exists before creating the asset
        string assetDirectory = Path.GetDirectoryName(assetPath);
        if (!AssetDatabase.AssetPathExists(assetDirectory))
        {
            CreateDirectoryRecursively(assetDirectory);
        }
        
        AssetDatabase.CreateAsset(dialogueAsset, assetPath);
        AssetDatabase.SaveAssets();
        
        return dialogueAsset;
    }

    private void CreateDirectoryRecursively(string path)
    {
        string[] pathParts = path.Split('/');
        string currentPath = pathParts[0];
        
        for (int i = 1; i < pathParts.Length; i++)
        {
            string nextPath = currentPath + "/" + pathParts[i];
            
            if (!AssetDatabase.AssetPathExists(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, pathParts[i]);
            }
            
            currentPath = nextPath;
        }
        
        AssetDatabase.Refresh();
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
