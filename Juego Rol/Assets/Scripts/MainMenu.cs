using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnNewGameButton()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnLoadGameButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnOptionsButton()
    {
        SceneManager.LoadScene("OptionsScene");
    }

    public void OnExitGameButton()
    {
        Application.Quit();
    }
}
