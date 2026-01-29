using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.FloatingOrigin
{
    public class ShiftableFloatingOrigin : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            FloatingOriginManager.Instance.RegisterShiftable(this);
        }

        protected virtual void OnDisable()
        {
            if (FloatingOriginManager.Instance)
                FloatingOriginManager.Instance.UnregisterShiftable(this);
        }

        public virtual void OnShift(Vector3 offset)
        {
            transform.position += offset;
        }
    }
}
