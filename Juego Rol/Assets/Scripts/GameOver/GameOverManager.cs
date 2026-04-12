using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class GameOverManager : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame || 
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            CargarStartScreen();
        }
    }

    public void CargarStartScreen()
    {
        SceneManager.LoadScene("StartScreen");
    }
}