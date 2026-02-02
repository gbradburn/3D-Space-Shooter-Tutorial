# Multiplayer Architecture Diagrams

Visual reference for the multiplayer architecture design.

**Updated**: Edgegap hosting with username/password authentication

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

## Scene Architecture

**Client Build** (2 scenes):
- `Login` scene - Authentication UI (client-only, excluded from server build)
- `Main` scene - Game scene with multiplayer gameplay

**Server Build** (1 scene):
- `Main` scene only - Game scene (no Login scene)

**Build Profile Configuration**:
- Login scene is excluded from Dedicated Server build profile
- Server boots directly into Main scene
- Clients boot into Login scene, then load Main after authentication

---

## Authentication Flow (Username/Password)

```mermaid
sequenceDiagram
    participant Client
    participant LoginScene as Login Scene
    participant LoginUI
    participant AuthManager as AuthenticationManager
    participant UnityAuth as Unity Authentication
    participant MainScene as Main Scene
    participant NetworkManager
    participant Server
    participant Validator as NetworkAuthValidator
    
    Note over Client: Game Launch → Login Scene
    LoginScene->>AuthManager: Start() - Initialize Unity Services
    AuthManager->>UnityAuth: UnityServices.InitializeAsync()
    UnityAuth-->>AuthManager: Services Ready
    
    AuthManager->>AuthManager: Check IsSignedIn
    
    alt Already Signed In (Cached Session)
        AuthManager-->>LoginScene: OnAuthenticationSuccess
        LoginScene->>MainScene: SceneManager.LoadScene("Main")
    else Not Signed In
        LoginScene->>LoginUI: Show Login/Register UI
        
        Note over LoginUI: Player Action
        
        alt Register New Account
            LoginUI->>LoginUI: Validate Username (3-20 chars, alphanumeric)
            LoginUI->>LoginUI: Validate Password (8-30 chars, mixed case + numbers)
            LoginUI->>AuthManager: SignUpWithUsernamePassword(username, password)
            AuthManager->>UnityAuth: SignUpWithUsernamePasswordAsync()
            
            alt Success
                UnityAuth-->>AuthManager: Player ID created
                AuthManager->>AuthManager: Store PlayerName = username
                AuthManager-->>LoginUI: OnAuthenticationSuccess(PlayerId)
                LoginUI->>MainScene: SceneManager.LoadScene("Main")
            else Failure
                UnityAuth-->>AuthManager: Error (Account exists, invalid format, etc)
                AuthManager-->>LoginUI: OnAuthenticationFailed(friendly error)
                Note over LoginUI: Show error message
            end
            
        else Login to Existing Account
            LoginUI->>LoginUI: Validate input
            LoginUI->>AuthManager: SignInWithUsernamePassword(username, password)
            AuthManager->>UnityAuth: SignInWithUsernamePasswordAsync()
            
            alt Success
                UnityAuth-->>AuthManager: Player ID + Access Token
                AuthManager->>AuthManager: Store PlayerName = username
                AuthManager-->>LoginUI: OnAuthenticationSuccess(PlayerId)
                LoginUI->>MainScene: SceneManager.LoadScene("Main")
            else Failure
                UnityAuth-->>AuthManager: Error (Invalid credentials)
                AuthManager-->>LoginUI: OnAuthenticationFailed("Invalid username or password")
                Note over LoginUI: Show error message
            end
        end
    end
    
    Note over MainScene: Main Scene Loaded (Client)
    Note over MainScene: Player clicks "Join Game"
    MainScene->>MainScene: ClientConnectionManager.ConnectToServer()
    MainScene->>AuthManager: Get PlayerId, PlayerName, AccessToken
    AuthManager-->>MainScene: Auth data
    
    MainScene->>MainScene: Create AuthConnectionData payload
    MainScene->>MainScene: Serialize to JSON bytes
    MainScene->>NetworkManager: Set ConnectionData = payload
    MainScene->>NetworkManager: StartClient()
    
    NetworkManager->>Server: Connection Request + Payload
    
    Note over Server: Server Boot → Main Scene Only
    Server->>Validator: ConnectionApprovalCallback invoked
    Validator->>Validator: Deserialize JSON payload
    Validator->>Validator: Extract PlayerId, Token, PlayerName
    Validator->>Validator: ValidateAuthToken(PlayerId, Token)
    
    alt Valid Token
        Validator->>Server: response.Approved = true
        Validator->>Server: response.CreatePlayerObject = true
        Server-->>Client: Connection Approved
        Server->>Server: Spawn Player NetworkObject
        Server->>Server: Set NetworkPlayerData (PlayerId, PlayerName)
        Server-->>Client: Player Spawned with identity
        Note over Client: Game Start (authenticated)
    else Invalid Token
        Validator->>Server: response.Approved = false
        Server-->>Client: Connection Rejected
        Note over Client: Show "Authentication failed" error
    end
```

**Key Features**:

