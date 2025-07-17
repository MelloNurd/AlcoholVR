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
    [SerializeField] GameObject loadMenu;
    bool firstTime = true;

    void Awake()
    {
        demoCharacterCreator = FindFirstObjectByType<DemoCharacterCreator>();
        buttonText = buttonPrefab.GetComponentInChildren<TextMeshProUGUI>();
        characterNameText = demoCharacterCreator.CharacterName;
    }

    private void Start()
    {
        FillCharactersList();
        Debug.Log("CharactersFiller Start called, filling characters list.");
        ChangeNameField("Preset 1");
        demoCharacterCreator.LoadCharacter();
        loadMenu.SetActive(false);
    }

    public void FillCharactersList()
    {
        // Get all .asset files in the savePath directory and instantiate a button for each with the name of the file
        var files = System.IO.Directory.GetFiles(demoCharacterCreator.savePath, "*.asset");
        foreach (var file in files)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
            var button = Instantiate(buttonPrefab, transform);

            // Get the text component from the instantiated button, not the prefab
            var buttonTextComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonTextComponent.text = fileName;

            // Fix closure issue by capturing fileName in a local variable
            string capturedFileName = fileName;
            button.GetComponent<Button>().onClick.AddListener(() => ChangeNameField(capturedFileName));
            button.GetComponent<Button>().onClick.AddListener(() => demoCharacterCreator.SetLastSelectedPreset(button, capturedFileName));
            button.GetComponent<Button>().onClick.AddListener(() => demoCharacterCreator.LoadCharacter());
            if(fileName == "Preset 1" && firstTime)
            {
                demoCharacterCreator.SetLastSelectedPreset(button, fileName);
                firstTime = false;
            }
        }
    }

    public void ChangeNameField(string newName)
    {
        if (characterNameText != null)
        {
            // If characterNameText is part of a TMP_InputField, set the input field's text
            var inputField = characterNameText.GetComponentInParent<TMP_InputField>();
            if (inputField != null)
            {
                inputField.text = newName;
            }
            else
            {
                characterNameText.text = newName;
            }
        }
    }

    public void Refresh()
    {
        // Clear existing buttons
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        // Refill the list
        FillCharactersList();
    }
}
