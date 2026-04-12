using UnityEngine;
using LLMUnity;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DungeonMasterAI
{
    public class DungeonMasterController : MonoBehaviour
    {
        [Header("LLM")]
        public LLMCharacter llmCharacter;

        [Header("Referencias")]
        public NarrativeUIController narrativeUI;
        public ChoiceManager         choiceManager;

        [Header("System Prompt")]
        [TextArea(8, 20)]
        public string systemPrompt = "You are a Dungeon Master for a dark fantasy RPG.";

        [Header("Opening Prompt")]
        [TextArea(4, 8)]
        public string openingPrompt = "Start a new adventure. 60 words max. Then the JSON choices block.";

        [Header("Debug — ver respuesta completa en consola")]
        public bool debugLog = true;

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
            // Durante el streaming solo mostramos la parte narrativa,
            // pero NO cortamos en {"choices" porque el JSON puede estar incompleto
            narrativeUI.UpdateStreaming(ExtractNarrative(streamBuffer));
        }

        void OnComplete()
        {
            lastFullResponse = streamBuffer;

            if (debugLog)
                Debug.Log($"[DM] === RESPUESTA COMPLETA ===\n{lastFullResponse}\n=== FIN ===");

            // Ahora sí tenemos la respuesta entera: mostramos narrativa limpia
            narrativeUI.Finalize(ExtractNarrative(lastFullResponse));
            narrativeUI.SetLoading(false);
            isWaiting = false;

            // Y parseamos el JSON completo
            ParseAndShowChoices(lastFullResponse);
        }

        // ── Extrae solo el texto narrativo (sin JSON) ─────────────────────
        // Busca el primer { que forme parte del JSON de choices.
        // Durante el streaming puede que aún no haya llegado, y eso está bien.

        string ExtractNarrative(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Busca el inicio del JSON de choices
            int jsonIdx = FindJsonStart(text);
            if (jsonIdx > 0)
                return text.Substring(0, jsonIdx).TrimEnd();

            // Si no hay JSON todavía, devuelve todo el texto
            return text.TrimEnd();
        }

        // ── Busca el inicio del JSON de choices ───────────────────────────

        int FindJsonStart(string text)
        {
            // Busca {"choices" con posibles espacios
            var m = Regex.Match(text, @"\{""choices""");
            if (m.Success) return m.Index;

            // También busca el bloque ```json por si el modelo lo genera igual
            int fence = text.IndexOf("```json");
            if (fence >= 0) return fence;

            return -1;
        }

        // ── Parseo JSON — 2 intentos en cascada ───────────────────────────

        void ParseAndShowChoices(string raw)
        {
            if (debugLog)
                Debug.Log("[DM] Intentando parsear choices...");

            // Intento 1: JSON completo con "choices"
            string json = TryExtractChoicesJson(raw);
            if (json != null)
            {
                if (TryDeserializeAndShow(json)) return;
            }

            // Intento 2: regex campo a campo (tolera JSON malformado)
            ChoiceData[] choices = TryParseWithRegex(raw);
            if (choices != null && choices.Length >= 3)
            {
                Debug.Log("[DM] Choices recuperados por regex.");
                choiceManager.ShowChoices(choices);
                return;
            }

            Debug.LogWarning("[DM] No se encontraron choices. Revisa el log de respuesta completa.");
        }

        string TryExtractChoicesJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            // Elimina posibles backticks
            text = Regex.Replace(text, @"```json\s*", "");
            text = Regex.Replace(text, @"```\s*",     "");

            // Busca el { más cercano a "choices"
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '{') continue;

                // Comprueba que cerca haya "choices"
                int lookahead = Mathf.Min(i + 50, text.Length);
                if (!text.Substring(i, lookahead - i).Contains("\"choices\"")) continue;

                // Extrae el objeto completo contando llaves
                int depth = 0;
                for (int j = i; j < text.Length; j++)
                {
                    if (text[j] == '{') depth++;
                    else if (text[j] == '}')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            string candidate = text.Substring(i, j - i + 1);
                            if (debugLog) Debug.Log("[DM] JSON candidato:\n" + candidate);
                            return candidate;
                        }
                    }
                }
            }
            return null;
        }

        bool TryDeserializeAndShow(string json)
        {
            try
            {
                DMResponseData data = JsonUtility.FromJson<DMResponseData>(json);
                if (data?.choices != null && data.choices.Length > 0)
                {
                    Debug.Log($"[DM] {data.choices.Length} choices parseados OK.");
                    choiceManager.ShowChoices(data.choices);
                    return true;
                }
                Debug.LogWarning("[DM] JSON OK pero choices vacío.");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DM] Error deserializando: {e.Message}");
            }
            return false;
        }

        // ── Regex fallback: extrae choices individuales ───────────────────

        ChoiceData[] TryParseWithRegex(string text)
        {
            var list = new System.Collections.Generic.List<ChoiceData>();
            var matches = Regex.Matches(text,
                @"\{[^{}]*""text""\s*:\s*""([^""]+)""\s*,\s*""dc""\s*:\s*(\d+)\s*,\s*""stat""\s*:\s*""([^""]+)""[^{}]*\}|" +
                @"\{[^{}]*""stat""\s*:\s*""([^""]+)""\s*,\s*""dc""\s*:\s*(\d+)\s*,\s*""text""\s*:\s*""([^""]+)""[^{}]*\}");

            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                string txt, stat;
                int dc;

                if (!string.IsNullOrEmpty(m.Groups[1].Value))
                {
                    txt  = m.Groups[1].Value;
                    dc   = int.Parse(m.Groups[2].Value);
                    stat = m.Groups[3].Value.ToUpper();
                }
                else
                {
                    stat = m.Groups[4].Value.ToUpper();
                    dc   = int.Parse(m.Groups[5].Value);
                    txt  = m.Groups[6].Value;
                }

                string[] valid = {"STR","DEX","CON","INT","WIS","CHA"};
                if (!System.Array.Exists(valid, s => s == stat)) continue;

                list.Add(new ChoiceData { text = txt, dc = dc, stat = stat });
                if (list.Count == 3) break;
            }

            return list.Count > 0 ? list.ToArray() : null;
        }

        // ── System prompt con stats ───────────────────────────────────────

        string BuildSystemPrompt()
        {
            string charBlock = "";
            if (PlayerStats.Instance != null)
            {
                var s = PlayerStats.Instance;
                charBlock =
                    $"\nPLAYER CHARACTER: {s.CharacterName}\n" +
                    $"STR:{s.Strength}({Mod(s.Strength)}) DEX:{s.Dexterity}({Mod(s.Dexterity)}) " +
                    $"CON:{s.Constitution}({Mod(s.Constitution)}) INT:{s.Intelligence}({Mod(s.Intelligence)}) " +
                    $"WIS:{s.Wisdom}({Mod(s.Wisdom)}) CHA:{s.Charisma}({Mod(s.Charisma)})\n" +
                    $"HP: {s.currentHP}/{s.maxHP}\n";
            }
            return systemPrompt + charBlock;
        }

        string Mod(int score)
        {
            int m = PlayerStats.ScoreToModifier(score);
            return m >= 0 ? $"+{m}" : $"{m}";
        }

        string BuildOpeningPrompt()
        {
            string[] settings = {
                "a sinking merchant ship", "a burning village at dawn",
                "a collapsing mine",       "a royal court mid-assassination",
                "a cursed forest",         "an underground black market",
                "a besieged monastery",    "a plague-ridden city"
            };
            string[] tones = { "grim", "mysterious", "darkly comedic", "epic", "intimate" };

            string setting  = settings[Random.Range(0, settings.Length)];
            string tone     = tones  [Random.Range(0, tones.Length)];
            string charName = PlayerStats.Instance?.CharacterName ?? "the adventurer";

            return $"Character: {charName}. Setting: {setting}. Tone: {tone}. " +
                   $"60 words narration, then the JSON choices block.";
        }
    }

    [System.Serializable]
    public class DMResponseData { public ChoiceData[] choices; }

    [System.Serializable]
    public class ChoiceData { public string text; public int dc; public string stat; }
}
