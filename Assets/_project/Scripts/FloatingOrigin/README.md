# Floating Origin System

**Registration-Based Architecture** ✅

## Overview

The Floating Origin System prevents floating-point precision issues when objects travel far from the world origin (0,0,0). This refactored implementation uses a registration-based architecture where objects explicitly opt-in to shifting, eliminating the need for expensive scene traversal.

## Architecture

### Core Components

1. **FloatingOriginManager** - Singleton that coordinates all shifts
2. **ShiftableFloatingOrigin** - Base component for objects that should shift
3. **FloatingOriginReference** - Component for reference objects (players)
4. **Specialized Shiftables** - Components for specific object types (Rigidbody, ParticleSystem, etc.)

### How It Works

```
FloatingOriginManager
├── Tracks registered reference objects (players)
├── Calculates centroid of all references
├── Triggers shift when centroid exceeds threshold
└── Invokes OnShift() on all registered shiftables

Objects self-register when enabled:
- ShiftableFloatingOrigin → OnEnable() → Register
- FloatingOriginReference → OnEnable() → Register
```

## Quick Start

### Player Ship Setup

1. Add `FloatingOriginReference` component to player root
2. Add `ShiftableRigidbody` component to player root

```
PlayerShip (root)
├── FloatingOriginReference (marks as tracking point)
└── ShiftableRigidbody (handles physics shifting)
```

### World Objects Setup

**Objects with Rigidbody:**
- Add `ShiftableRigidbody`

**Static/Kinematic objects:**
- Add `ShiftableFloatingOrigin`

**Objects with ParticleSystem:**
- Add `ShiftableParticleSystem`

**Objects with TrailRenderer:**
- Add `ShiftableTrailRenderer`

## Component Reference

### ShiftableFloatingOrigin

Base component for any object that should shift. Automatically registers/unregisters with the manager.

**Usage:**
```csharp
// Attach to GameObject - it will auto-register
// Override OnShift() for custom behavior
public override void OnShift(Vector3 offset)
{
    base.OnShift(offset);
    // Custom shift logic here
}
```

### FloatingOriginReference

Marks an object as a reference point for calculating when to shift. In single-player, typically attached to the player ship. In multiplayer, attached to each player.

**Properties:**
- `Weight` - Used in multiplayer to weight the centroid calculation (default: 1.0)

### ShiftableRigidbody

Specialized component for objects with Rigidbody. Properly shifts physics state.

**Usage:**
```csharp
// Attach to any GameObject with Rigidbody
// Automatically handles position shifting via Rigidbody.position
```

### ShiftableParticleSystem

Specialized component for objects with ParticleSystem. Shifts active particles.

**Usage:**
```csharp
// Attach to any GameObject with ParticleSystem
// Shifts both transform and active particle positions
```

### ShiftableTrailRenderer

Specialized component for objects with TrailRenderer. Clears trail on shift to avoid artifacts.

**Usage:**
```csharp
// Attach to any GameObject with TrailRenderer
// Shifts transform and clears trail
```

## Multiplayer Support

The system is multiplayer-ready with centroid-based shifting:

**Single-player:**
- One player = shifts based on player position

**Multiplayer:**
- Multiple players = shifts based on weighted average (centroid)
- Players on opposite sides naturally balance out
- Enable via: `FloatingOriginManager.Instance.SetMultiplayerMode(true);`

**Example:**
```
Player 1 at (10000, 0, 0)
Player 2 at (-10000, 0, 0)
Centroid = (0, 0, 0) → No shift needed!

Player 1 at (8000, 0, 0)
Player 2 at (7000, 0, 0)
Centroid = (7500, 0, 0) → Shift when > threshold
```

## API Reference

### FloatingOriginManager

**Public Properties:**
- `AbsoluteWorldOffset` - Total accumulated offset in double precision
- `TotalShiftCount` - Number of shifts performed
- `CentroidDistance` - Current distance from origin
- `RegisteredShiftablesCount` - Number of registered shiftable objects
- `RegisteredReferencesCount` - Number of registered reference objects

**Public Methods:**
- `RegisterShiftable(ShiftableFloatingOrigin)` - Register object for shifting
- `UnregisterShiftable(ShiftableFloatingOrigin)` - Unregister object
- `RegisterReference(FloatingOriginReference)` - Register reference object
- `UnregisterReference(FloatingOriginReference)` - Unregister reference
- `PerformOriginShift()` - Manually trigger a shift
- `SetMultiplayerMode(bool)` - Enable/disable multiplayer mode
- `GetAbsolutePosition(Vector3)` - Convert local to absolute position
- `GetLocalPosition(Vector3Double)` - Convert absolute to local position

## Benefits Over Scene Traversal

✅ **No scene traversal** - Only registered objects are shifted  
✅ **Explicit opt-in** - Objects choose to participate  
✅ **Automatic cleanup** - Objects unregister when destroyed  
✅ **Multiplayer-ready** - Easy centroid calculation  
✅ **Extensible** - Easy to create specialized behaviors  
✅ **Performant** - O(n) where n = registered objects  
✅ **Testable** - Clear lifecycle and dependencies

