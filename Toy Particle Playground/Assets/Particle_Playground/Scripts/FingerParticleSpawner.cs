using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.VFX;

public class FingerParticleSpawner : MonoBehaviour
{
    public LeapProvider provider;
    public VisualEffect visualEffect;

    Vector3 leftHandPrev = Vector3.zero;
    Vector3 rightHandPrev = Vector3.zero;

    private void Awake()
    {
        provider.OnUpdateFrame += OnHandFrame;
    }

    private void OnHandFrame(Frame _newFrame)
    {
        var hand = _newFrame.GetHand(Chirality.Right);

        if (hand != null)
        {
            // Spawning
            if(IsFingerPointing(hand, out Vector3 spawnPos, out Vector3 spawnDir))
            {
                visualEffect.SetBool("SpawnR", true);
                visualEffect.SetVector3("SpawnPosR", spawnPos);
                visualEffect.SetVector3("SpawnDirectionR", spawnDir);
            }
            else
            {
                visualEffect.SetBool("SpawnR", false);
            }

            // Interacting

            visualEffect.SetBool("isPinchingRight", hand.IsPinching());
            visualEffect.SetVector3("RightHandPos", hand.PalmPosition);
            visualEffect.SetVector3("RightHandPrevPos", rightHandPrev);

            rightHandPrev = hand.PalmPosition;
        }
        else
        {
            visualEffect.SetBool("SpawnR", false);
            visualEffect.SetBool("isPinchingRight", false);
            visualEffect.SetVector3("RightHandPos", Vector3.zero);
            visualEffect.SetVector3("RightHandPrevPos", Vector3.zero);
        }

        hand = _newFrame.GetHand(Chirality.Left);

        if (hand != null)
        {
            // Spawning
            if (IsFingerPointing(hand, out Vector3 spawnPos, out Vector3 spawnDir))
            {
                visualEffect.SetBool("SpawnL", true);
                visualEffect.SetVector3("SpawnPosL", spawnPos);
                visualEffect.SetVector3("SpawnDirectionL", spawnDir);
            }
            else
            {
                visualEffect.SetBool("SpawnL", false);
            }

            // Interacting

            visualEffect.SetBool("isPinchingLeft", hand.IsPinching());
            visualEffect.SetVector3("LeftHandPos", hand.PalmPosition);
            visualEffect.SetVector3("LeftHandPrevPos", leftHandPrev);

            leftHandPrev = hand.PalmPosition;
        }
        else
        {
            visualEffect.SetBool("SpawnL", false);
            visualEffect.SetBool("isPinchingLeft", false);
            visualEffect.SetVector3("LeftHandPos", Vector3.zero);
            visualEffect.SetVector3("LeftHandPrevPos", Vector3.zero);
        }
    }

    bool IsFingerPointing(Hand _hand, out Vector3 _spawnPos, out Vector3 _spawnDir)
    {
        Finger index = _hand.GetIndex();

        if (index.IsExtended && 
            !_hand.GetRing().IsExtended)
        {
            if(Vector3.Dot(index.Direction, Camera.main.transform.forward) > 0.2f)
            {
                _spawnPos = index.TipPosition;
                _spawnDir = index.Direction;
                return true;
            }
        }

        _spawnPos = Vector3.zero;
        _spawnDir = Vector3.zero;

        return false;
    }
}
