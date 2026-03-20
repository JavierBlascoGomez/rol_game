using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonMasterAI
{
    /// <summary>
    /// Genera botones de elección a partir de las opciones del DM.
    /// 
    /// SETUP:
    ///   - choiceContainer: Panel donde aparecen los botones (ej. HorizontalLayoutGroup)
    ///   - choiceButtonPrefab: Prefab con Button + TextMeshProUGUI hijo
    /// </summary>
    public class ChoiceManager : MonoBehaviour
    {
        [Header("UI")]
        public Transform choiceContainer;
        public GameObject choiceButtonPrefab;

        private readonly List<GameObject> activeButtons = new List<GameObject>();

        public void ShowChoices(string[] choices)
        {
            HideChoices();
            choiceContainer.gameObject.SetActive(true);

            foreach (string choice in choices)
            {
                var btn = Instantiate(choiceButtonPrefab, choiceContainer);
                activeButtons.Add(btn);

                btn.GetComponentInChildren<TextMeshProUGUI>().text = choice;

                string captured = choice;
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    HideChoices();
                    GameEventSystem.Instance.TriggerEvent(GameEvent.Choice(captured));
                });
            }
        }

        public void HideChoices()
        {
            foreach (var btn in activeButtons)
                if (btn != null) Destroy(btn);

            activeButtons.Clear();
            choiceContainer.gameObject.SetActive(false);
        }
    }
}
