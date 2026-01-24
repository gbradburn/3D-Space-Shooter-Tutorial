using UnityEngine;

public interface IEffect
{
    float Duration { get; }
    void Play(Vector3 position, Quaternion rotation);
}
