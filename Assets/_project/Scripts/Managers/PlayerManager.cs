using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.SpaceShooter.Events;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    [Header("Player Prefabs")]
    [SerializeField] GameObject _playerShipPrefab;
    
    [Header("Spawn Configuration")]
    [SerializeField] List<PlayerSpawnPoint> _spawnPoints = new();
    [SerializeField] float _spawnProtectionDuration = 3f;
    [SerializeField] bool _autoSpawnLocalPlayer = true;
    
    readonly List<GameObject> _activePlayers = new();
    readonly Dictionary<GameObject, int> _playerIndexMap = new();
    readonly Dictionary<GameObject, bool> _playerLocalMap = new();
    
    public int ActivePlayerCount => _activePlayers.Count;
    public List<GameObject> ActivePlayers => new(_activePlayers);
    
    bool _hasInitialized;
    
    protected override void OnRuntimeInitialize()
    {
        base.OnRuntimeInitialize();
        Initialize();
    }
    
    protected override void Start()
    {
        base.Start();
        if (!_hasInitialized)
        {
            Initialize();
        }
    }
    
    void Initialize()
    {
        if (_hasInitialized) return;
        _hasInitialized = true;
        
        AutoDiscoverSpawnPoints();
        
        if (_autoSpawnLocalPlayer)
        {
            SpawnLocalPlayer();
        }
    }
    
    void AutoDiscoverSpawnPoints()
    {
        if (_spawnPoints.Count != 0) return;
        _spawnPoints = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None)
            .OrderBy(sp => sp.SpawnIndex)
            .ToList();
            
        if (_enableDebugLog) Debug.Log($"PlayerManager: Auto-discovered {_spawnPoints.Count} spawn points", this);
    }
    
    public GameObject SpawnLocalPlayer() => SpawnPlayer(0, isLocalPlayer: true);
    
    public GameObject SpawnPlayer(int playerIndex, bool isLocalPlayer = false)
    {
        var spawnPoint = GetAvailableSpawnPoint(playerIndex);
        
        if (!spawnPoint)
        {
            Debug.LogError("PlayerManager: No available spawn points!", this);
            return null;
        }
        
        if (!_playerShipPrefab)
        {
            Debug.LogError("PlayerManager: Player Ship Prefab not assigned!", this);
            return null;
        }
        
        var player = Instantiate(_playerShipPrefab, spawnPoint.Position, spawnPoint.Rotation);
        player.name = $"Player_{playerIndex}";
        
        spawnPoint.SetOccupied(true);
        
        _activePlayers.Add(player);
        _playerIndexMap[player] = playerIndex;
        _playerLocalMap[player] = isLocalPlayer;
        
        OnPlayerSpawned(player, playerIndex, isLocalPlayer);
        
        if (_enableDebugLog) 
            Debug.Log($"PlayerManager: Spawned {(isLocalPlayer ? "local" : "remote")} player {playerIndex}", this);
        
        return player;
    }
    
    PlayerSpawnPoint GetAvailableSpawnPoint(int preferredIndex)
    {
        if (_spawnPoints.Count == 0)
        {
            Debug.LogError("PlayerManager: No spawn points configured!", this);
            return null;
        }
        
        if (preferredIndex >= 0 && preferredIndex < _spawnPoints.Count)
        {
            if (!_spawnPoints[preferredIndex].IsOccupied)
                return _spawnPoints[preferredIndex];
        }
        
        foreach (var spawnPoint in _spawnPoints)
        {
            if (!spawnPoint.IsOccupied)
                return spawnPoint;
        }
        
        Debug.LogWarning("PlayerManager: All spawn points occupied, reusing spawn point 0", this);
        return _spawnPoints[0];
    }
    
    void OnPlayerSpawned(GameObject player, int playerIndex, bool isLocalPlayer)
    {
        var shipController = player.GetComponent<ShipController>();
        if (shipController)
        {
            shipController.SetIsLocalPlayer(isLocalPlayer);
        }

        EventBus.Instance.Raise(new PlayerSpawnedEvent(player, playerIndex, isLocalPlayer));
        
        if (isLocalPlayer)
        {
            var cameraManager = FindFirstObjectByType<CameraManager>();
            if (cameraManager != null)
            {
                SetCameraFollowTarget(player.transform);
            }
        }
        
        var damageHandler = player.GetComponent<DamageHandler>();
        if (damageHandler != null)
        {
            damageHandler.ObjectDestroyed.AddListener(() => OnPlayerDestroyed(player));
        }
    }
    
    void SetCameraFollowTarget(Transform playerTransform)
    {
        var followCam = GameObject.FindGameObjectWithTag("FollowCamera");
        if (followCam)
        {
            var virtualCam = followCam.GetComponent<Cinemachine.CinemachineVirtualCamera>();
            if (virtualCam)
            {
                virtualCam.Follow = playerTransform;
                virtualCam.LookAt = playerTransform;
            }
        }
        
        var cockpitCam = GameObject.FindGameObjectWithTag("CockpitCamera");
        if (cockpitCam)
        {
            cockpitCam.transform.SetParent(playerTransform, false);
        }
    }
    
    void OnPlayerDestroyed(GameObject player)
    {
        if (!_activePlayers.Contains(player))
            return;
        
        var playerIndex = _playerIndexMap.ContainsKey(player) ? _playerIndexMap[player] : -1;
        var wasLocal = _playerLocalMap.ContainsKey(player) && _playerLocalMap[player];
        
        EventBus.Instance.Raise(new PlayerDestroyedEvent(player, playerIndex));
        
        _activePlayers.Remove(player);
        _playerIndexMap.Remove(player);
        _playerLocalMap.Remove(player);
        
        FreeSpawnPointNearPosition(player.transform.position);
        
        if (_enableDebugLog) 
            Debug.Log($"PlayerManager: Player {playerIndex} destroyed. Remaining: {_activePlayers.Count}", this);
    }
    
    void FreeSpawnPointNearPosition(Vector3 position)
    {
        foreach (var spawnPoint in _spawnPoints)
        {
            if (Vector3.Distance(spawnPoint.Position, position) < 50f)
            {
                spawnPoint.SetOccupied(false);
                break;
            }
        }
    }
    
    public GameObject GetLocalPlayer()
    {
        foreach (var kvp in _playerLocalMap)
        {
            if (kvp.Value)
                return kvp.Key;
        }
        return null;
    }
    
    public GameObject GetPlayer(int playerIndex)
    {
        foreach (var kvp in _playerIndexMap)
        {
            if (kvp.Value == playerIndex)
                return kvp.Key;
        }
        return null;
    }
    
    public bool IsLocalPlayer(GameObject player) => 
        _playerLocalMap.ContainsKey(player) && _playerLocalMap[player];
}
