using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TargetIndicator _targetIndicatorPrefab;
    [SerializeField] Canvas _mainCanvas;

    public static UIManager Instance;

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
}
