using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;
using System.IO;


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
            if (CharacterName.text.Length == 0)
            {
                Debug.LogWarning("Please enter in a name with at least one letter");
            }

            var CharacterSave = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
            CharacterSave.SaveCharacter(character);
            AssetDatabase.CreateAsset(CharacterSave, "Assets/" + savePath + "/" + CharacterName.text + ".asset");
            AssetDatabase.Refresh();
#endif
        }

        [ContextMenu("Load")]
        public void LoadCharacter()
        {
#if UNITY_EDITOR
            string path = savePath + "/" + CharacterName.text + ".asset";
            Debug.Log("Attempting to load from path: " + path);

            var CharacterSave = AssetDatabase.LoadAssetAtPath<BSMC_CharacterObject>(path);
            if (CharacterSave == null)
            {
                Debug.LogError("Couldn't load character from path: " + path);

                // Check if file exists at the path
                if (System.IO.File.Exists(path))
                {
                    Debug.LogError("File exists but couldn't be loaded as BSMC_CharacterObject");
                }
                else
                {
                    Debug.LogError("File does not exist at the specified path");
                }
                return;
            }

            character.characterData = CharacterSave;
            character.LoadFromObject();
            Debug.Log("Character loaded successfully: " + CharacterName.text);
#endif
        }
    }
}
