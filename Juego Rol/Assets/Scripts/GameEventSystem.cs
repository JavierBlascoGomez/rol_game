using UnityEngine;

namespace DungeonMasterAI
{
    /// <summary>
    /// Punto de entrada único para todas las interacciones del juego.
    /// Cualquier script llama: GameEventSystem.Instance.TriggerEvent(...)
    /// </summary>
    public class GameEventSystem : MonoBehaviour
    {
        public static GameEventSystem Instance { get; private set; }

        [SerializeField] private DungeonMasterController dungeonMaster;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void TriggerEvent(GameEvent gameEvent)
        {
            dungeonMaster.SendToDM(gameEvent.Message);
        }
    }
}
