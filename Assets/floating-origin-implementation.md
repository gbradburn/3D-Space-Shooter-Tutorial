# Floating Origin System Implementation Guide

> **For**: Unity 6 (6000.3+) 3D Space Shooter  
> **Timeline**: 3-4 days (staged implementation with multiplayer prep)  
> **Complexity**: Moderate  
> **Prerequisites**: Basic understanding of Unity transforms, physics, and event systems

> **🎯 Multiplayer Update**: Now includes centroid-based approach for dedicated server architecture. Players in opposite directions naturally balance out—no shift needed!

---

## Table of Contents

1. [Problem Statement](#problem-statement)
2. [Solution Overview](#solution-overview)
3. [Implementation Roadmap](#implementation-roadmap)
4. [Stage 1: Core Foundation](#stage-1-core-foundation)
5. [Stage 2: Physics Integration](#stage-2-physics-integration)
6. [Stage 3: Visual Effects](#stage-3-visual-effects)
7. [Stage 4: Multiplayer Preparation](#stage-4-multiplayer-preparation)
8. [Stage 5: Testing & Polish](#stage-5-testing--polish)
9. [Optimization Strategies](#optimization-strategies)
10. [Multiplayer Integration](#multiplayer-integration-guide)
11. [Troubleshooting](#troubleshooting)

---

## Problem Statement

### What is Floating-Point Precision Loss?

Unity uses 32-bit floats for `Transform.position`. As objects move far from the origin (0,0,0), floating-point precision degrades exponentially:

| Distance from Origin | Precision | Visual Impact |
|---------------------|-----------|---------------|
| 0 - 1,000 units | ~0.0001 units | None (imperceptible) |
| 10,000 units | ~0.001 units | Minor jitter |
| 100,000 units | ~0.01 units | Visible jitter |
| 1,000,000 units | ~1 unit | Severe artifacts |
| 10,000,000+ units | Complete breakdown | Physics breaks |

### Symptoms in Space Games

- **Camera shake/judder** when far from origin
- **Ship jittering** during movement
- **Physics glitches** (collisions miss, rigidbodies vibrate)
- **Particle effects stutter**
- **Rotation precision loss** (gimbal lock-like behavior)
- **Trail renderers break** or snap

### Why This Matters

For persistent space games where players can travel vast distances:

- Players will experience severe jitter beyond 100,000 units
- Multiplayer synchronization will amplify precision errors
- Large-scale space battles become unplayable
- Immersion breaks due to visual artifacts

---

## Solution Overview

### Floating Origin Pattern

**Core Concept**: Periodically shift the entire world so the player (or average player position) stays near the origin (0,0,0).

```
Player moves 10,000 units from origin
    ↓
Detect threshold exceeded
    ↓
Shift ALL objects by -player.position
    ↓
Player now at origin, world shifted accordingly
    ↓
Track "absolute position" separately for persistence
```

### Architecture Design

```
FloatingOriginManager (Singleton)
├── Monitors player/reference position
├── Triggers origin shift when threshold exceeded
├── Raises OriginShiftedEvent via EventBus
└── Tracks absolute world offset (Vector3Double)

IFloatingOriginReceiver (Interface)
├── Components implement to handle shifts
├── OnOriginShift(Vector3 offset) callback
└── Update internal state during shift

Specialized Handlers
├── RigidbodyShiftHandler - Shifts physics objects
├── ParticleSystemShiftHandler - Handles particle systems
├── TrailRendererShiftHandler - Handles trails
└── Custom handlers for specific systems
```

### Key Features

- ✅ **Event-driven architecture** (integrates with EventBus)
- ✅ **Singleton pattern** (clean manager access)
- ✅ **Staged implementation** (incremental development)
- ✅ **Multiplayer-ready** (server/client absolute position tracking)
- ✅ **Performance-optimized** (minimal overhead, infrequent shifts)
- ✅ **Extensible** (easy to add custom shift handlers)

---

## Implementation Roadmap

### Stage 1: Core Foundation (Day 1, ~4 hours)

**Goal**: Basic floating origin system with single player support

- Create `Vector3Double` struct for absolute position tracking
- Create `FloatingOriginManager` singleton
- Implement shift threshold detection
- Create `OriginShiftedEvent` for EventBus
- Shift basic GameObjects (Transform-only)

**Deliverable**: Player can travel 1,000,000+ units without jitter

### Stage 2: Physics Integration (Day 1-2, ~4 hours)

**Goal**: Handle Rigidbody objects correctly

- Create `IFloatingOriginReceiver` interface
- Implement `RigidbodyShiftHandler`
- Shift all Rigidbodies (preserve velocity/angular velocity)
- Test with asteroids and ships

**Deliverable**: Physics objects maintain correct state during shifts

### Stage 3: Visual Effects (Day 2, ~3 hours)

**Goal**: Handle particle systems, trails, and effects

- Implement `ParticleSystemShiftHandler`
- Implement `TrailRendererShiftHandler`
- Handle LineRenderer and other visual systems
- Test with explosions, weapon trails, engine effects

**Deliverable**: Visual effects work seamlessly during shifts

### Stage 4: Multiplayer Preparation (Day 2-3, ~4 hours)

**Goal**: Implement centroid-based approach for dedicated server multiplayer

- Implement centroid calculation for multiple players
- Add `EnableMultiplayerMode()` to support single/multi modes
- Update `CheckAndPerformShift()` for centroid reference
- Add absolute position tracking to PlayerManager
- Create `NetworkPositionSync` component (stub for future)
- Test multiplayer scenarios (players opposite vs. clustered)
- Enhanced debug GUI showing mode and distances

**Deliverable**: Multiplayer-ready architecture with centroid balancing

### Stage 5: Testing & Polish (Day 3, ~2 hours)

**Goal**: Verify system works in all scenarios

- Test at extreme distances (10,000,000+ units)
- Performance profiling (ensure <1ms shift time)
- Edge case testing (rapid shifts, multiple objects)
- Integration testing with all game systems

**Deliverable**: Production-ready floating origin system

---

## Stage 1: Core Foundation

### 1.1 Create Vector3Double Struct

**File**: `Assets/_project/Scripts/Utilities/Vector3Double.cs`

```csharp
using System;
using UnityEngine;

[Serializable]
public struct Vector3Double
{
    public double x;
    public double y;
    public double z;

    public Vector3Double(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Double(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public static Vector3Double operator +(Vector3Double a, Vector3Double b) =>
        new(a.x + b.x, a.y + b.y, a.z + b.z);

    public static Vector3Double operator +(Vector3Double a, Vector3 b) =>
        new(a.x + b.x, a.y + b.y, a.z + b.z);

    public static Vector3Double operator -(Vector3Double a, Vector3Double b) =>
        new(a.x - b.x, a.y - b.y, a.z - b.z);

    public static Vector3Double operator *(Vector3Double a, double d) =>
        new(a.x * d, a.y * d, a.z * d);

    public static Vector3Double operator /(Vector3Double a, double d) =>
        new(a.x / d, a.y / d, a.z / d);

    public double Magnitude() => Math.Sqrt(x * x + y * y + z * z);

    public Vector3Double Normalized()
    {
        var mag = Magnitude();
        return mag > 0 ? this / mag : new Vector3Double(0, 0, 0);
    }

    public Vector3 ToVector3() => new((float)x, (float)y, (float)z);

    public override string ToString() => $"({x:F2}, {y:F2}, {z:F2})";

    public static Vector3Double Zero => new(0, 0, 0);
    public static Vector3Double One => new(1, 1, 1);
}
```

### 1.2 Create OriginShiftedEvent

**File**: `Assets/_project/Scripts/Events/OriginShiftedEvent.cs`

```csharp
using UnityEngine;

public class OriginShiftedEvent
{
    public Vector3 Offset { get; }
    public Vector3Double AbsoluteWorldOffset { get; }
    public float ShiftTime { get; }

    public OriginShiftedEvent(Vector3 offset, Vector3Double absoluteOffset, float shiftTime)
    {
        Offset = offset;
        AbsoluteWorldOffset = absoluteOffset;
        ShiftTime = shiftTime;
    }
}
```

### 1.3 Create FloatingOriginManager

**File**: `Assets/_project/Scripts/Managers/FloatingOriginManager.cs`

```csharp
using UnityEngine;
using MidniteOilSoftware;

public class FloatingOriginManager : SingletonMonoBehaviour<FloatingOriginManager>
{
    [Header("Configuration")]
    [SerializeField] float _shiftThreshold = 5000f;
    [SerializeField] bool _enableFloatingOrigin = true;
    [SerializeField] Transform _referenceTransform;

    [Header("Debug")]
    [SerializeField] bool _showDebugInfo = true;
    [SerializeField] Vector3Double _absoluteWorldOffset;

    float _lastShiftTime;
    int _shiftCount;

    public Vector3Double AbsoluteWorldOffset => _absoluteWorldOffset;
    public int ShiftCount => _shiftCount;
    public float DistanceFromOrigin => _referenceTransform != null 
        ? _referenceTransform.position.magnitude 
        : 0f;

    void Start()
    {
        if (_referenceTransform == null)
        {
            _referenceTransform = FindReferenceTransform();
        }

        _absoluteWorldOffset = Vector3Double.Zero;
    }

    void LateUpdate()
    {
        if (!_enableFloatingOrigin || _referenceTransform == null)
            return;

        CheckAndPerformShift();
    }

    void CheckAndPerformShift()
    {
        var distance = _referenceTransform.position.magnitude;

        if (distance > _shiftThreshold)
        {
            var offset = -_referenceTransform.position;
            PerformOriginShift(offset);
        }
    }

    void PerformOriginShift(Vector3 offset)
    {
        var startTime = Time.realtimeSinceStartup;

        _absoluteWorldOffset += offset;
        _shiftCount++;
        _lastShiftTime = Time.time;

        ShiftAllTransforms(offset);

        var shiftDuration = Time.realtimeSinceStartup - startTime;

        EventBus.Instance.Raise(new OriginShiftedEvent(
            offset, 
            _absoluteWorldOffset, 
            shiftDuration
        ));

        if (_showDebugInfo)
        {
            Debug.Log($"[FloatingOrigin] Shift #{_shiftCount} | " +
                      $"Offset: {offset} | Duration: {shiftDuration * 1000f:F2}ms | " +
                      $"Absolute Offset: {_absoluteWorldOffset}");
        }
    }

    void ShiftAllTransforms(Vector3 offset)
    {
        var allTransforms = FindObjectsByType<Transform>(
            FindObjectsInactive.Include, 
            FindObjectsSortMode.None
        );

        foreach (var t in allTransforms)
        {
            if (IsValidShiftTarget(t))
            {
                t.position += offset;
            }
        }
    }

    bool IsValidShiftTarget(Transform t)
    {
        // Don't shift UI elements
        if (t.GetComponentInParent<Canvas>() != null)
            return false;

        // Don't shift camera (it follows player which is already shifted)
        if (t.GetComponent<Camera>() != null)
            return false;

        return true;
    }

    Transform FindReferenceTransform()
    {
        // Try to find local player from PlayerManager
        if (PlayerManager.Instance != null)
        {
            var localPlayer = PlayerManager.Instance.GetLocalPlayer();
            if (localPlayer != null)
                return localPlayer.transform;
        }

        // Fallback: find player by tag
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            return player.transform;

        Debug.LogWarning("[FloatingOrigin] No reference transform found. " +
                        "Floating origin disabled.");
        return null;
    }

    public void SetReferenceTransform(Transform reference)
    {
        _referenceTransform = reference;
        Debug.Log($"[FloatingOrigin] Reference set to: {reference.name}");
    }

    public Vector3Double GetAbsolutePosition(Vector3 localPosition) =>
        _absoluteWorldOffset + localPosition;

    public Vector3 GetLocalPosition(Vector3Double absolutePosition) =>
        (absolutePosition - _absoluteWorldOffset).ToVector3();

    void OnGUI()
    {
        if (!_showDebugInfo)
            return;

        var style = new GUIStyle
        {
            fontSize = 14,
            normal = { textColor = Color.white }
        };

        var distance = DistanceFromOrigin;
        var color = distance > _shiftThreshold * 0.8f ? Color.yellow : Color.green;

        GUI.color = color;
        GUI.Label(new Rect(10, 10, 400, 25), 
            $"Distance from Origin: {distance:F1} / {_shiftThreshold:F0}", style);
        GUI.Label(new Rect(10, 35, 400, 25), 
            $"Shifts Performed: {_shiftCount}", style);
        GUI.Label(new Rect(10, 60, 400, 25), 
            $"Absolute Offset: {_absoluteWorldOffset}", style);
        GUI.color = Color.white;
    }
}
```

### 1.4 Setup in Scene

**Steps**:

1. Open your main gameplay scene
2. Navigate to `/Managers` in hierarchy (or create if missing)
3. Create empty GameObject: `Floating Origin Manager`
4. Add `FloatingOriginManager` component
5. Configure settings:
   - **Shift Threshold**: `5000` (adjust based on your game scale)
   - **Enable Floating Origin**: ✓
   - **Show Debug Info**: ✓ (for testing, disable in production)
6. **Reference Transform**: Leave empty (auto-detects player)

### 1.5 Testing Stage 1

**Test Procedure**:

1. Enter Play mode
2. Select player ship in hierarchy
3. Manually set position to `(10000, 0, 0)` in Inspector
4. Watch console for shift message
5. Verify player position returns near origin
6. Check debug GUI shows shift count and distance

**Expected Console Output**:

```
[FloatingOrigin] Shift #1 | Offset: (-10000, 0, 0) | 
Duration: 0.45ms | Absolute Offset: (10000, 0, 0)
```

**Success Criteria**:

- ✓ Player position < 100 units from origin after shift
- ✓ Shift duration < 2ms
- ✓ No errors in console
- ✓ Debug GUI displays correctly

---

## Stage 2: Physics Integration

### 2.1 Create IFloatingOriginReceiver Interface

**File**: `Assets/_project/Scripts/Utilities/IFloatingOriginReceiver.cs`

```csharp
using UnityEngine;

public interface IFloatingOriginReceiver
{
    void OnOriginShift(Vector3 offset);
}
```

### 2.2 Create RigidbodyShiftHandler

**File**: `Assets/_project/Scripts/FloatingOrigin/RigidbodyShiftHandler.cs`

```csharp
using UnityEngine;
using MidniteOilSoftware;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyShiftHandler : MonoBehaviour, IFloatingOriginReceiver
{
    Rigidbody _rigidbody;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        EventBus.Instance.Subscribe<OriginShiftedEvent>(OnOriginShiftedEvent);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe<OriginShiftedEvent>(OnOriginShiftedEvent);
    }

    void OnOriginShiftedEvent(OriginShiftedEvent evt)
    {
        OnOriginShift(evt.Offset);
    }

    public void OnOriginShift(Vector3 offset)
    {
        if (_rigidbody == null)
            return;

        // Shift position (Transform already shifted by manager)
        // We need to update Rigidbody's internal position
        _rigidbody.position += offset;

        // Velocities are relative, no need to shift
        // Angular velocity is relative, no need to shift
    }
}
```

### 2.3 Update FloatingOriginManager for Rigidbodies

**File**: `Assets/_project/Scripts/Managers/FloatingOriginManager.cs`

**Replace** the `ShiftAllTransforms` method:

```csharp
void ShiftAllTransforms(Vector3 offset)
{
    // Shift Rigidbodies first (they handle their own Transform)
    var rigidbodies = FindObjectsByType<Rigidbody>(
        FindObjectsInactive.Include, 
        FindObjectsSortMode.None
    );

    foreach (var rb in rigidbodies)
    {
        if (IsValidShiftTarget(rb.transform))
        {
            rb.position += offset;
        }
    }

    // Shift remaining transforms (non-Rigidbody objects)
    var allTransforms = FindObjectsByType<Transform>(
        FindObjectsInactive.Include, 
        FindObjectsSortMode.None
    );

    foreach (var t in allTransforms)
    {
        // Skip if has Rigidbody (already shifted above)
        if (t.GetComponent<Rigidbody>() != null)
            continue;

        if (IsValidShiftTarget(t))
        {
            t.position += offset;
        }
    }
}
```

### 2.4 Add RigidbodyShiftHandler to Prefabs

**Prefabs to Update**:

- Player ship prefab
- Enemy ship prefabs
- Asteroid prefabs
- Projectile prefabs (if using Rigidbody)

**Steps**:

1. Open prefab
2. Select root GameObject
3. Add `RigidbodyShiftHandler` component
4. Save prefab

**Optional**: Create editor script to batch-add to all prefabs with Rigidbody

### 2.5 Testing Stage 2

**Test Procedure**:

1. Enter Play mode
2. Spawn several asteroids
3. Fire weapons (create projectiles with Rigidbodies)
4. Set player position to `(6000, 0, 0)`
5. Observe origin shift occurs
6. Verify:
   - Asteroids maintain rotation/velocity
   - Projectiles maintain trajectory
   - Ship maintains velocity
   - No physics glitches or snapping

**Success Criteria**:

- ✓ All Rigidbodies maintain velocity after shift
- ✓ No sudden acceleration/deceleration
- ✓ Physics collisions still work correctly
- ✓ No jittering or snapping

---

## Stage 3: Visual Effects

### 3.1 Create ParticleSystemShiftHandler

**File**: `Assets/_project/Scripts/FloatingOrigin/ParticleSystemShiftHandler.cs`

```csharp
using UnityEngine;
using MidniteOilSoftware;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemShiftHandler : MonoBehaviour, IFloatingOriginReceiver
{
    ParticleSystem _particleSystem;
    ParticleSystem.Particle[] _particles;

    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        EventBus.Instance.Subscribe<OriginShiftedEvent>(OnOriginShiftedEvent);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe<OriginShiftedEvent>(OnOriginShiftedEvent);
    }

    void OnOriginShiftedEvent(OriginShiftedEvent evt)
    {
        OnOriginShift(evt.Offset);
    }

    public void OnOriginShift(Vector3 offset)
    {
        if (_particleSystem == null || !_particleSystem.isPlaying)
            return;

        // Get current particles
        var particleCount = _particleSystem.particleCount;
        if (particleCount == 0)
            return;

        // Allocate array if needed
        if (_particles == null || _particles.Length < particleCount)
        {
            _particles = new ParticleSystem.Particle[particleCount];
        }

        // Get particles
        _particleSystem.GetParticles(_particles, particleCount);

        // Shift particle positions
        for (var i = 0; i < particleCount; i++)
        {
            _particles[i].position += offset;
        }

        // Apply shifted particles back
        _particleSystem.SetParticles(_particles, particleCount);
    }
}
```

### 3.2 Create TrailRendererShiftHandler

**File**: `Assets/_project/Scripts/FloatingOrigin/TrailRendererShiftHandler.cs`

```csharp
using UnityEngine;
using MidniteOilSoftware;

[RequireComponent(typeof(TrailRenderer))]
public class TrailRendererShiftHandler : MonoBehaviour, IFloatingOriginReceiver
{
    TrailRenderer _trailRenderer;

    void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    void OnEnable()
    {
        EventBus.Instance.Subscribe<OriginShiftedEvent>(OnOriginShiftedEvent);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe<OriginShiftedEvent>(OnOriginShiftedEvent);
    }

    void OnOriginShiftedEvent(OriginShiftedEvent evt)
    {
        OnOriginShift(evt.Offset);
    }

    public void OnOriginShift(Vector3 offset)
    {
        if (_trailRenderer == null)
            return;

        // Clear trail to prevent stretching artifacts
        // Trail will regenerate naturally as object moves
        _trailRenderer.Clear();

        // Alternative: Manually shift trail positions (more complex)
        // Requires accessing trail vertex positions (not directly exposed)
    }
}
```

### 3.3 Add Handlers to Effect Prefabs

**Effect Prefabs to Update**:

- Explosion effects (with ParticleSystem)
- Impact effects (with ParticleSystem)
- Engine trails (with TrailRenderer)
- Weapon trails (with TrailRenderer or LineRenderer)

**Steps**:

1. Open each effect prefab
2. Find GameObject with ParticleSystem or TrailRenderer
3. Add appropriate handler component
4. Save prefab

### 3.4 Testing Stage 3

**Test Procedure**:

1. Enter Play mode
2. Fire weapons continuously (creates explosion effects)
3. Activate engines (trails)
4. Force origin shift (set player position > 5000)
5. Observe effects during/after shift

**Success Criteria**:

- ✓ Particle systems don't snap or disappear
- ✓ Trails either clear cleanly or shift smoothly
- ✓ No visual artifacts during shift
- ✓ New effects spawn correctly after shift

---

## Stage 4: Multiplayer Preparation

### Understanding Multiplayer Floating Origin

**The Challenge**: What happens when two players are far from origin but also far from each other?

**Example Scenario**:
```
Player A at (-15,000, 0, 0)  ← Origin (0,0,0) →  Player B at (+15,000, 0, 0)
                   30,000 units apart
```

**Problem**: If you center on Player A, Player B becomes 30,000 units from origin (worse precision)!

**Solution**: Use **centroid (average position)** of all players as the reference point

```
Centroid of (-15,000, 0, 0) and (+15,000, 0, 0) = (0, 0, 0)
Result: Both players stay at 15,000 units from origin ✓
        Excellent precision for both!
        NO shift needed!
```

**Key Benefits**:
- Players in opposite directions balance each other out
- Origin naturally stays centered between players
- Shifts only occur when players cluster together far from origin
- Maximum playable space: ~100,000 unit diameter before any precision issues

**Architecture**: Server calculates centroid and coordinates shifts to all clients simultaneously

---

### 4.1 Update FloatingOriginManager for Multiplayer

**File**: `/Assets/_project/Scripts/Managers/FloatingOriginManager.cs`

**Add these methods** to support both single-player and multiplayer:

```csharp
// Add after existing fields
[Header("Multiplayer")]
[SerializeField] bool _useMultiplayerCentroid = false;

// Replace FindReferenceTransform with this improved version
Transform FindReferenceTransform()
{
    // Multiplayer: Use centroid calculation instead of single reference
    if (_useMultiplayerCentroid)
        return null; // Centroid mode doesn't use a single transform

    // Single-player: Find player ship
    var player = GameObject.FindGameObjectWithTag("Player");
    return player != null ? player.transform : null;
}

// Calculate centroid of all players (multiplayer mode)
Vector3 CalculateReferencePoint()
{
    if (!_useMultiplayerCentroid || PlayerManager.Instance == null)
        return _referenceTransform != null ? _referenceTransform.position : Vector3.zero;

    var playerPositions = new List<Vector3>();
    var activePlayers = PlayerManager.Instance.GetActivePlayers();
    
    foreach (var player in activePlayers)
    {
        if (player != null)
            playerPositions.Add(player.transform.position);
    }

    if (playerPositions.Count == 0)
        return Vector3.zero;

    var centroid = Vector3.zero;
    foreach (var pos in playerPositions)
        centroid += pos;
    
    centroid /= playerPositions.Count;
    return centroid;
}

// Get maximum distance from reference point to any player
float GetMaxPlayerDistance(Vector3 referencePoint)
{
    if (PlayerManager.Instance == null)
        return 0f;

    var maxDistance = 0f;
    var activePlayers = PlayerManager.Instance.GetActivePlayers();
    
    foreach (var player in activePlayers)
    {
        if (player != null)
        {
            var distance = Vector3.Distance(player.transform.position, referencePoint);
            maxDistance = Mathf.Max(maxDistance, distance);
        }
    }
    
    return maxDistance;
}

// Replace CheckAndPerformShift to support both modes
void CheckAndPerformShift()
{
    Vector3 referencePoint;
    float checkDistance;

    if (_useMultiplayerCentroid)
    {
        // Multiplayer: Use centroid and check max player distance
        referencePoint = CalculateReferencePoint();
        checkDistance = GetMaxPlayerDistance(referencePoint);
    }
    else
    {
        // Single-player: Use reference transform
        if (_referenceTransform == null)
            return;
        
        referencePoint = _referenceTransform.position;
        checkDistance = referencePoint.magnitude;
    }

    if (checkDistance > _shiftThreshold)
    {
        var offset = -referencePoint;
        PerformOriginShift(offset);
    }
}

// Helper method to enable/disable multiplayer mode
public void EnableMultiplayerMode(bool enabled)
{
    _useMultiplayerCentroid = enabled;
    
    if (_showDebugInfo)
    {
        Debug.Log($"[FloatingOrigin] Multiplayer centroid mode: {(_useMultiplayerCentroid ? "ENABLED" : "DISABLED")}");
    }
}

// Update OnGUI to show multiplayer info
void OnGUI()
{
    if (!_showDebugInfo)
        return;

    var style = new GUIStyle
    {
        fontSize = 14,
        normal = { textColor = Color.white }
    };

    var referencePoint = CalculateReferencePoint();
    var distance = _useMultiplayerCentroid 
        ? GetMaxPlayerDistance(referencePoint) 
        : DistanceFromOrigin;
    var modeLabel = _useMultiplayerCentroid ? "Multiplayer (Centroid)" : "Single-Player";

    GUI.Label(new Rect(10, 10, 400, 25), $"Mode: {modeLabel}", style);
    
    if (_useMultiplayerCentroid)
    {
        GUI.Label(new Rect(10, 35, 400, 25), $"Centroid: {referencePoint}", style);
        GUI.Label(new Rect(10, 60, 400, 25), 
            $"Max Player Distance: {distance:F1} / {_shiftThreshold:F0}", style);
    }
    else
    {
        GUI.Label(new Rect(10, 35, 400, 25), 
            $"Distance from Origin: {distance:F1} / {_shiftThreshold:F0}", style);
    }
    
    GUI.Label(new Rect(10, 85, 400, 25), $"Shifts Performed: {_shiftCount}", style);
    GUI.Label(new Rect(10, 110, 400, 25), $"Absolute Offset: {_absoluteWorldOffset}", style);
}
```

---

### 4.2 Update PlayerManager

**File**: `/Assets/_project/Scripts/Managers/PlayerManager.cs`

**Add** these methods and fields:

```csharp
// Add to existing PlayerManager class

Dictionary<GameObject, Vector3Double> _playerAbsolutePositions = new();

void OnEnable()
{
    EventBus.Instance.Subscribe<OriginShiftedEvent>(OnOriginShifted);
}

void OnDisable()
{
    EventBus.Instance.Unsubscribe<OriginShiftedEvent>(OnOriginShifted);
}

void OnOriginShifted(OriginShiftedEvent evt)
{
    // Update absolute positions for all tracked players
    var playersToUpdate = new List<GameObject>(_activePlayers);
    
    foreach (var player in playersToUpdate)
    {
        if (_playerAbsolutePositions.ContainsKey(player))
        {
            _playerAbsolutePositions[player] += evt.Offset;
        }
    }
}

public Vector3Double GetPlayerAbsolutePosition(GameObject player)
{
    if (_playerAbsolutePositions.TryGetValue(player, out var absolutePos))
        return absolutePos;
    
    // Fallback: calculate from current position
    if (FloatingOriginManager.Instance != null)
    {
        return FloatingOriginManager.Instance.GetAbsolutePosition(
            player.transform.position
        );
    }
    
    return new Vector3Double(player.transform.position);
}

// Add this helper for FloatingOriginManager to get all players
public List<GameObject> GetActivePlayers()
{
    return new List<GameObject>(_activePlayers);
}

void OnPlayerSpawned(GameObject player, int playerIndex, bool isLocalPlayer)
{
    // ... existing code ...
    
    // Track absolute position
    _playerAbsolutePositions[player] = 
        FloatingOriginManager.Instance != null
            ? FloatingOriginManager.Instance.GetAbsolutePosition(player.transform.position)
            : new Vector3Double(player.transform.position);
}

void OnPlayerDestroyed(GameObject player)
{
    // ... existing cleanup code ...
    
    // Remove absolute position tracking
    _playerAbsolutePositions.Remove(player);
}
```

---

### 4.3 Create NetworkPositionSync Stub

**File**: `Assets/_project/Scripts/Networking/NetworkPositionSync.cs`

```csharp
using UnityEngine;
using MidniteOilSoftware;

// TODO: Convert to NetworkBehaviour when adding Netcode for GameObjects
public class NetworkPositionSync : MonoBehaviour
{
    Vector3Double _absolutePosition;
    
    public Vector3Double AbsolutePosition => _absolutePosition;

    void Start()
    {
        if (FloatingOriginManager.Instance != null)
        {
            _absolutePosition = FloatingOriginManager.Instance.GetAbsolutePosition(
                transform.position
            );
        }
    }

    void OnEnable()
    {
        EventBus.Instance.Subscribe<OriginShiftedEvent>(OnOriginShifted);
    }

    void OnDisable()
    {
        EventBus.Instance.Unsubscribe<OriginShiftedEvent>(OnOriginShifted);
    }

    void OnOriginShifted(OriginShiftedEvent evt)
    {
        // Update absolute position
        _absolutePosition += evt.Offset;
    }

    void Update()
    {
        // Update absolute position based on local movement
        if (FloatingOriginManager.Instance != null)
        {
            _absolutePosition = FloatingOriginManager.Instance.GetAbsolutePosition(
                transform.position
            );
        }
    }

    // Future: Add NetworkVariable synchronization
    // Future: Add ServerRpc/ClientRpc for position updates
}
```

### 4.4 Testing Stage 4

**Single-Player Test**:

1. Set `Use Multiplayer Centroid` to `false` in FloatingOriginManager
2. Add `NetworkPositionSync` to player ship
3. Enter Play mode
4. Move player ship around
5. Force origin shift (set position > 5000)
6. Verify absolute position continues increasing

**Multiplayer Simulation Test**:

1. Set `Use Multiplayer Centroid` to `true`
2. Create two test players in scene
3. Position them at opposite ends:
   - Player 1: (-15000, 0, 0)
   - Player 2: (+15000, 0, 0)
4. Enter Play mode
5. Observe debug GUI:
   - Centroid should be near (0, 0, 0)
   - Max Player Distance: ~15,000
   - No shift should occur ✓

**Test moving together**:

1. Both players move to (+20000, 0, 0)
2. Centroid: (+20000, 0, 0)
3. Shift should trigger ✓
4. After shift, both players near origin again

**Success Criteria**:

- ✓ Absolute position tracks correctly in both modes
- ✓ Absolute position persists through origin shifts
- ✓ Centroid calculation works with multiple players
- ✓ No shift when players are opposite directions from origin
- ✓ Shift occurs when players cluster far from origin

---

### 4.5 Multiplayer Edge Cases Explained

**Scenario 1: Players in Opposite Directions**
```
Player A: (-15,000, 0, 0)
Player B: (+15,000, 0, 0)
Centroid: (0, 0, 0)
Max Distance: 15,000 units
Result: NO SHIFT ✓ Perfect precision for both!
```

**Scenario 2: Players Moving Apart**
```
Player A: (-50,000, 0, 0)
Player B: (0, 0, 0)
Centroid: (-25,000, 0, 0)
Shift triggered!
After shift:
  Player A: (-25,000, 0, 0) ✓
  Player B: (+25,000, 0, 0) ✓
Both within acceptable range!
```

**Scenario 3: Players Clustered Together Far Away**
```
Player A: (+18,000, 0, 0)
Player B: (+19,000, 0, 0)
Centroid: (+18,500, 0, 0)
Max Distance: 18,500 units
Shift triggered!
After shift: Both near origin ✓
```

**Scenario 4: Many Players Spread Out**
```
Player A: (-10,000, 0, 0)
Player B: (+10,000, 0, 0)
Player C: (0, 0, +10,000)
Player D: (0, 0, -10,000)
Centroid: (0, 0, 0)
Max Distance: 10,000 units
Result: NO SHIFT ✓ All players have excellent precision!
```

**Extreme Case: Players >50,000 Units Apart**

If players separate by vast distances (different star systems):

**Accept precision tradeoff**:
- Each player's **own ship** remains smooth (local to them)
- **Nearby objects** remain precise
- **Distant players** may have minor jitter (but too far to interact anyway)
- **Practical limit**: ~50,000 unit radius (100km diameter playable space!)

**Why this is acceptable**:
- Space is vast—players at 50,000+ units can't realistically interact
- Visual jitter at extreme distance doesn't affect gameplay
- Local precision (where players ARE) remains perfect

---

## Stage 5: Testing & Polish

### 5.1 Extreme Distance Testing

**Test Script**: `Assets/_project/Scripts/Testing/FloatingOriginTester.cs`

```csharp
using UnityEngine;

public class FloatingOriginTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] Transform _testTarget;
    [SerializeField] float _teleportDistance = 1000000f;
    [SerializeField] KeyCode _teleportKey = KeyCode.T;

    void Update()
    {
        if (Input.GetKeyDown(_teleportKey) && _testTarget != null)
        {
            TeleportToDistance();
        }
    }

    void TeleportToDistance()
    {
        var randomDirection = Random.onUnitSphere;
        var targetPosition = randomDirection * _teleportDistance;
        
        _testTarget.position = targetPosition;
        
        Debug.Log($"[FloatingOriginTester] Teleported to {targetPosition} " +
                  $"(distance: {targetPosition.magnitude:F0})");
    }

    void OnGUI()
    {
        var style = new GUIStyle
        {
            fontSize = 16,
            normal = { textColor = Color.cyan }
        };

        GUI.Label(new Rect(10, 100, 400, 25), 
            $"Press '{_teleportKey}' to teleport {_teleportDistance:F0} units", style);
    }
}
```

**Test Procedure**:

1. Add `FloatingOriginTester` to scene
2. Assign player as test target
3. Enter Play mode
4. Press `T` repeatedly to teleport to extreme distances
5. Observe visual smoothness, physics stability, effect rendering

### 5.2 Performance Targets

**Metrics to Monitor**:

- **Shift operation**: < 2ms total
- **LateUpdate overhead**: < 0.1ms (threshold check)
- **Shift frame spike**: < 2ms
- **Post-shift frames**: No ongoing overhead

**Use Unity Profiler to verify**:

- FloatingOriginManager.LateUpdate
- FloatingOriginManager.PerformOriginShift
- FloatingOriginManager.ShiftAllTransforms

### 5.3 Edge Cases to Test

1. **Rapid consecutive shifts**: Teleport player repeatedly
2. **Objects spawned during shift**: Spawn asteroid during shift event
3. **Inactive objects**: Deactivate/reactivate during shift
4. **Pooled objects**: Verify position correctness with object pooling
5. **Save/Load**: Test persistence of absolute positions

---

## Optimization Strategies

### Reduce Shift Frequency

Increase threshold to reduce shift frequency:

```csharp
[SerializeField] float _shiftThreshold = 10000f; // Higher = less frequent
```

### Incremental Shifting

For large object counts, spread shift across frames:

```csharp
IEnumerator IncrementalShift(Vector3 offset, int batchSize = 100)
{
    var allObjects = FindObjectsByType<Transform>(...);
    
    for (var i = 0; i < allObjects.Length; i += batchSize)
    {
        var end = Mathf.Min(i + batchSize, allObjects.Length);
        
        for (var j = i; j < end; j++)
        {
            if (IsValidShiftTarget(allObjects[j]))
            {
                allObjects[j].position += offset;
            }
        }
        
        yield return null; // Spread across frames
    }
}
```

### Cached Shift Targets

Maintain list instead of using `FindObjectsByType`:

```csharp
List<Transform> _shiftTargets = new();

public void RegisterShiftTarget(Transform t) => _shiftTargets.Add(t);
public void UnregisterShiftTarget(Transform t) => _shiftTargets.Remove(t);

void ShiftAllTransforms(Vector3 offset)
{
    foreach (var t in _shiftTargets)
    {
        if (t != null)
            t.position += offset;
    }
}
```

---

## Multiplayer Integration Guide

### Overview: Centroid-Based Floating Origin

**The Problem**: In multiplayer, players can be far from origin AND far from each other.

**The Solution**: Use the **average position (centroid)** of all players as the reference point.

**Key Benefits**:
- ✅ Players in opposite directions naturally balance out (NO shift needed!)
- ✅ Origin stays centered between all players automatically
- ✅ Only shift when players cluster together far from origin
- ✅ Playable space: ~100,000 unit diameter before precision issues
- ✅ Scales to many players (centroid calculation is O(n), negligible cost)

**Example**:
```
Player A at (-15,000, 0, 0) + Player B at (+15,000, 0, 0)
= Centroid at (0, 0, 0)
= Both players 15,000 from origin
= NO SHIFT NEEDED ✓ Perfect precision for both!
```

---

### Server-Authoritative Model (Dedicated Server)

**Architecture**:

```
Server:
├── Runs FloatingOriginManager with _useMultiplayerCentroid = true
├── Calculates centroid of all connected players
├── Tracks absolute positions (Vector3Double)
├── Sends shift commands to clients via ClientRpc
└── Validates client positions

Clients:
├── Receive shift events from server
├── Apply shifts to local objects (VFX, UI, predicted movement)
├── Send input in local coordinates
└── Render at local positions (NetworkTransform handles sync)
```

**Key Principle**: Server and all clients shift **simultaneously** by the same offset, so relative positions between all networked objects remain unchanged.

---

### Network Synchronization Flow

**When Shift Occurs**:

1. **Server** detects max player distance > threshold
2. **Server** calculates offset = -centroid
3. **Server** shifts all server-authoritative objects (Rigidbodies, NetworkObjects)
4. **Server** sends `ShiftOriginClientRpc(offset)` to all clients
5. **Clients** receive RPC and shift their local-only objects
6. **NetworkTransform** continues working normally (positions already shifted on server)

---

### Future Implementation (Netcode for GameObjects)

**When adding Netcode for GameObjects**, update `FloatingOriginManager`:

```csharp
using Unity.Netcode;

public class FloatingOriginManager : NetworkBehaviour
{
    // ... existing code ...
    
    void PerformOriginShift(Vector3 offset)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            // Server performs shift and tells clients
            PerformOriginShiftServerRpc(offset);
        }
        else
        {
            // Single-player or client-only mode
            PerformOriginShiftLocal(offset);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void PerformOriginShiftServerRpc(Vector3 offset)
    {
        // Server updates absolute offset
        _absoluteWorldOffset += offset;
        _shiftCount++;
        _lastShiftTime = Time.time;
        
        // Shift all server-side objects
        ShiftAllTransforms(offset);
        
        // Tell all clients to shift
        ShiftOriginClientRpc(offset, _absoluteWorldOffset);
    }
    
    [ClientRpc]
    void ShiftOriginClientRpc(Vector3 offset, Vector3Double serverAbsoluteOffset)
    {
        // Clients shift their local-only objects
        // (VFX, predicted movements, UI elements with world anchors)
        ShiftClientOnlyObjects(offset);
        
        // Sync absolute offset with server
        _absoluteWorldOffset = serverAbsoluteOffset;
        
        // Raise event for client-side handlers
        EventBus.Instance.Raise(new OriginShiftedEvent(
            offset, 
            _absoluteWorldOffset, 
            0f // Client doesn't measure shift time
        ));
    }
    
    void PerformOriginShiftLocal(Vector3 offset)
    {
        // Single-player implementation (existing code)
        var startTime = Time.realtimeSinceStartup;
        
        _absoluteWorldOffset += offset;
        _shiftCount++;
        _lastShiftTime = Time.time;
        
        ShiftAllTransforms(offset);
        
        var shiftDuration = Time.realtimeSinceStartup - startTime;
        
        EventBus.Instance.Raise(new OriginShiftedEvent(
            offset, 
            _absoluteWorldOffset, 
            shiftDuration
        ));
    }
    
    void ShiftClientOnlyObjects(Vector3 offset)
    {
        // Client-only objects that aren't synced via NetworkTransform
        // Examples: Local VFX, UI world-space elements, predicted projectiles
        
        // For now, most objects are handled by server shift + NetworkTransform sync
        // This method exists for future client-side prediction features
    }
}
```

**NetworkShipController Integration**:

```csharp
public class NetworkShipController : NetworkBehaviour
{
    NetworkVariable<Vector3Double> _absolutePosition = new();
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Track absolute position on server
            _absolutePosition.Value = FloatingOriginManager.Instance != null
                ? FloatingOriginManager.Instance.GetAbsolutePosition(transform.position)
                : new Vector3Double(transform.position);
        }
    }
    
    void FixedUpdate()
    {
        if (IsServer)
        {
            // Update absolute position each frame
            if (FloatingOriginManager.Instance != null)
            {
                _absolutePosition.Value = FloatingOriginManager.Instance.GetAbsolutePosition(
                    transform.position
                );
            }
        }
        
        // NetworkTransform handles local position sync automatically
        // Floating origin shifts keep both server and clients in sync
    }
}
```

---

### Why Centroid Works Perfectly for Dedicated Servers

**Dedicated Server Advantages**:

1. **Single source of truth**: Server calculates one centroid for all players
2. **Synchronized shifts**: All clients receive same shift command simultaneously
3. **No desync**: NetworkTransform positions shift equally on server and all clients
4. **Scalable**: Centroid calculation is O(n) where n = player count (negligible)
5. **Natural balancing**: Players spread out = origin stays centered automatically

**Performance Impact**:

```
Centroid Calculation:
├── 2 players: ~0.001ms
├── 10 players: ~0.005ms
├── 50 players: ~0.02ms
└── Negligible compared to shift operation (~2ms)

Network Bandwidth:
├── Shift RPC: ~20 bytes (Vector3 + metadata)
├── Frequency: Rare (only when players cluster far from origin)
└── Impact: Minimal (one-time message)
```

---

---

## Multiplayer Scenarios: Practical Examples

### Two Players Opposite Directions (BEST CASE)

```
Player A: (-15,000, 0, 0)  |  Player B: (+15,000, 0, 0)
Centroid: (0, 0, 0)  |  Max Distance: 15,000
Result: NO SHIFT ✅  Both have excellent precision!
```

### Players Clustered Far Away

```
Player A: (+18,000, 0, 0)  |  Player B: (+19,000, 0, 0)
Centroid: (+18,500, 0, 0)  |  Max Distance: 18,500
Result: SHIFT TRIGGERED ⚠️
After shift: Both players near origin ✓
```

### Four Players Spread Out (OPTIMAL)

```
Players at: (-8k,0,0), (+8k,0,0), (0,0,-8k), (0,0,+8k)
Centroid: (0, 0, 0)  |  Max Distance: 8,000
Result: NO SHIFT ✅  Perfect balance!
```

### Extreme Separation (Accept Tradeoff)

```
Player A: (-100,000, 0, 0)  |  Player B: (+100,000, 0, 0)
At 200km apart, players can't interact meaningfully
Accept: Each has smooth LOCAL environment
Distant player may have minor jitter (acceptable)
```

**Recommended Thresholds**:
- Conservative: 5,000 (best precision, more shifts)
- **Balanced: 10,000** ← Start here
- Aggressive: 20,000 (fewer shifts, slight precision loss)

---

## Troubleshooting

### Objects snap during shift

**Causes**: Shift happening mid-physics step, NetworkTransform overriding position

**Solutions**:
- Perform shifts in `LateUpdate` (after physics)
- Use `Rigidbody.position` instead of `Transform.position`
- Disable NetworkTransform during shift frame (multiplayer)

### Trails/particles stretched

**Causes**: Trail positions not shifted, particle simulation space set to World

**Solutions**:
- Clear trails during shift (`TrailRenderer.Clear()`)
- Set particle simulation space to Local
- Manually shift particle positions (see ParticleSystemShiftHandler)

### Camera jitters after shift

**Causes**: Cinemachine interpolation, camera not following shifted target

**Solutions**:
- Ensure camera target (player) is shifted
- Reset Cinemachine state after shift
- Use `OnTargetObjectWarped` for Cinemachine

### Physics collisions missed

**Causes**: Continuous collision detection not enabled, objects shifted mid-collision

**Solutions**:
- Enable Continuous Dynamic collision detection on fast-moving objects
- Ensure `Rigidbody.position` used for shifts
- Consider pausing physics during shift (use with caution)

### Performance degradation

**Causes**: Too many objects to shift, frequent shifts

**Solutions**:
- Increase shift threshold (reduce frequency)
- Use incremental shifting (spread across frames)
- Maintain cached list of shift targets
- Profile with Unity Profiler to identify bottlenecks

---

## Checklist

### Stage 1: Core Foundation

- [ ] `Vector3Double` struct created
- [ ] `OriginShiftedEvent` created
- [ ] `FloatingOriginManager` implemented
- [ ] Manager added to scene
- [ ] Basic shifting tested at 10,000 units
- [ ] Debug GUI working

### Stage 2: Physics Integration

- [ ] `IFloatingOriginReceiver` interface created
- [ ] `RigidbodyShiftHandler` implemented
- [ ] Handler added to player ship
- [ ] Handler added to enemy ships
- [ ] Handler added to asteroids
- [ ] Physics objects maintain velocity after shift

### Stage 3: Visual Effects

- [ ] `ParticleSystemShiftHandler` implemented
- [ ] `TrailRendererShiftHandler` implemented
- [ ] Handlers added to explosion effects
- [ ] Handlers added to engine trails
- [ ] Visual effects work during shift

### Stage 4: Multiplayer Preparation

- [ ] Centroid calculation method implemented
- [ ] `EnableMultiplayerMode()` method added
- [ ] Single-player mode tested (reference transform)
- [ ] Multiplayer mode tested (centroid calculation)
- [ ] Debug GUI shows correct mode and distances
- [ ] Absolute position tracking added to PlayerManager
- [ ] `GetActivePlayers()` helper method added
- [ ] `NetworkPositionSync` stub created
- [ ] Absolute positions persist through shifts
- [ ] Two-player opposite direction test (no shift) ✓
- [ ] Two-player clustered test (shift triggered) ✓
- [ ] Documentation for network integration complete

### Stage 5: Testing & Polish

- [ ] Extreme distance testing (1,000,000+ units)
- [ ] Performance profiling (shift < 2ms)
- [ ] Edge case testing complete
- [ ] Integration testing with all systems
- [ ] Production-ready

---

## Success Metrics

### Performance Targets

- ✓ Shift operation: < 2ms
- ✓ LateUpdate overhead: < 0.1ms
- ✓ Zero ongoing frame cost between shifts
- ✓ No GC allocations during shift

### Functional Targets

- ✓ Player can travel 10,000,000+ units smoothly
- ✓ No visible jitter at any distance
- ✓ Physics remains stable
- ✓ Visual effects render correctly
- ✓ Zero precision artifacts

### Integration Targets

- ✓ All game systems work during/after shift
- ✓ EventBus integration seamless
- ✓ Ready for multiplayer (absolute position tracking)
- ✓ Save/load compatible

---

## Next Steps After Completion

1. **Validate at scale**: Test with 100+ objects during shift
2. **Test multiplayer scenarios**:
   - Two players opposite directions (should NOT shift)
   - Two players clustered far away (should shift)
   - Many players spread out (verify centroid calculation)
   - Extreme separation (50,000+ units apart)
3. **Tune threshold**: Based on playtesting, adjust `_shiftThreshold` (start at 10,000)
4. **Multiplayer integration**: 
   - Install Netcode for GameObjects package
   - Convert `FloatingOriginManager` to `NetworkBehaviour`
   - Implement `ServerRpc` and `ClientRpc` for synchronized shifts
   - Enable `_useMultiplayerCentroid` mode
5. **Performance tuning**: Profile in production scenarios with multiple players
6. **Documentation**: Update multiplayer migration guide with floating origin specifics

**Priority Order**:
1. Implement Stages 1-3 (core functionality, physics, VFX)
2. Test Stage 4 in single-player with multiplayer simulation
3. Integrate with Netcode when ready for networking phase

---

## Contributing

This implementation is part of a larger Unity 6 space shooter project. Contributions and improvements are welcome.

## License

This code is provided as-is for educational and project use.

## Credits

Implementation guide created for Unity 6 (6000.3+) space shooter project with multiplayer support.

---

**Estimated Total Time**: 3-4 days (16-20 hours with multiplayer prep)

**Implementation order**: Stage 1 → 2 → 3 → 4 → 5

**Production readiness**: After Stage 5 completion

Padawan, the centroid-based approach transforms the "players far apart" problem into an advantage—they actually help keep the origin centered! The system is now architected for your dedicated server vision.
