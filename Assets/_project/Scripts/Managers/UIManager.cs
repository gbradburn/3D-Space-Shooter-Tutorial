using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] TargetIndicator _targetIndicatorPrefab;
    [SerializeField] Canvas _mainCanvas;
    [SerializeField] TMP_Text _scoreText, _highScoreText;

    List<TargetIndicator> _targetIndicators;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _targetIndicators = new List<TargetIndicator>();
    }

    void OnEnable()
    {
        SubscribeToEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    void Start()
    {
        SubscribeToEvents();
    }

    public void AddTarget(Transform target)
    {
        var targetIndicator = Instantiate(_targetIndicatorPrefab, _mainCanvas.transform);
        targetIndicator.Init(target, _mainCanvas);
        _targetIndicators.Add(targetIndicator);
    }

    public void RemoveTarget(Transform target)
    {
        var key = target.GetInstanceID();
        var indicator = _targetIndicators.FirstOrDefault(i => i.Key == key);
        if (indicator)
        {
            _targetIndicators.Remove(indicator);
            Destroy(indicator.gameObject);
        }
    }

    public void UpdateTargetIndicators(List<Transform> targets, int lockedOnTarget)
    {
        foreach (var targetIndicator in _targetIndicators)
        {
            targetIndicator.gameObject.SetActive(targets.Any(target => target.GetInstanceID() == targetIndicator.Key));
            targetIndicator.LockedOn = targetIndicator.Key == lockedOnTarget;
        }
    }

    void SubscribeToEvents()
    {
        SubscribeToScoreManagerEvents();
    }

    void UnsubscribeFromEvents()
    {
        UnsubscribeFromScoreManagerEvents();
    }

    void SubscribeToScoreManagerEvents()
    {
        if (!ScoreManager.Instance) return;
        UnsubscribeFromScoreManagerEvents();
        ScoreManager.Instance.ScoreChanged += OnScoreChanged;
        ScoreManager.Instance.HighScoreChanged += OnHighScoreChanged;
    }

    void UnsubscribeFromScoreManagerEvents()    
    {
        if (!ScoreManager.Instance) return;
        ScoreManager.Instance.ScoreChanged -= OnScoreChanged;
        ScoreManager.Instance.HighScoreChanged -= OnHighScoreChanged;    
    }

    void OnScoreChanged(int score)
    {
        _scoreText.text = score.ToString();
    }

    void OnHighScoreChanged(int highScore)
    {
        _highScoreText.text = highScore.ToString();
    }
}
