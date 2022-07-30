using UnityEngine;

public abstract class WeaponControlsBase : MonoBehaviour, IWeaponControls
{
    public abstract bool PrimaryFired { get; }
    public abstract bool SecondaryFired { get; }
}