using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;


namespace Bozo.ModularCharacters
{


    public static class BMAC_SaveSystem
    {
        public static string filePath = Application.persistentDataPath + "/BoZo_StylizedModularCharacters/CustomCharacters";
        public static string iconFilePath = Application.persistentDataPath + "/BoZo_StylizedModularCharacters/CustomCharacters/Icons";
        public static string assetPath = "/BoZo_StylizedModularCharacters/CustomCharacters/Resources/";
        public static string iconAssetPath = "/BoZo_StylizedModularCharacters/CustomCharacters/Icons/";

        /// <summary>
        /// Saves a character to the persistent data path
        /// Prevents saving if a character with the same name already exists (either player-created or premade)
        /// </summary>
        public static bool SaveCharacter(OutfitSystem outfitSystem, string saveName, Texture2D icon = null) 
        {
            // Check if a character with this name already exists
            if (CharacterExists(saveName))
            {
                Debug.LogWarning($"Cannot save as '{saveName}' - A character with this name already exists. Please choose a different name.");
                return false;
            }

            var data = GetCharacterData(outfitSystem);

            data.characterName = saveName;




            var CharacterSave = ScriptableObject.CreateInstance<CharacterObject>();
            CharacterSave.data = data;
            Debug.Log(icon);
            CharacterSave.icon = icon;
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(CharacterSave, "Assets/" + assetPath + "/" + saveName + ".asset");
            AssetDatabase.Refresh();
#endif

            string saveData = JsonUtility.ToJson(data);
            System.IO.File.WriteAllText(filePath + "/" + saveName + ".json", saveData);
            Debug.Log("Character Saved:" + filePath + "/" + saveName + ".json");
            return true;
        }

        /// <summary>
        /// Saves a character without checking for duplicates (used for confirmed overwrites)
        /// </summary>
        public static void SaveCharacterForced(OutfitSystem outfitSystem, string saveName, Texture2D icon = null) 
        {
            var data = GetCharacterData(outfitSystem);

            data.characterName = saveName;

            var CharacterSave = ScriptableObject.CreateInstance<CharacterObject>();
            CharacterSave.data = data;
            CharacterSave.icon = icon;

            // Save icon file if provided
            if (icon != null)
            {
                byte[] iconBytes = icon.EncodeToPNG();
                
                if (!System.IO.Directory.Exists(iconFilePath))
                {
                    System.IO.Directory.CreateDirectory(iconFilePath);
                }
                
                string iconPath = System.IO.Path.Combine(iconFilePath, saveName + ".png");
                System.IO.File.WriteAllBytes(iconPath, iconBytes);
                Debug.Log("Character Icon Saved: " + iconPath);
            }

            string saveData = JsonUtility.ToJson(data);
            System.IO.File.WriteAllText(filePath + "/" + saveName + ".json", saveData);
            Debug.Log("Character Saved: " + filePath + "/" + saveName + ".json");
        }

        public static CharacterData GetCharacterData(OutfitSystem outfitSystem)
        {
            if (outfitSystem.mergedMode) return outfitSystem.data;

            var data = new CharacterData();

            //Saving BlendShapes
            var bodyShapeValues = outfitSystem.GetBodyShapeValues();
            data.bodyIDs = bodyShapeValues.Keys.ToList();
            data.bodyShapes = bodyShapeValues.Values.ToList();

            var faceShapeValues = outfitSystem.GetFaceShapeValues();
            data.faceIDs = faceShapeValues.Keys.ToList();
            data.faceShapes = faceShapeValues.Values.ToList();

            //Saving Body Mods
            var modData = new List<BodyModData>();
            var modKeys = outfitSystem.bodyModifiers.Keys.ToList();
            for (int i = 0; i < modKeys.Count; i++)
            {
                var mod = outfitSystem.bodyModifiers[modKeys[i]].GetData();
                modData.Add(mod);
            }
            data.bodyMods = modData;
            data.bodyModsKeys = modKeys;

            //Saving Outfits
            var outfits = outfitSystem.GetOutfits();
            var outfitDataList = new List<OutfitData>();

            for (int i = 0; i < outfits.Count; i++)
            {
                if (outfits[i] == null) continue;
                outfitDataList.Add(outfits[i].GetOutfitData());
            }

            data.stance = outfitSystem.stance;
            data.outfitDatas = outfitDataList;

            return data;
        }

