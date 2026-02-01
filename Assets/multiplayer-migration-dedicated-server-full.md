# Multiplayer Migration Guide (Dedicated Server)

**Updated for Dedicated Server Architecture**

This document is the **updated version** of the multiplayer migration guide, aligned with your vision for a persistent, dedicated server-based multiplayer game.

**Vision**: Persistent world, one server, players join anytime  
**Architecture**: Dedicated Server (Edgegap Hosting)

> ⚠️ **Updated March 2026**: Unity Multiplay discontinued. Now using Edgegap for server hosting.

> **📋 Quick Reference**: [Architecture Decision Summary](multiplayer-migration-dedicated-server.md) | [Full Analysis](ecs-and-multiplayer-architecture-analysis.md)

---

## ⚠️ Key Differences from Original Guide

The original `multiplayer-migration.md` was written for **P2P with Lobby/Relay**. This updated guide is for **Dedicated Server**.

**Why the change?** Your stated vision: *"develop this as a hosted application using Unity's multiplayer hosting services and have players just join that single game"* = Dedicated Server model.

---

## Project Overview

### Current Game Type

Single-player 3D space combat game with:

- Player-controlled spaceship with desktop/gamepad controls
- AI-controlled enemy ships with patrol and attack behaviors
- Physics-based movement and combat
- Weapon systems (blasters and missiles)
- Asteroid fields as environmental obstacles
- Score tracking and game state management

### Target Multiplayer Type

**Persistent Multiplayer Space Combat**

- **Vision**: One persistent server, players join anytime (no lobbies/sessions)
- **Architecture**: Dedicated server (authoritative, cheat-proof)
- **Player Count**: 2-50+ players in shared space
- **Game Modes**: 
  - PvE: Team up against AI enemies
  - PvP: Optional deathmatch/team deathmatch modes
- **World**: Persistent asteroid fields, synchronized physics
- **Budget**: ~$50-70/month for small server (2 cores, 4GB RAM)

---

## Multiplayer Architecture Design

### Network Topology: Dedicated Server (Edgegap)

```
Edgegap Edge Network
├── Dedicated Linux Server (Containerized, Authoritative)
│   ├── Runs game simulation for active sessions
│   ├── Auto-spawned via Arbiter matchmaking
│   ├── Validates all player actions
│   ├── Controls AI, physics, damage
│   └── Replicates state to clients
│
└── Clients (equal peers)
    ├── Client 1 → Server (low-latency edge connection)
    ├── Client 2 → Server (low-latency edge connection)
    ├── Client 3 → Server (low-latency edge connection)
    └── Client N → Server (low-latency edge connection)
```

**Key Points**:

- **Dedicated Linux server** in Docker container (no player hosts)
- Deployed to Edgegap's edge network (closest to players for low latency)
- All players are clients (equal latency, no host advantage)
- **Server has full authority** over game state, AI, physics, damage
- Clients send input, server validates and replicates state
- **Session-based**: Edgegap spins up servers on-demand via Arbiter matchmaking
- **Auto-scaling**: Servers shut down when empty to save costs
- **Cheat-proof**: All validation server-side

### Authority Model

**Server Authority** (Dedicated Server runs these):
- Game state management
- AI enemy behavior and spawning
- Physics simulation (Rigidbody updates)
- Damage calculation and validation
- Projectile spawning (server spawns, replicates to clients)
- Score tracking
- Floating Origin shifts (server coordinates, replicates to clients)

**Client Authority** (Each client handles these):
- Player input (keyboard/mouse/gamepad)
- Camera following local player
- UI rendering (health bars, HUD)
- VFX and audio (local cosmetic effects)
- Prediction/interpolation for smooth movement

---

## Required Packages & Dependencies

### Unity Gaming Services Packages

**Install These**:

```json
{
  "com.unity.services.core": "1.12.0+",
  "com.unity.services.authentication": "3.3.0+",
  "com.unity.services.multiplayer": "1.0.0+",
  "com.unity.netcode.gameobjects": "2.0.0+",
  "com.unity.dedicated-server": "1.0.0+",
  "com.unity.multiplayer.tools": "2.2.1+"
}
```

**Edgegap Plugin** (install from GitHub):

```
https://github.com/edgegap/edgegap-unity-plugin.git
```

**DO NOT Install** (deprecated or not needed):

