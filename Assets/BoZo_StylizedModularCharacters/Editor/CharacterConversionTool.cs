using System.IO;
using UnityEngine;
using UnityEditor;

namespace Bozo.ModularCharacters
{
    public class CharacterConversionTool : EditorWindow
    {
        private string sourceFolder = "Assets/Resources/Characters";
        private string targetDirectory;
        private int convertedCount = 0;
        private int errorCount = 0;
        private bool conversionComplete = false;

        [MenuItem("Tools/Character System/Convert Characters to JSON")]
        public static void ShowWindow()
        {
            GetWindow<CharacterConversionTool>("Character Conversion Tool");
        }

        private void OnEnable()
        {
            // Set default target directory to the same as DemoCharacterCreator uses
            targetDirectory = Path.Combine(Application.persistentDataPath, "SavedCharacters");
        }

        private void OnGUI()
        {
            GUILayout.Label("Character Conversion Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Source folder selection
            GUILayout.Label("Source Folder (ScriptableObjects):");
            EditorGUILayout.BeginHorizontal();
            sourceFolder = EditorGUILayout.TextField(sourceFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Source Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Convert absolute path to relative path
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        sourceFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Target directory selection
            GUILayout.Label("Target Directory (JSON files):");
            EditorGUILayout.BeginHorizontal();
            targetDirectory = EditorGUILayout.TextField(targetDirectory);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Target Directory", "", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    targetDirectory = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Info display
            if (AssetDatabase.IsValidFolder(sourceFolder))
            {
                string[] guids = AssetDatabase.FindAssets("t:BSMC_CharacterObject", new[] { sourceFolder });
                GUILayout.Label($"Found {guids.Length} BSMC_CharacterObject(s) in source folder");
            }
            else
            {
                EditorGUILayout.HelpBox("Source folder does not exist or is invalid", MessageType.Warning);
            }

            if (!Directory.Exists(targetDirectory))
            {
                EditorGUILayout.HelpBox("Target directory will be created during conversion", MessageType.Info);
            }
            else
            {
                string[] existingFiles = Directory.GetFiles(targetDirectory, "*.json");
                GUILayout.Label($"Target directory contains {existingFiles.Length} existing JSON file(s)");
            }

            GUILayout.Space(10);

            // Conversion options
            EditorGUILayout.HelpBox(
                "This tool will:\n" +
                "• Find all BSMC_CharacterObject assets in the source folder\n" +
                "• Convert them to JSON format using JsonUtility\n" +
                "• Save them to the target directory with .json extension\n" +
                "• Preserve original file names", 
                MessageType.Info);

            GUILayout.Space(10);

            // Convert button
            EditorGUI.BeginDisabledGroup(!AssetDatabase.IsValidFolder(sourceFolder));
            if (GUILayout.Button("Convert Characters", GUILayout.Height(30)))
            {
                ConvertCharacters();
            }
            EditorGUI.EndDisabledGroup();

            // Results display
            if (conversionComplete)
            {
                GUILayout.Space(10);
                GUILayout.Label("Conversion Results:", EditorStyles.boldLabel);
                
                if (errorCount == 0)
                {
                    EditorGUILayout.HelpBox($"Successfully converted {convertedCount} character(s)!", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"Converted {convertedCount} character(s) with {errorCount} error(s). Check console for details.", MessageType.Warning);
                }

                if (GUILayout.Button("Open Target Directory"))
                {
                    EditorUtility.RevealInFinder(targetDirectory);
                }

                if (GUILayout.Button("Reset"))
                {
                    conversionComplete = false;
                    convertedCount = 0;
                    errorCount = 0;
                }
            }
        }

        private void ConvertCharacters()
        {
            conversionComplete = false;
            convertedCount = 0;
            errorCount = 0;

            // Ensure target directory exists
            if (!Directory.Exists(targetDirectory))
            {
                try
                {
                    Directory.CreateDirectory(targetDirectory);
                    Debug.Log($"Created target directory: {targetDirectory}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to create target directory: {e.Message}");
                    errorCount++;
                    conversionComplete = true;
                    return;
                }
            }

            // Find all BSMC_CharacterObject assets in the source folder
            string[] guids = AssetDatabase.FindAssets("t:BSMC_CharacterObject", new[] { sourceFolder });
            
            if (guids.Length == 0)
            {
                Debug.LogWarning($"No BSMC_CharacterObject assets found in {sourceFolder}");
                conversionComplete = true;
                return;
            }

            Debug.Log($"Starting conversion of {guids.Length} character(s)...");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                
                try
                {
                    // Load the ScriptableObject
                    BSMC_CharacterObject characterObject = AssetDatabase.LoadAssetAtPath<BSMC_CharacterObject>(assetPath);
                    
                    if (characterObject == null)
                    {
                        Debug.LogError($"Failed to load character object at path: {assetPath}");
                        errorCount++;
                        continue;
                    }

                    // Get the file name without extension
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    
                    // Convert to JSON
                    string jsonData = JsonUtility.ToJson(characterObject, true);
                    
                    // Create target file path
                    string targetFilePath = Path.Combine(targetDirectory, fileName + ".json");
                    
                    // Save JSON file
                    File.WriteAllText(targetFilePath, jsonData);
                    
                    Debug.Log($"Converted '{characterObject.name}' to {targetFilePath}");
                    convertedCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error converting {assetPath}: {e.Message}");
                    errorCount++;
                }
            }

            conversionComplete = true;
            
            if (errorCount == 0)
            {
                Debug.Log($"Successfully converted all {convertedCount} character(s) to JSON format!");
            }
            else
            {
                Debug.LogWarning($"Conversion completed with {errorCount} error(s). {convertedCount} character(s) were successfully converted.");
            }

            // Refresh the project to show any new files if they were created in the Assets folder
            AssetDatabase.Refresh();
        }
    }
}