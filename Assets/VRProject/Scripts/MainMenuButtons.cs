using UnityEngine;

public class MainMenuButtons : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
