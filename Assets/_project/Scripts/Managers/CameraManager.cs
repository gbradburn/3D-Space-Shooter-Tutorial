using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

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
            for (int i = 0; i < _virtualCameras.Count; ++i)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) return (VirtualCameras)i;
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
            if (camIndex++ == selectedCamera)
            {
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
