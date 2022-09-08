using UnityEngine;

public class Targetable : MonoBehaviour
{
    void OnEnable()
    {
        UIManager.Instance.AddTarget(transform);
    }

    void OnDisable()
    {
        RemoveTarget();
    }

    void OnDestroy()
    {
        RemoveTarget();
    }

    void RemoveTarget()
    {
        UIManager.Instance.RemoveTarget(transform);
    }
}
