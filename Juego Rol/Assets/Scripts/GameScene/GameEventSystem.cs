using UnityEngine;
 
namespace DungeonMasterAI
{
    /// <summary>
    /// Puente central: recibe GameEvents desde cualquier script y los envía al DM.
    /// </summary>
    public class GameEventSystem : MonoBehaviour
    {
        public static GameEventSystem Instance { get; private set; }
 
        [SerializeField] private DungeonMasterController dm;
 
        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }
 
        public void TriggerEvent(GameEvent gameEvent)
        {
            if (dm == null) { Debug.LogError("[GameEventSystem] DungeonMasterController no asignado."); return; }
            dm.SendToDM(gameEvent.Message);
        }
    }
}
