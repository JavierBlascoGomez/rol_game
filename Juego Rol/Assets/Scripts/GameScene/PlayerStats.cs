using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonMasterAI
{
    /// <summary>
    /// Lee SaveData_Character.json (creado en la escena de creación de personaje)
    /// y expone los modificadores D&D al resto del sistema.
    /// Se autodestruye si ya existe una instancia (DontDestroyOnLoad).
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("Escena de Game Over")]
        public string gameOverSceneName = "GameOver";

        [Header("HP")]
        public int maxHP     = 20;
        public int currentHP = 20;

        // Datos cargados del JSON
        public string CharacterName   { get; private set; } = "Aventurero";
        public int    Strength        { get; private set; } = 10;
        public int    Dexterity       { get; private set; } = 10;
        public int    Constitution    { get; private set; } = 10;
        public int    Intelligence    { get; private set; } = 10;
        public int    Wisdom          { get; private set; } = 10;
        public int    Charisma        { get; private set; } = 10;

        public System.Action<int, int> OnHPChanged;
        public System.Action           OnPlayerDeath;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCharacterData();
            currentHP = maxHP;
        }

        void LoadCharacterData()
        {
            string path = Path.Combine(Application.persistentDataPath, "SaveData_Character.json");

            if (!File.Exists(path))
            {
                Debug.LogWarning("[PlayerStats] SaveData_Character.json no encontrado. Usando valores por defecto.");
                return;
            }

            string json = File.ReadAllText(path);
            CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);

            if (data == null) { Debug.LogError("[PlayerStats] Error al parsear el JSON."); return; }

            CharacterName = data.characterName;
            Strength      = data.strength;
            Dexterity     = data.dexterity;
            Constitution  = data.constitution;
            Intelligence  = data.intelligence;
            Wisdom        = data.wisdom;
            Charisma      = data.charisma;

            Debug.Log($"[PlayerStats] Cargado: {CharacterName} STR{Strength} DEX{Dexterity} CON{Constitution} INT{Intelligence} WIS{Wisdom} CHA{Charisma}");
        }

        public int GetModifier(string statKey)
        {
            int score = statKey?.ToUpper() switch
            {
                "STR" => Strength,
                "DEX" => Dexterity,
                "CON" => Constitution,
                "INT" => Intelligence,
                "WIS" => Wisdom,
                "CHA" => Charisma,
                _     => 10
            };
            return ScoreToModifier(score);
        }

        public string GetStatName(string statKey)
        {
            return statKey?.ToUpper() switch
            {
                "STR" => "Fuerza",
                "DEX" => "Destreza",
                "CON" => "Constitución",
                "INT" => "Inteligencia",
                "WIS" => "Sabiduría",
                "CHA" => "Carisma",
                _     => statKey ?? "???"
            };
        }

        public void TakeDamage(int amount)
        {
            currentHP = Mathf.Max(0, currentHP - amount);
            OnHPChanged?.Invoke(currentHP, maxHP);

            if (currentHP <= 0)
            {
                OnPlayerDeath?.Invoke();
                Invoke(nameof(LoadGameOver), 3f);
            }
        }

        // Fórmula D&D estándar — idéntica a CharacterStatsUI.GetModifier
        public static int ScoreToModifier(int score)
        {
            if (score <= 1)  return -5;
            if (score <= 3)  return -4;
            if (score <= 5)  return -3;
            if (score <= 7)  return -2;
            if (score <= 9)  return -1;
            if (score <= 11) return  0;
            if (score <= 13) return  1;
            if (score <= 15) return  2;
            if (score <= 17) return  3;
            if (score <= 19) return  4;
            return                   5;
        }

        [System.Serializable]
        private class CharacterSaveData
        {
            public string characterName;
            public int    strength;
            public int    dexterity;
            public int    constitution;
            public int    intelligence;
            public int    wisdom;
            public int    charisma;
        }

        void LoadGameOver() => SceneManager.LoadScene(gameOverSceneName);
    }
}
