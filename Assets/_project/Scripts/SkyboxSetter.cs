using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Skybox))]
public class SkyboxSetter : MonoBehaviour
{
    [Required]
    [PreviewField]
    [SerializeField] List<Material> _skyboxMaterials;

    Skybox _skybox;

    void Awake()
    {
        _skybox = GetComponent<Skybox>();
    }

    void OnEnable()
    {
        ChangeSkybox(0);
    }

    void ChangeSkybox(int skyBox)
    {
        if (_skybox != null && skyBox >= 0 && skyBox <= _skyboxMaterials.Count)
        {
            _skybox.material = _skyboxMaterials[skyBox];
        }
    }
}
