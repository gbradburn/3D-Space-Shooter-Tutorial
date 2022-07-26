public abstract class WeaponControlsBase : IWeaponControls
{
    public abstract bool PrimaryFired { get; }
    public abstract bool SecondaryFired { get; }
}