        public static async Task LoadCharacter(OutfitSystem outfitSystem, CharacterData characterObject = null, bool manualShapeApply = false, bool async = false)
        {

            CharacterData loadData = characterObject;


            //Loading Outfits

            List<Outfit> outfits = new List<Outfit>();

            if (async)
            {
                outfits = await LoadOutfits(loadData.outfitDatas);
            }
            else
            {
                foreach (var item in loadData.outfitDatas)
                {
                    // Try loading from Resources first (for premade characters)
                    var outfit = Resources.Load<Outfit>(item.outfit);
                    
                    // If not found in Resources, try alternative paths
                    if (outfit == null)
                    {
                        // Try with "Base/" prefix
                        outfit = Resources.Load<Outfit>("Base/" + item.outfit);
                    }
                    
                    if (outfit == null)
                    {
                        // Try with "Common/" prefix
                        outfit = Resources.Load<Outfit>("Common/" + item.outfit);
                    }
                    
                    if (outfit == null)
                    {
                        // Try removing any existing prefix and loading directly
                        string cleanPath = item.outfit.Replace("Base/", "").Replace("Common/", "");
                        outfit = Resources.Load<Outfit>(cleanPath);
                    }
                    
                    outfits.Add(outfit);
                }

            }



            outfitSystem.RemoveAllOutfits();

            for (int i = 0; i < loadData.outfitDatas.Count; i++)
            {
                var outfitData = loadData.outfitDatas[i];
                var outfit = outfits[i];


                if (outfit == null)
                {
                    Debug.LogWarning("Outfit Path: " + outfitData.outfit + " returns null make sure Prefab is named correctly");
                    continue;
                }
                //Check If Outfit is the same

                var inst = outfitSystem.InstantiateOutfit(outfit);


                //inst.Attach();

                if (inst.customShader)
                {
                    inst.SetSwatch(outfitData.swatch);
                    inst.SetColor(outfitData.color);
                }
                else
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (outfitData.colors.Count > c)
                        {
                            inst.SetColor(outfitData.colors[c], c + 1);
                        }

                        if (c + 1 <= 3 && outfitData.decal != "")
                        {
                            inst.SetDecalColor(outfitData.decalColors[c], c + 1);
                        }
                        if (c + 1 <= 3 && outfitData.pattern != "")
                        {
                            inst.SetPatternColor(outfitData.patternColors[c], c + 1);
                        }
                    }

                    var decal = Resources.Load<Texture>(outfitData.decal);
                    inst.SetDecal(decal);
                    inst.SetDecalSize(outfitData.decalScale);

                    var pattern = Resources.Load<Texture>(outfitData.pattern);
                    inst.SetPattern(pattern);
                    inst.SetPatternSize(outfitData.patternScale);
                }


            }



            //Loading Body Morphs

            for (int i = 0; i < loadData.bodyIDs.Count; i++)
            {
                outfitSystem.SetShape(loadData.bodyIDs[i], loadData.bodyShapes[i]);
            }

            for (int i = 0; i < loadData.faceIDs.Count; i++)
            {
                outfitSystem.SetShape(loadData.faceIDs[i], loadData.faceShapes[i]);
            }

            if (!manualShapeApply) LoadBodyMods(outfitSystem, loadData);


