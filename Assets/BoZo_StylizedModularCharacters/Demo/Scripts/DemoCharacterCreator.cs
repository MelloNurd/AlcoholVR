using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using TMPro;
using System.IO;
using System.Linq;
using Unity.Multiplayer.Center.Common;
using PrimeTween;
using System.Xml;
using UnityEngine.UI;
using Newtonsoft.Json;


namespace Bozo.ModularCharacters
{
    public class DemoCharacterCreator : MonoBehaviour
    {
        [SerializeField] OutfitSystem character;
        [SerializeField] List<GameObject> _outfits = new List<GameObject>();
        [SerializeField] ColorPickerControl colorPickerControl;

        [SerializeField] Dictionary<string, int> indexes = new Dictionary<string, int>();
        [SerializeField] Dictionary<string, List<GameObject>> outfits = new Dictionary<string, List<GameObject>>();


        [SerializeField] Material[] skinPresets;
        [SerializeField] int skinIndex;
        [SerializeField] Material[] eyesPresets;
        [SerializeField] int eyesIndex;
        [SerializeField] Texture2D[] accessories;
        [SerializeField] int accessoriesIndex;

        [SerializeField] CharacterSpinner Spinner;

        [Header("Save Options")]
        public TextMeshProUGUI CharacterName;

        [SerializeField] CharactersFiller charactersFiller;
        [SerializeField] GameObject confirmParent;
        [SerializeField] SliderManager sliderManager;
        [SerializeField] GameObject popupPrompts;
        TextMeshProUGUI savedPrompt;
        TextMeshProUGUI noSavePrompt;
        TextMeshProUGUI noDeletePrompt;
        TextMeshProUGUI addCharacterPrompt;
        [SerializeField] List<BSMC_CharacterObject> LockedPresets = new List<BSMC_CharacterObject>();
        GameObject lastSelectedButton;
        string lastSelectedPresetName;
        TextMeshProUGUI lastSelectedPresetText;
        Image lastSelectedImage;
        public float fadeInTime = 2f;

        // Unified save directory for both editor and runtime

