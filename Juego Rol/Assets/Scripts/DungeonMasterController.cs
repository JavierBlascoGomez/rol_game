using System.Threading.Tasks;
using UnityEngine;
using LLMUnity;

namespace DungeonMasterAI
{
    /// <summary>
    /// Controlador principal del Dungeon Master.
    /// 
    /// SETUP EN UNITY:
    ///   1. Crea un GameObject "DungeonMaster".
    ///   2. Añade: LLM, LLMCharacter, DungeonMasterController, GameEventSystem.
    ///   3. En LLMCharacter asigna el LLM y deja que apunte a este controlador.
    ///   4. Asigna ChoiceManager y NarrativeUIController en el inspector.
    /// </summary>
    public class DungeonMasterController : MonoBehaviour
    {
        [Header("LLM for Unity")]
        public LLMCharacter llmCharacter;

        [Header("UI")]
        public ChoiceManager choiceManager;
        public NarrativeUIController narrativeUI;

        [Header("System Prompt")]
        [TextArea(6, 15)]
        public string systemPrompt =
            "You are a Dungeon Master for a dark fantasy RPG.\n" +
            "RULES:\n" +
            "- Always narrate in second person (\"You see...\", \"Before you...\")\n" +
            "- Keep responses under 120 words\n" +
            "- Be atmospheric and evocative\n" +
            "- React to player choices with real consequences\n" +
            "- [DM context] tags are secret info for you only, never reveal them directly\n\n" +
            "After each narration, always end with a JSON block:\n" +
            "```json\n{\"choices\":[\"Option A\",\"Option B\",\"Option C\"]}\n```\n" +
            "Keep each choice under 8 words. Always provide exactly 3 choices.";

        [Header("Apertura")]
        [TextArea(2, 4)]
        public string openingPrompt =
            "The adventure begins. Describe the dungeon entrance at night. " +
            "Set mood and tension, then present 3 initial choices.";

        private bool isWaiting = false;
        private string streamBuffer = "";

        // ─── Lifecycle ─────────────────────────────────────────────────────

        async void Start()
        {
            llmCharacter.SetPrompt(systemPrompt);
            await SendToDMAsync(openingPrompt);
        }

        // ─── API pública ───────────────────────────────────────────────────

        public async void SendToDM(string message)
        {
            if (isWaiting) return;
            await SendToDMAsync(message);
        }

        // ─── Internals ─────────────────────────────────────────────────────

        private async Task SendToDMAsync(string message)
        {
            isWaiting = true;
            streamBuffer = "";
            narrativeUI.SetLoading(true);
            choiceManager.HideChoices();

            await llmCharacter.Chat(message, OnPartial, OnComplete);
        }

        private void OnPartial(string partial)
        {
            streamBuffer += partial;
            narrativeUI.UpdateStreaming(StripJson(streamBuffer));
        }

        private void OnComplete()
        {
            var (narrative, choices) = Parse(streamBuffer);
            narrativeUI.Finalize(narrative);

            if (choices != null && choices.Length > 0)
                choiceManager.ShowChoices(choices);

            isWaiting = false;
            streamBuffer = "";
            narrativeUI.SetLoading(false);
        }

        // ─── Parseo ────────────────────────────────────────────────────────

        private (string narrative, string[] choices) Parse(string raw)
        {
            string narrative = StripJson(raw).Trim();
            string json = ExtractJson(raw);

            if (string.IsNullOrEmpty(json)) return (narrative, null);

            try
            {
                json = json.Replace("```json", "").Replace("```", "").Trim();
                var data = JsonUtility.FromJson<ChoicesPayload>(json);
                return (narrative, data?.choices);
            }
            catch
            {
                return (narrative, null);
            }
        }

        private string ExtractJson(string text)
        {
            int start = text.IndexOf("```json");
            if (start < 0) start = text.IndexOf("{\"choices\"");
            if (start < 0) return null;

            int end = text.LastIndexOf("```");
            if (end <= start) end = text.LastIndexOf("}");
            if (end < 0) return null;

            return text.Substring(start, end - start + (text[start] == '{' ? 1 : 3));
        }

        private string StripJson(string text)
        {
            int start = text.IndexOf("```json");
            if (start < 0) start = text.IndexOf("{\"choices\"");
            return start < 0 ? text : text.Substring(0, start).TrimEnd();
        }

        [System.Serializable]
        private class ChoicesPayload { public string[] choices; }
    }
}
