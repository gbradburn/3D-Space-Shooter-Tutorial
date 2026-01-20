using System;
using System.Collections.Generic;
using Cinemachine;
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
    }

    [SerializeField]
    List<CinemachineVirtualCamera> _virtualCameras;

    public Transform ActiveCamera { get; private set; }
    public UnityEvent ActiveCameraChanged;
    
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
    }

    void Start()
    {
        SetActiveCamera(VirtualCameras.CockpitCamera);
    }


    void Update()
    {
        SetActiveCamera(CameraKeyPressed);
    }
    
    void SetActiveCamera(VirtualCameras selectedCamera)
    {
        if (selectedCamera == VirtualCameras.NoSelection)
        {
            return;
        }

        VirtualCameras camIndex = VirtualCameras.CockpitCamera;
        foreach (var cam in _virtualCameras)
        {
            if (!cam) continue; 
            if (camIndex++ == selectedCamera)
            {
                if (!cam.gameObject) continue;
                cam.gameObject.SetActive(true);
                ActiveCamera = cam.transform;
                ActiveCameraChanged.Invoke();
            }
            else
            {
                cam.gameObject.SetActive(false);
            }
        }
    }

}
