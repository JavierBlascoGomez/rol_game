using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StatRollerButton : MonoBehaviour
{
    [Header("Configuración de la Estadística")]
    [Tooltip("Escribe aquí el nombre de la estadística a la que corresponde este botón (ej: fuerza, destreza)")]
    public string statName;


    [Header("Referencias UI")]
    public Button myButton;
    public Image dado1Background;
    public TextMeshProUGUI dado1Text;
    public Image dado2Background;
    public TextMeshProUGUI dado2Text;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip diceRollSound;

    [Header("Colores")]
    public Color colorHigh = Color.blue;
    public Color colorLow = Color.red;
    public Color colorDraw = Color.gray;
    private bool hasRolled = false;

    public StatsSetUp statsSetUp;

    void Start()
    {
        dado1Text.text = "";
        dado2Text.text = "";
        dado1Text.gameObject.SetActive(false);
        dado2Text.gameObject.SetActive(false);

        myButton.onClick.AddListener(OnRollClicked);
    }

    void OnRollClicked()
    {
        if (hasRolled) return; 
        
        hasRolled = true;
        myButton.interactable = false; 
        StartCoroutine(RollRoutine());
    }

    IEnumerator RollRoutine()
    {
        if (audioSource != null && diceRollSound != null)
        {
            audioSource.PlayOneShot(diceRollSound);
            yield return new WaitForSeconds(diceRollSound.length);
        }

        int roll1 = Random.Range(1, 21);
        int roll2 = Random.Range(1, 21);

        dado1Text.text = roll1.ToString();
        dado2Text.text = roll2.ToString();
        dado1Text.gameObject.SetActive(true);
        dado2Text.gameObject.SetActive(true);

        if (roll1 > roll2)
        {
            dado1Background.color = colorHigh;
            dado2Background.color = colorLow;
        }
        else if (roll2 > roll1)
        {
            dado2Background.color = colorHigh;
            dado1Background.color = colorLow;
        }
        else
        {
            dado1Background.color = colorDraw;
            dado2Background.color = colorDraw;
        }

        int statFinal = Mathf.Max(roll1, roll2);

        if (statsSetUp != null)
        {
            statsSetUp.AssignStat(statName, statFinal);
        }
    }
}
