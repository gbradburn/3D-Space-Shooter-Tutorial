# Floating Origin System Architecture

> **Project**: 3D Space Shooter Tutorial  
> **Unity Version**: 6000.3  
> **Status**: Phase 1 Complete, Refactoring to Registration-Based Architecture

---

## Table of Contents

1. [Overview](#overview)
2. [The Problem](#the-problem)
3. [The Solution](#the-solution)
4. [System Architecture](#system-architecture)
5. [Component Roles](#component-roles)
6. [Shift Flow](#shift-flow)
7. [Single-Player vs Multiplayer](#single-player-vs-multiplayer)
8. [Data Flow](#data-flow)
9. [Registration Lifecycle](#registration-lifecycle)

---

## Overview

The Floating Origin System prevents floating-point precision degradation by periodically shifting the entire game world to keep the player (or average player position in multiplayer) near the world origin (0, 0, 0).

### Key Principles

- **Everything moves together** - Player, enemies, asteroids, effects all shift by the same offset
- **Spatial relationships preserved** - Relative positions between objects remain unchanged
- **Registration-based** - Objects explicitly opt-in to participate in shifts
- **Event-driven** - Integrates with EventBus for loose coupling
- **Multiplayer-ready** - Supports centroid-based shifting for multiple players

---

## The Problem

### Floating-Point Precision Loss

```mermaid
graph LR
    A[Position: 0 units] -->|Precision: 0.0001| B[Good]
    C[Position: 1,000 units] -->|Precision: 0.0001| D[Good]
    E[Position: 10,000 units] -->|Precision: 0.001| F[Minor Jitter]
    G[Position: 100,000 units] -->|Precision: 0.01| H[Visible Jitter]
    I[Position: 1,000,000+ units] -->|Precision: 1+ unit| J[Physics Breaks]
    
    style B fill:#0f0
    style D fill:#0f0
    style F fill:#ff0
    style H fill:#f80
    style J fill:#f00
```

### Symptoms

- Camera shake/judder when far from origin
- Ship jittering during movement
- Physics glitches (collisions miss, rigidbodies vibrate)
- Particle effects stutter
- Trail renderers break or snap

---

## The Solution

### Core Concept

When the reference point (player or centroid of players) exceeds a threshold distance from origin, shift the entire world back.

```mermaid
sequenceDiagram
    participant P as Player Ship
    participant M as FloatingOriginManager
    participant W as World Objects
    
    Note over P: Travels to (5000, 0, 0)
    P->>M: Position exceeds threshold
    M->>M: Calculate offset: -(5000, 0, 0)
    M->>P: Shift position by offset → (0, 0, 0)
    M->>W: Shift all objects by same offset
    Note over P,W: All spatial relationships preserved
    M->>M: Track absolute offset using Vector3Double
```

### Before and After Shift

```mermaid
graph TB
    subgraph "Before Shift"
        A1[Player: 5000, 0, 0]
        B1[Asteroid: 6000, 0, 0]
        C1[Enemy: 4000, 0, 0]
        O1[Origin: 0, 0, 0]
    end
    
    subgraph "After Shift by -5000, 0, 0"
        A2[Player: 0, 0, 0]
        B2[Asteroid: 1000, 0, 0]
        C2[Enemy: -1000, 0, 0]
        O2[Origin: 0, 0, 0]
    end
    
    A1 -.->|Shift| A2
    B1 -.->|Shift| B2
    C1 -.->|Shift| C2
    
    style A1 fill:#ff0
    style A2 fill:#0f0
    style O2 fill:#0f0
```

---

## System Architecture

### Component Hierarchy

```mermaid
classDiagram
    class FloatingOriginManager {
        -List~ShiftableFloatingOrigin~ registeredShiftables
        -List~FloatingOriginReference~ registeredReferences
        -Vector3Double absoluteWorldOffset
        -float shiftThreshold
        +RegisterShiftable(shiftable)
        +UnregisterShiftable(shiftable)
        +RegisterReference(reference)
        +UnregisterReference(reference)
        +PerformOriginShift()
        -CalculateCentroid() Vector3
        -ShiftAllRegistered(offset)
    }
    
    class Vector3Double {
        +double x
        +double y
        +double z
        +ToVector3() Vector3
        +magnitude double
    }
    
    class FloatingOriginReference {
        +Vector3 position
        +float weight
        +OnEnable()
        +OnDisable()
    }
    
    class ShiftableFloatingOrigin {
        <<abstract>>
        +OnEnable()
        +OnDisable()
        +OnShift(offset) virtual
    }
    
    class ShiftableRigidbody {
        -Rigidbody rb
        +OnShift(offset) override
    }
    
    class ShiftableParticleSystem {
        -ParticleSystem ps
        +OnShift(offset) override
    }
    
    class ShiftableTrailRenderer {
        -TrailRenderer trail
        +OnShift(offset) override
    }
    
    FloatingOriginManager --> Vector3Double : tracks offset
    FloatingOriginManager --> FloatingOriginReference : calculates centroid from
    FloatingOriginManager --> ShiftableFloatingOrigin : shifts all
    ShiftableFloatingOrigin <|-- ShiftableRigidbody
    ShiftableFloatingOrigin <|-- ShiftableParticleSystem
    ShiftableFloatingOrigin <|-- ShiftableTrailRenderer
```

---

## Component Roles

### FloatingOriginManager (Singleton)

**Responsibility**: Orchestrates the entire floating origin system

- Maintains registration lists for references and shiftables
- Calculates centroid from all reference positions
- Detects when shift threshold is exceeded
- Performs the actual shift operation
- Raises `OriginShiftedEvent` via EventBus
- Tracks absolute world offset using `Vector3Double`

```mermaid
graph TD
    FOM[FloatingOriginManager]
    FOM --> R1[Manages References]
    FOM --> R2[Manages Shiftables]
    FOM --> R3[Calculates Centroid]
    FOM --> R4[Detects Threshold]
    FOM --> R5[Performs Shifts]
    FOM --> R6[Tracks Absolute Position]
    
    R1 --> R1A[List of FloatingOriginReference]
    R2 --> R2A[List of ShiftableFloatingOrigin]
    R3 --> R3A[Average position of all references]
    R4 --> R4A[centroid.magnitude > threshold]
    R5 --> R5A[Call OnShift on all shiftables]
    R6 --> R6A[Vector3Double absoluteWorldOffset]
```

### FloatingOriginReference

**Responsibility**: "I influence WHEN to shift"

- Registers with manager on `OnEnable()`
- Unregisters on `OnDisable()`
- Provides position for centroid calculation
- Optional weight property for weighted averaging (multiplayer)

**Used By**:
- Player ship (single-player and multiplayer)
- AI-controlled reference points (optional)

### ShiftableFloatingOrigin (Base Class)

**Responsibility**: "I need to BE shifted when shift happens"

- Abstract base class for all shiftable objects
- Registers with manager on `OnEnable()`
- Unregisters on `OnDisable()`
- Virtual `OnShift(Vector3 offset)` method with default implementation
- Default behavior: `transform.position += offset`

### ShiftableRigidbody

**Responsibility**: "Shift my physics-simulated body correctly"

- Extends `ShiftableFloatingOrigin`
- Overrides `OnShift()` to use `Rigidbody.position` instead of `transform.position`
- Preserves velocity and angular velocity during shift
- Prevents physics glitches

**Used By**:
- Player ship (has Rigidbody)
- Asteroids (have Rigidbody)
- Projectiles (have Rigidbody)
- Enemies (have Rigidbody)

### ShiftableParticleSystem

**Responsibility**: "Shift my particles in world space"

- Extends `ShiftableFloatingOrigin`
- Shifts both transform and particle positions
- Handles world-space particle systems correctly

**Used By**:
- Explosions
- Engine trails
- Weapon effects

### ShiftableTrailRenderer

**Responsibility**: "Clear my trail during shift to prevent snapping"

- Extends `ShiftableFloatingOrigin`
- Shifts transform
- Clears trail to prevent visual artifacts

**Used By**:
- Projectile trails
- Ship engine trails
- Movement indicators

---

## Shift Flow

### Complete Shift Sequence

```mermaid
sequenceDiagram
    participant R as FloatingOriginReference(s)
    participant M as FloatingOriginManager
    participant S as ShiftableFloatingOrigin(s)
    participant E as EventBus
    
    loop Every LateUpdate()
        R->>M: Provide position
        M->>M: Calculate centroid
        M->>M: Check centroid.magnitude > threshold
    end
    
    alt Threshold Exceeded
        M->>M: Calculate offset = -centroid
        M->>M: Update absoluteWorldOffset
        
        loop For each registered shiftable
            M->>S: OnShift(offset)
            S->>S: Update position/physics/particles
        end
        
        M->>E: Raise OriginShiftedEvent(offset, centroid)
        
        Note over M: Shift complete, system stabilized
    end
```

### Why The Player Needs Both Components

```mermaid
graph TB
    subgraph "Player Ship GameObject"
        PC[Player Ship]
        PC --> FOR[FloatingOriginReference]
        PC --> SRB[ShiftableRigidbody]
    end
    
    FOR --> |"Tells manager: Use my position<br/>to calculate WHEN to shift"| CALC[Centroid Calculation]
    SRB --> |"Tells manager: Shift my Rigidbody<br/>WHEN the shift happens"| SHIFT[Shift Execution]
    
    CALC --> DEC{Distance > Threshold?}
    DEC -->|Yes| SHIFT
    SHIFT --> UPD[Player.Rigidbody.position += offset]
    
    style FOR fill:#ff0
    style SRB fill:#0ff
    style UPD fill:#0f0
```

**Key Point**: The player doesn't stay at origin while the world moves. Instead, the player moves WITH the world, and the shift is calculated to make the player END UP near the origin.

---

## Single-Player vs Multiplayer

### Single-Player Architecture

```mermaid
graph TD
    subgraph "Single-Player Mode"
        P[Player Ship]
        P --> FOR1[FloatingOriginReference]
        P --> SRB1[ShiftableRigidbody]
        
        FOR1 --> M1[FloatingOriginManager]
        M1 --> |"Centroid = Player position"| CALC1[Calculate Shift]
        
        CALC1 --> |"Offset = -Player.position"| SHIFT1[Shift All Objects]
        SHIFT1 --> P
        SHIFT1 --> E1[Enemies]
        SHIFT1 --> A1[Asteroids]
        SHIFT1 --> FX1[Effects]
    end
    
    style FOR1 fill:#ff0
    style CALC1 fill:#0f0
```

**Single Reference**: Only the player ship has `FloatingOriginReference`  
**Centroid**: Equals the player's position  
**Result**: Player stays near origin

### Multiplayer Architecture

```mermaid
graph TD
    subgraph "Multiplayer Mode"
        P1[Player 1 Ship]
        P2[Player 2 Ship]
        P3[Player 3 Ship]
        
        P1 --> FOR1[FloatingOriginReference]
        P2 --> FOR2[FloatingOriginReference]
        P3 --> FOR3[FloatingOriginReference]
        
        P1 --> SRB1[ShiftableRigidbody]
        P2 --> SRB2[ShiftableRigidbody]
        P3 --> SRB3[ShiftableRigidbody]
        
        FOR1 --> M2[FloatingOriginManager]
        FOR2 --> M2
        FOR3 --> M2
        
        M2 --> |"Centroid = Average(P1, P2, P3)"| CALC2[Calculate Shift]
        
        CALC2 --> |"Offset = -Centroid"| SHIFT2[Shift All Objects]
        SHIFT2 --> P1
        SHIFT2 --> P2
        SHIFT2 --> P3
        SHIFT2 --> E2[Enemies]
        SHIFT2 --> A2[Asteroids]
    end
    
    style FOR1 fill:#ff0
    style FOR2 fill:#ff0
    style FOR3 fill:#ff0
    style CALC2 fill:#0f0
```

**Multiple References**: All player ships have `FloatingOriginReference`  
**Centroid**: Average position of all players  
**Result**: Centroid stays near origin

**Natural Balancing**: Players in opposite directions naturally balance out:
- Player at (10000, 0, 0)
- Player at (-10000, 0, 0)
- Centroid: (0, 0, 0) → No shift needed!

---

## Data Flow

### Registration Flow

```mermaid
flowchart TB
    START([GameObject Spawned])
    START --> ENABLE[OnEnable Called]
    
    ENABLE --> CHECK{Has FloatingOrigin<br/>Component?}
    
    CHECK -->|FloatingOriginReference| REG_REF[Register with Manager<br/>as Reference]
    CHECK -->|ShiftableFloatingOrigin| REG_SHIFT[Register with Manager<br/>as Shiftable]
    CHECK -->|No| SKIP[Skip Registration]
    
    REG_REF --> ADDED_REF[Added to registeredReferences list]
    REG_SHIFT --> ADDED_SHIFT[Added to registeredShiftables list]
    
    ADDED_REF --> ACTIVE[Active in System]
    ADDED_SHIFT --> ACTIVE
    SKIP --> END([End])
    
    ACTIVE --> DISABLE{OnDisable Called?}
    DISABLE -->|Yes| UNREG[Unregister from Manager]
    DISABLE -->|No| ACTIVE
    
    UNREG --> REMOVED[Removed from lists]
    REMOVED --> END
    
    style REG_REF fill:#ff0
    style REG_SHIFT fill:#0ff
    style ACTIVE fill:#0f0
```

### Shift Calculation Flow

```mermaid
flowchart TB
    START([LateUpdate])
    START --> HASREF{Has registered<br/>references?}
    
    HASREF -->|No| SKIP[Skip - No references]
    HASREF -->|Yes| COUNT[Count references: N]
    
    COUNT --> SINGLE{N == 1?}
    
    SINGLE -->|Yes| CENT1["Centroid = reference[0].position"]
    SINGLE -->|No| CENT2[Centroid = Average of all references]
    
    CENT1 --> DIST[Calculate distance = centroid.magnitude]
    CENT2 --> DIST
    
    DIST --> THRESHOLD{distance > threshold?}
    
    THRESHOLD -->|No| SKIP
    THRESHOLD -->|Yes| OFFSET[Calculate offset = -centroid]
    
    OFFSET --> UPDATE_ABS[absoluteWorldOffset += offset]
    
    UPDATE_ABS --> LOOP_START[For each shiftable in registeredShiftables]
    LOOP_START --> CALL_SHIFT[shiftable.OnShift offset]
    CALL_SHIFT --> LOOP_CHECK{More shiftables?}
    LOOP_CHECK -->|Yes| LOOP_START
    LOOP_CHECK -->|No| EVENT[Raise OriginShiftedEvent]
    
    EVENT --> END([End])
    SKIP --> END
    
    style THRESHOLD fill:#ff0
    style CALL_SHIFT fill:#0ff
    style EVENT fill:#0f0
```

---

## Registration Lifecycle

### Component Lifecycle Diagram

```mermaid
stateDiagram-v2
    [*] --> Instantiated: GameObject created
    Instantiated --> Registered: OnEnable() called
    Registered --> Active: Added to manager lists
    
    Active --> Shifting: Origin shift triggered
    Shifting --> Active: OnShift() complete
    
    Active --> Unregistered: OnDisable() called
    Unregistered --> [*]: Removed from lists
    
    Active --> Unregistered: GameObject destroyed
    
    note right of Registered
        FloatingOriginReference registers in:
        - registeredReferences
        
        ShiftableFloatingOrigin registers in:
        - registeredShiftables
    end note
    
    note right of Shifting
        Manager calls OnShift(offset) on all
        registered shiftables during shift
    end note
```

### Example: Player Ship Lifecycle

```mermaid
sequenceDiagram
    participant G as GameObject (Player Ship)
    participant FOR as FloatingOriginReference
    participant SRB as ShiftableRigidbody
    participant M as FloatingOriginManager
    
    Note over G: Player spawns
    G->>FOR: Awake()
    G->>SRB: Awake()
    
    G->>FOR: OnEnable()
    FOR->>M: RegisterReference(this)
    Note over M: Add to registeredReferences
    
    G->>SRB: OnEnable()
    SRB->>M: RegisterShiftable(this)
    Note over M: Add to registeredShiftables
    
    Note over G,M: Player active in game...
    
    M->>M: LateUpdate - Check threshold
    M->>FOR: Get position for centroid
    M->>M: Threshold exceeded!
    M->>SRB: OnShift(offset)
    SRB->>SRB: rb.position += offset
    
    Note over G,M: More gameplay...
    
    Note over G: Player dies or scene unloads
    G->>FOR: OnDisable()
    FOR->>M: UnregisterReference(this)
    Note over M: Remove from registeredReferences
    
    G->>SRB: OnDisable()
    SRB->>M: UnregisterShiftable(this)
    Note over M: Remove from registeredShiftables
```

---

## Performance Considerations

### Why Registration-Based Is Efficient

```mermaid
graph TB
    subgraph "OLD: Scene Traversal (Slow)"
        OLD1[FloatingOriginManager]
        OLD1 --> OLD2[SceneManager.GetAllScenes]
        OLD2 --> OLD3[For each scene]
        OLD3 --> OLD4[GetRootGameObjects]
        OLD4 --> OLD5[For each root object]
        OLD5 --> OLD6[Shift transform.position]
        OLD5 --> OLD7[Recurse children]
        
        OLD7 -.-> OLD5
    end
    
    subgraph "NEW: Registration-Based (Fast)"
        NEW1[FloatingOriginManager]
        NEW1 --> NEW2[For each in registeredShiftables]
        NEW2 --> NEW3[shiftable.OnShift offset]
    end
    
    style OLD1 fill:#f00
    style NEW1 fill:#0f0
```

**Old Approach**: O(n) where n = total GameObjects in scene  
**New Approach**: O(m) where m = registered shiftables (typically << n)

### Typical Registration Counts

```mermaid
pie title "Registered Objects in Typical Scene"
    "Player Ship" : 1
    "Enemy Ships" : 20
    "Asteroids" : 50
    "Projectiles" : 30
    "Particle Systems" : 15
    "Trail Renderers" : 10
```

**Total**: ~126 objects vs. potentially thousands of GameObjects in scene

---

## Integration Points

### EventBus Integration

```mermaid
graph LR
    subgraph "Floating Origin System"
        FOM[FloatingOriginManager]
    end
    
    subgraph "EventBus"
        EB[EventBus.Instance]
    end
    
    subgraph "Listeners"
        CAM[Camera Controller]
        UI[UI System]
        NET[Network Sync]
        AUDIO[Audio System]
    end
    
    FOM -->|Raise OriginShiftedEvent| EB
    EB -->|Subscribe| CAM
    EB -->|Subscribe| UI
    EB -->|Subscribe| NET
    EB -->|Subscribe| AUDIO
    
    CAM -.->|Adjust camera rig| RESP[Response]
    UI -.->|Update HUD positions| RESP
    NET -.->|Sync to server| RESP
    AUDIO -.->|Update listener| RESP
```

### Example Event Listener

```csharp
using MidniteOilSoftware.Core;
using MidniteOilSoftware.SpaceShooter.Events;

void OnEnable()
{
    EventBus.Instance.Subscribe<OriginShiftedEvent>(OnOriginShifted);
}

void OnDisable()
{
    EventBus.Instance?.Unsubscribe<OriginShiftedEvent>(OnOriginShifted);
}

void OnOriginShifted(OriginShiftedEvent evt)
{
    // evt.Offset - The Vector3 offset applied
    // evt.OldCentroid - The centroid before shift
    // Handle custom shift logic here
}
```

---

## Summary

### Key Takeaways

1. **Both components needed**: Player needs `FloatingOriginReference` (trigger) and `ShiftableRigidbody` (response)
2. **Everything shifts together**: Player doesn't stay still - entire world shifts including player
3. **Registration-based**: Explicit opt-in prevents performance issues
4. **Event-driven**: Loose coupling via EventBus for extensibility
5. **Multiplayer-ready**: Centroid-based approach handles multiple players naturally

### Architecture Benefits

- ✅ No scene traversal required
- ✅ Automatic lifecycle management via OnEnable/OnDisable
- ✅ Type-safe component specialization
- ✅ Extensible via inheritance
- ✅ Testable and debuggable
- ✅ Multiplayer-compatible from the start

---

*Generated for 3D Space Shooter Tutorial - Unity 6000.3*
