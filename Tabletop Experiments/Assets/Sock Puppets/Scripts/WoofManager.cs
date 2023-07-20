using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoofManager : MonoBehaviour
{
    public LeapProvider leapProvider;
    public Chirality chirality = Chirality.Right;
    public AudioSource audioSource;
    public AudioClip woof;
    public AudioClip bark;


    public float[] pitches = new float[] { 0.9f, 0.95f, 1, 1.1f, 1.25f, 1.3f};

    private bool grabbingLastFrame;
    // Start is called before the first frame update
    void Start()
    {
        if (leapProvider == null)
        {
            leapProvider = FindObjectOfType<LeapProvider>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Hand hand = leapProvider.CurrentFrame.GetHand(chirality);

        if (hand != null)
        {
            bool grabbing = IsGrabbing(hand);
            if (!grabbingLastFrame && grabbing)
            {
                audioSource.Stop();
            }

            if(grabbingLastFrame && !grabbing)
            {
                audioSource.clip = Random.Range(0, 100) > 85 ? bark : woof;
                audioSource.pitch = pitches[Random.Range(0, pitches.Length)];
                audioSource.Play();
            }

            if (!grabbingLastFrame && !grabbing)
            {
                // Vary pitch based on grab strength... needs work
               // audioSource.pitch = Unity.Mathematics.math.remap(1, 0, 0.6f, 1.5f, hand.GrabStrength);
            }

            grabbingLastFrame = grabbing;
        }
    }

    private bool IsGrabbing(Hand hand)
    {
        if (hand == null) return false;
        return hand.GrabStrength > 0.75f || hand.PinchStrength > 0.75f;
    }
}
