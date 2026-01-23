using System.Collections.Generic;
using Cinemachine;
using MidniteOilSoftware.Core;
using MidniteOilSoftware.SpaceShooter.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    enum VirtualCameras
    {
        NoSelection = -1,
        CockpitCamera = 0,
        FollowCamera = 1,
        EnemyFollowCamera = 2,
        KillCamera = 3,
    }

    [SerializeField]
    List<CinemachineVirtualCamera> _virtualCameras;

    public Transform ActiveCamera { get; private set; }
    public UnityEvent ActiveCameraChanged;

    VirtualCameras _previousCamera = VirtualCameras.CockpitCamera;
    bool _isKillCamActive;
    
    VirtualCameras CameraKeyPressed
    {
        get
        {
            if (Keyboard.current == null) return VirtualCameras.NoSelection;

            for (var i = 0; i < _virtualCameras.Count; ++i)
            {
                var keyNumber = i + 1;
                
                var numpadKey = Keyboard.current.FindKeyOnCurrentKeyboardLayout($"numpad{keyNumber}");
                if (numpadKey != null && numpadKey.wasPressedThisFrame) return (VirtualCameras)i;
                
                var numberKey = Keyboard.current.FindKeyOnCurrentKeyboardLayout($"{keyNumber}");
                if (numberKey != null && numberKey.wasPressedThisFrame) return (VirtualCameras)i;
            }

            return VirtualCameras.NoSelection;
        }
    }

    void Awake()
    {
        ActiveCameraChanged = new UnityEvent();
        Debug.Log($"<color=cyan>[CAMERA]</color> CameraManager initialized with {_virtualCameras.Count} virtual cameras.", this);
    }

    void OnEnable()
    {
        EventBus.Instance.Subscribe<PlayerDestroyedEvent>(OnPlayerDestroyed);
        EventBus.Instance.Subscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
        Debug.Log("<color=cyan>[CAMERA]</color> Subscribed to PlayerDestroyedEvent and PlayerSpawnedEvent.", this);
    }

    void OnDisable()
    {
        if (EventBus.Instance)
        {
            EventBus.Instance.Unsubscribe<PlayerDestroyedEvent>(OnPlayerDestroyed);
            EventBus.Instance.Unsubscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
        }
    }

    void Start()
    {
        SetActiveCamera(VirtualCameras.CockpitCamera);
    }

    void Update()
    {
        if (!_isKillCamActive)
        {
            SetActiveCamera(CameraKeyPressed);
        }
    }

    void OnPlayerDestroyed(PlayerDestroyedEvent e)
    {
        Debug.Log($"<color=yellow>[CAMERA]</color> OnPlayerDestroyed received. Player: {e.Player.name}, IsLocal: {PlayerManager.Instance?.IsLocalPlayer(e.Player)}, Position: {e.DeathPosition}", this);
        
        if (PlayerManager.Instance && PlayerManager.Instance.IsLocalPlayer(e.Player))
        {
            Debug.Log($"<color=green>[CAMERA]</color> Local player destroyed! Activating Kill Camera at position {e.DeathPosition}", this);
            ActivateKillCamera(e.Player.transform.position);
        }
        else
        {
            Debug.Log("<color=grey>[CAMERA]</color> Non-local player destroyed, Kill Camera not activated.", this);
        }
    }

    void OnPlayerSpawned(PlayerSpawnedEvent e)
    {
        Debug.Log($"<color=yellow>[CAMERA]</color> OnPlayerSpawned received. Player: {e.Player.name}, IsLocal: {e.IsLocalPlayer}, KillCamActive: {_isKillCamActive}", this);
        
        if (e.IsLocalPlayer)
        {
            RegisterCockpitCamera();
            
            if (_isKillCamActive)
            {
                Debug.Log("<color=green>[CAMERA]</color> Local player respawned while Kill Cam active. Deactivating Kill Camera.", this);
                DeactivateKillCamera();
            }
        }
    }

    void RegisterCockpitCamera()
    {
        var cockpitCam = GameObject.FindGameObjectWithTag("CockpitCamera");
        if (!cockpitCam)
        {
            Debug.LogWarning("<color=yellow>[CAMERA]</color> Cockpit camera not found! Searching by tag 'CockpitCamera'.", this);
            return;
        }

        var virtualCam = cockpitCam.GetComponentInChildren<CinemachineVirtualCamera>();
        if (!virtualCam)
        {
            Debug.LogWarning("<color=yellow>[CAMERA]</color> CinemachineVirtualCamera component not found on Cockpit camera!", this);
            return;
        }

        if (_virtualCameras.Count > 0 && _virtualCameras[0] == virtualCam)
        {
            return;
        }

        if (_virtualCameras.Count == 0)
        {
            _virtualCameras.Add(virtualCam);
        }
        else
        {
            _virtualCameras[0] = virtualCam;
        }
        
        Debug.Log($"<color=green>[CAMERA]</color> ✓ Registered Cockpit camera: {virtualCam.name} at index 0", this);
    }

    public void ActivateKillCamera(Vector3 explosionPosition)
    {
        Debug.Log($"<color=orange>[CAMERA]</color> ActivateKillCamera called at position {explosionPosition}", this);
        
        var killCam = GetKillCamera();
        if (!killCam)
        {
            Debug.LogError("<color=red>[CAMERA]</color> Kill Camera not found! Check CameraManager's Virtual Cameras list (Element 3).", this);
            return;
        }

        _previousCamera = (VirtualCameras)GetActiveCameraIndex();
        Debug.Log($"<color=orange>[CAMERA]</color> Storing previous camera: {_previousCamera}", this);
        
        _isKillCamActive = true;

        var lookAtTarget = new GameObject("KillCam_LookAtTarget").transform;
        lookAtTarget.position = explosionPosition;
        Debug.Log($"<color=orange>[CAMERA]</color> Created LookAt target at {explosionPosition}", this);

        killCam.LookAt = lookAtTarget;
        killCam.Follow = lookAtTarget;
        Debug.Log($"<color=orange>[CAMERA]</color> Set Kill Camera Follow and LookAt to target", this);

        SetActiveCamera(VirtualCameras.KillCamera);
        Debug.Log("<color=green>[CAMERA]</color> ✓ Kill Camera ACTIVATED successfully!", this);

        Destroy(lookAtTarget.gameObject, 5f);
    }

    void DeactivateKillCamera()
    {
        Debug.Log($"<color=orange>[CAMERA]</color> DeactivateKillCamera called. Returning to {_previousCamera}", this);
        
        _isKillCamActive = false;

        var killCam = GetKillCamera();
        if (killCam)
        {
            killCam.LookAt = null;
            killCam.Follow = null;
            Debug.Log("<color=orange>[CAMERA]</color> Cleared Kill Camera targets", this);
        }

        SetActiveCamera(_previousCamera);
        Debug.Log($"<color=green>[CAMERA]</color> ✓ Kill Camera DEACTIVATED. Switched to {_previousCamera}", this);
    }

    CinemachineVirtualCamera GetKillCamera()
    {
        var killCamIndex = (int)VirtualCameras.KillCamera;
        if (killCamIndex >= 0 && killCamIndex < _virtualCameras.Count)
        {
            var cam = _virtualCameras[killCamIndex];
            if (cam)
            {
                Debug.Log($"<color=cyan>[CAMERA]</color> Kill Camera found: {cam.name}", this);
            }
            return cam;
        }
        Debug.LogWarning($"<color=yellow>[CAMERA]</color> Kill Camera index ({killCamIndex}) out of range. Virtual Cameras count: {_virtualCameras.Count}", this);
        return null;
    }

    int GetActiveCameraIndex()
    {
        for (var i = 0; i < _virtualCameras.Count; i++)
        {
            if (_virtualCameras[i] && _virtualCameras[i].gameObject.activeSelf)
            {
                return i;
            }
        }
        return 0;
    }
    
    void SetActiveCamera(VirtualCameras selectedCamera)
    {
        if (selectedCamera == VirtualCameras.NoSelection)
        {
            return;
        }

        Debug.Log($"<color=cyan>[CAMERA]</color> SetActiveCamera: {selectedCamera}", this);

        var targetIndex = (int)selectedCamera;
        for (var i = 0; i < _virtualCameras.Count; i++)
        {
            var cam = _virtualCameras[i];
            if (!cam) continue;
            
            if (i == targetIndex)
            {
                if (!cam.gameObject) continue;
                cam.gameObject.SetActive(true);
                ActiveCamera = cam.transform;
                ActiveCameraChanged.Invoke();
                Debug.Log($"<color=green>[CAMERA]</color> Camera switched to: {cam.name} ({selectedCamera})", this);
            }
            else
            {
                cam.gameObject.SetActive(false);
            }
        }
    }

}
