using UnityEngine;

public class AddPointsWhenDestroyed : MonoBehaviour
{
    [SerializeField] int _points = 20;

    bool _scored;

    void OnDestroy()
    {
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
