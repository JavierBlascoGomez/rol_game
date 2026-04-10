using UnityEngine;
using LLMUnity;
using System.Threading.Tasks;

namespace DungeonMasterAI
{
    /// <summary>
    /// Controlador principal del LLM.
    /// Gestiona el system prompt (con stats del personaje), streaming y parseo de opciones.
    /// </summary>
    public class DungeonMasterController : MonoBehaviour
    {
        [Header("LLM")]
        public LLMCharacter llmCharacter;

        [Header("Referencias")]
        public NarrativeUIController narrativeUI;
        public ChoiceManager         choiceManager;

        [Header("System Prompt")]
        [TextArea(8, 16)]
        public string systemPrompt =
            "You are a Dungeon Master for a dark fantasy RPG.\n\n" +

            "NARRATIVE RULES:\n" +
            "- Always narrate in second person (\"You see...\", \"Before you...\")\n" +
            "- Keep responses under 120 words\n" +
            "- Be atmospheric and evocative\n" +
            "- React to player choices with real consequences\n\n" +

            "VARIETY RULES:\n" +
            "- Every new game must feel completely different\n" +
            "- Randomize: setting, tone, inciting incident and threat\n" +
            "- Never start two games the same way\n\n" +

            "DIFFICULTY RULES — CRITICAL:\n" +
            "- Every choice must have a hidden difficulty (DC) and a required stat\n" +
            "- Stats: STR (strength), DEX (dexterity), CON (constitution),\n" +
            "  INT (intelligence), WIS (wisdom), CHA (charisma)\n" +
            "- DC scale: 8=trivial, 12=easy, 15=medium, 18=hard, 22=very hard\n" +
            "- Scale DC to the narrative danger of each option\n\n" +

            "RESPONSE FORMAT — ALWAYS end with this exact JSON block:\n" +
            "```json\n" +
            "{\"choices\":[\n" +
            "  {\"text\":\"Option A\",\"dc\":12,\"stat\":\"STR\"},\n" +
            "  {\"text\":\"Option B\",\"dc\":15,\"stat\":\"DEX\"},\n" +
            "  {\"text\":\"Option C\",\"dc\":8,\"stat\":\"CHA\"}\n" +
            "]}\n" +
            "```\n" +
            "Keep each choice text under 8 words. Always exactly 3 choices. Valid JSON only.";

        [Header("Opening Prompt")]
        [TextArea(4, 8)]
        public string openingPrompt =
            "Start a brand new adventure. " +
            "Randomly pick a unique combination of a setting, a tone, and an inciting incident. " +
            "Do NOT start at a dungeon entrance. Begin in medias res. " +
            "80 words max, then 3 choices with their DC and stat.";

        // ── Estado interno ────────────────────────────────────────────────

        private string streamBuffer     = "";
        private bool   isWaiting        = false;
        private string lastFullResponse = "";

        // ── Lifecycle ─────────────────────────────────────────────────────

        async void Start()
        {
            if (llmCharacter == null) { Debug.LogError("[DM] LLMCharacter no asignado."); return; }

            llmCharacter.prompt = BuildSystemPrompt();

            await SendToDMAsync(BuildOpeningPrompt());
        }

        // ── API pública ───────────────────────────────────────────────────

        public void SendToDM(string message) => _ = SendToDMAsync(message);

        public async Task SendToDMAsync(string message)
        {
            if (isWaiting) return;

            isWaiting    = true;
            streamBuffer = "";

            narrativeUI.SetLoading(true);
            choiceManager.HideChoices();

            await llmCharacter.Chat(message, OnPartial, OnComplete);
        }

        // ── Callbacks LLM ─────────────────────────────────────────────────

        void OnPartial(string partial)
        {
            streamBuffer = partial;
            narrativeUI.UpdateStreaming(StripJsonBlock(streamBuffer));
        }

        void OnComplete()
        {
            lastFullResponse = streamBuffer;
            narrativeUI.Finalize(StripJsonBlock(lastFullResponse));
            narrativeUI.SetLoading(false);
            isWaiting = false;
            ParseAndShowChoices(lastFullResponse);
        }

        // ── Parseo JSON ───────────────────────────────────────────────────
        // CORRECCIÓN: el parseo ahora busca el JSON tanto dentro de bloques
        // ```json ... ``` como suelto en el texto, y usa un cierre robusto.

        void ParseAndShowChoices(string raw)
        {
            string json = ExtractJson(raw);

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[DM] JSON de opciones no encontrado en:\n" + raw);
                return;
            }

            try
            {
                DMResponseData data = JsonUtility.FromJson<DMResponseData>(json);
                if (data?.choices != null && data.choices.Length > 0)
                {
                    choiceManager.ShowChoices(data.choices);
                }
                else
                {
                    Debug.LogWarning("[DM] JSON parseado pero sin choices.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DM] Error parseando JSON: {e.Message}\n{json}");
            }
        }

