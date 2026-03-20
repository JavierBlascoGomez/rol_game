using System;

namespace DungeonMasterAI
{
    public enum EventType
    {
        Choice,
        TalkToNPC,
        ExamineObject,
        EnterArea,
        CombatAction,
        Custom
    }

    [Serializable]
    public class GameEvent
    {
        public EventType Type;
        public string Message; // Texto final que llega al DM

        // ── Constructores de conveniencia ──────────────────────────────────

        public static GameEvent Choice(string text) => new GameEvent
        {
            Type = EventType.Choice,
            Message = $"The player chooses: \"{text}\". Narrate the consequence and offer 2-3 new choices."
        };

        public static GameEvent TalkTo(string npcName, string background = "") => new GameEvent
        {
            Type = EventType.TalkToNPC,
            Message = string.IsNullOrEmpty(background)
                ? $"The player talks to {npcName}. Continue the conversation in character."
                : $"The player talks to {npcName}. [DM context, never reveal directly: {background}] Roleplay as {npcName}."
        };

        public static GameEvent Examine(string objectName, string hiddenContext = "") => new GameEvent
        {
            Type = EventType.ExamineObject,
            Message = string.IsNullOrEmpty(hiddenContext)
                ? $"The player examines {objectName}. Describe what they see."
                : $"The player examines {objectName}. [DM context: {hiddenContext}] Describe what they observe."
        };

        public static GameEvent EnterZone(string locationName) => new GameEvent
        {
            Type = EventType.EnterArea,
            Message = $"The player enters {locationName}. Describe this area vividly and present 2-3 things they can do."
        };

        public static GameEvent Combat(string enemy, string action, int roll, int damage) => new GameEvent
        {
            Type = EventType.CombatAction,
            Message = roll >= 10
                ? $"Player uses {action} on {enemy}. Hit! Roll {roll}/20, deals {damage} damage. Narrate dramatically."
                : $"Player uses {action} on {enemy}. Miss! Roll {roll}/20. Narrate the failed attempt."
        };

        public static GameEvent Custom(string message) => new GameEvent
        {
            Type = EventType.Custom,
            Message = message
        };
    }
}
