using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.FloatingOrigin
{
    [RequireComponent(typeof(Rigidbody))]
    public class ShiftableRigidbody : ShiftableFloatingOrigin
    {
        Rigidbody _rigidbody;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnShift(Vector3 offset)
        {
            if (!_rigidbody) return;

            _rigidbody.position += offset;
        }
    }
}
