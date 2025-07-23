using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HandColorer : MonoBehaviour
{
    Material skin;
    [SerializeField] BSMC_CharacterObject characterObject;
    public List<Color> SkinColor = new List<Color>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (characterObject == null)
        {
            string path = "Assets/BoZo_StylizedModularCharacters/CustomCharacters/Unaccessible/PlayerCharacter.asset";
            path = path.Cleaned();

            // Get characterObject from Assets/BoZo_StylizedModularCharacters/CustomCharacters/Unaccessible/PlayerCharacter.asset
            characterObject = AssetDatabase.LoadAssetAtPath<BSMC_CharacterObject>(path);
        }
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
        skin.SetColor("_SkinTone", newColor);
    }
}
