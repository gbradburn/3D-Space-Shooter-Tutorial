namespace MidniteOilSoftware.SpaceShooter.Events
{
    public struct WeaponSystemsInitializedEvent
    {
        public Blaster[] Blasters { get; }
        public MissileLauncher[] MissileLaunchers { get; }
        public bool IsLocalPlayer { get; }
        
        public WeaponSystemsInitializedEvent(Blaster[] blasters, MissileLauncher[] missileLaunchers, bool isLocalPlayer)
        {
            Blasters = blasters;
            MissileLaunchers = missileLaunchers;
            IsLocalPlayer = isLocalPlayer;
        }
    }
}
