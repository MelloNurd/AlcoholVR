using Bozo.ModularCharacters;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public string SceneName = "GameScene";
    DemoCharacterCreator characterCreator;
    private void Awake()
    {
        characterCreator = FindFirstObjectByType<DemoCharacterCreator>();
    }

    public void SaveAndStart()
    {
        characterCreator.CharacterName.text = "PlayerCharacter";
        characterCreator.StartSave();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
    }
}
