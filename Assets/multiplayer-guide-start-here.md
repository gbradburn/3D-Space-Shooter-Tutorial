# 📌 Multiplayer Migration - Start Here

## ⚠️ Important: Architecture Update (March 2026)

**Unity Multiplay has been discontinued** as of March 2026. The multiplayer strategy has been updated to use **Edgegap** for dedicated server hosting.

The architecture remains **Dedicated Server** (not P2P), but hosting has moved from Unity Multiplay to Edgegap edge network.

---

## Your Vision

> *"Develop this as a hosted application using Unity's multiplayer hosting services and have players just join that single game"*

This is a **Dedicated Server** model, now hosted on **Edgegap** instead of Unity Multiplay.

---

## 📚 Updated Documentation

### 1. **Quick Summary** (Read this first)
[Dedicated Server Architecture Summary](multiplayer-migration-dedicated-server.md)

**What it covers**:

- Why dedicated server vs P2P
- Architecture comparison table
- Migration roadmap (5-6 weeks)
- Updated packages (Edgegap, NOT Unity Multiplay)
- What NOT to use (deprecated packages)

---

### 2. **Full Migration Guide** (Implementation details)
[Complete Dedicated Server Migration Guide](multiplayer-migration-dedicated-server-full.md)

**What it covers**:

- Package installation (Edgegap, Multiplayer Services, Dedicated Server)
- Unity 6 Multiplayer Role feature (automatic asset stripping)
- NetworkManager setup with Unity Transport
- Code examples (NetworkShipController, NetworkDamageHandler)
- Phase-by-phase migration steps
- Server build and Edgegap deployment
- Testing with Multiplayer Play Mode
- Arbiter matchmaking integration

---

### 3. **Architecture Analysis** (Why these decisions)
[ECS & Multiplayer Architecture Analysis](ecs-and-multiplayer-architecture-analysis.md)

**What it covers**:
- P2P vs Dedicated Server comparison
- ECS/DOTS evaluation (asteroids only, hybrid approach)
- Floating Origin requirement (HIGH PRIORITY)
- Cost analysis and budget planning
- Timeline estimates

---

### 4. **Preparation Status** (What's done)
[Multiplayer Preparation Checklist](multiplayer-prep.md)

**What it covers**:
- Phase 0 foundation: 100% complete ✅
- Systems ready: DamageHandler, Shield, PlayerManager, Object Pooling
- Next priority: Floating Origin implementation (3 days)
- Readiness verification checklist

---

## 🚀 Quick Start

### Step 1: Understand the Architecture
Read [Dedicated Server Summary](multiplayer-migration-dedicated-server.md) (5 minutes)

### Step 2: Implement Floating Origin (HIGH PRIORITY)
⚠️ **Required before multiplayer testing**

See [architecture analysis](ecs-and-multiplayer-architecture-analysis.md) Section 2.2 for implementation

**Why**: Solves floating-point precision at large distances (essential for persistent world)

**Time**: 3 days

### Step 3: Follow Migration Roadmap
Use [Full Migration Guide](multiplayer-migration-dedicated-server-full.md) for step-by-step implementation

**Timeline**: 5-6 weeks total

---

## ❌ What Changed from Original Plan

**Before (Unity Multiplay)**:
- Unity Multiplay for server hosting
- `com.unity.services.multiplay` package
- Upload builds to UGS dashboard
- $800 free credit, $0.096/hour after

**Now (Edgegap)**:
- Edgegap edge network for server hosting
- Edgegap Unity Plugin (GitHub)
- Docker containerization via plugin
- 400 free server-hours/month, ~$0.45/hour after
- Arbiter matchmaking (auto-scaling)

---

## 📦 Package Changes

### ✅ Install These

- `com.unity.services.core` - Unity Gaming Services foundation
- `com.unity.services.authentication` - Player authentication
- `com.unity.services.multiplayer` - Multiplayer Services (includes Netcode)
- `com.unity.netcode.gameobjects` - Core networking library  
- `com.unity.dedicated-server` - Unity 6 Dedicated Server package
- `com.unity.multiplayer.tools` - Network debugging and profiling
- Edgegap Unity Plugin (from GitHub) - Server deployment

### ❌ DO NOT Install (Deprecated/Unnecessary)

- `com.unity.services.multiplay` - DEPRECATED (Unity Multiplay discontinued March 2026)
- `com.unity.services.lobby` - Not needed (use Edgegap Arbiter)
- `com.unity.services.relay` - Not needed (dedicated server)
- Midnite Oil multiplayer package - Wrong architecture (P2P)

### ✅ Keep These
- `com.midniteoilsoftware.core` (EventBus, Singleton - useful!)

---

## 💰 Budget

**Edgegap Free Tier**: 400 free server-hours per month (perfect for development)

**After free tier**: ~$0.45/hour for small server (2 vCPU, 2GB RAM)

**Auto-scaling**: Arbiter shuts down servers when empty (pay per session, not 24/7)

---

## Next Action

1. ✅ Read architecture summary
2. ✅ **Implement Floating Origin** (COMPLETED)
3. Install Unity Authentication packages (Phase 0.5)
4. Implement player authentication and connection approval
5. Install Multiplayer Services and Dedicated Server packages
6. Install Edgegap Unity Plugin from GitHub
7. Follow Full Migration Guide Phase 1

---

## Questions?

All decisions are documented in the architecture analysis with full justification, cost comparisons, and technical details.
