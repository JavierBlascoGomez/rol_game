using System.IO;
using UnityEngine;
using TMPro;

namespace DungeonMasterAI
{
    /// <summary>
    /// Lee SaveData_Character.json y muestra las estadísticas del personaje
    /// junto a su bonificador calculado.
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

        // ─── Lifecycle ─────────────────────────────────────────────────────

        void Start() => LoadAndDisplay();

        // ─── API pública ───────────────────────────────────────────────────

        public void LoadAndDisplay()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "SaveData_Character.json");

            if (!File.Exists(savePath))
            {
                Debug.LogWarning($"[CharacterStatsUI] No se encontró el archivo: {savePath}");
                return;
            }

            string json = File.ReadAllText(savePath);
            CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);

            if (data == null)
            {
                Debug.LogError("[CharacterStatsUI] Error al parsear el JSON.");
                return;
            }

            Display(data);
        }

        // ─── Display ───────────────────────────────────────────────────────

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

            int mod = GetModifier(score);
            string modStr   = mod >= 0 ? $"+{mod}" : $"{mod}";
            string modColor = mod > 0 ? positiveColor : mod < 0 ? negativeColor : neutralColor;

            label.text = $"{statName}   {score}   <color={modColor}>[{modStr}]</color>";
        }

        // ─── Cálculo de bonificador ────────────────────────────────────────

        public static int GetModifier(int score)
        {
            if (score == 0)              return -5;
            if (score <= 2)              return -4;
            if (score <= 4)              return -3;
            if (score <= 6)              return -2;
            if (score <= 8)             return -1;
            if (score <= 11)             return  0;
            if (score <= 13)             return  1;
            if (score <= 15)             return  2;
            if (score <= 17)             return  3;
            if (score <= 19)             return  4;
            return                               5;  // 20
        }

        // ─── Modelo de datos ───────────────────────────────────────────────

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
