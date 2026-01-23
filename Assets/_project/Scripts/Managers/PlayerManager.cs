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
    
    [Header("Respawn Configuration")]
    [SerializeField] float _respawnDelay = 3f;
    [SerializeField] bool _enableAutoRespawn = true;
    
    readonly List<GameObject> _activePlayers = new();
    readonly Dictionary<GameObject, int> _playerIndexMap = new();
    readonly Dictionary<GameObject, bool> _playerLocalMap = new();
    readonly Dictionary<GameObject, CountdownTimer> _respawnTimers = new();
    
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

    protected override void OnDestroy()
    {
        Debug.Log($"PlayerManager: OnDestroy called, cleaning up timers.", this);
        base.OnDestroy();
        if (TimerManager.Instance != null)
        {
            foreach(var timerPair in _respawnTimers)
            {
                TimerManager.Instance.ReleaseTimer<CountdownTimer>(timerPair.Value);
            }
        }
        _respawnTimers.Clear();
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
        DontDestroyOnLoad(player);
        
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
            if (cameraManager)
            {
                SetCameraFollowTarget(player.transform);
            }
        }
        
        var damageHandler = player.GetComponent<DamageHandler>();
        if (damageHandler)
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
        var deathPosition = player.transform.position;
        
        var damageHandler = player.GetComponent<DamageHandler>();
        var explosion = damageHandler ? damageHandler.LastExplosion : null;
        
        EventBus.Instance.Raise(new PlayerDestroyedEvent(player, playerIndex, deathPosition, explosion));
        
        if (_enableDebugLog) 
            Debug.Log($"PlayerManager: Player {playerIndex} destroyed. Remaining: {_activePlayers.Count}", this);

        if (_enableAutoRespawn)
        {
            StartRespawnTimer(player, playerIndex, wasLocal);
            return;
        }
        
        _activePlayers.Remove(player);
        _playerIndexMap.Remove(player);
        _playerLocalMap.Remove(player);
        FreeSpawnPointNearPosition(player.transform.position);
    }

    void StartRespawnTimer(GameObject player, int playerIndex, bool isLocal)
    {
        if (_enableDebugLog)
        {
            Debug.Log($"PlayerManager: Starting respawn timer for player {playerIndex} ({_respawnDelay} seconds)", this);
        }
        
        var timer = TimerManager.Instance.CreateTimer<CountdownTimer>(_respawnDelay);
        _respawnTimers[player] = timer as CountdownTimer;

        timer.OnTimerStop = () => OnRespawnTimerComplete(player, playerIndex, isLocal);
        timer.Start();
    }

    void OnRespawnTimerComplete(GameObject player, int playerIndex, bool isLocal)
    {
        if (!player) return;
        if (_respawnTimers.ContainsKey(player))
        {
            TimerManager.Instance.ReleaseTimer<CountdownTimer>(_respawnTimers[player]);
            _respawnTimers.Remove(player);
        }

        RespawnPlayer(player, playerIndex, isLocal);
    }

    void RespawnPlayer(GameObject player, int playerIndex, bool isLocal)
    {
        if (!player || !player.transform)
        {
            if (_enableDebugLog)
            {
                Debug.LogError($"RespawnPlayer: Player object is null or destroyed, cannot respawn player {playerIndex}", this);
            }
            return;
        }
        FreeSpawnPointNearPosition(player.transform.position);
        var spawnPoint = GetAvailableSpawnPoint(playerIndex);
        if (!spawnPoint)
        {
            Debug.LogError("PlayerManager: No available spawn points for respawn!", this);
            return;
        }
        
        player.transform.SetPositionAndRotation(spawnPoint.Position, spawnPoint.Rotation);
        spawnPoint.SetOccupied(true);
        var rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        player.SetActive(true);
        EventBus.Instance.Raise(new PlayerSpawnedEvent(player, playerIndex, isLocal));

        if (_enableDebugLog)
        {
            Debug.Log($"PlayerManager: Respawned player {playerIndex} at spawn point", this);
        }
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