        private void Awake()
        {
            outfits.Clear();
            _outfits.Clear();

            var ob = Resources.LoadAll("", typeof(Outfit));
            foreach (var item in ob)
            {
                //If object name is named BSMC_Top_Naked, skip it
                if (item.name.Contains("Top_Naked") || item.name.Contains("Bottom_Naked"))
                {
                    continue;
                }
                _outfits.Add(item.GameObject());
            }

            foreach (var item in _outfits)
            {
                var sortingList = item.name.Split("_");
                var sorting = sortingList[1];

                if (outfits.TryGetValue(sorting, out List<GameObject> value))
                {
                    value.Add(item);
                }
                else
                {
                    indexes.Add(sorting, 0);
                    outfits.Add(sorting, new List<GameObject>());
                    outfits[sorting].Add(item);
                }
            }

            savedPrompt = popupPrompts.transform.Find("SavedPrompt").GetComponent<TextMeshProUGUI>();
            noSavePrompt = popupPrompts.transform.Find("NoSavePrompt").GetComponent<TextMeshProUGUI>();
            noDeletePrompt = popupPrompts.transform.Find("NoDeletePrompt").GetComponent<TextMeshProUGUI>();
            addCharacterPrompt = popupPrompts.transform.Find("AddCharacterPrompt").GetComponent<TextMeshProUGUI>();
            FadeTextInAndOut(savedPrompt, 0f, 0f);
            FadeTextInAndOut(noSavePrompt, 0f, 0f);
            FadeTextInAndOut(noDeletePrompt, 0f, 0f);
            FadeTextInAndOut(addCharacterPrompt, 0f, 0f);

            // Ensure save directory exists
            EnsureSaveDirectoryExists();
        }

        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(CharacterFileConverting.DestinationPath))
            {
                Directory.CreateDirectory(CharacterFileConverting.DestinationPath);
                Debug.Log($"Created save directory: {CharacterFileConverting.DestinationPath}");
            }
        }

        public void IndexUp(string catagory)
        {
            indexes[catagory] += 1;
            if (indexes[catagory] >= outfits[catagory].Count) indexes[catagory] = 0;
            var index = indexes[catagory];

            var outfit = outfits[catagory][index];

            var inst = Instantiate(outfit, character.transform);
            SetColorPickerObject(inst.transform);

            var type = (OutfitType)Enum.Parse(typeof(OutfitType), catagory);


        }

        public void IndexDown(string catagory)
        {
            indexes[catagory] -= 1;
            if (indexes[catagory] < 0) indexes[catagory] = outfits[catagory].Count - 1;
            var index = indexes[catagory];

            var outfit = outfits[catagory][index];


            var inst = Instantiate(outfit, character.transform);

            SetColorPickerObject(inst.transform);

            var type = (OutfitType)Enum.Parse(typeof(OutfitType), catagory);
        }

        public void IndexUpSkin()
        {
            skinIndex += 1;
            if (skinIndex >= skinPresets.Length) skinIndex = 0;

            character.SetSkin(skinPresets[skinIndex], true);
            colorPickerControl.SelectSkin(character.transform);
        }

        public void IndexDownSkin()
        {
            skinIndex -= 1;
            if (skinIndex < 0) skinIndex = skinPresets.Length - 1;

            character.SetSkin(skinPresets[skinIndex], true);
            colorPickerControl.SelectSkin(character.transform);
        }

        public void IndexUpEyes()
        {
            eyesIndex += 1;
            if (eyesIndex >= eyesPresets.Length) eyesIndex = 0;

            character.SetEyes(eyesPresets[eyesIndex]);
        }

        public void IndexDownEyes()
        {
            eyesIndex -= 1;
            if (eyesIndex <= 0) eyesIndex = eyesPresets.Length - 1;

            character.SetEyes(eyesPresets[eyesIndex]);
        }

        public void SetColorPickerObject(string type)
        {

            var outfit = character.GetOutfit((OutfitType)Enum.Parse(typeof(OutfitType), type));
            colorPickerControl.ChangeObject(outfit);
        }

        public void SetColorPickerObject(Transform outfit)
        {
            colorPickerControl.ChangeObject(outfit);
        }

        public void ReplaceCharacter(OutfitSystem character)
        {
            Destroy(this.character.gameObject);
            this.character = character;
            Spinner.SetCharacter(character.transform);
        }

        public void SelectSkin()
        {
            colorPickerControl.SelectSkin(character.transform);
        }

        public void SelectEyes()
        {
            colorPickerControl.SelectEyes(character.transform);
        }

        public void IndexDownAcc()
        {
            accessoriesIndex -= 1;
            if (accessoriesIndex <= 0) accessoriesIndex = accessories.Length - 1;

            character.SetAccessories(accessories[accessoriesIndex]);
        }

        public void IndexUpAcc()
        {
            accessoriesIndex -= 1;
            if (accessoriesIndex <= 0) accessoriesIndex = accessories.Length - 1;

            character.SetAccessories(accessories[accessoriesIndex]);
        }

        public void SelectAcc()
        {
            accessoriesIndex -= 1;
            if (accessoriesIndex <= 0) eyesIndex = eyesPresets.Length - 1;

            colorPickerControl.SelectAcc(character.transform);
        }

        public void ToggleTop()
        {
            var color = character.CharacterMaterial.GetColor("_UnderwearTopColor_Opacity");

            if (color.a == 1) { color.a = 0; }
            else { color.a = 1; }
            character.GetCharacterBody().material.SetColor("_UnderwearTopColor_Opacity", color);
            character.SetSkin(character.GetCharacterBody().material);
        }

        public void ToggleBot()
        {
            var color = character.CharacterMaterial.GetColor("_UnderwearBottomColor_Opacity");
            if (color.a == 1) { color.a = 0; }
            else { color.a = 1; }
            character.GetCharacterBody().material.SetColor("_UnderwearBottomColor_Opacity", color);
            character.SetSkin(character.GetCharacterBody().material);
        }

        public void CopyColor(string copyTo)
        {
            var copyOutfit = character.GetOutfit((OutfitType)Enum.Parse(typeof(OutfitType), copyTo)).GetComponentInChildren<Renderer>(true);
            colorPickerControl.CopyColor(copyOutfit);
        }

        [ContextMenu("Save")]
        public void SaveCharacter()
        {
            SaveCharacterData();
        }

        /// <summary>
        /// Unified save method that works in both editor and runtime
        /// </summary>
        public void SaveCharacterData()
        {
            if (CharacterName.text.Cleaned().Length == 0)
            {
                FadeTextInAndOut(addCharacterPrompt, 2f, fadeInTime);
                Debug.LogWarning("Please enter in a name with at least one letter");
                return;
            }

            string fileName = CharacterName.text.Cleaned() + ".json";
            string filePath = Path.Combine(CharacterFileConverting.DestinationPath, fileName);
            string name = CharacterName.text.Cleaned();
            filePath = Path.GetFullPath(filePath); // Ensure the path is absolute

            // Check if it's a locked preset
            if (LockedPresets.Any(p => p.name == name))
            {
                FadeTextInAndOut(noSavePrompt, 2f, fadeInTime);
                Debug.LogWarning("Cannot save over a locked preset: " + name);
                return;
            }

            // Check if file already exists and show confirmation if needed
            if (File.Exists(filePath) && confirmParent != null)
            {
                confirmParent.SetActive(true);
                return;
            }

            try
            {
                // Create character save object
                var characterSave = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
                characterSave.SaveCharacter(character);

                // Convert to JSON
                string jsonData = JsonUtility.ToJson(characterSave, true);

                // Ensure directory exists
                EnsureSaveDirectoryExists();

                // Save to file
                File.WriteAllText(filePath, jsonData);

                FadeTextInAndOut(savedPrompt, 2f, fadeInTime);
                Debug.Log($"Character saved successfully to: {filePath}");

                // Clean up temporary object
                DestroyImmediate(characterSave);

                // Refresh character list if available
                if (charactersFiller != null)
                {
                    charactersFiller.Refresh();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save character: {e.Message}");
                FadeTextInAndOut(noSavePrompt, 2f, fadeInTime);
            }
        }

        public void ForceSave()
        {
            ForceSaveCharacterData();
        }

        /// <summary>
        /// Force save without confirmation, works in both editor and runtime
        /// </summary>
        public void ForceSaveCharacterData()
        {
            if (CharacterName.text.Cleaned().Length == 0)
            {
                Debug.LogWarning("Cannot save: Character name is empty");
                return;
            }

            string fileName = CharacterName.text.Cleaned() + ".json";
            string filePath = Path.Combine(CharacterFileConverting.DestinationPath, fileName);
            filePath = Path.GetFullPath(filePath); // Ensure the path is absolute

            try
            {
                var characterSave = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
                characterSave.SaveCharacter(character);

                string jsonData = JsonUtility.ToJson(characterSave, true);

                EnsureSaveDirectoryExists();
                File.WriteAllText(filePath, jsonData);

                FadeTextInAndOut(savedPrompt, 2f, fadeInTime);
                Debug.Log($"Character force saved to: {filePath}");

                DestroyImmediate(characterSave);

                if (charactersFiller != null)
                {
                    charactersFiller.Refresh();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to force save character: {e.Message}");
            }
        }

        public void StartSave()
        {
            StartSaveCharacterData();
        }

        /// <summary>
        /// Auto-save default character, works in both editor and runtime
        /// </summary>
        public void StartSaveCharacterData()
        {
            try
            {
                var characterSave = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
                characterSave.SaveCharacter(character);

                string fileName = "PlayerCharacter.json";
                string filePath = Path.Combine(CharacterFileConverting.JsonOutputRoot, CharacterFileConverting.UnaccessibleCharactersFolder, fileName);

                string jsonData = JsonUtility.ToJson(characterSave, true);

                EnsureSaveDirectoryExists();
                File.WriteAllText(filePath, jsonData);

                Debug.Log($"Default character saved to: {filePath}");

                DestroyImmediate(characterSave);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save default character: {e.Message}");
            }
        }

        public void LoadCharacter()
        {
            LoadCharacterData();
        }

        /// <summary>
        /// Unified load method that works in both editor and runtime
        /// </summary>
        public void LoadCharacterData()
        {
            string assetName = CharacterName.text.Trim();

            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogWarning("Cannot load: Character name is empty");
                return;
            }

            string fileName = assetName + ".json";
            string filePath = Path.Combine(CharacterFileConverting.DestinationPath, fileName).Cleaned();

            filePath = Path.GetFullPath(filePath); // Ensure the path is absolute

            Debug.Log($"Attempting to load character from: {filePath}");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Character file not found: {filePath}");
                return;
            }

            try
            {
                // Read JSON data
                string jsonData = File.ReadAllText(filePath);

                // Create temporary ScriptableObject and populate from JSON
                BSMC_CharacterObject characterSave = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
                JsonUtility.FromJsonOverwrite(jsonData, characterSave);

                // Load the character
                character.characterData = characterSave;
                character.LoadFromObject();

                Debug.Log($"Character loaded successfully: {assetName}");

                if (sliderManager != null)
                {
                    sliderManager.InitializeSliders();
                }

                // Wait a frame to ensure everything is initialized
                StartCoroutine(DelayedSkinSelection());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load character {assetName}: {e.Message}");
            }
        }

        /// <summary>
        /// Load the default player character
        /// </summary>
        public void LoadDefaultCharacter()
        {
            string fileName = "PlayerCharacter.json";
            string filePath = Path.Combine(CharacterFileConverting.DestinationPath, fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning("Default character file not found");
                return;
            }

            try
            {
                string jsonData = File.ReadAllText(filePath);
                var characterSave = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
                JsonUtility.FromJsonOverwrite(jsonData, characterSave);

                character.characterData = characterSave;
                character.LoadFromObject();

                Debug.Log("Default character loaded successfully");

                if (sliderManager != null)
                {
                    sliderManager.InitializeSliders();
                }

                StartCoroutine(DelayedSkinSelection());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load default character: {e.Message}");
            }
        }

        /// <summary>
        /// Get all saved character names
        /// </summary>
        public string[] GetSavedCharacterNames()
        {
            if (!Directory.Exists(CharacterFileConverting.DestinationPath))
            {
                return new string[0];
            }

            try
            {
                string[] jsonFiles = Directory.GetFiles(CharacterFileConverting.DestinationPath, "*.json");
                string[] characterNames = new string[jsonFiles.Length];

                for (int i = 0; i < jsonFiles.Length; i++)
                {
                    characterNames[i] = Path.GetFileNameWithoutExtension(jsonFiles[i]);
                }

                return characterNames;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to get saved character names: {e.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// Check if a character file exists
        /// </summary>
        public bool CharacterExists(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) return false;

            string fileName = characterName + ".json";
            string filePath = Path.Combine(CharacterFileConverting.DestinationPath, fileName);

            return File.Exists(filePath);
        }

        private System.Collections.IEnumerator DelayedSkinSelection()
        {
            yield return null; // Wait one frame

            // Now it should be safe to call SelectSkin
            if (colorPickerControl != null)
            {
                colorPickerControl.SelectSkin(character.transform);
            }
        }

        public void SetLastSelectedPreset(GameObject gameObject, string presetName)
        {
            if (lastSelectedButton != null && lastSelectedImage != null && lastSelectedPresetName != null && lastSelectedPresetText != null)
            {
                Debug.Log("Deselecting last selected preset: " + lastSelectedPresetName);
                lastSelectedImage.color = new Color32(43, 43, 43, 255);
                lastSelectedPresetText.color = new Color32(255, 255, 255, 255);
            }
            lastSelectedButton = gameObject;
            lastSelectedPresetName = presetName;
            lastSelectedImage = gameObject.GetComponent<Image>();
            lastSelectedPresetText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            lastSelectedImage.color = new Color32(87, 87, 87, 255); // Set to white to indicate selection
            lastSelectedPresetText.color = new Color32(0, 0, 0, 255); 
        }

        public void DeleteLastSelectedPreset()
        {
            if (lastSelectedButton != null && !string.IsNullOrEmpty(lastSelectedPresetName))
            {
                // Check if the Character Save is locked preset
                if (LockedPresets.Any(p => p.name == lastSelectedPresetName))
                {
                    FadeTextInAndOut(noDeletePrompt, 2f, fadeInTime);
                    Debug.LogWarning("Cannot delete locked preset: " + lastSelectedPresetName);
                    return;
                }

                string fileName = lastSelectedPresetName + ".json";
                string filePath = Path.Combine(CharacterFileConverting.DestinationPath, fileName);

                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Debug.Log("Deleted preset: " + lastSelectedPresetName);

                        if (charactersFiller != null)
                        {
                            charactersFiller.Refresh();
                        }

                        Destroy(lastSelectedButton);
                        lastSelectedButton = null;
                        lastSelectedPresetName = null;
                        lastSelectedPresetText = null;
                        lastSelectedImage = null;
                    }
                    else
                    {
                        Debug.LogWarning($"Character file not found for deletion: {filePath}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to delete character {lastSelectedPresetName}: {e.Message}");
                }
            }   
        }

        public void FadeTextInAndOut(TextMeshProUGUI tmpText, float holdTime, float fadeDuration)
        {
            Color startColor = tmpText.color;
            startColor.a = 0f;
            tmpText.color = startColor;

            Color visibleColor = tmpText.color;
            visibleColor.a = 1f;

            Tween.Color(tmpText, startColor, visibleColor, fadeDuration)
                .OnComplete(() =>
                {
                    Tween.Delay(holdTime).OnComplete(() =>
                    {
                        Tween.Color(tmpText, visibleColor, startColor, fadeDuration);
                    });
                });
        }
    }
}