- **Dedicated Login Scene**: Separate scene for authentication (client-only)
- **Build Profile Separation**: Login scene excluded from server build
- **Session Persistence**: Auto-login on subsequent launches
- **Client-side Validation**: Username/password format checked before sending
- **Server-side Validation**: Token validated on connection approval
- **User-Friendly Errors**: Friendly error messages for all failure cases
- **Secure**: Access token never exposed to client code (internal to Unity Auth SDK)

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

## Complete Player Flow (Authentication → Matchmaking → Game)

```mermaid
graph TD
    Start[Player Launches Game<br/>Client-Only] --> LoadLogin[Load Login Scene]
    LoadLogin --> Init[AuthenticationManager.Start]
    Init --> InitServices[UnityServices.InitializeAsync]
    InitServices --> CheckCache{IsSignedIn?}
    
    CheckCache -->|Yes| AutoLogin[Load Cached Session]
    AutoLogin --> LoadMain[SceneManager.LoadScene Main]
    
    CheckCache -->|No| LoginUI[Show Login/Register UI]
    
    LoginUI --> UserChoice{User Action}
    UserChoice -->|Login| ValidateLogin[Validate Input Format]
    UserChoice -->|Register| ValidateReg[Validate Username & Password]
    
    ValidateLogin --> Login[SignInWithUsernamePasswordAsync]
    ValidateReg --> Register[SignUpWithUsernamePasswordAsync]
    
    Login --> ValidLogin{Valid?}
    Register --> ValidReg{Valid?}
    
    ValidLogin -->|Yes| StoreSession[Store PlayerId & PlayerName]
    ValidReg -->|Yes| StoreSession
    
    StoreSession --> LoadMain
    
    ValidLogin -->|No| LoginError[Show Friendly Error]
    ValidReg -->|No| RegError[Show Friendly Error]
    
    LoginError --> LoginUI
    RegError --> LoginUI
    
    LoadMain --> MainScene[Main Scene Loaded]
    MainScene --> FindMatch[Click Find Match/Join Game]
    FindMatch --> Arbiter[Edgegap Arbiter Matchmaking]
    
    Arbiter --> Check{Server Available?}
    Check -->|No| Provision[Provision New Server Container]
    Check -->|Yes| Assign[Assign to Existing Server]
    
    Provision --> Docker[Build Docker Container]
    Docker --> Edge[Deploy to Nearest Edge Node]
    Edge --> Ready[Server Ready & Listening]
    
    Assign --> Ready
    
    Ready --> CreatePayload[Create Auth Payload<br/>PlayerId, Token, PlayerName]
    CreatePayload --> Serialize[Serialize to JSON bytes]
    Serialize --> SetData[Set NetworkConfig.ConnectionData]
    SetData --> Connect[StartClient]
    
    Connect --> ServerReceive[Server Receives Connection]
    ServerReceive --> Callback[ConnectionApprovalCallback]
    Callback --> Deserialize[Deserialize Payload]
    Deserialize --> Validate[ValidateAuthToken]
    
    Validate --> Approved{Valid Token?}
    Approved -->|Yes| CreatePlayer[CreatePlayerObject = true]
    Approved -->|No| Reject[Approved = false]
    
    CreatePlayer --> SpawnPlayer[Spawn Player NetworkObject]
    SpawnPlayer --> SetIdentity[Set NetworkPlayerData<br/>PlayerId + PlayerName]
    SetIdentity --> GameStart[Game Start - Authenticated]
    
    Reject --> ShowError[Connection Rejected]
    ShowError --> MainScene
    
    GameStart --> Playing[Player In Game]
    Playing --> Disconnect{Player Leaves?}
    Disconnect -->|Yes| Cleanup[Despawn Player]
    Disconnect -->|No| Playing
    
    Cleanup --> CheckEmpty{Server Empty?}
    CheckEmpty -->|Yes| Shutdown[Arbiter Shuts Down Server]
    CheckEmpty -->|No| WaitPlayers[Wait for More Players]
    
    Shutdown --> Done[Server Terminated]
    
    style Start fill:#8e44ad,stroke:#6c3483,stroke-width:2px,color:#fff
    style LoadLogin fill:#9b59b6,stroke:#7d3c98,stroke-width:2px,color:#fff
    style LoginUI fill:#3498db,stroke:#2980b9,stroke-width:2px,color:#fff
    style Login fill:#16a085,stroke:#117a65,stroke-width:2px,color:#fff
    style Register fill:#16a085,stroke:#117a65,stroke-width:2px,color:#fff
    style LoadMain fill:#9b59b6,stroke:#7d3c98,stroke-width:2px,color:#fff
    style MainScene fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style Arbiter fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style Docker fill:#34495e,stroke:#2c3e50,stroke-width:2px,color:#fff
    style Edge fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style Validate fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
    style GameStart fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style Reject fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style Shutdown fill:#95a5a6,stroke:#7f8c8d,stroke-width:2px,color:#000
```

**Architecture Highlights**:

