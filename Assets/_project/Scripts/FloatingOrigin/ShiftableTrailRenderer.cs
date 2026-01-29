using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.FloatingOrigin
{
    [RequireComponent(typeof(TrailRenderer))]
    public class ShiftableTrailRenderer : ShiftableFloatingOrigin
    {
        TrailRenderer _trailRenderer;

        void Awake()
        {
            _trailRenderer = GetComponent<TrailRenderer>();
        }

        public override void OnShift(Vector3 offset)
        {
            base.OnShift(offset);

            if (_trailRenderer)
                _trailRenderer.Clear();
        }
    }
}
