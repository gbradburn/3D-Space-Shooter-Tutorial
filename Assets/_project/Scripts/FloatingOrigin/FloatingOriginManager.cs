using System.Collections.Generic;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.SpaceShooter.Events;
using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.FloatingOrigin
{
    public class FloatingOriginManager : SingletonMonoBehaviour<FloatingOriginManager>
    {
        [Header("Shift Settings")]
        [SerializeField] float _shiftThreshold = 5000f;
        [SerializeField] bool _enableAutoShift = true;

        [Header("Multiplayer")]
        [SerializeField] bool _multiplayerMode;
        
        [Header("Debug")]
        [SerializeField] bool _showDebugGUI = true;

        readonly List<ShiftableFloatingOrigin> _registeredShiftables = new();
        readonly List<FloatingOriginReference> _registeredReferences = new();
        
        Vector3Double _absoluteWorldOffset = Vector3Double.zero;
        int _totalShiftCount;
        float _centroidDistance;

        public Vector3Double AbsoluteWorldOffset => _absoluteWorldOffset;
        public int TotalShiftCount => _totalShiftCount;
        public float CentroidDistance => _centroidDistance;
        public int RegisteredShiftablesCount => _registeredShiftables.Count;
        public int RegisteredReferencesCount => _registeredReferences.Count;

        public void RegisterShiftable(ShiftableFloatingOrigin shiftable)
        {
            if (!_registeredShiftables.Contains(shiftable))
            {
                _registeredShiftables.Add(shiftable);
                
                if (_enableDebugLog)
                    Debug.Log($"Registered shiftable: {shiftable.name} (Total: {_registeredShiftables.Count})", this);
            }
        }

        public void UnregisterShiftable(ShiftableFloatingOrigin shiftable)
        {
            _registeredShiftables.Remove(shiftable);
            
            if (_enableDebugLog)
                Debug.Log($"Unregistered shiftable: {shiftable.name} (Total: {_registeredShiftables.Count})", this);
        }

        public void RegisterReference(FloatingOriginReference reference)
        {
            if (!_registeredReferences.Contains(reference))
            {
                _registeredReferences.Add(reference);
                
                if (_enableDebugLog)
                    Debug.Log($"Registered reference: {reference.name} (Total: {_registeredReferences.Count})", this);
            }
        }

        public void UnregisterReference(FloatingOriginReference reference)
        {
            _registeredReferences.Remove(reference);
            
            if (_enableDebugLog)
                Debug.Log($"Unregistered reference: {reference.name} (Total: {_registeredReferences.Count})", this);
        }

        void LateUpdate()
        {
            if (!_enableAutoShift || _registeredReferences.Count == 0) 
                return;

            var centroid = CalculateCentroid();
            _centroidDistance = centroid.magnitude;

            if (_centroidDistance >= _shiftThreshold)
            {
                if (_enableDebugLog)
                    Debug.Log($"<color=cyan>Threshold exceeded! Distance: {_centroidDistance:F2} >= {_shiftThreshold:F2}. Performing shift...</color>", this);
                
                PerformOriginShift();
            }
        }

        Vector3 CalculateCentroid()
        {
            if (_registeredReferences.Count == 0)
                return Vector3.zero;

            if (!_multiplayerMode && _registeredReferences.Count == 1)
                return _registeredReferences[0].Position;

            var totalWeight = 0f;
            var weightedSum = Vector3.zero;

            foreach (var reference in _registeredReferences)
            {
                weightedSum += reference.Position * reference.Weight;
                totalWeight += reference.Weight;
            }

            return totalWeight > 0 ? weightedSum / totalWeight : Vector3.zero;
        }

        public void PerformOriginShift()
        {
            if (_registeredReferences.Count == 0)
            {
                Debug.LogWarning("FloatingOriginManager: Cannot perform shift - no reference objects registered", this);
                return;
            }

            var centroid = CalculateCentroid();
            var offset = -centroid;

            _absoluteWorldOffset += new Vector3Double(offset);

            foreach (var shiftable in _registeredShiftables)
            {
                if (shiftable)
                    shiftable.OnShift(offset);
            }

            _totalShiftCount++;

            EventBus.Instance.Raise(new OriginShiftedEvent(offset, centroid));

            if (_enableDebugLog)
            {
                Debug.Log($"<color=yellow>Origin shifted by {offset} | " +
                          $"Shifted {_registeredShiftables.Count} objects | " +
                          $"Total shifts: {_totalShiftCount}</color>", this);
            }
        }

        public void SetMultiplayerMode(bool enabled)
        {
            _multiplayerMode = enabled;
            
            if (_enableDebugLog)
                Debug.Log($"FloatingOriginManager: Multiplayer mode set to {enabled}", this);
        }

        public Vector3Double GetAbsolutePosition(Vector3 localPosition) =>
            _absoluteWorldOffset + new Vector3Double(localPosition);

        public Vector3 GetLocalPosition(Vector3Double absolutePosition) =>
            (absolutePosition - _absoluteWorldOffset).ToVector3();

        void OnGUI()
        {
            if (!_showDebugGUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 220));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>Floating Origin System</b>");
            GUILayout.Label($"Mode: {(_multiplayerMode ? "Multiplayer" : "Single-Player")}");
            GUILayout.Label($"Registered References: {_registeredReferences.Count}");
            GUILayout.Label($"Registered Shiftables: {_registeredShiftables.Count}");
            GUILayout.Label($"Centroid Distance: {_centroidDistance:F2} / {_shiftThreshold:F2}");
            GUILayout.Label($"Total Shifts: {_totalShiftCount}");
            GUILayout.Label($"Absolute Offset: {_absoluteWorldOffset}");
            
            if (_registeredReferences.Count > 0 && GUILayout.Button("Force Shift Now"))
            {
                if (_enableDebugLog)
                    Debug.Log("<color=magenta>Manual shift triggered via GUI button</color>", this);
                
                PerformOriginShift();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
