using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AsteroidField))]
public class AsteroidFieldEditor : Editor
{
    static readonly Color Color = new Color(1f, 0, 0, .1f);
    
    [DrawGizmo(GizmoType.InSelectionHierarchy)]
    static void DrawGizmos(AsteroidField asteroidField, GizmoType gizmoType)
    {
        Gizmos.color = Color;
        Gizmos.DrawSphere(asteroidField.transform.position, asteroidField.Radius);
    }
}
