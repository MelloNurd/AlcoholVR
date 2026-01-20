using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace Bozo.ModularCharacters
{
    public class CharacterCreator : MonoBehaviour
    {
        public bool fullCustomization = false;
        [Header("Creator Dependencies")]
        public OutfitSystem character;
        [SerializeField] ColorPickerControl colorPickerControl;
        [SerializeField] CharacterSpinner Spinner;
        [SerializeField] Camera iconCamera;
        [SerializeField] RenderTexture iconTexture;
        public OutfitType[] outfitTypes;

        [Header("Outfit Dependencies")]
        private Dictionary<string, Outfit> OutfitDataBase = new Dictionary<string, Outfit>();
        [SerializeField] OutfitSelector outfitSelectorObject;
        private List<OutfitSelector> outfitSelectors = new List<OutfitSelector>();
        [SerializeField] Transform outfitContainer;

        [Header("Texture Dependencies")]
        [SerializeField] TextureSelector textureSelectorObject;
        private List<TextureSelector> textureSelectors = new List<TextureSelector>();
        [SerializeField] Transform decalContainer;
        [SerializeField] Transform patternContainer;
        [SerializeField] GameObject patternObject;
        [SerializeField] GameObject editBase;

        [Header("BodyShape Dependencies")]
        [SerializeField] BlendSlider blendSliderObject;
        private List<BlendSlider> blendSliders = new List<BlendSlider>();
        private List<BlendSlider> faceBlendSliders = new List<BlendSlider>();

        [SerializeField] BodyShapeSliders modSliderObject;
        private List<BodyShapeSliders> ModSliders = new List<BodyShapeSliders>();

        [SerializeField] Transform bodyShapeContainer;
        [SerializeField] Transform bodyModContainer;
        [SerializeField] Transform faceShapeContainer;
        [SerializeField] Transform faceModContainer;

        [SerializeField] GameObject currentPage;
        private GameObject previousPage;

        private Dictionary<string, List<GameObject>> outfits = new Dictionary<string, List<GameObject>>();
        private List<TexturePackage> textures = new List<TexturePackage>();

        [Header("Save Dependencies")]
        [SerializeField] SaveSelector saveSelector;
        [SerializeField] Dictionary<string, SaveSelector> saveSlots = new Dictionary<string, SaveSelector>();
        [SerializeField] Transform saveContainer;
        [SerializeField] GameObject DeleteConfirmWindow;
        [SerializeField] GameObject SaveConfirmWindow;
        [SerializeField] TMP_Text loadedCharacterNameText;
        [SerializeField] TMP_Text DeleteCharacterNameText;
        [SerializeField] TMP_Text SaveCharacterNameText;


        private OutfitType type;
        [SerializeField] Button removeOutfitButton;
        private List<String> NonRemovableCategories = new List<string>() { "Head", "Body", "Eyes", "Top", "Bottom", "Overall" };

        [Header("Save Options")]
        public TMP_InputField CharacterName;
        
        // Toggle this to control ScriptableObject creation during save
        [Header("Editor Save Options")]
        [Tooltip("When enabled, creates a ScriptableObject asset in the project during save (Editor only)")]
        public bool createScriptableObjectOnSave = false;
        [Tooltip("Path where ScriptableObject will be saved (relative to Assets folder)")]
        public string scriptableObjectSavePath = "BoZo_StylizedModularCharacters/CustomCharacters/Resources/";
        
        [Header("Load Menu Options")]
        [Tooltip("When disabled, only loads characters from the 'Characters' subfolder. When enabled, loads from all Resources subfolders.")]
        public bool loadFromAllSubfolders = false;

        [Header("Hands")]
        [SerializeField] HandColorer leftHand;
        [SerializeField] HandColorer rightHand;

        [Header("Prompts")]
        [SerializeField] GameObject popupPrompts;
        TextMeshProUGUI savedPrompt;
        TextMeshProUGUI noSavePrompt;
        TextMeshProUGUI noDeletePrompt;
        TextMeshProUGUI addCharacterPrompt;
        public float fadeInTime = 2f;
        public float holdTime = 2f;

        private void Awake()
        {
            outfits.Clear();
            OutfitDataBase.Clear();

            // Load only from the "Base" folder inside Resources
            var ob = Resources.LoadAll<Outfit>("Base");
            var textureObjects = Resources.LoadAll<TexturePackage>("");
            
            Debug.Log($"Total Outfits found in Resources/Base: {ob.Length}");
            
            foreach (var item in ob)
            {
                if (!item.showCharacterCreator) continue;

                if (OutfitDataBase.ContainsKey(item.name)) 
                {
                    // Enhanced logging to identify the duplicate
                    var existing = OutfitDataBase[item.name];
                    Debug.LogWarning($"Duplicate Outfit detected:\n" +
                                   $"  Name: {item.name}\n" +
                                   $"  First: {existing} (Type: {existing.GetType()})\n" +
                                   $"  Path: {GetAssetPath(existing)}\n" +
                                   $"  Duplicate: {item} (Type: {item.GetType()})\n" +
                                   $"  Path: {GetAssetPath(item)}");
                }
                else 
                {
                    OutfitDataBase.Add(item.name, item);
                }
            }
            
            foreach (var item in textureObjects)
            {
                textures.Add(item);
            }

            GenerateOutfitSelection();
            GenerateTextureSelection();

            savedPrompt = popupPrompts.transform.Find("SavedPrompt").GetComponent<TextMeshProUGUI>();
            noSavePrompt = popupPrompts.transform.Find("NoSavePrompt").GetComponent<TextMeshProUGUI>();
            noDeletePrompt = popupPrompts.transform.Find("NoDeletePrompt").GetComponent<TextMeshProUGUI>();
            addCharacterPrompt = popupPrompts.transform.Find("AddCharacterPrompt").GetComponent<TextMeshProUGUI>();
            FadeTextInAndOut(savedPrompt, 0f, 0f);
            FadeTextInAndOut(noSavePrompt, 0f, 0f);
            FadeTextInAndOut(noDeletePrompt, 0f, 0f);
            FadeTextInAndOut(addCharacterPrompt, 0f, 0f);
        }

        private void OnEnable()
        {
            character.OnOutfitChanged += OnOutfitUpdate;
            character.OnRigChanged += OnRigUpdate;
        }

        private void OnDisable()
        {
            character.OnOutfitChanged -= OnOutfitUpdate;
            character.OnRigChanged -= OnRigUpdate;
        }

        public void Start()
        {
            GetBodyBlends();
            GetFaceBlends();
            GetBodyMods();
            SwitchCatagory("Top");

            UpdateCharacterSaves();
            
            // Delay hand color update to ensure body outfit is fully loaded
            StartCoroutine(DelayedHandColorUpdate());
        }

        /// <summary>
        /// Delays hand color update to ensure the body outfit is fully initialized
        /// </summary>
        private IEnumerator DelayedHandColorUpdate()
        {
            // Wait a frame to ensure all initialization is complete
            yield return null;
            
            // Now update hand colors from the body
            UpdateHandColorsFromBody();
        }

        public void GenerateOutfitSelection() 
        {
            var outfits = OutfitDataBase.Values.ToArray();
            foreach (var item in outfits
            )
            {
                var selector = Instantiate(outfitSelectorObject, outfitContainer);
                selector.Init(item, this);
                //rename based on outfit name
                selector.gameObject.name = item.name + "_Outfit_Selector";
                outfitSelectors.Add(selector);
            }
        }

        public void GenerateTextureSelection()
        {
            foreach (var item in textures)
            {
                Transform container = null;

                if (item.type == TextureType.Decal) container = decalContainer;
                if (item.type == TextureType.Pattern) container = patternContainer;

                var selector = Instantiate(textureSelectorObject, container);
                selector.Init(item, this);
                textureSelectors.Add(selector);
            }
        }

        public void GetBodyBlends()
        {
            for (int i = 0; i < blendSliders.Count; i++)
            {
                Destroy(blendSliders[i].gameObject);
            }
            blendSliders.Clear();

            var shapes = character.GetShapes();
            foreach (var item in shapes)
            {
                var blendSlider = Instantiate(blendSliderObject, bodyShapeContainer);
                blendSlider.Init(character, item);
                blendSliders.Add(blendSlider);
            }
        }

        public void GetFaceBlends()
        {
            for (int i = 0; i < faceBlendSliders.Count; i++)
            {
                Destroy(faceBlendSliders[i].gameObject);
            }
            faceBlendSliders.Clear();

            var shapes = character.GetFaceShapes();
            foreach (var item in shapes)
            {
                var blendSlider = Instantiate(blendSliderObject, faceShapeContainer);
                blendSlider.Init(character, item);
                faceBlendSliders.Add(blendSlider);
            }
        }

        public void GetBodyMods()
        {
            var mods = character.GetMods().Values.ToList();

            for (int i = 0; i < ModSliders.Count; i++)
            {
                Destroy(ModSliders[i].gameObject);
            }
            ModSliders.Clear();

            var container = bodyShapeContainer;
            foreach (var item in mods)
            {
                if (item.sorting == "Head") container = faceModContainer;
                else container = bodyModContainer;
                var blendSlider = Instantiate(modSliderObject, container);
                blendSlider.Init(character, item);
                ModSliders.Add(blendSlider);
            }
        }

        public void UpdateCharacterSaves()
        {
            var saves = saveSlots.Values.ToArray();
            foreach (var item in saves)
            {
                Destroy(item.gameObject);
            }
            saveSlots.Clear();

            if (!Directory.Exists(BMAC_SaveSystem.filePath))
            {
                Directory.CreateDirectory(BMAC_SaveSystem.filePath);
                Directory.CreateDirectory(BMAC_SaveSystem.iconFilePath);
                print("Created Save JSON save Location At: " + BMAC_SaveSystem.filePath);
            }

            string path = BMAC_SaveSystem.filePath;
            string[] jsonFiles = Directory.GetFiles(path, "*.json");
            string[] icons = Directory.GetFiles(BMAC_SaveSystem.iconFilePath, "*.png");

            for (int i = 0; i < jsonFiles.Length; i++)
            {
                var json = File.ReadAllText(jsonFiles[i]);
                var data = JsonUtility.FromJson<CharacterData>(json);
                var image = File.ReadAllBytes(icons[i]);
                var texture = new Texture2D(2, 2);
                Sprite icon = null;
                if (texture.LoadImage(image))
                {
                    icon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }

                Debug.Log($"json file {i}: {jsonFiles[i]}");
                var selector = Instantiate(saveSelector, saveContainer);
                selector.Init(data, icon, this);
                saveSlots.Add(data.characterName, selector);
            }

            // Load CharacterObject assets based on loadFromAllSubfolders setting
            CharacterObject[] saveObjects;
            if (loadFromAllSubfolders)
            {
                // Load from all Resources subfolders
                saveObjects = Resources.LoadAll<CharacterObject>("");
                Debug.Log($"Loading characters from all Resources subfolders: {saveObjects.Length} found");
            }
            else
            {
                // Load only from the "Characters" subfolder
                saveObjects = Resources.LoadAll<CharacterObject>("Characters");
                Debug.Log($"Loading characters from Resources/Characters only: {saveObjects.Length} found");
            }

            for (int i = 0; i < saveObjects.Length; i++)
            {
                var ob = saveObjects[i];
                if (saveSlots.ContainsKey(ob.data.characterName))
                {
                    continue;
                }
                else
                {
                    var selector = Instantiate(saveSelector, saveContainer);
                    Sprite icon = null;
                    if(ob.icon != null)
                    {
                        icon = Sprite.Create(ob.icon, new Rect(0, 0, ob.icon.width, ob.icon.height), new Vector2(0.5f, 0.5f));
                    }
                    selector.Init(ob.data, icon, this);
                    saveSlots.Add(ob.data.characterName, selector);
                }
            }
        }
        
        public void OpenPage(GameObject page) 
        {
            currentPage.SetActive(false);
            previousPage = currentPage;
            currentPage = page;
            currentPage.SetActive(true);
        }

        public void BackPage() 
        {
            currentPage.SetActive(false);
            currentPage = previousPage;
            currentPage.SetActive(true);
            colorPickerControl.RemoveObject();
        }

        public void SetOutfit(Outfit outfit) 
        {
            // Check if user is switching from overall to top/bottom
            CheckAndHandleOverallReplacement(outfit);
            
            var inst = Instantiate(outfit, character.transform);
            SetColorPickerObject(inst);
            SwitchTextureCatagory(outfit.TextureCatagory);
            type = outfit.Type;

           // var type = (OutfitType)Enum.Parse(typeof(OutfitType), catagory);
        }

        /// <summary>
        /// Handles the case where user selects a top or bottom after wearing an overall outfit.
        /// Automatically equips a matching piece for the other slot to prevent nakedness.
        /// </summary>
        private void CheckAndHandleOverallReplacement(Outfit newOutfit)
        {
            if (newOutfit == null || newOutfit.Type == null) return;
            
            string newOutfitTypeName = newOutfit.Type.name;
            
            // Only handle if new outfit is Top or Bottom
            if (newOutfitTypeName != "Top" && newOutfitTypeName != "Bottom") return;
            
            // Check if user currently has an Overall equipped
            var currentOverall = character.GetOutfit("Overall");
            if (currentOverall == null) return;
            
            // User is switching from overall to top/bottom
            // Determine which slot needs to be filled
            string slotToFill = (newOutfitTypeName == "Top") ? "Bottom" : "Top";
            
            // Find a default outfit for the other slot
            Outfit defaultOutfit = FindDefaultOutfitForSlot(slotToFill);
            
            if (defaultOutfit != null)
            {
                // Get the Overall OutfitType
                var overallType = System.Array.Find(outfitTypes, t => t != null && t.name == "Overall");
                
                if (overallType != null)
                {
                    // Remove the overall first
                    character.RemoveOutfit(overallType, true);
                }
                
                // Instantiate and attach the default outfit for the other slot
                var defaultInst = Instantiate(defaultOutfit, character.transform);
                
                Debug.Log($"Auto-equipped {slotToFill}: {defaultOutfit.name} to complement {newOutfitTypeName}");
            }
            else
            {
                Debug.LogWarning($"Could not find a default {slotToFill} outfit to auto-equip");
            }
        }

        /// <summary>
        /// Finds a suitable default outfit for the specified slot (Top or Bottom)
        /// Prioritizes simple/basic outfits
        /// </summary>
        private Outfit FindDefaultOutfitForSlot(string slotName)
        {
            if (OutfitDataBase == null || OutfitDataBase.Count == 0) return null;
            
            // Get all outfits matching the slot type
            var matchingOutfits = OutfitDataBase.Values
                .Where(o => o.Type != null && o.Type.name == slotName && o.showCharacterCreator)
                .ToList();
            
            if (matchingOutfits.Count == 0) return null;
            
            // Try to find a "basic" or "default" outfit (case insensitive search)
            var basicOutfit = matchingOutfits.FirstOrDefault(o => 
                o.name.ToLower().Contains("basic") || 
                o.name.ToLower().Contains("default") ||
                o.name.ToLower().Contains("simple"));
            
            // If no basic outfit found, return the first available one
            return basicOutfit ?? matchingOutfits[0];
        }

        public void OnOutfitUpdate(Outfit outfit)
        {
            if (outfit == null) return;
            if(outfit.Type.name == "Head")
            {
                GetFaceBlends();
            }
            if (outfit.Type.name == "Body")
            {
                GetBodyBlends();
                
                // Update hand colors when body outfit changes
                UpdateHandColorsFromBody();
            }
        }

        public void OnRigUpdate(SkinnedMeshRenderer rig)
        {
            //GetBodyBlends();
            GetBodyMods();
        }

        /// <summary>
        /// Updates both hand colorers to match the current body outfit's Color_1
        /// Called when body outfit changes or is updated
        /// </summary>
        public void UpdateHandColorsFromBody()
        {
            if (leftHand == null && rightHand == null)
            {
                Debug.LogWarning("No hand colorers assigned to CharacterCreator");
                return;
            }

            // Get the current body outfit
            var bodyOutfit = character.GetOutfit("Body");
            if (bodyOutfit == null)
            {
                Debug.LogWarning("No body outfit found to sync hand colors");
                return;
            }

            // Get Color_1 (index 1) from the body outfit
            Color bodyColor = bodyOutfit.GetColor(1);

            // Update both hands
            if (leftHand != null)
            {
                leftHand.UpdateHandColor(bodyColor);
                Debug.Log($"Updated left hand color to: {bodyColor}");
            }

            if (rightHand != null)
            {
                rightHand.UpdateHandColor(bodyColor);
                Debug.Log($"Updated right hand color to: {bodyColor}");
            }
        }

        public void SetOutfitDecal(Texture texture)
        {
            if (!colorPickerControl.colorObject) return;
            colorPickerControl.colorObject.SetDecal(texture);
        }
        
        public void SetOutfitPattern(Texture texture)
        {
            if (!colorPickerControl.colorObject) return;
            colorPickerControl.colorObject.SetPattern(texture);
        }

        public void RemoveOutfit()
        {
            if (type == null) return;
            character.RemoveOutfit(type, true);
            colorPickerControl.RemoveObject();
        }

        public void RemoveOutfitDecal() 
        {
            if (!colorPickerControl.colorObject) return;
            colorPickerControl.colorObject.SetDecal(null);
        }

        public void RemoveOutfitPattern()
        {
            if (!colorPickerControl.colorObject) return;
            colorPickerControl.colorObject.SetPattern(null);
        }


        public void SwitchCatagory(string catagory) 
        {
            bool temp = false;
            foreach (var item in NonRemovableCategories)
            {
                if (catagory == item)
                {
                    removeOutfitButton.gameObject.SetActive(false);
                    temp = true;
                }
            }
            if (!temp)
            {
                removeOutfitButton.gameObject.SetActive(true);
            }

            foreach (var item in outfitSelectors)
            {
                item.SetVisable(catagory);
            }

            var outfit = character.GetOutfit(catagory);
            
            SetColorPickerObject(outfit);

            if (outfit == null)
            {
                // If no outfit exists in this category, try to find the OutfitType
                // from the outfitTypes array to properly track the current category
                var outfitType = System.Array.Find(outfitTypes, t => t != null && t.name == catagory);
                type = outfitType;

                return;
            }

 
            SwitchTextureCatagory(outfit.TextureCatagory);
            type = outfit.Type;
        }

        public void SwitchTextureCatagory(string catagory)
        {
            if (catagory == "")
            {
                catagory = "Outfit";
            }

            foreach (var item in textureSelectors)
            {
                item.SetVisable(catagory);
                if (catagory != "Eyes" && fullCustomization == false)
                {
                    patternObject.SetActive(false);
                    editBase.SetActive(false);
                }
                else
                {
                    patternObject.SetActive(true);
                    editBase.SetActive(true);
                }
            }
        }

        public void SetColorPickerObject(string type)
        {
            var outfit = character.GetOutfit(type);
            colorPickerControl.ChangeObject(outfit);
        }

        public void SetColorPickerObject(Outfit outfit)
        {
            colorPickerControl.ChangeObject(outfit);
        }

        public void ReplaceCharacter(OutfitSystem character)
        {
            Destroy(this.character.gameObject);
            this.character = character;
            Spinner.SetCharacter(character.transform);
        }

        public void GetCurrentCatagory()
        {
            if (type == null) return;
            SwitchCatagory(type.name);
        }

        public void CopyColor(string copyTo)
        {
            var copyOutfit = character.GetOutfit((OutfitType)Enum.Parse(typeof(OutfitType), copyTo));
            colorPickerControl.CopyColor(copyOutfit);
        }

        public void ToggleWalk(bool value)
        {
            character.animator.SetBool("isWalk", value);
        }

        public void SaveCharacter()
        {
            StartCoroutine(Save());
        }

        public Outfit GetOutfit(string outfitName)
        {
            return OutfitDataBase[outfitName];
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

        [ContextMenu("Save")]
        private IEnumerator Save()
        {
            yield return new WaitForEndOfFrame();

            // Validate character name
            if(CharacterName.text.Length == 0)
            {
                Debug.LogWarning("Please enter in a name with at least one letter");
                // Show "add character" prompt when no name is entered
                FadeTextInAndOut(addCharacterPrompt, holdTime, fadeInTime);
                yield break;
            }

            // Check if a character with this name already exists
            if (BMAC_SaveSystem.CharacterExists(CharacterName.text))
            {
                // Check if it's a player-created character (can be overwritten)
                if (BMAC_SaveSystem.IsPlayerCreatedCharacter(CharacterName.text))
                {
                    // Show confirmation window for overwriting player-created character
                    SaveCharacterNameText.text = "Overwrite: " + CharacterName.text + "?";
                    SaveConfirmWindow.SetActive(true);
                    yield break;
                }
                else
                {
                    // Cannot overwrite premade characters - show "no save" prompt
                    Debug.LogWarning($"Cannot save as '{CharacterName.text}' - A premade character with this name already exists. Please choose a different name.");
                    FadeTextInAndOut(noSavePrompt, holdTime, fadeInTime);
                    yield break;
                }
            }

            // If we get here, the name is unique - proceed with save
            yield return StartCoroutine(PerformSave());
        }

        /// <summary>
        /// Confirms overwriting an existing player-created character
        /// Called by the confirm button in SaveConfirmWindow
        /// </summary>
        public void ConfirmSave()
        {
            StartCoroutine(PerformSave());
            SaveConfirmWindow.SetActive(false);
        }

        /// <summary>
        /// Performs the actual save operation
        /// </summary>
        private IEnumerator PerformSave()
        {
            yield return new WaitForEndOfFrame();

            // Capture character icon
            RenderTexture.active = iconTexture;
            Texture2D icon = new Texture2D(iconTexture.width, iconTexture.height, TextureFormat.RGBA32, false);
            Rect rect = new Rect(new Rect(0, 0, iconTexture.width, iconTexture.height));
            icon.ReadPixels(rect, 0,0);
            icon.Apply();

            byte[] bytes = icon.EncodeToPNG();

            if(!Directory.Exists(BMAC_SaveSystem.iconFilePath))
            {
                Directory.CreateDirectory(BMAC_SaveSystem.iconFilePath);
            }

            string filePath = Path.Combine(BMAC_SaveSystem.iconFilePath, CharacterName.text + ".png");

            File.WriteAllBytes(filePath, bytes);

            Texture2D characterIcon = null;

            characterIcon = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            characterIcon.LoadImage(bytes);

            // Force save (bypassing the duplicate check since we've already confirmed)
            BMAC_SaveSystem.SaveCharacterForced(character, CharacterName.text, characterIcon);
            
#if UNITY_EDITOR
            // Optional: Create ScriptableObject asset in the project (Editor only)
            if (createScriptableObjectOnSave)
            {
                CreateCharacterScriptableObject(characterIcon);
            }
#endif
            
            UpdateCharacterSaves();
            
            // Set the newly saved character as the currently loaded character
            loadedCharacterNameText.text = CharacterName.text;
            
            // Show "saved" prompt on successful save
            FadeTextInAndOut(savedPrompt, holdTime, fadeInTime);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Creates a ScriptableObject asset for the character in the project
        /// Only works in Unity Editor
        /// </summary>
        private void CreateCharacterScriptableObject(Texture2D characterIcon)
        {
            try
            {
                // Get character data from the save system
                var characterData = BMAC_SaveSystem.GetCharacterData(character);
                characterData.characterName = CharacterName.text;
                
                // Create new CharacterObject ScriptableObject
                var characterObject = ScriptableObject.CreateInstance<CharacterObject>();
                characterObject.data = characterData;
                characterObject.icon = characterIcon;
                
                // Ensure the directory exists
                string fullPath = Path.Combine("Assets", scriptableObjectSavePath);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                
                // Create the asset
                string assetPath = Path.Combine(fullPath, CharacterName.text + ".asset");
                AssetDatabase.CreateAsset(characterObject, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Debug.Log($"Created CharacterObject ScriptableObject at: {assetPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create CharacterObject ScriptableObject: {ex.Message}");
            }
        }
#endif

        public async void LoadCharacter(CharacterData data)
        {
            loadedCharacterNameText.text = data.characterName;
            await BMAC_SaveSystem.LoadCharacter(character, data);
            
            // Update hand colors after loading a character
            UpdateHandColorsFromBody();
        }

        public void DeleteCharacter()
        {
            if (loadedCharacterNameText.text == "")
            {
                // Show "no delete" prompt when no character is loaded
                FadeTextInAndOut(noDeletePrompt, holdTime, fadeInTime);
                return;
            }
            
            // Check if this is a premade character (can't be deleted)
            if (!BMAC_SaveSystem.IsPlayerCreatedCharacter(loadedCharacterNameText.text))
            {
                // Show "no delete" prompt when trying to delete a premade character
                Debug.LogWarning($"Cannot delete '{loadedCharacterNameText.text}' - This is a premade character and cannot be deleted.");
                FadeTextInAndOut(noDeletePrompt, holdTime, fadeInTime);
                return;
            }
            
            DeleteCharacterNameText.text = "Delete: " + loadedCharacterNameText.text;
            DeleteConfirmWindow.SetActive(true);
        }

        public void ConfirmDelete()
        {
            BMAC_SaveSystem.DeleteCharacter(loadedCharacterNameText.text);
            loadedCharacterNameText.text = "";
            UpdateCharacterSaves();
        }
        
        private string GetAssetPath(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj);
#else
            return "Editor Only";
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("Fix Character Object Paths")]
        public void FixCharacterObjectPaths()
        {
            // Load all CharacterObject assets from the Resources folder
            var characterObjects = Resources.LoadAll<CharacterObject>("");
            
            Debug.Log($"Found {characterObjects.Length} CharacterObject assets to process");
            
            int fixedCount = 0;
            
            foreach (var characterObject in characterObjects)
            {
                bool modified = false;
                
                // Check if the data exists
                if (characterObject.data == null || characterObject.data.outfitDatas == null)
                {
                    Debug.LogWarning($"Skipping {characterObject.name} - no outfit data found");
                    continue;
                }
                
                // Process each outfit data
                foreach (var outfitData in characterObject.data.outfitDatas)
                {
                    if (string.IsNullOrEmpty(outfitData.outfit))
                        continue;
                    
                    string oldPath = outfitData.outfit;
                    
                    // Check if this is a Body outfit that needs fixing
                    if (oldPath.Contains("/Body/") || oldPath.StartsWith("Body/"))
                    {
                        // Remove existing prefix if any
                        string cleanPath = oldPath;
                        if (cleanPath.StartsWith("Base/"))
                            cleanPath = cleanPath.Substring(5); // Remove "Base/"
                        else if (cleanPath.StartsWith("Common/"))
                            cleanPath = cleanPath.Substring(7); // Remove "Common/"
                        
                        // Add Common prefix
                        string newPath = "Common/" + cleanPath;
                        
                        if (oldPath != newPath)
                        {
                            outfitData.outfit = newPath;
                            Debug.Log($"Fixed Body path in {characterObject.name}: {oldPath} -> {newPath}");
                            modified = true;
                        }
                    }
                    else
                    {
                        // For non-Body outfits, ensure they have Base/ prefix
                        if (!oldPath.StartsWith("Base/") && !oldPath.StartsWith("Common/"))
                        {
                            outfitData.outfit = "Base/" + oldPath;
                            Debug.Log($"Fixed path in {characterObject.name}: {oldPath} -> {outfitData.outfit}");
                            modified = true;
                        }
                    }
                }
                
                if (modified)
                {
                    // Mark the asset as dirty so Unity saves the changes
                    UnityEditor.EditorUtility.SetDirty(characterObject);
                    fixedCount++;
                }
            }
            
            // Save all modified assets
            if (fixedCount > 0)
            {
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
                Debug.Log($"Successfully fixed {fixedCount} CharacterObject assets. Changes saved!");
            }
            else
            {
                Debug.Log("No paths needed fixing - all assets are already correct!");
            }
        }

        [ContextMenu("Migrate Old Characters")]
        public void MigrateOldCharacters()
        {
            // Define paths
            string oldPath1 = "Characters";
            string oldPath2 = "X_Characters";
            string newBasePath = "Assets/BoZo_StylizedModularCharacters/CustomCharacters/Resources/";
            
            int migratedCount = 0;
            int errorCount = 0;
            
            // Create target directories if they don't exist
            string newPath1 = newBasePath + "Characters";
            string newPath2 = newBasePath + "X_Characters";
            
            if (!System.IO.Directory.Exists(newPath1))
            {
                System.IO.Directory.CreateDirectory(newPath1);
                Debug.Log($"Created directory: {newPath1}");
            }
            
            if (!System.IO.Directory.Exists(newPath2))
            {
                System.IO.Directory.CreateDirectory(newPath2);
                Debug.Log($"Created directory: {newPath2}");
            }
            
            // Load old character objects
            var oldCharacters1 = Resources.LoadAll<BSMC_CharacterObject>(oldPath1);
            var oldCharacters2 = Resources.LoadAll<BSMC_CharacterObject>(oldPath2);
            
            Debug.Log($"Found {oldCharacters1.Length} characters in Resources/{oldPath1}");
            Debug.Log($"Found {oldCharacters2.Length} characters in Resources/{oldPath2}");
            
            // Migrate Characters folder
            foreach (var oldChar in oldCharacters1)
            {
                if (MigrateCharacter(oldChar, newPath1))
                    migratedCount++;
                else
                    errorCount++;
            }
            
            // Migrate X_Characters folder
            foreach (var oldChar in oldCharacters2)
            {
                if (MigrateCharacter(oldChar, newPath2))
                    migratedCount++;
                else
                    errorCount++;
            }
            
            // Refresh the asset database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Migration complete! Successfully migrated {migratedCount} characters. Errors: {errorCount}");
        }

        private bool MigrateCharacter(BSMC_CharacterObject oldChar, string targetPath)
        {
            try
            {
                // Create new CharacterObject
                var newChar = ScriptableObject.CreateInstance<CharacterObject>();
                
                // Use the UpdateVersion method from the old character object to get converted data
                CharacterData convertedData = oldChar.UpdateVersion();
                
                if (convertedData == null)
                {
                    Debug.LogError($"Failed to convert data for {oldChar.name}");
                    return false;
                }
                
                // Assign converted data
                newChar.data = convertedData;
                
                // Try to copy icon if it exists
                if (oldChar != null)
                {
                    // Try to get the icon through reflection or serialization
                    var iconField = typeof(BSMC_CharacterObject).GetField("icon", 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);
                    
                    if (iconField != null)
                    {
                        var iconValue = iconField.GetValue(oldChar) as Texture2D;
                        if (iconValue != null)
                        {
                            newChar.icon = iconValue;
                        }
                    }
                }
                
                // Create asset at new location
                string assetPath = $"{targetPath}/{oldChar.name}.asset";
                AssetDatabase.CreateAsset(newChar, assetPath);
                
                Debug.Log($"Migrated: {oldChar.name} -> {assetPath}");
                
                // Try to export icon as PNG if it exists
                if (newChar.icon != null)
                {
                    ExportIconAsPNG(newChar.icon, targetPath, oldChar.name);
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to migrate {oldChar.name}: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private void ExportIconAsPNG(Texture2D icon, string targetPath, string characterName)
        {
            try
            {
                // Make texture readable if it isn't
                Texture2D readableTexture = icon;
                
                if (!icon.isReadable)
                {
                    // Create a temporary RenderTexture
                    RenderTexture tmp = RenderTexture.GetTemporary(
                        icon.width,
                        icon.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

                    // Blit the texture to the RenderTexture
                    Graphics.Blit(icon, tmp);
                    
                    // Backup the currently set RenderTexture
                    RenderTexture previous = RenderTexture.active;
                    
                    // Set the current RenderTexture to the temporary one
                    RenderTexture.active = tmp;
                    
                    // Create a new readable Texture2D
                    readableTexture = new Texture2D(icon.width, icon.height);
                    readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                    readableTexture.Apply();
                    
                    // Reset the active RenderTexture
                    RenderTexture.active = previous;
                    
                    // Release the temporary RenderTexture
                    RenderTexture.ReleaseTemporary(tmp);
                }
                
                // Encode to PNG
                byte[] bytes = readableTexture.EncodeToPNG();
                
                // Save to file
                string iconPath = $"{targetPath}/{characterName}_Icon.png";
                System.IO.File.WriteAllBytes(iconPath, bytes);
                
                AssetDatabase.ImportAsset(iconPath);
                
                Debug.Log($"Exported icon: {iconPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to export icon for {characterName}: {ex.Message}");
            }
        }
#endif

/// <summary>
/// Saves the current creator character as PlayerCharacter.json in the X_Characters folder
/// This character will auto-load in the next scene
/// </summary>
public void SaveAsPlayerCharacter()
{
    StartCoroutine(SavePlayerCharacterCoroutine());
}

/// <summary>
/// Coroutine to save the current character as the player character
/// </summary>
private IEnumerator SavePlayerCharacterCoroutine()
{
    yield return new WaitForEndOfFrame();
    
    string playerCharacterName = "PlayerCharacter";
    
    // Create X_Characters subfolder if it doesn't exist
    string xCharactersPath = Path.Combine(BMAC_SaveSystem.filePath, "X_Characters");
    if (!Directory.Exists(xCharactersPath))
    {
        Directory.CreateDirectory(xCharactersPath);
        Debug.Log($"Created X_Characters folder at: {xCharactersPath}");
    }
    
    // Create icons subfolder for X_Characters if it doesn't exist
    string xCharactersIconPath = Path.Combine(BMAC_SaveSystem.iconFilePath, "X_Characters");
    if (!Directory.Exists(xCharactersIconPath))
    {
        Directory.CreateDirectory(xCharactersIconPath);
        Debug.Log($"Created X_Characters icons folder at: {xCharactersIconPath}");
    }
    
    // Capture character icon
    RenderTexture.active = iconTexture;
    Texture2D icon = new Texture2D(iconTexture.width, iconTexture.height, TextureFormat.RGBA32, false);
    Rect rect = new Rect(0, 0, iconTexture.width, iconTexture.height);
    icon.ReadPixels(rect, 0, 0);
    icon.Apply();
    
    byte[] iconBytes = icon.EncodeToPNG();
    
    // Save icon to X_Characters subfolder
    string iconFilePath = Path.Combine(xCharactersIconPath, playerCharacterName + ".png");
    File.WriteAllBytes(iconFilePath, iconBytes);
    Debug.Log($"Player character icon saved to: {iconFilePath}");
    
    // Get character data
    var characterData = BMAC_SaveSystem.GetCharacterData(character);
    characterData.characterName = playerCharacterName;
    
    // Save JSON to X_Characters subfolder
    string jsonData = JsonUtility.ToJson(characterData, true);
    string jsonFilePath = Path.Combine(xCharactersPath, playerCharacterName + ".json");
    File.WriteAllText(jsonFilePath, jsonData);
    
    Debug.Log($"Player character saved to: {jsonFilePath}");
    
    // Show success feedback
    if (savedPrompt != null)
    {
        FadeTextInAndOut(savedPrompt, holdTime, fadeInTime);
    }
}
    }
}
