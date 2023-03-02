using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnfoldGliderOnEnable : MonoBehaviour
{
    public Animation anim;

    public AudioSource audioSource;
    public AudioClip[] audioClips;

    private void OnEnable()
    {
        anim.Play();
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
    }
}
