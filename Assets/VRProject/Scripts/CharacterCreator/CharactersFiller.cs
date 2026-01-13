using Bozo.ModularCharacters;
using Cysharp.Threading.Tasks;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharactersFiller : MonoBehaviour
{
    public GameObject buttonPrefab;
    TextMeshProUGUI buttonText;
    DemoCharacterCreator demoCharacterCreator;
    TMP_InputField characterNameText; // Changed from Text to TextMeshProUGUI
    [SerializeField] GameObject loadMenu;
    bool firstTime = true;
    [SerializeField] AudioClip buttonClickSound;

    void Awake()
    {
        demoCharacterCreator = FindFirstObjectByType<DemoCharacterCreator>();
        buttonText = buttonPrefab.GetComponentInChildren<TextMeshProUGUI>();
        characterNameText = demoCharacterCreator.CharacterName;
    }

    private async void Start()
    {
        FillCharactersList();
        Debug.Log("CharactersFiller Start called, filling characters list.");
        ChangeNameField("Preset 1");
        loadMenu.SetActive(false);

        await UniTask.Delay(50);

        demoCharacterCreator.LoadCharacter();
    }

    public void FillCharactersList()
    {
        var files = System.IO.Directory.GetFiles(CharacterFileConverting.DestinationPath, "*.json");
        foreach (var file in files)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
            var button = Instantiate(buttonPrefab, transform);

            var buttonTextComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonTextComponent.text = fileName;

            string capturedFileName = fileName;
            button.GetComponent<Button>().onClick.AddListener(() => ChangeNameField(capturedFileName));
            button.GetComponent<Button>().onClick.AddListener(() => demoCharacterCreator.SetLastSelectedPreset(button, capturedFileName));
            button.GetComponent<Button>().onClick.AddListener(() => demoCharacterCreator.LoadCharacter());
            button.GetComponent<Button>().onClick.AddListener(() => PlayerAudio.PlaySound(buttonClickSound)); // Use the static method directly  
            if (fileName == "Preset 1" && firstTime)
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
