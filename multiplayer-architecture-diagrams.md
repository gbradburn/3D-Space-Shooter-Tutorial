# Multiplayer Architecture Diagrams

Visual reference for the multiplayer architecture design.

---

## Network Variables Strategy

```mermaid
graph TD
    subgraph "Player Ship Components"
        P1[NetworkTransform<br/>Position/Rotation]
        P2[NetworkVariable Health]
        P3[NetworkVariable Shield]
        P4[NetworkVariable Score]
        P5[ServerRpc Input]
    end
    
    subgraph "Enemy Ship Components"
        E1[NetworkTransform<br/>Position/Rotation]
        E2[NetworkVariable Health]
        E3[NetworkVariable AI State]
        E4[Server-Only Target]
    end
    
    subgraph "Projectile Components"
        PR1[NetworkTransform<br/>Interpolated]
        PR2[Server Spawned]
    end
    
    subgraph "Game State"
        G1[NetworkVariable Phase]
        G2[NetworkVariable Players]
        G3[NetworkVariable Timer]
    end
    
    style P1 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style P2 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style P3 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style P4 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style P5 fill:#16a085,stroke:#117a65,stroke-width:2px,color:#fff
    style E1 fill:#e67e22,stroke:#ca6f1e,stroke-width:2px,color:#fff
    style E2 fill:#e67e22,stroke:#ca6f1e,stroke-width:2px,color:#fff
    style E3 fill:#e67e22,stroke:#ca6f1e,stroke-width:2px,color:#fff
    style E4 fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style PR1 fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
    style PR2 fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style G1 fill:#8e44ad,stroke:#6c3483,stroke-width:2px,color:#fff
    style G2 fill:#8e44ad,stroke:#6c3483,stroke-width:2px,color:#fff
    style G3 fill:#8e44ad,stroke:#6c3483,stroke-width:2px,color:#fff
```

**Legend**:

- 🔵 Blue: Player-owned components
- 🟢 Teal: Client-to-server RPCs
- 🟠 Orange: Enemy components (server-controlled)
- 🔴 Red: Server-only logic
- 🟡 Yellow: Projectile components
- 🟣 Purple: Game state components

---

## Multiplayer Data Flow

```mermaid
sequenceDiagram
    participant Client
    participant Server
    participant AI
    participant Physics
    
    Client->>Server: Input RPC (pitch, roll, yaw)
    Server->>Physics: Apply forces to Rigidbody
    Physics->>Server: Updated position/rotation
    Server->>Client: NetworkTransform sync
    
    Client->>Server: Fire Weapon RPC
    Server->>Server: Spawn Projectile (NetworkObject)
    Server->>Client: Replicate Projectile
    
    Physics->>Server: Projectile collision detected
    Server->>Server: Calculate damage (authority)
    Server->>Client: Update health (NetworkVariable)
    
    AI->>Server: Enemy decision (server-only)
    Server->>Physics: Apply enemy movement
    Server->>Client: Enemy state sync
```

---

## Component Migration Flow

```mermaid
graph LR
    subgraph "Single-Player"
        SP1[ShipController]
        SP2[DamageHandler]
        SP3[Projectile]
        SP4[GameManager]
    end
    
    subgraph "Multiplayer"
        MP1[NetworkShipController<br/>+ NetworkBehaviour]
        MP2[NetworkDamageHandler<br/>+ NetworkVariable Health]
        MP3[NetworkProjectile<br/>+ NetworkObject]
        MP4[NetworkGameManager<br/>+ NetworkVariable State]
    end
    
    SP1 -.convert.-> MP1
    SP2 -.convert.-> MP2
    SP3 -.convert.-> MP3
    SP4 -.convert.-> MP4
    
    style SP1 fill:#95a5a6,stroke:#7f8c8d,stroke-width:2px,color:#000
    style SP2 fill:#95a5a6,stroke:#7f8c8d,stroke-width:2px,color:#000
    style SP3 fill:#95a5a6,stroke:#7f8c8d,stroke-width:2px,color:#000
    style SP4 fill:#95a5a6,stroke:#7f8c8d,stroke-width:2px,color:#000
    style MP1 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style MP2 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style MP3 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style MP4 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
```

---

## Player Spawning Architecture

```mermaid
graph TD
    Start[Game Start] --> NM[NetworkManager]
    NM --> PC{Player Connected}
    PC -->|Yes| PM[PlayerManager]
    PM --> SP[Get Available SpawnPoint]
    SP --> Spawn[Spawn PlayerShip Prefab]
    Spawn --> NO[Add NetworkObject]
    NO --> Owner[Assign Ownership to Client]
    Owner --> Camera[Setup Camera for Owner]
    Camera --> Input[Enable Input for Owner]
    Input --> Ready[Player Ready]
    
    PC -->|No| Wait[Wait for Connection]
    Wait --> PC
    
    style Start fill:#8e44ad,stroke:#6c3483,stroke-width:2px,color:#fff
    style NM fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style PM fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style NO fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
    style Owner fill:#16a085,stroke:#117a65,stroke-width:2px,color:#fff
    style Ready fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
```

