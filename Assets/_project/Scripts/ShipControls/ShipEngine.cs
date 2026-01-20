using UnityEngine;

public class ShipEngine : MonoBehaviour
{
    [SerializeField] GameObject _thruster;

    IMovementControls _shipMovementControls;
    Rigidbody _rigidbody;
    float _thrustForce;
    float _thrustAmount;

    bool ThrustersEnabled => _shipMovementControls != null && 
                             !Mathf.Approximately(0f, _shipMovementControls.ThrustAmount);

    void Update()
    {
        ActivateThrusters();
    }

    void FixedUpdate()
    {
        if (!ThrustersEnabled) return;
        _rigidbody.AddForce(transform.forward * _thrustAmount * Time.fixedDeltaTime);
    }

    public void Init(IMovementControls movementControls, Rigidbody rb, float thrustForce)
    {
        if (movementControls == null)
        {
            Debug.LogError($"movementControls is null in ShipEngine.Init()");
            return;
        }
        _shipMovementControls = movementControls;
        _rigidbody = rb;
        _thrustForce = thrustForce;
    }

    void ActivateThrusters()
    {
        if (!_thruster) return;
        _thruster.SetActive(ThrustersEnabled);
        if (!ThrustersEnabled) return;
        _thrustAmount = _thrustForce * _shipMovementControls.ThrustAmount;
    }
    
}
