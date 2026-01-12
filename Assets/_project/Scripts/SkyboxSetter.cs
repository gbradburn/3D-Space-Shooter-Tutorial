using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Skybox))]
public class SkyboxSetter : MonoBehaviour
{
    [SerializeField] List<Material> _skyboxMaterials;

    Skybox _skybox;

    void Awake()
    {
        _skybox = GetComponent<Skybox>();
    }

    void OnEnable()
    {
        ChangeSkybox(Random.Range(0, _skyboxMaterials.Count));
    }

    void ChangeSkybox(int skyBox)
    {
        if (_skybox && skyBox >= 0 && skyBox <= _skyboxMaterials.Count)
        {
            _skybox.material = _skyboxMaterials[skyBox];
        }
    }
}