---

## Weapon System Data Flow

```mermaid
graph TD
    Input[Player Input] --> WC[WeaponControls]
    WC --> Trigger{Fire Button?}
    
    Trigger -->|Yes| Check{Cooldown OK?}
    Check -->|Yes| RPC[FireWeaponServerRpc]
    Check -->|No| Wait[Wait]
    
    RPC --> Server[Server Authority]
    Server --> Pool{Use Pool?}
    
    Pool -->|Yes| GetPool[PoolManager.Get Projectile]
    Pool -->|No| Spawn[Instantiate Projectile]
    
    GetPool --> Setup[Setup NetworkObject]
    Spawn --> Setup
    
    Setup --> Activate[Activate & Launch]
    Activate --> Replicate[Replicate to All Clients]
    
    Replicate --> C1[Client 1 Shows Projectile]
    Replicate --> C2[Client 2 Shows Projectile]
    Replicate --> C3[Client 3 Shows Projectile]
    
    style Input fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style RPC fill:#16a085,stroke:#117a65,stroke-width:2px,color:#fff
    style Server fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style Pool fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
    style Replicate fill:#8e44ad,stroke:#6c3483,stroke-width:2px,color:#fff
```

---

## Damage Calculation Authority

```mermaid
sequenceDiagram
    participant P1 as Player 1 (Client)
    participant Server
    participant P2 as Player 2 (Client)
    
    Note over P1: Projectile collision<br/>detected locally
    P1->>P1: OnTriggerEnter (local only)
    
    Note over Server: Server also detects<br/>(authoritative)
    Server->>Server: OnTriggerEnter
    Server->>Server: Calculate damage
    Server->>Server: Apply to NetworkVariable Health
    
    Note over Server,P2: NetworkVariable replicates
    Server->>P1: Health updated
    Server->>P2: Health updated
    
    P1->>P1: Update health UI
    P2->>P2: Update health UI
    
    Note over Server: Check if destroyed
    Server->>Server: Health <= 0?
    Server->>Server: Despawn NetworkObject
    Server->>P1: Object destroyed
    Server->>P2: Object destroyed
    
    P1->>P1: Play death VFX
    P2->>P2: Play death VFX
```

---

## Enemy AI Synchronization

```mermaid
graph TD
    Server[Server Authority] --> AI[AI System]
    AI --> State{AI State}
    
    State -->|Patrol| Patrol[Calculate Patrol Path]
    State -->|Chase| Chase[Calculate Intercept]
    State -->|Attack| Attack[Fire Weapons]
    
    Patrol --> Move[Apply Movement Forces]
    Chase --> Move
    Attack --> Move
    Attack --> Fire[Spawn Projectiles]
    
    Move --> NV1[NetworkVariable Position]
    Fire --> NV2[NetworkObject Projectile]
    
    NV1 --> C1[Client 1]
    NV1 --> C2[Client 2]
    NV2 --> C1
    NV2 --> C2
    
    C1 --> Render1[Render Enemy Ship]
    C2 --> Render2[Render Enemy Ship]
    
    style Server fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style AI fill:#e67e22,stroke:#ca6f1e,stroke-width:2px,color:#fff
    style NV1 fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
    style NV2 fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
    style C1 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style C2 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
```

---

## Lobby & Matchmaking Flow

```mermaid
graph TD
    Start[Player Launches Game] --> Menu[Main Menu]
    Menu --> Choice{Player Choice}
    
    Choice -->|Host| CreateLobby[Create Lobby]
    Choice -->|Join| LobbyList[Show Lobby List]
    
    CreateLobby --> Relay[Allocate Relay]
    Relay --> StartServer[Start as Host]
    StartServer --> LobbyWait[Wait in Lobby]
    
    LobbyList --> SelectLobby[Select Lobby]
    SelectLobby --> JoinRelay[Join via Relay]
    JoinRelay --> Connect[Connect to Host]
    Connect --> LobbyWait
    
    LobbyWait --> AllReady{All Ready?}
    AllReady -->|No| LobbyWait
    AllReady -->|Yes| LoadGame[Load Game Scene]
    
    LoadGame --> SpawnPlayers[Spawn All Players]
    SpawnPlayers --> GameStart[Game Start]
    
    style Start fill:#8e44ad,stroke:#6c3483,stroke-width:2px,color:#fff
    style CreateLobby fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style JoinRelay fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style GameStart fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
```

---

## Object Pooling with Networking

