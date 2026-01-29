using MidniteOilSoftware.SpaceShooter.FloatingOrigin;
using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.Debugging
{
    public class FloatingOriginTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] float _testSpeed = 1000f;
        [SerializeField] bool _autoMove;
        [SerializeField] Vector3 _moveDirection = Vector3.forward;

        [Header("Manual Test")]
        [SerializeField] float _jumpDistance = 10000f;

        void Update()
        {
            if (!_autoMove) return;
            
            transform.position += _moveDirection.normalized * (_testSpeed * Time.deltaTime);
        }

        public void JumpToDistance()
        {
            var newPos = _moveDirection.normalized * _jumpDistance;
            transform.position = newPos;
            Debug.Log($"<color=green>Jumped to position: {transform.position}, Distance from origin: {transform.position.magnitude:F2}</color>", this);
        }

        public void ResetPosition()
        {
            transform.position = Vector3.zero;
            Debug.Log("<color=green>Position reset to origin</color>", this);
        }

        void OnGUI()
        {
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 170, 400, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>Floating Origin Test</b>");
            GUILayout.Label($"Position: {transform.position}");
            
            var absPos = FloatingOriginManager.Instance ? 
                FloatingOriginManager.Instance.GetAbsolutePosition(transform.position) : 
                Vector3Double.zero;
            GUILayout.Label($"Absolute: {absPos}");
            
            if (GUILayout.Button("Jump to " + _jumpDistance))
                JumpToDistance();
            
            if (GUILayout.Button("Reset Position"))
                ResetPosition();
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
