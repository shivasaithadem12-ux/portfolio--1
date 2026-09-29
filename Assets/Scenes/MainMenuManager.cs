using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainMenuPanel;

    [Header("Music")]
    public AudioSource backgroundMusic;

    [Header("Gameplay")]
    public string gameSceneName = "SampleScene";

    // Called when Start button is clicked
    public void StartGame()
    {
        if (backgroundMusic != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        }

        
    }

    // Called when Quit button is clicked
    public void QuitGame()
    {
        Debug.Log("Game Quit");

        Application.Quit();
    }
}