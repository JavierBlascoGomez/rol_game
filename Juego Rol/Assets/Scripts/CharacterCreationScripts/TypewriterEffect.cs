using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TypewriterEffect : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración de Texto")]
    public TextMeshProUGUI textComponent;
    public TextAsset textFile; 
    public float typingSpeed = 0.05f;

    [Header("Configuración de Audio")]
    public AudioSource audioSource;
    public AudioClip beepSound;

    private string[] textLines;      
    private int currentLineIndex;    
    private bool isTyping;           
    private Coroutine typingCoroutine;

    [Header("GameManager")]
    public CharacterCreationSceneManager characterCreationSceneManager;

    void OnEnable()
    {
        if (textFile != null && textComponent != null)
        {

            textLines = textFile.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            currentLineIndex = 0;
            
            ShowNextLine();
        }
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                AdvanceText();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AdvanceText();
    }

    private void AdvanceText()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            textComponent.text = textLines[currentLineIndex];
            isTyping = false;
        }
        else
        {
            currentLineIndex++;

            if (currentLineIndex < textLines.Length)
            {
                ShowNextLine();
            }
            else
            {
                Debug.Log("El texto ha terminado.");
                characterCreationSceneManager.NextStep();
            }
        }
    }

    private void ShowNextLine()
    {
        typingCoroutine = StartCoroutine(TypeText(textLines[currentLineIndex]));
    }

    IEnumerator TypeText(string lineToType)
    {
        isTyping = true;
        textComponent.text = ""; 

        foreach (char c in lineToType)
        {
            textComponent.text += c;

            if (c != ' ' && audioSource != null && beepSound != null)
            {
                audioSource.PlayOneShot(beepSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}
