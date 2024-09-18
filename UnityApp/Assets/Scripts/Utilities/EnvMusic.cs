using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvMusic : MonoSingleton<EnvMusic>
{
    [SerializeField] private AudioSource _source1;
    [SerializeField] private AudioSource _source2;

    [Space]

    [SerializeField] private AudioClip _sound;

    [Space]

    [SerializeField] private float _loopTime;

    void Start()
    {
        _source1.clip = _sound;
        _source2.clip = _sound;

        _source1.Play();
        _source2.Stop();
    }

    void Update()
    {
        if (_source1.time >= _loopTime && !_source2.isPlaying)
        {
            Debug.Log("Changing to audio source 2...");
            _source2.time = 0;
            _source2.Play();
        }
        if (_source2.time >= _loopTime && !_source1.isPlaying)
        {
            Debug.Log("Changing to audio source 1...");
            _source1.time = 0;
            _source1.Play();
        }
    }
}
