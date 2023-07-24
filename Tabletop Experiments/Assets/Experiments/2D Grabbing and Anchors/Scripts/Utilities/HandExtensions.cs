using Leap.Unity;
using System;
using UnityEngine;

[Serializable]
public enum LeapPos
{
    PALM_POS,
    INDEX_TIP_POS,
    INDEX_BASE_POS,
    THUMB_TIP_POS,
    THUMB_BASE_POS,
    //INDEX_THUMB_TIP_MIDPOINT_POS,
    //INDEX_THUMB_BASE_MIDPOINT_POS,
} 

public static class HandExtensions
{
    private const float _grabThreshold = 0.75f; //todo: modulate this by dist to sensor?

    public static Vector3 ScreenSpace(this Leap.Hand hand, LeapPos position)
    {
        if (hand == null) return Vector3.zero;

        switch (position)
        {
            case LeapPos.PALM_POS:
                return Camera.main.WorldToScreenPoint(hand.PalmPosition);
            case LeapPos.INDEX_TIP_POS:
                return Camera.main.WorldToScreenPoint(hand.Fingers[1].TipPosition);
            case LeapPos.INDEX_BASE_POS:
                return Camera.main.WorldToScreenPoint(hand.Fingers[1].bones[0].Center);
            case LeapPos.THUMB_TIP_POS:
                return Camera.main.WorldToScreenPoint(hand.Fingers[0].TipPosition);
            case LeapPos.THUMB_BASE_POS:
                return Camera.main.WorldToScreenPoint(hand.Fingers[0].bones[0].Center);
            default:
                return Vector3.negativeInfinity;
        }
    }

    //TODO: fine tune this based on tracking volume
    public static bool IsGrabbing(this Leap.Hand hand)
    {
        if (hand == null) return false;
        return hand.GrabStrength > _grabThreshold || hand.PinchStrength > _grabThreshold;
    }

    public static Leap.Hand Get(Chirality chirality)
    {
        return chirality == Chirality.Right ? Hands.Right : chirality == Chirality.Left ? Hands.Left : null;
    }
}
