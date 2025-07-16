using Bozo.ModularCharacters;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharactersFiller : MonoBehaviour
{
    public GameObject buttonPrefab;
    TextMeshProUGUI buttonText;
    DemoCharacterCreator demoCharacterCreator;
    TextMeshProUGUI characterNameText; // Changed from Text to TextMeshProUGUI

    void Awake()
    {
        demoCharacterCreator = FindFirstObjectByType<DemoCharacterCreator>();
        buttonText = buttonPrefab.GetComponentInChildren<TextMeshProUGUI>();

        // Fixed: Direct assignment instead of GetComponent<Text>()
        characterNameText = demoCharacterCreator.CharacterName;

        // Add null check with debug info
        if (characterNameText == null)
        {
            Debug.LogError("CharacterName is null! Make sure DemoCharacterCreator.CharacterName is assigned in the inspector.");
        }
        else
        {
            Debug.Log("CharacterName field found: " + characterNameText.name);
        }
    }

    private void Start()
    {
        FillCharactersList();
    }

    public void FillCharactersList()
    {
        // Get all .asset files in the savePath directory and instantiate a button for each with the name of the file
        var files = System.IO.Directory.GetFiles(demoCharacterCreator.savePath, "*.asset");
        Debug.Log(files.Length + " characters found in " + demoCharacterCreator.savePath);
        foreach (var file in files)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
            var button = Instantiate(buttonPrefab, transform);

            // Get the text component from the instantiated button, not the prefab
            var buttonTextComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonTextComponent.text = fileName;

            // Fix closure issue by capturing fileName in a local variable
            string capturedFileName = fileName;
            button.GetComponent<Button>().onClick.AddListener(() => {
                Debug.Log("Button clicked for: " + capturedFileName);
                ChangeNameField(capturedFileName);
            });
            button.GetComponent<Button>().onClick.AddListener(() => demoCharacterCreator.LoadCharacter());
        }
    }

    public void ChangeNameField(string newName)
    {
        Debug.Log("ChangeNameField called with: " + newName);
        if (characterNameText != null)
        {
            // If characterNameText is part of a TMP_InputField, set the input field's text
            var inputField = characterNameText.GetComponentInParent<TMP_InputField>();
            if (inputField != null)
            {
                inputField.text = newName;
                Debug.Log("Input field text set to: " + newName);
            }
            else
            {
                characterNameText.text = newName;
                Debug.Log("TextMeshPro text set to: " + newName);
            }
        }
        else
        {
            Debug.LogError("characterNameText is null!");
        }
    }

}
