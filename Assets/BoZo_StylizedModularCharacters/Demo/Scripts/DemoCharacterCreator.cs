using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Bozo.ModularCharacters;
using System.IO;
using Cysharp.Threading.Tasks;

namespace Bozo.ModularCharacters
{
    public class DemoCharacterCreator : MonoBehaviour
    {
        [Header("Character System")]
        public OutfitSystem character;
        public TMP_InputField CharacterName;
        
        [Header("UI")]
        private GameObject lastSelectedPreset;
        private string lastPresetName;

        private void Awake()
        {
            if (character == null)
            {
                character = FindFirstObjectByType<OutfitSystem>();
            }
        }

        public void SetLastSelectedPreset(GameObject presetButton, string presetName)
        {
            // Deselect previous button
            if (lastSelectedPreset != null)
            {
                var previousImage = lastSelectedPreset.GetComponent<Image>();
                if (previousImage != null)
                {
                    previousImage.color = Color.white; // Reset to default color
                }
            }

            // Select new button
            lastSelectedPreset = presetButton;
            lastPresetName = presetName;
            
            var currentImage = lastSelectedPreset.GetComponent<Image>();
            if (currentImage != null)
            {
                currentImage.color = Color.green; // Highlight selected
            }

            if (CharacterName != null)
            {
                CharacterName.text = presetName;
            }
        }

        public async void LoadCharacter()
        {
            if (string.IsNullOrEmpty(lastPresetName))
            {
                Debug.LogWarning("No preset selected to load.");
                return;
            }

            string filePath = Path.Combine(CharacterFileConverting.DestinationPath, lastPresetName + ".json");
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Character file not found: {filePath}");
                return;
            }

            try
            {
                string jsonData = File.ReadAllText(filePath);
                var characterObject = JsonUtility.FromJson<BSMC_CharacterObject>(jsonData);
                
                if (characterObject != null && character != null)
                {
                    var characterData = characterObject.GetCharacterData();
                    await BMAC_SaveSystem.LoadCharacter(character, characterData);
                    Debug.Log($"Loaded character: {lastPresetName}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading character {lastPresetName}: {ex.Message}");
            }
        }

        public void StartSave()
        {
            StartCoroutine(SaveCharacter());
        }

        private IEnumerator SaveCharacter()
        {
            yield return new WaitForEndOfFrame();
            
            if (string.IsNullOrEmpty(CharacterName.text))
            {
                Debug.LogWarning("Please enter a character name before saving.");
                yield break;
            }

            if (character == null)
            {
                Debug.LogError("No OutfitSystem found to save.");
                yield break;
            }

            // Save using the new system
            BMAC_SaveSystem.SaveCharacter(character, CharacterName.text);
            Debug.Log($"Character saved: {CharacterName.text}");
        }
    }
}
