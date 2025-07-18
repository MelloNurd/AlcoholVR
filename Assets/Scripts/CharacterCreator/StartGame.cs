using Bozo.ModularCharacters;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    DemoCharacterCreator characterCreator;
    private void Awake()
    {
        characterCreator = FindFirstObjectByType<DemoCharacterCreator>();
    }

    public void SavePlayerCharacter()
    {
        characterCreator.CharacterName.text = "PlayerCharacter";
        characterCreator.StartSave();
    }
}
