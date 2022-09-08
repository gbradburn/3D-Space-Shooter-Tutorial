using UnityEngine;
using UnityEngine.UI;

public class TargetIndicator : MonoBehaviour
{
    [SerializeField] Image _targetBox, _lockOnImage, _offScreenImage;
    [SerializeField] float _offScreenMargin = 45f;

    Transform _target;
    Canvas _mainCanvas;
    Camera _mainCamera;
    RectTransform _rectTransform, _canvasRect;

    Vector3 _screenCenter = Vector3.zero;

    Vector3 ScreenCenter
    {
        get
        {
            var rect =  _canvasRect.rect;
            _screenCenter.x = rect.width * 0.5f;
            _screenCenter.y = rect.height * 0.5f;
            _screenCenter.z = 0f;
            return _screenCenter * _canvasRect.localScale.x;
        }
    }
    
    public int Key { get; private set; }
    public bool LockedOn { get; set; }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        // Get normalized position of target
        Vector3 targetViewportPos = _mainCamera.WorldToViewportPoint(_target.position);
        
        // Display target reticle on target
        if (TargetIsVisible(targetViewportPos))
        {
            DisplayOnScreenReticle(targetViewportPos);
            return;
        }

        DisplayOffScreenReticle(targetViewportPos);
    }

    public void Init(Transform target, Canvas mainCanvas)
    {
        _target = target;
        Key = _target.GetInstanceID();
        _mainCanvas = mainCanvas;
        _canvasRect = _mainCanvas.GetComponent<RectTransform>();
        _mainCamera = Camera.main;
    }


    bool TargetIsVisible(Vector3 position)
    {
        return (position.x is >= 0 and <= 1 && position.y is >= 0 and <= 1 && position.z >= 0);
    }    
  
    void DisplayOnScreenReticle(Vector3 targetViewportPos)
    {
        Vector3 position = _mainCamera.ViewportToScreenPoint(targetViewportPos);
        position.z = 0;
        _rectTransform.position = position;
        _targetBox.enabled = !LockedOn;
        _lockOnImage.enabled = LockedOn;
        _offScreenImage.enabled = false;
    }
    
    void DisplayOffScreenReticle(Vector3 targetViewportPos)
    {
        _targetBox.enabled = false;
        _lockOnImage.enabled = false;

        Vector3 indicatorPosition = (_mainCamera.ViewportToScreenPoint(targetViewportPos) - ScreenCenter) *
                                    Mathf.Sign(targetViewportPos.z);
        indicatorPosition.z = 0;

        float x = (ScreenCenter.x - _offScreenMargin) / Mathf.Abs(indicatorPosition.x);
        float y = (ScreenCenter.y - _offScreenMargin) / Mathf.Abs(indicatorPosition.y);

        if (x < y)
        {
            float angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
            indicatorPosition.x = Mathf.Sign(indicatorPosition.x) * (_screenCenter.x - _offScreenMargin) *
                                  _canvasRect.localScale.x;
            indicatorPosition.y = Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.x;
        }
        else
        {
            float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition, Vector3.forward);
            indicatorPosition.y = Mathf.Sign(indicatorPosition.y) * (_screenCenter.y - _offScreenMargin) *
                                  _canvasRect.localScale.y;
            indicatorPosition.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.y;
        }

        indicatorPosition += ScreenCenter;
        _rectTransform.position = indicatorPosition;

        Vector3 rotation = _rectTransform.eulerAngles;
        rotation.z = Vector3.SignedAngle(Vector3.up, indicatorPosition - ScreenCenter, Vector3.forward);
        _rectTransform.eulerAngles = rotation;
        _offScreenImage.enabled = true;

    }


}
