using UnityEngine;

public class AddPointsWhenDestroyed : MonoBehaviour
{
    [SerializeField] int _points = 20;

    bool _scored;

    void OnDestroy()
    {
        Debug.Log($"{name} destroyed, adding {_points} points.", this);
        AddScore();
    }

    void OnDisable()
    {
        AddScore();
    }

    void AddScore()
    {
        if (_scored) return;
        _scored = true;
        ScoreManager.Instance.AddPoints(_points);
    }
}
