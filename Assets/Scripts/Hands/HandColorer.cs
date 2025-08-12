using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class HandColorer : MonoBehaviour
{
    Material skin;
    BSMC_CharacterObject characterObject;
    public List<Color> SkinColor = new List<Color>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string path = Path.Combine(CharacterFileConverting.JsonOutputRoot, CharacterFileConverting.UnaccessibleCharactersFolder, "PlayerCharacter.json");

        // Read JSON data
        string jsonData = File.ReadAllText(path);

        // Create temporary ScriptableObject and populate from JSON
        characterObject = ScriptableObject.CreateInstance<BSMC_CharacterObject>();
        JsonUtility.FromJsonOverwrite(jsonData, characterObject);

        // Load colors from the ScriptableObject if it's assigned
        if (characterObject != null && characterObject.SkinColor.Count > 0)
        {
            SkinColor = new List<Color>(characterObject.SkinColor);
        }
 
        skin = GetComponent<Renderer>().material;

        // Ensure we have enough colors before applying them
        if (SkinColor.Count >= 10)
        {
            skin.SetColor("_SkinTone", SkinColor[0]);
            skin.SetColor("_SkinUnderTone", SkinColor[1]);
            skin.SetColor("_BrowColor", SkinColor[2]);
            skin.SetColor("_LashesColor", SkinColor[3]);
            skin.SetColor("_FuzzColor", SkinColor[4]);
            skin.SetColor("_UnderwearBottomColor_Opacity", SkinColor[5]);
            skin.SetColor("_UnderwearTopColor_Opacity", SkinColor[6]);
            skin.SetColor("_Acc_Color_1", SkinColor[7]);
            skin.SetColor("_Acc_Color_2", SkinColor[8]);
            skin.SetColor("_Acc_Color_3", SkinColor[9]);

            // Also set the accessory texture if available
            if (characterObject.skinAccessory != null)
            {
                skin.SetTexture("_Accessory", characterObject.skinAccessory);
            }
        }
        else
        {
            Debug.LogWarning("Not enough skin colors available in the character object or SkinColor list.");
        }
    }

    public void UpdateHandColor(Color newColor)
    {
        if (skin == null)
        {
            Debug.Log("HandColorer: skin material not initialized yet. Skipping color update.");
            return;
        }

        skin.SetColor("_SkinTone", newColor);
    }
}