- ❌ `com.unity.services.multiplay` - DEPRECATED (Unity Multiplay discontinued March 2026)
- ❌ `com.unity.services.lobby` - Not needed (use Edgegap Arbiter for matchmaking)
- ❌ `com.unity.services.relay` - Not needed (direct connection to dedicated server)

### Already Installed

- ✅ `com.unity.inputsystem`: 1.17.0
- ✅ `com.unity.multiplayer.center`: 1.0.1
- ✅ `com.midniteoilsoftware.core` (EventBus, Singleton)

### What About Midnite Oil Multiplayer Package?

**Decision**: ❌ **Skip it**

**Why**: The Midnite Oil multiplayer boilerplate is designed for **P2P with Lobby/Relay** architecture, which contradicts your dedicated server vision.

The package provides:
- Lobby UI boilerplate (not needed - no lobbies)
- Relay integration helpers (not needed - direct server connection)
- P2P host/client setup (not needed - dedicated server)

**What you need instead**:

1. Unity Netcode for GameObjects ✅
2. Unity Multiplayer Services package ✅
3. Unity Dedicated Server package (Unity 6) ✅
4. Edgegap Unity Plugin for containerization ✅
5. Edgegap Arbiter for matchmaking (built-in)

You already have `com.midniteoilsoftware.core` for EventBus and Singleton, which is all you need from Midnite Oil.

---

## Migration Roadmap

### Phase 0: Foundation - 3 days (HIGH PRIORITY)

✅ **COMPLETED** (see [multiplayer-prep](multiplayer-prep.md)):
- Player combat system (DamageHandler, Shield)
- PlayerManager (spawning, tracking)
- Object pooling (projectiles, missiles, effects)
- EventBus integration
- Architecture analysis

⚠️ **HIGH PRIORITY - TODO**:
- [ ] **Implement Floating Origin** (3 days)
  - **Why**: Solves floating-point precision at large distances
  - **Required before**: Multiplayer testing at scale
  - See [ecs-and-multiplayer-architecture-analysis](ecs-and-multiplayer-architecture-analysis.md) for implementation

### Phase 1: Setup & Configuration - 1 week

#### 1.1 Install Required Packages

```
1. Open Package Manager (Window → Package Manager)
2. Add packages via "Add package by name":
   - com.unity.services.core
   - com.unity.services.authentication  
   - com.unity.netcode.gameobjects
   - com.unity.services.multiplay
   - com.unity.multiplayer.tools
3. Wait for packages to install and compile
```

#### 1.2 Configure Unity Gaming Services

```
1. Link project to Unity Cloud Project:
   - Edit → Project Settings → Services
   - Click "Create Unity project ID" or select existing
   - Select or create organization

2. Enable UGS services:
   - Go to https://dashboard.unity3d.com
   - Navigate to your project
   - Enable: Authentication, Multiplay
   - Skip: Lobby, Relay

3. Note your Project ID (needed for server builds)
```

#### 1.3 Setup NetworkManager

```
1. Create NetworkManager GameObject:
   - Hierarchy → Create Empty → "NetworkManager"
   - Add Component: NetworkManager

2. Configure NetworkManager:
   - Transport: Unity Transport
   - Network Tick Rate: 60Hz
   - Connection Approval: Enabled
   - Player Prefab: (assign in Phase 2)

3. Configure Unity Transport:
   - Connection Type: IP Address
   - Address: 0.0.0.0 (server listens on all)
   - Port: 7777
   - Max Payload Size: 6144 bytes

4. Save as prefab:
   - Drag to /Assets/_project/Prefabs/NetworkManager.prefab
```

#### 1.4 Create Server Build Configuration

```
1. Create server build target:
   - File → Build Settings
   - Target Platform: Dedicated Server
   - Or: Linux (for server deployment)

2. Add conditional symbols:
   - Project Settings → Player
   - Scripting Define Symbols: "DEDICATED_SERVER"
   - Use #if DEDICATED_SERVER for headless mode

3. Scene structure:
   - Main.unity: Gameplay (existing)
   - Optional: MainMenu.unity with "Join Server" UI
```

### Phase 2: Core Networking - 2 weeks

#### 2.1 Convert ShipController to NetworkBehaviour

Create `/Assets/_project/Scripts/ShipControls/NetworkShipController.cs`:

