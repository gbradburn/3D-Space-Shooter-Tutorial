using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.Events
{
    public struct PlayerSpawnedEvent
    {
        public GameObject Player { get; }
        public int PlayerIndex { get; }
        public bool IsLocalPlayer { get; }
        
        public PlayerSpawnedEvent(GameObject player, int playerIndex, bool isLocalPlayer)
        {
            Player = player;
            PlayerIndex = playerIndex;
            IsLocalPlayer = isLocalPlayer;
        }
    }

    public struct PlayerDestroyedEvent
    {
        public GameObject Player { get; }
        public int PlayerIndex { get; }
        public Vector3 DeathPosition { get; }
        public GameObject Explosion { get; }
        
        public PlayerDestroyedEvent(GameObject player, int playerIndex, Vector3 deathPosition, GameObject explosion = null)
        {
            Player = player;
            PlayerIndex = playerIndex;
            DeathPosition = deathPosition;
            Explosion = explosion;
        }
    }
}
