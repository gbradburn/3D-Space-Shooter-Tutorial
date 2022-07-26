using UnityEngine;

public class ShipInputControls : MonoBehaviour
{
    [SerializeField] ShipInputManager.InputType _inputType = ShipInputManager.InputType.HumanDesktop;

    public IMovementControls MovementControls { get; private set; }
    public IWeaponControls WeaponControls { get; private set; }
    
    void Start()
    {
        MovementControls = ShipInputManager.GetMovementControls(_inputType);
        WeaponControls = ShipInputManager.GetWeaponControls(_inputType);
    }

    void OnDestroy()
    {
        MovementControls = null;
    }
}