```csharp
using Unity.Netcode;
using UnityEngine;

public class NetworkShipController : NetworkBehaviour
{
    [SerializeField] Shield _shield;
    [SerializeField] protected MovementControlsBase _movementControls;
    [SerializeField] protected WeaponControlsBase _weaponControls;
    
    Rigidbody _rigidBody;
    NetworkDamageHandler _damageHandler;
    
    // Network variables
    NetworkVariable<int> _networkHealth = new NetworkVariable<int>();
    NetworkVariable<float> _networkShieldStrength = new NetworkVariable<float>();
    
    // Input (client → server)
    float _pitchAmount, _rollAmount, _yawAmount, _thrustAmount;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _rigidBody = GetComponent<Rigidbody>();
        _damageHandler = GetComponent<NetworkDamageHandler>();
        
        if (IsServer)
        {
            InitializeShipSystems();
        }
        
        if (IsOwner)
        {
            EnablePlayerControls();
        }
        else
        {
            DisableNonOwnerControls();
        }
    }
    
    void Update()
    {
        if (!IsOwner) return;
        
        // Gather input
        _rollAmount = _movementControls.RollAmount;
        _yawAmount = _movementControls.YawAmount;
        _pitchAmount = _movementControls.PitchAmount;
        _thrustAmount = _movementControls.ThrustAmount;
        
        // Send to server
        SendInputToServerRpc(_pitchAmount, _rollAmount, _yawAmount, _thrustAmount);
    }
    
    [ServerRpc]
    void SendInputToServerRpc(float pitch, float roll, float yaw, float thrust)
    {
        _pitchAmount = pitch;
        _rollAmount = roll;
        _yawAmount = yaw;
        _thrustAmount = thrust;
    }
    
    void FixedUpdate()
    {
        if (!IsServer) return;
        
        ApplyShipPhysics();
    }
    
    void ApplyShipPhysics()
    {
        // Server-side physics simulation
        // Use _pitchAmount, _rollAmount, _yawAmount, _thrustAmount
        // Apply to _rigidBody
    }
}
```

#### 2.2 Create NetworkDamageHandler

Extend `DamageHandler` for network synchronization:

```csharp
using Unity.Netcode;

public class NetworkDamageHandler : NetworkBehaviour
{
    NetworkVariable<int> _networkHealth = new NetworkVariable<int>();
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            _networkHealth.Value = maxHealth;
        }
        
        _networkHealth.OnValueChanged += OnHealthChanged;
    }
    
    public void TakeDamage(int damage, GameObject source)
    {
        if (!IsServer) return;
        
        int actualDamage = damage;
        
        // Try shield first
        if (_shield && _shield.IsActive)
        {
            actualDamage = _shield.AbsorbDamage(damage);
        }
        
        _networkHealth.Value -= actualDamage;
        
        if (_networkHealth.Value <= 0)
        {
            DestroyObjectClientRpc();
        }
    }
    
    [ClientRpc]
    void DestroyObjectClientRpc()
    {
        // Play destruction VFX
        // Disable GameObject
        gameObject.SetActive(false);
    }
    
    void OnHealthChanged(int oldValue, int newValue)
    {
        // Update UI on all clients
        EventBus.Instance.Raise(new HealthChangedEvent(gameObject, newValue));
    }
}
```

#### 2.3 Update PlayerManager for Networking

Convert `PlayerManager` to use `NetworkManager.Singleton` for spawning:

```csharp
public class NetworkPlayerManager : SingletonMonoBehaviour<NetworkPlayerManager>
{
    [SerializeField] GameObject _playerShipPrefab;
    
    public void SpawnPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        var spawnPoint = GetAvailableSpawnPoint();
        var playerShip = Instantiate(_playerShipPrefab, spawnPoint.Position, spawnPoint.Rotation);
        
        var networkObject = playerShip.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);
        
        spawnPoint.Occupy();
    }
}
```

### Phase 3: Server Authority - 1 week

#### 3.1 Server-Authoritative Enemy Spawning

```csharp
public class NetworkEnemyManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        StartCoroutine(SpawnEnemyWaves());
    }
    
    IEnumerator SpawnEnemyWaves()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(5f);
        }
    }
    
    void SpawnEnemy()
    {
        var enemy = Instantiate(_enemyPrefab, GetRandomPosition(), Quaternion.identity);
        enemy.GetComponent<NetworkObject>().Spawn();
    }
}
```

