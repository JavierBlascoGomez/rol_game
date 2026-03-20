using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonMasterAI
{
    /// <summary>
    /// Muestra el texto del DM con efecto de streaming.
    /// 
    /// SETUP:
    ///   - narrativeText: TextMeshProUGUI dentro de un ScrollRect
    ///   - scrollRect: para hacer auto-scroll al final
    ///   - loadingIndicator: GameObject con dots o spinner (puede ser null)
    /// </summary>
    public class NarrativeUIController : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI narrativeText;
        public ScrollRect scrollRect;
        public GameObject loadingIndicator;

        private string history = "";
        private const string PlayerColor  = "#88CCFF";
        private const string DMColor      = "#F0E6D0";

        // ─── API ──────────────────────────────────────────────────────────

        /// <summary>Muestra texto parcial durante el streaming (con cursor ▌).</summary>
        public void UpdateStreaming(string partial)
        {
            narrativeText.text = history +
                $"<color={DMColor}>{partial}</color>▌";
        }

        /// <summary>Fija el texto final en el historial.</summary>
        public void Finalize(string finalText)
        {
            if (!string.IsNullOrEmpty(history)) history += "\n\n";
            history += $"<color={DMColor}>{finalText}</color>";
            narrativeText.text = history;
            StartCoroutine(ScrollToBottom());
        }

        /// <summary>Añade la acción del jugador al historial en color diferente.</summary>
        public void AppendPlayerAction(string action)
        {
            if (!string.IsNullOrEmpty(history)) history += "\n";
            history += $"<color={PlayerColor}>▶ {action}</color>\n";
            narrativeText.text = history;
        }

        public void SetLoading(bool loading)
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(loading);
        }

        // ─── Internals ────────────────────────────────────────────────────

        private IEnumerator ScrollToBottom()
        {
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
