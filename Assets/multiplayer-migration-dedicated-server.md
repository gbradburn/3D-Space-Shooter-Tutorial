# Multiplayer Migration: Dedicated Server Architecture

**Updated**: Aligned with architecture analysis recommendations

**Vision**: Persistent world, one server, players join anytime  
**Architecture**: Dedicated Server (Edgegap Hosting)

> ⚠️ **Updated March 2026**: Unity Multiplay discontinued. Now using Edgegap for server hosting.

---

## ⚠️ Important Architecture Decision

Based on comprehensive analysis, this project will use **Dedicated Server** architecture, NOT P2P/Lobby/Relay.

### What Changed

**BEFORE** (original plan):
- P2P with Lobby & Relay
- Midnite Oil boilerplate package
- Session-based lobbies
- Host player runs server

**NOW** (updated plan):
- Dedicated server (Edgegap hosting)
- Skip Midnite Oil multiplayer package
- Direct server connection via Edgegap matchmaking
- Authoritative Linux server with Docker containerization

### Why Dedicated Server?

Your stated vision: *"develop this as a hosted application using Unity's multiplayer hosting services and have players just join that single game"*

This is **literally a dedicated server model**, not P2P.

```
Your Vision              Dedicated Server    P2P/Lobby
─────────────────────────────────────────────────────────
One persistent game      ✅ Yes              ❌ No (sessions)
Everyone joins anytime   ✅ Direct connect   ❌ Need lobby
No lobby/session model   ✅ Optional         ❌ Required
Authoritative server     ✅ Yes              ⚠️ Host-based
Scalable player count    ✅ 50-100+          ❌ 16 max
Cheat prevention         ✅ Server validates ❌ Host can cheat
Full comparison: See  Section 3
```

---

## Updated Migration Roadmap

### Phase 0: Foundation - 3 days (HIGH PRIORITY)

✅ **COMPLETED**:
- Player combat, PlayerManager, Object pooling, EventBus
- Architecture analysis complete

✅ **COMPLETED**:

- [x] **Implement Floating Origin** (3 days) - REQUIRED before multiplayer
  - Solves floating-point precision at large distances
  - Essential for persistent world
  - See architecture analysis for implementation

### Phase 0.5: Authentication Setup - 2 days

**Why**: Secure player identity for persistent world, score tracking, anti-cheat

**Authentication Type**: Username/Password with account registration

**Scene Architecture**: Dedicated Login scene (client-only)

**Packages**:
- `com.unity.services.core`
- `com.unity.services.authentication`

**Tasks**:
1. Install Unity Authentication packages
2. Link project to Unity Cloud (Project Settings → Services)
3. **Create Login scene** (client-only):
   - Create new scene: `Assets/_project/Scenes/Login.unity`
   - Add to Build Settings (index 0 - first scene to load)
   - Configure Build Profile to exclude from Dedicated Server build
4. Create `AuthenticationManager` singleton with username/password sign-in/sign-up
   - Lives in Login scene, persists via DontDestroyOnLoad
5. Create `LoginUIManager` with login and registration UI
   - Canvas with username/password input fields
   - Login and Register buttons
   - Error message display
   - Loading state indicators
6. Implement input validation (client-side)
   - Username: 3-20 chars, alphanumeric
   - Password: 8-30 chars, mixed case + numbers
7. Implement scene transition on successful auth
   - On success: `SceneManager.LoadScene("Main")`
   - On cached session: Auto-load Main scene
8. Create `NetworkAuthValidator` for server-side token validation
   - Attach to NetworkManager in Main scene
   - Implement ConnectionApprovalCallback
9. Create `ClientConnectionManager` to send auth payload
   - Serializes PlayerId, PlayerName, AccessToken to JSON
   - Sets NetworkConfig.ConnectionData before StartClient()
10. Create `NetworkPlayerData` NetworkBehaviour
    - Stores authenticated PlayerId and PlayerName per player
11. **Configure Build Profiles**:
    - Client Build: Includes Login + Main scenes
    - Dedicated Server Build: Main scene only (Login excluded)

**Features**:
- User account registration and login
- Session persistence (auto-login on subsequent launches)
- Password requirements (8+ chars, mixed case, numbers)
- Username requirements (3-20 chars, alphanumeric)
- Client and server-side validation
- **Login scene excluded from server build** (via Build Profiles)

**Result**: Players authenticate in Login scene before Main scene loads, servers skip Login and boot directly to Main

### Phase 1: Multiplayer Setup - 1 week

**Packages to Install**:

- ✅ `com.unity.services.core` - Unity Gaming Services foundation
- ✅ `com.unity.services.authentication` - Player authentication
- ✅ `com.unity.netcode.gameobjects` - Core networking library
- ✅ `com.unity.services.multiplayer` - UGS multiplayer services (includes Netcode)
- ✅ `com.unity.dedicated-server` - Unity 6 dedicated server package (Multiplayer Role feature)
- ✅ `com.unity.multiplayer.tools` - Network debugging and profiling
- ✅ Edgegap Unity Plugin (from GitHub) - Server containerization and deployment

**DO NOT Install** (deprecated/unnecessary):