- **Login Scene**: Dedicated client-only scene for authentication (excluded from server build)
- **Build Profile Separation**: Server boots directly to Main scene, clients to Login scene
- **Session Persistence**: Cached login tokens for seamless re-authentication
- **Edgegap Arbiter**: Auto-scaling matchmaking with server lifecycle management
- **Docker Containers**: Servers run in isolated containers deployed to edge nodes
- **Token Validation**: Server-side approval prevents unauthorized connections
- **Auto-Shutdown**: Servers automatically terminate when empty to save costs
- **Edge Deployment**: Servers spawn on nodes closest to players for low latency

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

## Edgegap Server Lifecycle

```mermaid
graph LR
    subgraph "Arbiter Matchmaking"
        Request[Player Requests Match]
        Request --> Check{Server Available?}
        Check -->|No| Provision[Provision New Server]
        Check -->|Yes| Assign[Assign to Existing]
    end
    
    subgraph "Server Lifecycle"
        Provision --> Build[Build Docker Container]
        Build --> Deploy[Deploy to Edge Node]
        Deploy --> Boot[Server Boots Up]
        Boot --> Listen[Listen for Connections]
        
        Assign --> Listen
        
        Listen --> Players[Accept Players]
        Players --> Active[Server Active]
        
        Active --> Monitor{Players Connected?}
        Monitor -->|Yes| Active
        Monitor -->|No| Idle[Server Idle]
        
        Idle --> Timeout{Idle Timeout?}
        Timeout -->|No| Monitor
        Timeout -->|Yes| Shutdown[Shutdown Server]
        
        Shutdown --> Cleanup[Release Resources]
        Cleanup --> Terminated[Container Terminated]
    end
    
    subgraph "Cost Optimization"
        Terminated --> Save[💰 Stop Billing]
        Active --> Bill[💰 Pay Per Hour]
    end
    
    style Request fill:#3498db,stroke:#2980b9,stroke-width:2px,color:#fff
    style Provision fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style Build fill:#34495e,stroke:#2c3e50,stroke-width:2px,color:#fff
    style Active fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style Shutdown fill:#e67e22,stroke:#ca6f1e,stroke-width:2px,color:#fff
    style Save fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
    style Bill fill:#f39c12,stroke:#c87f0a,stroke-width:2px,color:#000
```

**Benefits of Session-Based Architecture**:

- **Cost Efficiency**: Only pay for active game sessions (~$0.45/hour)
- **Auto-Scaling**: Arbiter spins up/down servers based on demand
- **Low Latency**: Servers deploy to edge nodes closest to players
- **Zero Maintenance**: No need to manage 24/7 persistent servers
- **Free Tier**: 400 server-hours/month free for development

---

## Package Dependencies

```mermaid
graph TD
    subgraph "Unity Gaming Services (UGS)"
        Core[com.unity.services.core<br/>Unity Services Foundation]
        Auth[com.unity.services.authentication<br/>Player Authentication]
        Multi[com.unity.services.multiplayer<br/>Multiplayer Services]
    end
    
    subgraph "Netcode & Networking"
        NGO[com.unity.netcode.gameobjects<br/>Core Networking Library]
        Tools[com.unity.multiplayer.tools<br/>Network Profiling & Debug]
        DedServer[com.unity.dedicated-server<br/>Unity 6 Server Package]
    end
    
    subgraph "Edgegap Platform"
        Plugin[Edgegap Unity Plugin<br/>GitHub Package]
        Arbiter[Edgegap Arbiter<br/>Built-in Matchmaking]
    end
    
    subgraph "Project Utilities"
        MO[com.midniteoilsoftware.core<br/>EventBus & Singleton]
    end
    
    Core --> Auth
    Core --> Multi
    Multi --> NGO
    NGO --> Tools
    DedServer --> NGO
    
    Plugin --> Core
    Plugin -.Arbiter API.-> Arbiter
    
    style Core fill:#3498db,stroke:#2980b9,stroke-width:2px,color:#fff
    style Auth fill:#16a085,stroke:#117a65,stroke-width:2px,color:#fff
    style NGO fill:#2d89ef,stroke:#1a5490,stroke-width:2px,color:#fff
    style DedServer fill:#e74c3c,stroke:#c0392b,stroke-width:2px,color:#fff
    style Plugin fill:#34495e,stroke:#2c3e50,stroke-width:2px,color:#fff
    style MO fill:#27ae60,stroke:#1e8449,stroke-width:2px,color:#fff
```

**Deprecated Packages** (DO NOT Install):

- ❌ `com.unity.services.multiplay` - Unity Multiplay discontinued March 2026
- ❌ `com.unity.services.lobby` - Not needed (use Edgegap Arbiter)
- ❌ `com.unity.services.relay` - Not needed (dedicated server)

---

**See Also**:

- [multiplayer-guide-start-here.md](multiplayer-guide-start-here.md) - Migration overview
- [multiplayer-migration-dedicated-server.md](multiplayer-migration-dedicated-server.md) - Dedicated server summary
- [multiplayer-migration-dedicated-server-full.md](multiplayer-migration-dedicated-server-full.md) - Full migration guide
- [ecs-and-multiplayer-architecture-analysis.md](ecs-and-multiplayer-architecture-analysis.md) - Architecture decisions
