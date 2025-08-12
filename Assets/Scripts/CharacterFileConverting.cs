using System.Collections;
using System.Collections.Generic;
using System.IO;
using Bozo.ModularCharacters;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.Events;

public class CharacterFileConverting : MonoBehaviour
{
    public static string JsonOutputRoot => Path.Combine(Application.persistentDataPath);
    public static string CharactersFolder => "Characters";
    public static string UnaccessibleCharactersFolder => "X_" + CharactersFolder; // Folder for characters that are not accessible by the player
    public static string DestinationPath => Path.Combine(JsonOutputRoot, CharactersFolder);

    public static UnityEvent<bool> OnConversionFinish = new();

    int conversionCount = 0;

    private void Start()
    {
        conversionCount += ConvertCharactersFromResources(CharactersFolder);
        conversionCount += ConvertCharactersFromResources("X_" + CharactersFolder); // Characters in X_folder are unaccessible by the player
        Debug.Log($"Total converted characters: {conversionCount}");
    }

    public int ConvertCharactersFromResources(string dir)
    {
        int localConversionCount = 0;

        var destination = Path.Combine(JsonOutputRoot, dir);
        Directory.CreateDirectory(destination);

        BSMC_CharacterObject[] characterObjects = Resources.LoadAll<BSMC_CharacterObject>(dir);

        if (characterObjects.Length == 0)
        {
            Debug.LogWarning("No character objects found in Resources/Characters. Please ensure you have characters to convert.");
            return localConversionCount;
        }

        try
        {
            // Convert each character
            for (int i = 0; i < characterObjects.Length; i++)
            {
                BSMC_CharacterObject characterObject = characterObjects[i];

                string fileName = characterObject.name + ".json";
                string targetFilePath = Path.Combine(destination, fileName);

                if (File.Exists(targetFilePath))
                {
                    continue; // Skip already existing file
                }

                // Convert to JSON
                string jsonData = JsonUtility.ToJson(characterObject, true);

                // Save file
                File.WriteAllText(targetFilePath, jsonData);
                localConversionCount++;
            }

            OnConversionFinish.Invoke(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during character conversion: {ex.Message}");
            OnConversionFinish.Invoke(false);
            return localConversionCount;
        }

        Debug.Log($"Converted {localConversionCount} characters from \"Resources/{dir}\" to \"{destination}\"");
        return localConversionCount;
    }
}
