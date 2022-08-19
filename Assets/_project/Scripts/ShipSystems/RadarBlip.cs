using UnityEngine;

public class RadarBlip : MonoBehaviour
{
    [SerializeField] GameObject _lineAbove, _lineBelow;

    public GameObject LineAbove => _lineAbove;
    public GameObject LineBelow => _lineBelow;
}
