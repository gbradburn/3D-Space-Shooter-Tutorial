using System;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] AudioMixerSnapshot _patrolSnapshot, _combatSnapshot;
    [SerializeField] AudioClip[] _patrolMusic, _combatMusic;
    [SerializeField] AudioSource _patrolAudioSource, _combatAudioSource;

    int _patrolMusicIndex, _combatMusicIndex;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayCombatMusic()
    {
        _combatAudioSource.Stop();
        _combatAudioSource.clip = _combatMusic[_combatMusicIndex];
        _combatAudioSource.Play();
        _combatSnapshot.TransitionTo(1f);
        _combatMusicIndex = (_combatMusicIndex + 1) % _combatMusic.Length;        
    }

    public void PlayPatrolMusic()
    {
        _patrolAudioSource.Stop();
        _patrolAudioSource.clip = _patrolMusic[_patrolMusicIndex];
        _patrolAudioSource.Play();
        _patrolSnapshot.TransitionTo(1f);
        _patrolMusicIndex = (_patrolMusicIndex + 1) % _patrolMusic.Length;
    }

    public void PlayGameOverMusic()
    {
    }
}
