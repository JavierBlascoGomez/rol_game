using System.IO;
using UnityEngine;
using TMPro;

namespace DungeonMasterAI
{
    /// <summary>
    /// Lee SaveData_Character.json y muestra las estadísticas del personaje
    /// junto a su bonificador calculado.
    ///
    /// CORRECCIÓN: GetModifier ahora delega en PlayerStats.ScoreToModifier
    /// para garantizar que ambas clases usen exactamente la misma fórmula D&D.
    ///
    /// SETUP EN UNITY:
    ///   1. Crea un panel de UI con 7 TextMeshProUGUI:
    ///      uno para el nombre y uno por cada stat.
    ///   2. Asigna cada campo en el inspector.
    ///   3. Llama a LoadAndDisplay() cuando quieras refrescar los datos
    ///      (se llama automáticamente en Start).
    /// </summary>
    public class CharacterStatsUI : MonoBehaviour
    {
        [Header("Nombre")]
        public TextMeshProUGUI characterNameText;

        [Header("Estadísticas  (formato: 'STR   13  [+1]')")]
        public TextMeshProUGUI strengthText;
        public TextMeshProUGUI dexterityText;
        public TextMeshProUGUI constitutionText;
        public TextMeshProUGUI intelligenceText;
        public TextMeshProUGUI wisdomText;
        public TextMeshProUGUI charismaText;

        [Header("Colores de bonificador")]
        public string positiveColor = "#80FF99";
        public string neutralColor  = "#CCCCCC";
        public string negativeColor = "#FF6666";

        // ── Lifecycle ─────────────────────────────────────────────────────

        void Start() => LoadAndDisplay();

        // ── API pública ───────────────────────────────────────────────────

        public void LoadAndDisplay()
        {
            // Prioridad 1: usar PlayerStats si ya está instanciado (evita doble lectura de JSON)
            if (PlayerStats.Instance != null)
            {
                DisplayFromPlayerStats(PlayerStats.Instance);
                return;
            }

            // Prioridad 2: leer JSON directamente
            string savePath = Path.Combine(Application.persistentDataPath, "SaveData_Character.json");

            if (!File.Exists(savePath))
            {
                Debug.LogWarning($"[CharacterStatsUI] No se encontró el archivo: {savePath}");
                return;
            }

            string json = File.ReadAllText(savePath);
            CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);

            if (data == null) { Debug.LogError("[CharacterStatsUI] Error al parsear el JSON."); return; }

            Display(data);
        }

        // ── Display ───────────────────────────────────────────────────────

        private void DisplayFromPlayerStats(PlayerStats s)
        {
            if (characterNameText != null)
                characterNameText.text = s.CharacterName;

            SetStatText(strengthText,     "STR", s.Strength);
            SetStatText(dexterityText,    "DEX", s.Dexterity);
            SetStatText(constitutionText, "CON", s.Constitution);
            SetStatText(intelligenceText, "INT", s.Intelligence);
            SetStatText(wisdomText,       "WIS", s.Wisdom);
            SetStatText(charismaText,     "CHA", s.Charisma);
        }

        private void Display(CharacterSaveData data)
        {
            if (characterNameText != null)
                characterNameText.text = data.characterName;

            SetStatText(strengthText,     "STR", data.strength);
            SetStatText(dexterityText,    "DEX", data.dexterity);
            SetStatText(constitutionText, "CON", data.constitution);
            SetStatText(intelligenceText, "INT", data.intelligence);
            SetStatText(wisdomText,       "WIS", data.wisdom);
            SetStatText(charismaText,     "CHA", data.charisma);
        }

        private void SetStatText(TextMeshProUGUI label, string statName, int score)
        {
            if (label == null) return;

            // CORRECCIÓN: usa la misma fórmula que PlayerStats
            int mod = PlayerStats.ScoreToModifier(score);
            string modStr   = mod >= 0 ? $"+{mod}" : $"{mod}";
            string modColor = mod > 0 ? positiveColor : mod < 0 ? negativeColor : neutralColor;

            label.text = $"{statName}   {score}   <color={modColor}>[{modStr}]</color>";
        }

        // ── Modelo de datos (solo para lectura directa de JSON) ────────────

        [System.Serializable]
        private class CharacterSaveData
        {
            public string characterName;
            public int strength;
            public int dexterity;
            public int constitution;
            public int intelligence;
            public int wisdom;
            public int charisma;
        }
    }
}
