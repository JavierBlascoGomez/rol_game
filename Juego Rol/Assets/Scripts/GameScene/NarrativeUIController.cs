using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace DungeonMasterAI
{
    /// <summary>
    /// Gestiona el texto narrativo y el HP como texto.
    /// El texto se ancla abajo (pivot Y=0) para efecto de ventana deslizante.
    /// </summary>
    
    public class NarrativeUIController : MonoBehaviour
    {
        [Header("Narrativa")]
        public TextMeshProUGUI narrativeText;
        public GameObject      loadingIndicator;

        [Header("HP UI")]
        public TextMeshProUGUI hpText;

        [Header("Colores HP")]
        public Color colorHigh = new Color(0.2f, 0.9f, 0.3f);
        public Color colorMid  = new Color(1f,   0.8f, 0.1f);
        public Color colorLow  = new Color(1f,   0.2f, 0.1f);

        private string currentText = "";

        // ── Lifecycle ─────────────────────────────────────────────────────

        void Start()
        {
            if (loadingIndicator != null) loadingIndicator.SetActive(false);

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnHPChanged   += UpdateHP;
                PlayerStats.Instance.OnPlayerDeath += OnDeath;
                UpdateHP(PlayerStats.Instance.currentHP, PlayerStats.Instance.maxHP);
            }
        }

        void OnDestroy()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnHPChanged   -= UpdateHP;
                PlayerStats.Instance.OnPlayerDeath -= OnDeath;
            }
        }

        // ── Narrativa ─────────────────────────────────────────────────────

        public void UpdateStreaming(string fullTextSoFar)
        {
            currentText        = fullTextSoFar;
            narrativeText.text = currentText;
        }

        public void Finalize(string fullText)
        {
            currentText        = fullText + "\n\n";
            narrativeText.text = currentText;
            StartCoroutine(ScrollNextFrame());
        }

        public void SetLoading(bool active)
        {
            if (loadingIndicator != null) loadingIndicator.SetActive(active);
        }

        IEnumerator ScrollNextFrame()
        {
            yield return new WaitForEndOfFrame();
            // Si usas ScrollRect asigna scrollRect.verticalNormalizedPosition = 0f aquí
        }

        // ── HP ────────────────────────────────────────────────────────────

        void UpdateHP(int current, int max)
        {
            if (hpText != null)
            {
                hpText.text = $"{current} / {max}";

                float ratio = max > 0 ? (float)current / max : 0f;
                hpText.color = ratio > 0.5f ? colorHigh : ratio > 0.25f ? colorMid : colorLow;
            }
        }

        void OnDeath()
        {
            if (narrativeText != null)
                narrativeText.text += "\n\n<color=#FF0000><b>Has caído...</b></color>";
        }
    }
}