            outfitSystem.animator.Rebind();
            outfitSystem.SetStance(loadData.stance);
        }

        public static void LoadBodyMods(OutfitSystem outfitSystem, CharacterData loadData)
        {
            for (int i = 0; i < loadData.bodyModsKeys.Count; i++)
            {
                outfitSystem.bodyModifiers[loadData.bodyModsKeys[i]].SetData(loadData.bodyMods[i]);
            }
        }

        public static CharacterData GetDataFromID(string saveName)
        {
            CharacterData loadData;
            Debug.Log("Attempted Load at: " + filePath + "/" + saveName + ".json");
            if (!System.IO.File.Exists(filePath + "/" + saveName + ".json"))
            {
                Debug.LogWarning($"Save ID: {saveName} does not exist. Make sure input matches an existing Save");
                return null;
            }
            else
            {
                string data = System.IO.File.ReadAllText(filePath + "/" + saveName + ".json");
                loadData = JsonUtility.FromJson<CharacterData>(data);
                return loadData;
            }
        }

        public static async Task<List<Outfit>> LoadOutfits(List<OutfitData> outfitDatas)
        {
            List<Task<Outfit>> loadTasks = new List<Task<Outfit>>();
            
            foreach (var data in outfitDatas)
            {
                loadTasks.Add(LoadOutfitWithFallback(data.outfit));
            }
            
            return (await Task.WhenAll(loadTasks)).ToList();
        }

        private static async Task<Outfit> LoadOutfitWithFallback(string path)
        {
            // Try original path
            var outfit = await LoadResourceAsync<Outfit>(path);
            if (outfit != null) return outfit;
            
            // Try with "Base/" prefix
            outfit = await LoadResourceAsync<Outfit>("Base/" + path);
            if (outfit != null) return outfit;
            
            // Try with "Common/" prefix
            outfit = await LoadResourceAsync<Outfit>("Common/" + path);
            if (outfit != null) return outfit;
            
            // Try removing any existing prefix
            string cleanPath = path.Replace("Base/", "").Replace("Common/", "");
            outfit = await LoadResourceAsync<Outfit>(cleanPath);
            
            return outfit;
        }

        public static async Task<T> LoadResourceAsync<T>(string path) where T : UnityEngine.Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            var tcs = new TaskCompletionSource<T>();

            request.completed += operation =>
            {
                if (request.asset == null)
                {
                    tcs.SetResult(null);
                }
                else if (request.asset is not T result)
                {
                    tcs.SetResult(null);
                }
                else
                {
                    tcs.SetResult(result);
                }
            };

            return await tcs.Task;
        }

        /// <summary>
        /// Checks if a character with the given name exists (either player-created or premade)
        /// </summary>
        public static bool CharacterExists(string characterName)
        {
            // Check if it exists as a player-created character
            if (IsPlayerCreatedCharacter(characterName))
                return true;
            
            // Check if it exists as a premade CharacterObject in Resources
            // Need to check all possible paths where CharacterObjects might be stored
            var saveObjects = Resources.LoadAll<CharacterObject>("");
            foreach (var saveObject in saveObjects)
            {
                if (saveObject.data != null && saveObject.data.characterName == characterName)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a character is a player-created character (can be deleted)
        /// Returns true if the character has a JSON save file in the persistent data path
        /// </summary>
        public static bool IsPlayerCreatedCharacter(string characterName)
        {
            string jsonPath = filePath + "/" + characterName + ".json";
            return System.IO.File.Exists(jsonPath);
        }

        /// <summary>
        /// Deletes a player-created character
        /// Only deletes characters that exist in the persistent data path (not premade Base folder characters)
        /// </summary>
        public static void DeleteCharacter(string characterName)
        {
            // Check if this is a player-created character
            if (!IsPlayerCreatedCharacter(characterName))
            {
                Debug.LogWarning($"Cannot delete '{characterName}' - This is a premade character and cannot be deleted. Only player-created characters can be deleted.");
                return;
            }

            // Delete player-created character files
            System.IO.File.Delete(filePath + "/" + characterName + ".json");
            System.IO.File.Delete(iconFilePath + "/" + characterName + ".png");
            
            // Note: Asset files are only created in editor mode, so we only attempt to delete them if they exist
            string assetFilePath = "Assets/" + assetPath + "/" + characterName + ".asset";
            string assetMetaPath = "Assets/" + assetPath + "/" + characterName + ".meta";
            string iconAssetFilePath = "Assets/" + iconAssetPath + characterName + ".png";
            string iconAssetMetaPath = "Assets/" + iconAssetPath + characterName + ".meta";
            
            if (System.IO.File.Exists(assetFilePath))
                System.IO.File.Delete(assetFilePath);
            if (System.IO.File.Exists(assetMetaPath))
                System.IO.File.Delete(assetMetaPath);
            if (System.IO.File.Exists(iconAssetFilePath))
                System.IO.File.Delete(iconAssetFilePath);
            if (System.IO.File.Exists(iconAssetMetaPath))
                System.IO.File.Delete(iconAssetMetaPath);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

            Debug.Log($"Deleted player-created character: {characterName}");
        }
    }

    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public List<string> bodyIDs;
        public List<float> bodyShapes;
        public List<string> faceIDs;
        public List<float> faceShapes;
        public List<string> bodyModsKeys;
        public List<BodyModData> bodyMods;
        public List<OutfitData> outfitDatas;
        public OutfitData bodyData;
        public float stance;
    }

    [System.Serializable]
    public class OutfitData
    {
        public string outfit;
        public List<Color> colors;

        public string decal;
        public List<Color> decalColors;
        public Vector4 decalScale;

        public string pattern;
        public List<Color> patternColors;
        public Vector4 patternScale;

        //Custom Shader Data
        public Color color = Color.white;
        public int swatch;

    }

}