## Debug GUI

The system includes a debug GUI showing:
- Mode (Single-Player / Multiplayer)
- Registered references count
- Registered shiftables count
- Centroid distance / threshold
- Total shifts performed
- Absolute world offset
- Manual "Force Shift Now" button

Toggle via `Show Debug GUI` in inspector.

## Creating Custom Shiftable Components

Extend `ShiftableFloatingOrigin` for custom shift behavior:

```csharp
using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.FloatingOrigin
{
    public class ShiftableCustom : ShiftableFloatingOrigin
    {
        public override void OnShift(Vector3 offset)
        {
            base.OnShift(offset);  // Shifts transform
            
            // Add custom shift logic here
        }
    }
}
```

## Migration Guide

If migrating from the old scene-traversal system:

1. Update `FloatingOriginManager` (already done)
2. Add components to prefabs:
   - Player: `FloatingOriginReference` + `ShiftableRigidbody`
   - Asteroids: `ShiftableRigidbody`
   - Static objects: `ShiftableFloatingOrigin`
3. Remove any manual shift handling code
4. Test thoroughly in play mode

## Performance Notes

- Registration/unregistration is O(1) with Contains check
- Shift operation is O(n) where n = registered objects
- Centroid calculation is O(n) where n = reference objects
- Much faster than traversing entire scene hierarchy
- No GC allocations during shift (uses pre-allocated lists)

1. **FloatingOriginManager** is already added to the Main scene as a persistent singleton
2. The manager automatically detects the player when PlayerSpawnedEvent is raised
3. When the player exceeds 5000 units from origin, the entire world shifts
4. No additional setup required!

## Files

- **Vector3Double.cs** - 64-bit double-precision vector for absolute position tracking
- **FloatingOriginManager.cs** - Main singleton manager handling origin shifts
- **FloatingOriginEvents.cs** - EventBus event definitions

## How It Works

```
Player moves 5000+ units from origin
    ↓
FloatingOriginManager detects threshold exceeded
    ↓
Shifts ALL GameObjects by -player.position
    ↓
Player now at origin, world shifted accordingly
    ↓
Raises OriginShiftedEvent via EventBus
    ↓
Tracks absolute position using Vector3Double
```

## Debug GUI

When playing, you'll see a debug overlay showing:
- Distance from origin
- Shift threshold
- Total number of shifts performed
- Absolute world offset
- Current reference object

## Testing

Use the `FloatingOriginTest` component to verify the system:

1. Add `FloatingOriginTest` to your player or a test object
2. Enable "Auto Move" or use "Jump to Distance" button
3. Watch the debug GUI confirm shifts occur
4. Verify no visual jitter during movement

## API

### FloatingOriginManager

```csharp
// Get the singleton instance
var manager = FloatingOriginManager.Instance;

// Get absolute world position (accounts for all shifts)
Vector3Double absPos = manager.GetAbsolutePosition(transform.position);

// Convert absolute position back to local
Vector3 localPos = manager.GetLocalPosition(absolutePosition);

// Manually trigger a shift (normally automatic)
manager.PerformOriginShift();

// Change reference object (defaults to player)
manager.SetReferenceObject(myTransform);
```

### Vector3Double

```csharp
// Create from Vector3
Vector3Double pos = new Vector3Double(myVector3);

// Convert back to Vector3
Vector3 localPos = doublePos.ToVector3();

// Math operations
Vector3Double result = posA + posB;
Vector3Double scaled = pos * 2.0;
double distance = pos.magnitude;
```

## Settings

Configure in the FloatingOriginManager inspector:

- **Shift Threshold** (default: 5000) - Distance before triggering shift
- **Enable Auto Shift** (default: true) - Automatically shift when threshold exceeded
- **Reference Object** - What to keep at origin (auto-finds Player tag)
- **Show Debug GUI** - Display debug overlay

## Next Phases

- **Phase 2**: Physics Integration (Rigidbody handling)
- **Phase 3**: Visual Effects (Particles, Trails)
- **Phase 4**: Multiplayer Support (Centroid-based)
- **Phase 5**: Testing & Optimization

## EventBus Integration

Subscribe to origin shifts:

```csharp
using MidniteOilSoftware.Core;
using MidniteOilSoftware.SpaceShooter.Events;

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
    Debug.Log($"Origin shifted by {evt.Offset}");
    // Handle shift in your systems
}
```

## Performance

- Shifts occur infrequently (every 5000+ units of travel)
- Single-frame operation (no multi-frame processing)
- Hierarchical transform shifting (optimized)
- Minimal runtime overhead when not shifting

## Troubleshooting

**Q: Objects are jittering far from origin**  
A: Ensure FloatingOriginManager is active and has a valid reference object

**Q: Physics objects behaving strangely after shift**  
A: Phase 2 (Physics Integration) not yet implemented - coming next!

**Q: Particle systems break during shift**  
A: Phase 3 (Visual Effects) not yet implemented - coming soon!

**Q: Want to use custom reference point**  
A: Call `FloatingOriginManager.Instance.SetReferenceObject(yourTransform)`
