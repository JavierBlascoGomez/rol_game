using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement; // ¡IMPORTANTE! Necesario para manejar archivos en el disco

// 1. Creamos una clase "contenedor" solo para los datos que queremos guardar.
// [System.Serializable] es obligatorio para que Unity pueda convertirlo a JSON.
[System.Serializable]
public class CharacterData
{
    public string characterName;
    public int strength;
    public int dexterity;     
    public int constitution;  
    public int intelligence;  
    public int wisdom;        
    public int charisma;
}

public class StatsSetUp : MonoBehaviour
{
    [Header("Character Name")]
    public string characterName;

    [Header("Stats")]
    public int strength;
    public int dexterity;     
    public int constitution;  
    public int intelligence;  
    public int wisdom;        
    public int charisma;

    [Header("Managers")]
    [Tooltip("Arrastra aquí el objeto que tiene el CharacterCreationSceneManager")]
    public CharacterCreationSceneManager sceneManager;

    private int statsRolledCount = 0;

    [Tooltip("Arrastra aquí el Input Field del Canvas 3")]
    public TMP_InputField nameInputField;

    public void AssignStat(string statName, int value)
    {
        string nameLower = statName.ToLower().Trim();

        switch (nameLower)
        {
            case "fuerza": case "strength": strength = value; break;
            case "destreza": case "dexterity": dexterity = value; break;
            case "constitucion": case "constitution": case "constitución": constitution = value; break;
            case "inteligencia": case "intelligence": intelligence = value; break;
            case "sabiduria": case "sabiduría": case "wisdom": wisdom = value; break;
            case "carisma": case "charisma": charisma = value; break;
            default:
                Debug.LogWarning("Nombre de estadística no reconocido: " + statName);
                return; 
        }

        statsRolledCount++;
        Debug.Log($"Asignado {statName} con valor {value}. Total tiradas: {statsRolledCount}/6");

        if (statsRolledCount >= 6)
        {
            Debug.Log("Estadisticas generadas");
            sceneManager.NextStep();
        }

    }

    public void ConfirmCharacterName()
    {
        if (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
        {
            characterName = nameInputField.text;
            Debug.Log("¡Personaje creado! Nombre: " + characterName);

            SaveCharacterToJSON();

            SceneManager.LoadScene("GameScene"); 
        }
        else
        {
            Debug.LogWarning("Nombre vacío.");
        }
    }

    private void SaveCharacterToJSON()
    {
        CharacterData data = new CharacterData();
        data.characterName = characterName;
        data.strength = strength;
        data.dexterity = dexterity;
        data.constitution = constitution;
        data.intelligence = intelligence;
        data.wisdom = wisdom;
        data.charisma = charisma;

        string jsonText = JsonUtility.ToJson(data, true);
        string savePath = Path.Combine(Application.persistentDataPath, "SaveData_Character.json");
        File.WriteAllText(savePath, jsonText);

        try 
        {
            File.WriteAllText(savePath, jsonText);
            
            Debug.Log("<color=green><b>SISTEMA:</b></color> Personaje guardado con éxito.");
            Debug.Log("<color=cyan>Ruta:</color> " + savePath);
            Application.OpenURL(Application.persistentDataPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al guardar el JSON: " + e.Message);
        }
    }
}
