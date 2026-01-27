# Floating Origin System

**Phase 1 - Core Foundation** ✅

## Overview

The Floating Origin System prevents floating-point precision issues when objects travel far from the world origin (0,0,0). This is critical for space games where players can travel vast distances.

## Quick Start

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
