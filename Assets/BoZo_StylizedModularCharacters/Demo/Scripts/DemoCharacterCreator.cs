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
        public string savePath;

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
        Image lastSelectedImage;
        public float fadeInTime = 2f;

        private void Awake()
        {
            outfits.Clear();
            _outfits.Clear();

            var ob = Resources.LoadAll("", typeof(Outfit));
            foreach (var item in ob)
            {
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
        }

        public void IndexDownSkin()
        {
            skinIndex -= 1;
            if (skinIndex < 0) skinIndex = skinPresets.Length - 1;

            character.SetSkin(skinPresets[skinIndex], true);
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
#if UNITY_EDITOR
            if (CharacterName.text.Cleaned().Length == 0)
            {
                FadeTextInAndOut(addCharacterPrompt, 2f, fadeInTime);
                Debug.LogWarning("Please enter in a name with at least one letter");
                return;
            }

            //check if file already exists
            string assetPath = savePath + "/" + CharacterName.text + ".asset";
            assetPath = assetPath.Cleaned();

            string name = CharacterName.text.Cleaned();

            //if file already exists, ask for confirmation but not if it's a locked preset
            if (LockedPresets.Any(p => p.name == name))
            {
                FadeTextInAndOut(noSavePrompt, 2f, fadeInTime);
                Debug.LogWarning("Cannot save over a locked preset: " + name);
                return;
            }

            if (File.Exists(assetPath))
            {
                confirmParent.SetActive(true);
                return;
            }

            FadeTextInAndOut(savedPrompt, 2f, fadeInTime);

            var CharacterSave = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
            CharacterSave.SaveCharacter(character);
            AssetDatabase.CreateAsset(CharacterSave, assetPath);
            AssetDatabase.Refresh();
            charactersFiller.Refresh();
#endif
        }

        public void ForceSave()
        {
            FadeTextInAndOut(savedPrompt, 2f, fadeInTime);

            var CharacterSave = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
            CharacterSave.SaveCharacter(character);
            string assetPath = savePath + "/" + CharacterName.text + ".asset";
            assetPath = assetPath.Cleaned();
            AssetDatabase.CreateAsset(CharacterSave, assetPath);
            AssetDatabase.Refresh();
            charactersFiller.Refresh();
        }

        public void StartSave()
        {
            var CharacterSave = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
            CharacterSave.SaveCharacter(character);
            string assetPath = savePath + "/Unaccessible/PlayerCharacter.asset";
            assetPath = assetPath.Cleaned();
            AssetDatabase.CreateAsset(CharacterSave, assetPath);
            AssetDatabase.Refresh();
            charactersFiller.Refresh();
        }

        public void LoadCharacter()
        {
            AssetDatabase.Refresh();

            string assetName = CharacterName.text.Trim();

            string path = savePath + "/" + assetName + ".asset";
            path = path.Cleaned();

            if (!AssetDatabase.IsValidFolder(savePath))
            {
                Debug.LogWarning("Save path is not a valid folder: " + savePath);
                return;
            }

            var CharacterSave = AssetDatabase.LoadAssetAtPath<BSMC_CharacterObject>(path);
            
            if (CharacterSave == null)
            {
                Debug.LogError("Couldn't load character from path: " + path);

                // Check if asset exists using AssetDatabase API
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType != null)
                {
                    Debug.LogError($"Asset exists but couldn't be loaded as BSMC_CharacterObject. Asset type: {assetType}");
                }
                else
                {
                    Debug.LogError("Asset does not exist at the specified path");

                    // Let's try to find the asset with a different approach
                    string[] guids = AssetDatabase.FindAssets($"{assetName} t:BSMC_CharacterObject", new[] { savePath });
                    if (guids.Length > 0)
                    {
                        string foundPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        Debug.Log($"Found asset at: {foundPath}");
                        CharacterSave = AssetDatabase.LoadAssetAtPath<BSMC_CharacterObject>(foundPath);
                    }
                }

                if (CharacterSave == null)
                {
                    return;
                }
            }

            character.characterData = CharacterSave;
            character.LoadFromObject();
            Debug.Log("Character loaded successfully: " + assetName);
            sliderManager.InitializeSliders();
        }

        public void SetLastSelectedPreset(GameObject gameObject, string presetName)
        {
            Debug.Log("Button: " + lastSelectedButton);
            Debug.Log("Image: " + lastSelectedImage);
            if (lastSelectedButton != null && lastSelectedImage != null)
            {
                Debug.Log("Deselecting last selected preset: " + lastSelectedPresetName);
                lastSelectedImage.color = new Color32(43, 43, 43, 255);
            }
            lastSelectedButton = gameObject;
            lastSelectedPresetName = presetName;
            lastSelectedImage = gameObject.GetComponent<Image>();
            lastSelectedImage.color = new Color(87, 87, 87, 255); // Set to white to indicate selection
        }

        public void DeleteLastSelectedPreset()
        {
            if (lastSelectedButton != null)
            {
                string assetPath = savePath + "/" + CharacterName.text + ".asset";
                assetPath = assetPath.Cleaned();

                // Check if the Character Save is locked preset
                if (LockedPresets.Any(p => p.name == lastSelectedPresetName))
                {
                    FadeTextInAndOut(noDeletePrompt, 2f, fadeInTime);
                    Debug.LogWarning("Cannot delete locked preset: " + lastSelectedPresetName);
                    return;
                }

                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
                charactersFiller.Refresh();
                Debug.Log("Deleted preset: " + lastSelectedPresetName);

                Destroy(lastSelectedButton);
                lastSelectedButton = null;
                lastSelectedPresetName = null;
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
