using UnityEditor;
using UnityEngine;
using MidniteOilSoftware.SpaceShooter.FloatingOrigin;

namespace MidniteOilSoftware.SpaceShooter.Editor
{
    public static class FloatingOriginEditorUtilities
    {
        [MenuItem("Tools/Floating Origin/Add Shiftable Components to Selection")]
        static void AddShiftableComponentsToSelection()
        {
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects in the scene or project.", "OK");
                return;
            }

            var addedCount = 0;
            var skippedCount = 0;

            foreach (var selectedObject in Selection.gameObjects)
            {
                if (AddAppropriateShiftableComponent(selectedObject))
                    addedCount++;
                else
                    skippedCount++;
            }

            Debug.Log($"<color=lime>Added shiftable components to {addedCount} GameObject(s). Skipped {skippedCount} (already had component).</color>");
        }

        static bool AddAppropriateShiftableComponent(GameObject obj)
        {
            if (obj.GetComponent<ShiftableFloatingOrigin>())
                return false;

            if (obj.GetComponent<Rigidbody>())
            {
                Undo.AddComponent<ShiftableRigidbody>(obj);
                return true;
            }

            if (obj.GetComponent<ParticleSystem>())
            {
                Undo.AddComponent<ShiftableParticleSystem>(obj);
                return true;
            }

            if (obj.GetComponent<TrailRenderer>())
            {
                Undo.AddComponent<ShiftableTrailRenderer>(obj);
                return true;
            }

            Undo.AddComponent<ShiftableFloatingOrigin>(obj);
            return true;
        }

        [MenuItem("Tools/Floating Origin/Add Reference Component to Selection")]
        static void AddReferenceComponentToSelection()
        {
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects in the scene or project.", "OK");
                return;
            }

            var addedCount = 0;

            foreach (var selectedObject in Selection.gameObjects)
            {
                if (!selectedObject.GetComponent<FloatingOriginReference>())
                {
                    Undo.AddComponent<FloatingOriginReference>(selectedObject);
                    addedCount++;
                }
            }

            Debug.Log($"<color=lime>Added FloatingOriginReference to {addedCount} GameObject(s).</color>");
        }

        [MenuItem("Tools/Floating Origin/Setup Player Ship (Reference + Shiftable)")]
        static void SetupPlayerShip()
        {
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a player ship GameObject.", "OK");
                return;
            }

            foreach (var selectedObject in Selection.gameObjects)
            {
                if (!selectedObject.GetComponent<FloatingOriginReference>())
                {
                    Undo.AddComponent<FloatingOriginReference>(selectedObject);
                    Debug.Log($"<color=lime>Added FloatingOriginReference to {selectedObject.name}</color>");
                }

                if (!selectedObject.GetComponent<ShiftableFloatingOrigin>())
                {
                    if (selectedObject.GetComponent<Rigidbody>())
                    {
                        Undo.AddComponent<ShiftableRigidbody>(selectedObject);
                        Debug.Log($"<color=lime>Added ShiftableRigidbody to {selectedObject.name}</color>");
                    }
                    else
                    {
                        Undo.AddComponent<ShiftableFloatingOrigin>(selectedObject);
                        Debug.Log($"<color=lime>Added ShiftableFloatingOrigin to {selectedObject.name}</color>");
                    }
                }
            }

            Debug.Log("<color=lime>Player ship setup complete!</color>");
        }

        [MenuItem("Tools/Floating Origin/Remove All Shiftable Components from Selection")]
        static void RemoveShiftableComponentsFromSelection()
        {
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects.", "OK");
                return;
            }

            var removedCount = 0;

            foreach (var selectedObject in Selection.gameObjects)
            {
                var shiftables = selectedObject.GetComponents<ShiftableFloatingOrigin>();
                foreach (var shiftable in shiftables)
                {
                    Undo.DestroyObjectImmediate(shiftable);
                    removedCount++;
                }

                var references = selectedObject.GetComponents<FloatingOriginReference>();
                foreach (var reference in references)
                {
                    Undo.DestroyObjectImmediate(reference);
                    removedCount++;
                }
            }

            Debug.Log($"<color=yellow>Removed {removedCount} floating origin component(s).</color>");
        }
    }
}