- ❌ `com.unity.services.multiplay` - DEPRECATED (Unity Multiplay discontinued)
- ❌ `com.unity.services.lobby` - Not needed for dedicated server
- ❌ `com.unity.services.relay` - Not needed for dedicated server
- ❌ Midnite Oil multiplayer package - Wrong architecture (P2P)

**Tasks**:

1. Install Unity Authentication (Phase 0.5 - already done)
2. Install Multiplayer Services and Netcode for GameObjects
3. Install Dedicated Server package (Unity 6)
4. Install Edgegap Unity Plugin from GitHub
5. Configure NetworkManager (Unity Transport, 60Hz tick, Connection Approval enabled)
6. Add `NetworkAuthValidator` component to NetworkManager
7. Set up Multiplayer Role for assets (automatic stripping)
8. Create Edgegap account and API token
9. Configure server build settings (Linux Dedicated Server)

### Phase 2: Core Networking - 2 weeks

**Convert to NetworkBehaviour**:
- `ShipController` → `NetworkShipController`
- `DamageHandler` → `NetworkDamageHandler`
- `PlayerManager` → `NetworkPlayerManager`

**Implement**:
- NetworkTransform for ship sync
- ServerRpc for input (client → server)
- NetworkVariable for health, shield, score
- Server-authoritative weapon firing
- Server-authoritative damage validation

### Phase 3: Server Authority - 1 week

**Server-side systems**:
- Enemy spawning (server only)
- Asteroid spawning (server only)
- Projectile validation
- Floating Origin synchronization
- Cheat prevention

### Phase 4: Deployment - 1 week

**Server build**:

- Use File → Build Settings → Dedicated Server platform
- Configure Multiplayer Role for asset stripping
- Use `#if !UNITY_SERVER` for client-only code
- Test locally with Multiplayer Play Mode (Window → Multiplayer → Multiplayer Play Mode)

**Deploy to Edgegap**:

- Configure Edgegap Unity Plugin with API token
- Build and containerize Linux server (Docker)
- Push container to Edgegap registry
- Configure Arbiter matchmaking
- Deploy to edge nodes closest to players
- Test with multiple clients

**Total Timeline**: 5-6 weeks (excluding Floating Origin)

---

## Cost Breakdown

### Edgegap Pricing

**Free Tier**:

- 400 free server-hours per month
- Perfect for development and testing

**Small Server** (2 vCPU, 2GB RAM):

- Pay-as-you-go: ~$0.45/hour
- Commitment plans available for lower rates

**Cost optimization**:

- Auto-scaling: Servers spin down when empty
- Pay only for active game sessions
- No minimum commitment required
- Edgegap's Arbiter automatically manages server lifecycle

---

## What NOT to Use

### ❌ Midnite Oil Multiplayer Package

**Why skip it**:
- Built for P2P/Lobby/Relay architecture
- Contradicts dedicated server vision
- Provides lobby UI, relay integration, P2P setup (all unnecessary)

**What you DO need from Midnite Oil**:
- ✅ `com.midniteoilsoftware.core` (already have it)
  - EventBus ✓
  - SingletonMonoBehaviour ✓

Everything else for multiplayer comes from Unity packages.

---

## Key Technical Differences

### Dedicated Server vs P2P

| Feature | Dedicated Server | P2P (Midnite Oil) |
|---------|------------------|-------------------|
| Connection | Direct IP/Matchmaking | Lobby + Relay |
| Host | Linux server | Player host |
| Authority | Server | Host player |
| Setup | NetworkManager only | Lobby UI + Relay code |
| Latency | Equal for all | Host=0ms, others lag |
| Persistence | 24/7 server | Ends when host leaves |
| Cheating | Server validates | Host can cheat |
| Complexity | Server build setup | Lobby UI implementation |

---

## Next Steps

1. ✅ Review architecture analysis
2. ✅ **Implement Floating Origin** (COMPLETED)
3. Install Unity Authentication packages (Phase 0.5)
4. Implement player authentication and connection approval
5. Install Multiplayer Services and Dedicated Server packages
6. Install Edgegap Unity Plugin from GitHub
7. Set up NetworkManager with Unity Transport and authentication
8. Configure Multiplayer Role for asset stripping
9. Follow migration steps in full guide

---

## Reference Documents

- **Architecture Analysis**:
  - Full P2P vs Dedicated Server comparison
  - Cost analysis and budget planning
  - ECS recommendations
  - Floating Origin implementation
- **Preparation Checklist**:
  - Phase 0 foundation systems (100% complete)
  - Floating Origin requirement
  - Next priority tasks
- **Full Migration Guide**:
  - Component-by-component conversion
  - Code examples
  - Testing strategy
- **Architecture Diagrams**:
  - Visual network topology
  - Data flow diagrams
  - Authority model

- **Full Migration Guide**: [multiplayer-migration](multiplayer-migration.md)
  - Component-by-component conversion
  - Code examples
  - Testing strategy

- **Architecture Diagrams**: [multiplayer-architecture-diagrams](multiplayer-architecture-diagrams.md)
  - Visual network topology
  - Data flow diagrams
  - Authority model
