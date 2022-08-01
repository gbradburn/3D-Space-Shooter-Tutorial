using UnityEngine;

public abstract class MovementControlsBase : MonoBehaviour, IMovementControls
{
    public abstract float YawAmount { get; }
    public abstract float PitchAmount { get; }
    public abstract float RollAmount { get; }
    public abstract float ThrustAmount { get; }
}