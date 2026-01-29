using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.FloatingOrigin
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ShiftableParticleSystem : ShiftableFloatingOrigin
    {
        ParticleSystem _particleSystem;
        ParticleSystem.Particle[] _particles;

        void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            var maxParticles = _particleSystem.main.maxParticles;
            _particles = new ParticleSystem.Particle[maxParticles];
        }

        public override void OnShift(Vector3 offset)
        {
            base.OnShift(offset);

            if (!_particleSystem) return;

            var count = _particleSystem.GetParticles(_particles);
            
            for (var i = 0; i < count; i++)
            {
                _particles[i].position += offset;
            }

            _particleSystem.SetParticles(_particles, count);
        }
    }
}
