using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

using UnityEngine.Events;
using System;

[Serializable]
public class CreateAtPosEvent : UnityEvent<Vector3> { }

public class SummonFromGround : MonoBehaviour
{
    enum SummonState
    {
        IDLE,
        VELOCITY_MET,
        SUSTAIN_MET,
        COOLDOWN
    }

    public LeapProvider leapProvider;

    public Chirality targetHand;

    SummonState currentState = SummonState.IDLE;

    [Tooltip("X and Z are maximum allowed, Y is minimum required")]
    public Vector3 velocityTargetThresholds;

    [Tooltip("Time in S between possible TeleCreates")]
    public float cooldownLength;

    float currentCooldown;

    float prevFrameTimestamp;

    public float minYDistance = 0.05f;

    float completeYPos;

    public CreateAtPosEvent OnCreate;

    private void Awake()
    {
        if (leapProvider != null)
        {
            leapProvider.OnUpdateFrame += OnLeapFrame;
        }
    }

    private void OnLeapFrame(Frame _frame)
    {
        Hand activeHand = _frame.GetHand(targetHand);

        float timestampInS = Time.realtimeSinceStartup;

        if (activeHand != null)
        {
            HandleState(activeHand, timestampInS - prevFrameTimestamp);
        }
        else
        {
            ResetValues(SummonState.IDLE);
        }

        prevFrameTimestamp = timestampInS;
    }

    void HandleState(Hand _hand, float _timeDeltaS)
    {
        switch (currentState)
        {
            case SummonState.IDLE:
                if(CheckHandPose(_hand) && CheckVelocityThreshold(_hand.PalmVelocity))
                {
                    currentState = SummonState.VELOCITY_MET;
                    completeYPos = _hand.PalmPosition.y + minYDistance;
                }
                break;
            case SummonState.VELOCITY_MET:
                if (CheckHandPose(_hand) && CheckVelocityThreshold(_hand.PalmVelocity))
                {
                    if(_hand.PalmPosition.y > completeYPos)
                    {
                        currentState = SummonState.SUSTAIN_MET;
                    }
                }
                else
                {
                    ResetValues(SummonState.IDLE);
                }
                break;
            case SummonState.SUSTAIN_MET:
                // Spawn object here
                OnCreate.Invoke(_hand.PalmPosition);
                ResetValues();
                break;
            case SummonState.COOLDOWN:

                currentCooldown += _timeDeltaS;
                if(currentCooldown >= cooldownLength)
                {
                    currentCooldown = 0;
                    currentState = SummonState.IDLE;
                }
                break;
        }
    }

    void ResetValues(SummonState _newState = SummonState.COOLDOWN)
    {
        currentState = _newState;
    }

    bool CheckHandPose(Hand _hand)
    {
        bool grabStrengthCheck = _hand.GrabStrength < 0.2f;
        bool palmDirectionCheck = Vector3.Dot(_hand.PalmNormal, Vector3.down) >= 0.7f;

        return grabStrengthCheck && palmDirectionCheck;
    }

    bool CheckVelocityThreshold(Vector3 _palmVelocity)
    {
        if (_palmVelocity.y > velocityTargetThresholds.y)
        {
            if(Mathf.Abs(_palmVelocity.x) < velocityTargetThresholds.x &&
                Mathf.Abs(_palmVelocity.z) < velocityTargetThresholds.z)
            {
                return true;
            }
        }

        return false;
    }
}