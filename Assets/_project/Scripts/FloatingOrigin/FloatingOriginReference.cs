using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.FloatingOrigin
{
    public class FloatingOriginReference : MonoBehaviour
    {
        [SerializeField] float _weight = 1f;

        public Vector3 Position => transform.position;
        public float Weight => _weight;

        void OnEnable()
        {
            FloatingOriginManager.Instance.RegisterReference(this);
        }

        void OnDisable()
        {
            if (FloatingOriginManager.Instance)
                FloatingOriginManager.Instance.UnregisterReference(this);
        }
    }
}
