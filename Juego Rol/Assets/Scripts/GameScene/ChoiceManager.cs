using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonMasterAI
{
    /// <summary>
    /// Genera botones con DC y stat. Al pulsar ejecuta la tirada D20
    /// usando los stats reales del personaje cargado desde el JSON.
    /// </summary>
    public class ChoiceManager : MonoBehaviour
    {
        [Header("UI")]
        public GameObject      choiceButtonPrefab;
        public Transform       choiceContainer;
        public GameObject      diceResultPanel;
        public TextMeshProUGUI diceResultText;

        [Header("Daño por fallo (dados)")]
        public int minFailDamage = 2;
        public int maxFailDamage = 6;

        // ── Lifecycle ─────────────────────────────────────────────────────

        void Start()
        {
            if (diceResultPanel != null) diceResultPanel.SetActive(false);
        }

        // ── API pública ───────────────────────────────────────────────────

        public void ShowChoices(ChoiceData[] choices)
        {
            HideChoices();
            choiceContainer.gameObject.SetActive(true);

            foreach (ChoiceData choice in choices)
                CreateButton(choice);
        }

        public void HideChoices()
        {
            foreach (Transform child in choiceContainer)
                Destroy(child.gameObject);
            choiceContainer.gameObject.SetActive(false);
        }

        // ── Creación de botón ─────────────────────────────────────────────

        void CreateButton(ChoiceData choice)
        {
            GameObject btn = Instantiate(choiceButtonPrefab, choiceContainer);

            // El prefab necesita 2 TextMeshProUGUI:
            //   [0] → texto de la opción
            //   [1] → stat | modificador | CD
            TextMeshProUGUI[] texts = btn.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 1)
                texts[0].text = choice.text;

            if (texts.Length >= 2)
                texts[1].text = BuildStatLabel(choice);

            ChoiceData captured = choice;
            btn.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(captured));
        }

        string BuildStatLabel(ChoiceData choice)
        {
            if (PlayerStats.Instance == null)
                return $"{choice.stat}  |  CD {choice.dc}";

            string statName = PlayerStats.Instance.GetStatName(choice.stat);
            int    modifier = PlayerStats.Instance.GetModifier(choice.stat);
            string modStr   = modifier >= 0 ? $"+{modifier}" : $"{modifier}";

            return $"{statName} ({modStr})  |  CD {choice.dc}";
        }

        // ── Tirada D&D ────────────────────────────────────────────────────

        void OnChoiceSelected(ChoiceData choice)
        {
            HideChoices();

            int  modifier = PlayerStats.Instance?.GetModifier(choice.stat) ?? 0;
            int  roll     = Random.Range(1, 21);        // 1d20
            int  total    = roll + modifier;
            bool success  = total >= choice.dc;
            int  damage   = 0;

            if (!success)
            {
                damage = Random.Range(minFailDamage, maxFailDamage + 1);
                PlayerStats.Instance?.TakeDamage(damage);
            }

            ShowDiceResult(roll, modifier, total, choice, success, damage);

            GameEventSystem.Instance.TriggerEvent(
                GameEvent.CheckResult(choice.text, choice.stat, choice.dc,
                                      roll, modifier, success, damage));
        }

        // ── Panel de resultado ────────────────────────────────────────────

        void ShowDiceResult(int roll, int modifier, int total,
                            ChoiceData choice, bool success, int damage)
        {
            if (diceResultPanel == null || diceResultText == null) return;

            string statName = PlayerStats.Instance?.GetStatName(choice.stat) ?? choice.stat;
            string modStr   = modifier >= 0 ? $"+{modifier}" : $"{modifier}";
            string color    = success ? "#88FF88" : "#FF6666";
            string outcome  = success ? "✓ ÉXITO" : "✗ FALLO";

            diceResultText.text =
                $"<b>{statName}</b>  CD {choice.dc}\n" +
                $"🎲 {roll} {modStr} = <b>{total}</b>\n" +
                $"<color={color}><b>{outcome}</b></color>" +
                (success ? "" : $"\n<color=#FF4444>−{damage} HP</color>");

            diceResultPanel.SetActive(true);
            Invoke(nameof(HideDiceResult), 3f);
        }

        void HideDiceResult()
        {
            if (diceResultPanel != null) diceResultPanel.SetActive(false);
        }
    }
}