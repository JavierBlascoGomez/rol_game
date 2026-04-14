using UnityEngine;
 
namespace DungeonMasterAI
{
    public enum EventType { Choice, TalkTo, EnterZone, Custom }
 
    [System.Serializable]
    public class GameEvent
    {
        public EventType Type;
        public string    Message;
 
        public static GameEvent Choice(string choiceText) => new GameEvent
        {
            Type    = EventType.Choice,
            Message = $"The player chooses: {choiceText}"
        };
 
        /// <summary>
        /// Resultado de tirada con todo el contexto para que el DM narre.
        /// </summary>
        public static GameEvent CheckResult(string choiceText, string stat, int dc,
                                            int roll, int modifier, bool success,
                                            int damage = 0)
        {
            string modStr  = modifier >= 0 ? $"+{modifier}" : $"{modifier}";
            string total   = $"{roll}{modStr} = {roll + modifier}";
 
            string msg = success
                ? $"The player attempts: \"{choiceText}\". " +
                  $"{stat} check DC {dc}: rolled {total} — SUCCESS. " +
                  $"Narrate how they succeed vividly. Then present 3 new choices in the JSON block."
                : $"The player attempts: \"{choiceText}\". " +
                  $"{stat} check DC {dc}: rolled {total} — FAILURE. " +
                  $"The player takes {damage} damage. " +
                  $"Narrate the painful failure and its consequences on the character. " +
                  $"Then present 3 new choices in the JSON block.";
 
            return new GameEvent { Type = EventType.Choice, Message = msg };
        }
 
        public static GameEvent TalkTo(string npcName) => new GameEvent
        {
            Type    = EventType.TalkTo,
            Message = $"The player approaches {npcName} and initiates conversation."
        };
 
        public static GameEvent EnterZone(string zoneName) => new GameEvent
        {
            Type    = EventType.EnterZone,
            Message = $"The player enters {zoneName}. Describe the area and what stands out."
        };
 
        public static GameEvent Custom(string message) => new GameEvent
        {
            Type    = EventType.Custom,
            Message = message
        };
    }
}
