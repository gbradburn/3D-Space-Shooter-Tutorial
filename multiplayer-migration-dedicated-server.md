# Multiplayer Migration: Dedicated Server Architecture

**Updated**: Aligned with [ecs-and-multiplayer-architecture-analysis](ecs-and-multiplayer-architecture-analysis.md) recommendations

**Vision**: Persistent world, one server, players join anytime  
**Architecture**: Dedicated Server (Unity Multiplay)

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
- Dedicated server (Unity Multiplay)
- Skip Midnite Oil multiplayer package
- Direct server connection
- Authoritative Linux server

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
```

**Full comparison**: See [ecs-and-multiplayer-architecture-analysis](ecs-and-multiplayer-architecture-analysis.md) Section 3

---

## Updated Migration Roadmap

### Phase 0: Foundation - 3 days (HIGH PRIORITY)

✅ **COMPLETED**:
- Player combat, PlayerManager, Object pooling, EventBus
- Architecture analysis complete

⚠️ **TODO**:
- [ ] **Implement Floating Origin** (3 days) - REQUIRED before multiplayer
  - Solves floating-point precision at large distances
  - Essential for persistent world
  - See architecture analysis for implementation

### Phase 1: Multiplayer Setup - 1 week

**Packages to Install**:
- ✅ `com.unity.netcode.gameobjects`
- ✅ `com.unity.services.core`
- ✅ `com.unity.services.authentication`
- ✅ `com.unity.services.multiplay`
- ✅ `com.unity.multiplayer.tools`

**DO NOT Install**:
- ❌ `com.unity.services.lobby` (not needed)
- ❌ `com.unity.services.relay` (not needed)
- ❌ Midnite Oil multiplayer package (wrong architecture)

**Tasks**:
1. Install UGS packages
2. Configure NetworkManager (Unity Transport, 60Hz tick)
3. Link project to Unity Cloud
4. Enable Authentication & Multiplay services
5. Create server build configuration

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
- Create headless Linux build (no rendering)
- Configure dedicated server flags
- Test locally (ParrelSync for multi-client testing)

**Deploy to Unity Multiplay**:
- Upload server build to UGS
- Configure fleet (server instances)
- Set up matchmaking or direct connect
- Test with multiple clients

**Total Timeline**: 5-6 weeks (excluding Floating Origin)

---

## Cost Breakdown

### Unity Multiplay Pricing

**Free Tier**:
- $800 credit for 6 months
- Covers development + initial launch

**Small Server** (2 cores, 4GB RAM):
- Hourly: ~$0.096
- Monthly 24/7: ~$70

**Your $800 credit gets you**:
- ~8,300 hours (~11 months 24/7 on small server)
- Perfect for development and beta testing

**After free tier**:
- Optimize server resources
- Consider auto-scaling (spin down when empty)

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

1. ✅ Review architecture analysis: [ecs-and-multiplayer-architecture-analysis](ecs-and-multiplayer-architecture-analysis.md)
2. ⚠️ **Implement Floating Origin** (3 days) - HIGH PRIORITY
3. Install UGS packages for dedicated server
4. Set up NetworkManager
5. Follow migration steps in main guide: [multiplayer-migration](multiplayer-migration.md)

---

## Reference Documents

- **Architecture Analysis**: [ecs-and-multiplayer-architecture-analysis](ecs-and-multiplayer-architecture-analysis.md)
  - Full P2P vs Dedicated Server comparison
  - Cost analysis and budget planning
  - ECS recommendations
  - Floating Origin implementation

- **Preparation Checklist**: [multiplayer-prep](multiplayer-prep.md)
  - Phase 0 foundation systems (100% complete)
  - Floating Origin requirement
  - Next priority tasks

- **Full Migration Guide**: [multiplayer-migration](multiplayer-migration.md)
  - Component-by-component conversion
  - Code examples
  - Testing strategy

- **Architecture Diagrams**: [multiplayer-architecture-diagrams](multiplayer-architecture-diagrams.md)
  - Visual network topology
  - Data flow diagrams
  - Authority model
