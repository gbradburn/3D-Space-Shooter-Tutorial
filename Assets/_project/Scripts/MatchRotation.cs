using Sirenix.OdinInspector;
using UnityEngine;

public class MatchRotation : MonoBehaviour
{
    [SerializeField] [Required] Transform _target;

    void LateUpdate()
    {
        transform.rotation = _target.rotation;
    }
}
