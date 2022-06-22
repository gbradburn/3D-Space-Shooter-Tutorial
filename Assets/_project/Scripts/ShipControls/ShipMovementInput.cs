using UnityEngine;

public class ShipMovementInput : MonoBehaviour
{
    [SerializeField] ShipInputManager.InputType _inputType = ShipInputManager.InputType.HumanDesktop;

    public IMovementControls MovementControls { get; private set; }
    
    void Start()
    {
        MovementControls = ShipInputManager.GetInputcontrols(_inputType);
    }

    void OnDestroy()
    {
        MovementControls = null;
    }
}
