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
    void Start()
    {
        skin = GetComponent<Renderer>().material;
        SkinColor = GetColors();
        Debug.Log("Skincolor count: " + SkinColor.Count);
        SetColors(SkinColor);
    }

    public void UpdateHandColor(Color newColor)
    {
        skin.SetColor("_Color_7", newColor);
    }
    
    public void SetRandomHandColor()
    {
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
