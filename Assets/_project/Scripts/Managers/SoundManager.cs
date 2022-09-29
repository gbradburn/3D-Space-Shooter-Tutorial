using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] AudioMixerGroup _soundEffectsGroup;
    
    AudioSource _audioSource;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _audioSource = GetComponent<AudioSource>();
    }

    public static AudioSource Configure3DAudioSource(AudioSource audioSource)
    {
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1.0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 500f;
        audioSource.outputAudioMixerGroup = Instance._soundEffectsGroup;
        return audioSource;
    }

    public void PlayClip(AudioClip clip, float volume = 1f)
    {
        _audioSource.PlayOneShot(clip, volume);
    }
}