```mermaid
graph LR
    subgraph PoolManager
        Pool[Object Pool<br/>Projectiles]
    end
    
    subgraph Server
        Fire[Fire Weapon] --> GetPool{Pool Available?}
        GetPool -->|Yes| Reuse[Reuse Pooled Object]
        GetPool -->|No| Create[Spawn New NetworkObject]
        
        Reuse --> Activate[Activate & Setup]
        Create --> Activate
        
        Activate --> Spawn[Spawn on Network]
        Spawn --> Clients[Replicate to Clients]
    end
    
    subgraph Projectile Lifecycle
        Active[Active Projectile] --> Hit{Hit Target?}
        Hit -->|Yes| Return[Return to Pool]
        Hit -->|No| Timeout[Timeout]
        Timeout --> Return
        
        Return --> Despawn[Despawn from Network]
        Despawn --> Deactivate[Deactivate GameObject]
        Deactivate --> Pool
    end
    
    style Pool fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
    style Fire fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style Spawn fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style Return fill:#16a085,stroke:#117a65,stroke-width:2px,color:#fff
```

---

## Phase 0 Preparation Dependencies

```mermaid
graph TD
    Start[Start Multiplayer Migration] --> Prep{Phase 0 Complete?}
    
    Prep -->|No| P01[0.1 Player Combat System]
    Prep -->|No| P02[0.2 PlayerManager]
    Prep -->|No| P03[0.3 Object Pooling]
    Prep -->|No| P04[0.4 ECS Investigation]
    
    P01 --> Shield[DamageHandler + Shield]
    Shield --> UI[Health UI]
    
    P02 --> SpawnPoints[Spawn Point System]
    SpawnPoints --> Manager[PlayerManager Script]
    
    P03 --> PoolSystem[PoolManager + ObjectPool]
    PoolSystem --> Integrate[Integrate with Blaster]
    
    P04 --> Research[ECS Research]
    Research --> Decision[Make Decision]
    
    UI --> Complete
    Manager --> Complete
    Integrate --> Complete
    Decision --> Complete
    
    Complete[Phase 0 Complete] --> Phase1[Phase 1: Setup]
    
    Prep -->|Yes| Phase1
    
    style Start fill:#8e44ad,stroke:#6c3483,stroke-width:2px,color:#fff
    style Prep fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
    style P01 fill:#e67e22,stroke:#ca6f1e,stroke-width:2px,color:#fff
    style P02 fill:#e67e22,stroke:#ca6f1e,stroke-width:2px,color:#fff
    style P03 fill:#e67e22,stroke:#ca6f1e,stroke-width:2px,color:#fff
    style P04 fill:#e67e22,stroke:#ca6f1e,stroke-width:2px,color:#fff
    style Complete fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style Phase1 fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
```

---

## NetworkTransform Configuration

Detailed configuration settings for NetworkTransform component on player ships.

### Authority Configuration

```mermaid
graph TD
    A[NetworkTransform Component] --> B[Authority Settings]
    A --> C[Position Settings]
    A --> D[Rotation Settings]
    A --> E[Scale Settings]
    A --> F[Performance Settings]
    
    B --> B1[Server Authoritative ✓]
    B --> B2[Sync In Local Space ✗]
    
    C --> C1[Sync XYZ ✓]
    C --> C2[Threshold: 0.01m]
    C --> C3[Smooth Dampening ✓]
    
    D --> D1[Sync XYZ ✓]
    D --> D2[Threshold: 0.1°]
    D --> D3[Quaternion Sync ✓]
    D --> D4[Quaternion Compression ✓]
    
    E --> E1[Sync XYZ ✗]
    
    F --> F1[Half Float Precision ✓]
    F --> F2[Tick Sync Children ✗]
    F --> F3[Unreliable Deltas ✗]
    
    style B1 fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style B2 fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style C3 fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style D3 fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style D4 fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style E1 fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style F1 fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style F2 fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style F3 fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
```

### Interpolation Options Comparison

```mermaid
graph LR
    subgraph "Interpolation Methods"
        A[Legacy Lerp]
        B[Lerp]
        C[Smooth Dampening]
    end
    
    subgraph "Characteristics"
        A --> A1[❌ Deprecated]
        A --> A2[Linear movement]
        
        B --> B1[✓ Modern]
        B --> B2[Linear movement]
        B --> B3[Simpler, faster]
        
        C --> C1[✓ Recommended]
        C --> C2[Natural acceleration]
        C --> C3[Best for physics]
    end
    
    style A fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style B fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
    style C fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
```

### Bandwidth Optimization

```mermaid
graph TD
    Start[Player Ship] --> Settings{Optimization Settings}
    
    Settings --> HF[Half Float Precision ✓<br/>~2 KB/s saved]
    Settings --> QC[Quaternion Compression ✓<br/>~0.5 KB/s saved]
    Settings --> TH[Thresholds<br/>30-50% fewer updates]
    
    HF --> Total
    QC --> Total
    TH --> Total
    
    Total[Total Bandwidth] --> Result[2-4 KB/s per ship ✓]
    
    style HF fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style QC fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style TH fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style Result fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
```

---

**See Also**:

- [multiplayer-start-here.md](multiplayer-start-here.md) - Migration overview
- [multiplayer-prep.md](multiplayer-prep.md) - Phase 0 preparation
- [multiplayer-migration.md](multiplayer-migration.md) - Full migration guide

