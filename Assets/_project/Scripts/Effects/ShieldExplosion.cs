using System.Security.Cryptography;
using UnityEngine;

public class ShieldExplosion : MonoBehaviour
{
    [SerializeField] private Light _pointLight;
    [SerializeField] private ParticleSystem _particleSystem;
    private void Update()
    {
        if (_pointLight == null) return;
        if (_pointLight.range > 0)
        {
            _pointLight.range -= 2 * Time.deltaTime;
        }

        if (_pointLight.intensity > 0)
        {
            _pointLight.intensity -= 1 * Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }

    }
}