        /// <summary>
        /// Extrae el primer objeto JSON que contenga "choices" del texto del LLM.
        /// Soporta bloque ```json ... ``` y JSON suelto.
        /// </summary>
        string ExtractJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            // Intento 1: bloque ```json ... ```
            int fenceStart = text.IndexOf("```json");
            if (fenceStart >= 0)
            {
                int contentStart = text.IndexOf('\n', fenceStart);
                if (contentStart >= 0)
                {
                    int fenceEnd = text.IndexOf("```", contentStart);
                    if (fenceEnd > contentStart)
                    {
                        string candidate = text.Substring(contentStart, fenceEnd - contentStart).Trim();
                        if (candidate.Contains("\"choices\"")) return candidate;
                    }
                }
            }

            // Intento 2: JSON suelto — busca { ... } que contenga "choices"
            int start = text.IndexOf("{");
            while (start >= 0)
            {
                int depth = 0;
                int end   = -1;
                for (int i = start; i < text.Length; i++)
                {
                    if (text[i] == '{') depth++;
                    else if (text[i] == '}') { depth--; if (depth == 0) { end = i; break; } }
                }

                if (end > start)
                {
                    string candidate = text.Substring(start, end - start + 1);
                    if (candidate.Contains("\"choices\"")) return candidate;
                }

                start = text.IndexOf("{", start + 1);
            }

            return null;
        }

        /// <summary>
        /// Elimina el bloque JSON del texto para mostrarlo limpio en la UI.
        /// </summary>
        string StripJsonBlock(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Elimina bloque ```json ... ```
            int fence = text.IndexOf("```json");
            if (fence >= 0)
            {
                int end = text.IndexOf("```", fence + 3);
                return (end >= 0 ? text.Substring(0, fence) : text.Substring(0, fence)).TrimEnd();
            }

            // Elimina JSON suelto desde {"choices"
            int jsonStart = text.IndexOf("{\"choices\"");
            if (jsonStart >= 0) return text.Substring(0, jsonStart).TrimEnd();

            return text;
        }

        // ── System prompt con stats del personaje ─────────────────────────

        string BuildSystemPrompt()
        {
            string charBlock = "";

            if (PlayerStats.Instance != null)
            {
                var s = PlayerStats.Instance;
                charBlock =
                    $"\nPLAYER CHARACTER: {s.CharacterName}\n" +
                    $"Stats — STR:{s.Strength}({Mod(s.Strength)}) " +
                    $"DEX:{s.Dexterity}({Mod(s.Dexterity)}) " +
                    $"CON:{s.Constitution}({Mod(s.Constitution)}) " +
                    $"INT:{s.Intelligence}({Mod(s.Intelligence)}) " +
                    $"WIS:{s.Wisdom}({Mod(s.Wisdom)}) " +
                    $"CHA:{s.Charisma}({Mod(s.Charisma)})\n" +
                    $"HP: {s.currentHP}/{s.maxHP}\n" +
                    $"Use these stats to calibrate DCs: high stats should make relevant checks easier.\n";
            }

            return systemPrompt + charBlock;
        }

        string Mod(int score)
        {
            int m = PlayerStats.ScoreToModifier(score);
            return m >= 0 ? $"+{m}" : $"{m}";
        }

        // ── Opening aleatorio ─────────────────────────────────────────────

        string BuildOpeningPrompt()
        {
            string[] settings = {
                "a sinking merchant ship", "a burning village at dawn",
                "a collapsing mine", "a royal court mid-assassination",
                "a cursed forest in a storm", "an underground black market",
                "a besieged monastery", "a plague-ridden city under quarantine"
            };
            string[] tones = {
                "grim and hopeless", "tense and mysterious", "darkly comedic",
                "epic and grandiose", "intimate and personal"
            };

            string setting = settings[Random.Range(0, settings.Length)];
            string tone    = tones[Random.Range(0, tones.Length)];

            string charName = PlayerStats.Instance != null
                ? PlayerStats.Instance.CharacterName : "the adventurer";

            return $"The player's character is {charName}. " +
                   $"Start a new adventure set in {setting}. Tone: {tone}. " +
                   $"Begin in medias res, something is already happening. " +
                   $"80 words max, then 3 choices with DC and stat.";
        }
    }

    // ── Modelos de deserialización ────────────────────────────────────────

    [System.Serializable]
    public class DMResponseData { public ChoiceData[] choices; }

    [System.Serializable]
    public class ChoiceData
    {
        public string text;
        public int    dc;
        public string stat;
    }
}
