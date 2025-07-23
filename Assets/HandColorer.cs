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

        var skinMaterial = GetComponent<Renderer>().material;

        // Ensure we have enough colors before applying them
        if (SkinColor.Count >= 10)
        {
            skinMaterial.SetColor("_SkinTone", SkinColor[0]);
            skinMaterial.SetColor("_SkinUnderTone", SkinColor[1]);
            skinMaterial.SetColor("_BrowColor", SkinColor[2]);
            skinMaterial.SetColor("_LashesColor", SkinColor[3]);
            skinMaterial.SetColor("_FuzzColor", SkinColor[4]);
            skinMaterial.SetColor("_UnderwearBottomColor_Opacity", SkinColor[5]);
            skinMaterial.SetColor("_UnderwearTopColor_Opacity", SkinColor[6]);
            skinMaterial.SetColor("_Acc_Color_1", SkinColor[7]);
            skinMaterial.SetColor("_Acc_Color_2", SkinColor[8]);
            skinMaterial.SetColor("_Acc_Color_3", SkinColor[9]);

            // Also set the accessory texture if available
            if (characterObject.skinAccessory != null)
            {
                skinMaterial.SetTexture("_Accessory", characterObject.skinAccessory);
            }
        }
        else
        {
            Debug.LogWarning("Not enough skin colors available in the character object or SkinColor list.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
