using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Press On StartButton");
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Debug.Log("Press On QuitButton");
        Application.Quit();
        Debug.Log("Game Closed");
    }
}
