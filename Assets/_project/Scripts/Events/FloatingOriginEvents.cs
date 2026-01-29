using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.Events
{
    public struct OriginShiftedEvent
    {
        public Vector3 Offset { get; }
        public Vector3 NewReferencePosition { get; }
        public float ShiftMagnitude { get; }

        public OriginShiftedEvent(Vector3 offset, Vector3 newReferencePosition)
        {
            Offset = offset;
            NewReferencePosition = newReferencePosition;
            ShiftMagnitude = offset.magnitude;
        }
    }
}
