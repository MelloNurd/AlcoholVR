using Bozo.ModularCharacters;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class HandColorer : MonoBehaviour
{
    Material skin;
    public List<Color> SkinColor = new List<Color>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Initialize the material, but don't rely on this for external calls
        if (skin == null)
        {
            skin = GetComponent<Renderer>().material;
        }
        
        // Try to load PlayerCharacter colors from JSON
        if (!LoadPlayerCharacterColors())
        {
            // Fallback to getting colors from material if loading fails
            SkinColor = GetColors();
            Debug.Log("Using material colors. Skincolor count: " + SkinColor.Count);
            SetColors(SkinColor);
        }
    }

    /// <summary>
    /// Attempts to load PlayerCharacter.json from X_Characters subfolder
    /// and apply Color_7 to the hand material
    /// </summary>
    private bool LoadPlayerCharacterColors()
    {
        string playerCharacterPath = Path.Combine(BMAC_SaveSystem.filePath, "X_Characters", "PlayerCharacter.json");
        
        if (!File.Exists(playerCharacterPath))
        {
            Debug.LogWarning($"PlayerCharacter.json not found at: {playerCharacterPath}");
            return false;
        }

        try
        {
            // Read and deserialize the JSON
            string jsonData = File.ReadAllText(playerCharacterPath);
            CharacterData characterData = JsonUtility.FromJson<CharacterData>(jsonData);

            if (characterData == null || characterData.outfitDatas == null || characterData.outfitDatas.Count == 0)
            {
                Debug.LogWarning("PlayerCharacter.json is empty or invalid");
                return false;
            }

            // Find the Body outfit data (typically contains skin colors)
            OutfitData bodyOutfit = null;
            foreach (var outfitData in characterData.outfitDatas)
            {
                // Look for Body outfit - adjust this condition based on your naming convention
                if (outfitData.outfit != null && (outfitData.outfit.Contains("Body") || outfitData.outfit.Contains("body")))
                {
                    bodyOutfit = outfitData;
                    break;
                }
            }

            // If no specific body outfit found, use the first outfit with colors
            if (bodyOutfit == null)
            {
                foreach (var outfitData in characterData.outfitDatas)
                {
                    if (outfitData.colors != null && outfitData.colors.Count > 0)
                    {
                        bodyOutfit = outfitData;
                        break;
                    }
                }
            }

            if (bodyOutfit == null || bodyOutfit.colors == null || bodyOutfit.colors.Count < 7)
            {
                Debug.LogWarning("PlayerCharacter does not have Color_7 data available");
                return false;
            }

            // Color_7 is at index 6 (colors are typically 1-indexed but stored 0-indexed)
            Color skinColor = bodyOutfit.colors[0]; // Color_7
            
            // Apply the skin color to the hand
            UpdateHandColor(skinColor);
            
            Debug.Log($"Successfully loaded PlayerCharacter skin color: {skinColor}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading PlayerCharacter.json: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Updates hand colors to match a DataObject's skin color
    /// Supports both BSMC_CharacterObject (legacy) and CharacterObject (new format)
    /// </summary>
    /// <param name="dataObject">The data object to get colors from</param>
    public void SetColorsFromCharacterObject(DataObject dataObject)
    {
        if (dataObject == null)
        {
            Debug.LogWarning("Cannot set hand colors: DataObject is null");
            return;
        }

        // Try casting to legacy BSMC_CharacterObject first
        BSMC_CharacterObject bsmcCharacter = dataObject as BSMC_CharacterObject;
        if (bsmcCharacter != null)
        {
            if (bsmcCharacter.SkinColor == null || bsmcCharacter.SkinColor.Count == 0)
            {
                Debug.LogWarning("Cannot set hand colors: BSMC_CharacterObject has no skin colors");
                return;
            }

            // Get Color_1 from the character's skin colors (index 0)
            Color skinColor = bsmcCharacter.SkinColor[0];
            
            // Apply to the hand material
            UpdateHandColor(skinColor);
            
            Debug.Log($"Hand colors updated from BSMC_CharacterObject: {bsmcCharacter.name}, Color: {skinColor}");
            return;
        }

        // Try casting to new CharacterObject format
        CharacterObject characterObject = dataObject as CharacterObject;
        if (characterObject != null)
        {
            // Get CharacterData from the object
            CharacterData characterData = characterObject.GetCharacterData();
            
            if (characterData != null)
            {
                SetColorsFromCharacterData(characterData);
                return;
            }
            else
            {
                Debug.LogWarning("Cannot set hand colors: CharacterObject has no character data");
                return;
            }
        }

        // If neither cast succeeded, log an error
        Debug.LogWarning($"Cannot set hand colors: DataObject is neither BSMC_CharacterObject nor CharacterObject. Type: {dataObject.GetType().Name}");
    }

    /// <summary>
    /// Updates hand colors to match a CharacterData's skin color
    /// </summary>
    /// <param name="characterData">The character data to get colors from</param>
    public void SetColorsFromCharacterData(CharacterData characterData)
    {
        if (characterData == null)
        {
            Debug.LogWarning("Cannot set hand colors: CharacterData is null");
            return;
        }

        if (characterData.outfitDatas == null || characterData.outfitDatas.Count == 0)
        {
            Debug.LogWarning("Cannot set hand colors: CharacterData has no outfit data");
            return;
        }

        // Find the Body outfit data (typically contains skin colors)
        OutfitData bodyOutfit = null;
        foreach (var outfitData in characterData.outfitDatas)
        {
            if (outfitData.outfit != null && (outfitData.outfit.Contains("Body") || outfitData.outfit.Contains("body")))
            {
                bodyOutfit = outfitData;
                break;
            }
        }

        // If no specific body outfit found, use the first outfit with colors
        if (bodyOutfit == null)
        {
            foreach (var outfitData in characterData.outfitDatas)
            {
                if (outfitData.colors != null && outfitData.colors.Count > 0)
                {
                    bodyOutfit = outfitData;
                    break;
                }
            }
        }

        if (bodyOutfit == null || bodyOutfit.colors == null || bodyOutfit.colors.Count == 0)
        {
            Debug.LogWarning("Cannot set hand colors: No valid outfit data with colors found");
            return;
        }

        // Get Color_1 (index 0)
        Color skinColor = bodyOutfit.colors[0];
        
        // Apply to the hand material
        UpdateHandColor(skinColor);
        
        Debug.Log($"Hand colors updated from CharacterData, Color: {skinColor}");
    }

    public void UpdateHandColor(Color newColor)
    {
        // Ensure the material is initialized before use
        if (skin == null)
        {
            skin = GetComponent<Renderer>().material;
        }
        skin.SetColor("_Color_7", newColor);
    }
    
    public void SetRandomHandColor()
    {
        // Ensure the material is initialized before use
        if (skin == null)
        {
            skin = GetComponent<Renderer>().material;
        }
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        skin.SetColor("_Color_7", randomColor);
    }

    public List<Color> GetColors()
    {
        if (!skin) { skin = GetComponent<Renderer>().material; }
        
        var colors = new List<Color>();

        // Get main colors (Color_1 to Color_9)
        for (int i = 1; i <= 9; i++)
        {
            if (skin.HasProperty("_Color_" + i))
            {
                colors.Add(skin.GetColor("_Color_" + i));
            }
        }

        // Get decal colors (DecalColor_1 to DecalColor_3)
        for (int i = 1; i <= 3; i++)
        {
            if (skin.HasProperty("_DecalColor_" + i))
            {
                colors.Add(skin.GetColor("_DecalColor_" + i));
            }
        }

        // Get pattern colors (PatternColor_1 to PatternColor_3)
        for (int i = 1; i <= 3; i++)
        {
            if (skin.HasProperty("_PatternColor_" + i))
            {
                colors.Add(skin.GetColor("_PatternColor_" + i));
            }
        }

        return colors;
    }

    public void SetColors(List<Color> colors)
    {
        // Ensure the material is initialized before use
        if (skin == null)
        {
            skin = GetComponent<Renderer>().material;
        }
        int index = 0;
        // Set main colors (Color_1 to Color_9)
        for (int i = 1; i <= 9; i++)
        {
            if (skin.HasProperty("_Color_" + i) && index < colors.Count)
            {
                skin.SetColor("_Color_" + i, colors[index]);
                index++;
            }
        }
        // Set decal colors (DecalColor_1 to DecalColor_3)
        for (int i = 1; i <= 3; i++)
        {
            if (skin.HasProperty("_DecalColor_" + i) && index < colors.Count)
            {
                skin.SetColor("_DecalColor_" + i, colors[index]);
                index++;
            }
        }
        // Set pattern colors (PatternColor_1 to PatternColor_3)
        for (int i = 1; i <= 3; i++)
        {
            if (skin.HasProperty("_PatternColor_" + i) && index < colors.Count)
            {
                skin.SetColor("_PatternColor_" + i, colors[index]);
                index++;
            }
        }
    }
}
