using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonMasterAI
{
    public class ChoiceManager : MonoBehaviour
    {
        [Header("UI")]
        public GameObject      choiceButtonPrefab;
        public Transform       choiceContainer;
        public GameObject      diceResultPanel;
        public TextMeshProUGUI diceResultText;

        [Header("Fail damage (dice)")]
        public int minFailDamage = 2;
        public int maxFailDamage = 6;

        void Start()
        {
            if (diceResultPanel != null) diceResultPanel.SetActive(false);
        }

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

        void CreateButton(ChoiceData choice)
        {
            GameObject btn = Instantiate(choiceButtonPrefab, choiceContainer);

            TextMeshProUGUI[] texts = btn.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 1) texts[0].text = choice.text;
            if (texts.Length >= 2) texts[1].text = BuildStatLabel(choice);

            ChoiceData captured = choice;
            btn.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(captured));
        }

        string BuildStatLabel(ChoiceData choice)
        {
            if (PlayerStats.Instance == null)
                return $"{choice.stat}  |  DC {choice.dc}";

            string statName = PlayerStats.Instance.GetStatName(choice.stat);
            int    modifier = PlayerStats.Instance.GetModifier(choice.stat);
            string modStr   = modifier >= 0 ? $"+{modifier}" : $"{modifier}";

            return $"{statName} ({modStr})  |  DC {choice.dc}";
        }

        void OnChoiceSelected(ChoiceData choice)
        {
            HideChoices();

            int  modifier = PlayerStats.Instance?.GetModifier(choice.stat) ?? 0;
            int  roll     = Random.Range(1, 21);
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

        void ShowDiceResult(int roll, int modifier, int total,
                            ChoiceData choice, bool success, int damage)
        {
            if (diceResultPanel == null || diceResultText == null) return;

            string statName = PlayerStats.Instance?.GetStatName(choice.stat) ?? choice.stat;
            string modStr   = modifier >= 0 ? $"+{modifier}" : $"{modifier}";
            string color    = success ? "#88FF88" : "#FF6666";
            string outcome  = success ? "✓ SUCCESS" : "✗ FAILURE";

            diceResultText.text =
                $"<b>{statName}</b>  DC {choice.dc}\n" +
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