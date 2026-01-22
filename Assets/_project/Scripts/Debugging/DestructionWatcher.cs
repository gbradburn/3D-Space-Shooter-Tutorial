using UnityEngine;

public class DestructionWatcher : MonoBehaviour
{
    [SerializeField] bool _breakOnDisable = false;
    [SerializeField] bool _breakOnDestroy = false;
    
    bool _isQuitting;

    void OnApplicationQuit() => _isQuitting = true;

    void OnDisable()
    {
        if (_isQuitting || !Application.isPlaying) return;

        if (gameObject.activeInHierarchy)
        {
            Debug.Log($"<color=yellow>[WATCHER]</color> {name} disabled but still in hierarchy (just inactive).", this);
            return;
        }

        Debug.Log($"<color=red>[WATCHER]</color> {name} is being DISABLED (SetActive false or Destroy called)!", this);
        Debug.Log($"<color=cyan>Stack Trace:</color>\n{StackTraceUtility.ExtractStackTrace()}", this);
        
        if (_breakOnDisable)
        {
            Debug.Break();
        }
    }

    void OnDestroy()
    {
        if (_isQuitting) return;
        
        Debug.Log($"<color=orange>[WATCHER]</color> OnDestroy called for {name} - ACTUAL DESTRUCTION!", this);
        Debug.Log($"<color=cyan>Destroy Stack Trace:</color>\n{StackTraceUtility.ExtractStackTrace()}", this);
        
        if (_breakOnDestroy)
        {
            Debug.Break();
        }
    }
    
    void OnEnable()
    {
        if (!Application.isPlaying) return;
        Debug.Log($"<color=green>[WATCHER]</color> {name} enabled/re-enabled.", this);
    }
}