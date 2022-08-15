using UnityEngine;

public class MatchRotation : MonoBehaviour
{
    [SerializeField] Transform _target;

    void LateUpdate()
    {
        transform.rotation = _target.rotation;
    }
}
