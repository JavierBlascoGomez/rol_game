using UnityEngine;

public class CharacterCreationSceneManager : MonoBehaviour
{
    public int status = 1;

    [Header("Canvases")]
    public GameObject explanationCanvas;
    public GameObject statsGeneratorCanvas;
    public GameObject inputCharacterNameCanvas;

    void Start()
    {
        UpdateUI(); 
    }

    public void NextStep()
    {
        if (status < 3)
        {
            status++;
            UpdateUI(); 
        }
    }

    public void PreviousStep()
    {
        if (status > 1)
        {
            status--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        switch (status)
        {
            case 1:
                explanationCanvas.SetActive(true);
                statsGeneratorCanvas.SetActive(false);
                inputCharacterNameCanvas.SetActive(false);
                break;

            case 2:
                explanationCanvas.SetActive(false);
                statsGeneratorCanvas.SetActive(true);
                inputCharacterNameCanvas.SetActive(false);
                break;
                
            case 3:
                explanationCanvas.SetActive(false);
                statsGeneratorCanvas.SetActive(false);
                inputCharacterNameCanvas.SetActive(true);
                break;
        }
    }
}
