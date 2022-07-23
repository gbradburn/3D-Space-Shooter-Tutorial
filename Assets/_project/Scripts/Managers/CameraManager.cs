using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    enum VirtualCameras
    {
        NoCamera = -1,
        CockpitCamera = 0,
        FollowCamera
    }

    [BoxGroup("Virtual cameras")] [SerializeField] [Required]
    List<GameObject> _virtualCameras;

    VirtualCameras CameraKeyPressed
    {
        get
        {
            for (int i = 0; i < _virtualCameras.Count; ++i)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) return (VirtualCameras)i;
            }

            return VirtualCameras.NoCamera;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        SetActiveCamera(VirtualCameras.CockpitCamera);
    }

    // Update is called once per frame
    void Update()
    {
        SetActiveCamera(CameraKeyPressed);
    }
    
    void SetActiveCamera(VirtualCameras activeCamera)
    {
        if (activeCamera == VirtualCameras.NoCamera)
        {
            return;
        }
        
        foreach (GameObject cam in _virtualCameras)
        {
            cam.SetActive(cam.tag.Equals(activeCamera.ToString()));
        }
    }

}