#### 3.2 Server-Authoritative Projectiles

```csharp
public class NetworkBlaster : NetworkBehaviour
{
    public void Fire()
    {
        if (!IsOwner) return;
        
        FireServerRpc();
    }
    
    [ServerRpc]
    void FireServerRpc()
    {
        // Server spawns projectile
        var projectile = PoolManager.Instance.Get<Projectile>();
        projectile.transform.position = _firePoint.position;
        projectile.transform.rotation = _firePoint.rotation;
        
        projectile.GetComponent<NetworkObject>().Spawn();
        projectile.Initialize(_damage, _speed, gameObject);
    }
}
```

### Phase 4: Deployment - 1 week

#### 4.1 Create Headless Server Build

```
1. Create server build script:
   - File → Build Settings
   - Platform: Dedicated Server (or Linux)
   - Check "Server Build"

2. Disable rendering/audio for server:
   #if DEDICATED_SERVER
       Camera.main.enabled = false;
       AudioListener.pause = true;
   #endif

3. Build server executable
```

#### 4.2 Deploy to Unity Multiplay

```
1. Package server build:
   - Compress build folder
   - Include all dependencies

2. Upload to UGS:
   - Unity Dashboard → Multiplay
   - Create Fleet
   - Upload server build
   - Configure instance (2 cores, 4GB RAM)

3. Test deployment:
   - Launch server instance
   - Connect client build via IP
   - Verify gameplay
```

---

## Cost Planning

### Unity Multiplay Pricing

**Free Tier**:
- $800 credit (valid 6 months)
- Starts when you first activate UGS for organization
- Check status: Unity Dashboard → Metered Billing

**Small Server** (2 cores, 4GB RAM, Linux):
- Hourly: ~$0.096
- Monthly 24/7: ~$70
- Your target: ~$50/month (achievable with optimization)

**Your $800 credit gets you**:
- ~8,300 hours (~11.5 months on small server 24/7)
- Perfect for development and initial launch

**Cost optimization strategies**:
- Auto-scale: Shut down when no players online
- Start with 1 core, 2GB (even cheaper) for testing
- Monitor usage and scale as needed

---

## Testing Strategy

### Local Testing (ParrelSync)

```
1. Install ParrelSync package (GitHub)
2. Create test clone project
3. Run server build in main project
4. Run client build in clone project
5. Test connection and gameplay
```

### Server Testing

```
1. Deploy to Multiplay test environment
2. Connect multiple clients
3. Test:
   - Player spawning/despawning
   - Combat synchronization
   - AI behavior replication
   - Floating Origin shifts
   - Performance under load
```

---

## Key Differences from P2P

| Feature | Dedicated Server | P2P/Lobby |
|---------|------------------|-----------|
| **Connection** | Direct IP or matchmaking | Lobby + Relay |
| **Host** | Linux server (no player) | One player hosts |
| **Authority** | Server | Host player |
| **Setup** | NetworkManager only | Lobby UI + Relay code |
| **Latency** | Equal for all players | Host=0ms, others lag |
| **Persistence** | 24/7 server | Ends when host leaves |
| **Cheating** | Server validates all | Host can modify state |
| **Player Limit** | 50-100+ | 16 max (Relay) |
| **Cost** | $50-70/month | Free tier |
| **Complexity** | Server build deployment | Lobby UI implementation |

---

## Next Steps

1. ✅ Review [architecture analysis](ecs-and-multiplayer-architecture-analysis.md)
2. ⚠️ **Implement Floating Origin** (HIGH PRIORITY)
3. Install UGS packages (Multiplay, NOT Lobby/Relay)
4. Configure NetworkManager
5. Convert core systems to NetworkBehaviour
6. Create server build
7. Deploy to Unity Multiplay
8. Test with multiple clients

---

## Reference Documents

- **Architecture Decision**: [multiplayer-migration-dedicated-server](multiplayer-migration-dedicated-server.md)
- **Full Analysis**: [ecs-and-multiplayer-architecture-analysis](ecs-and-multiplayer-architecture-analysis.md)
- **Preparation**: [multiplayer-prep](multiplayer-prep.md)
- **Diagrams**: [multiplayer-architecture-diagrams](multiplayer-architecture-diagrams.md)
