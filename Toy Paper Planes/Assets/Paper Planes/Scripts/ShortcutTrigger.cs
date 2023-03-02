using UnityEngine;
using UnityEngine.Events;

using Leap.Unity;

public class ShortcutTrigger : MonoBehaviour
{
    public Chirality chirality;

    public LeapProvider _provider;
    public float triggerThreshold = 0.8f;
    public float releaseThreshold = 0.7f;

    public UnityEvent OnShortcutTriggered;
    public UnityEvent OnShortcutReleased;

    bool triggered = false;

    public SkinnedMeshRenderer pinchSphere;

    private void OnEnable()
    {
        if (_provider == null)
            _provider = FindObjectOfType<LeapProvider>();

        _provider.OnPostUpdateFrame += PositionShorcuts;

        OnShortcutReleased?.Invoke();
        triggered = false;
    }

    private void OnDisable()
    {
        _provider.OnPostUpdateFrame -= PositionShorcuts;
        OnShortcutReleased?.Invoke();
    }

    void PositionShorcuts(Leap.Frame frame)
    {
        bool foundHand = false;

        foreach (Leap.Hand hand in frame.Hands)
        {
            if (chirality == Chirality.Left ? hand.IsLeft : hand.IsRight)
            {
                foundHand = true;

                Vector3 thumbTip = hand.GetThumb().TipPosition;
                Vector3 indexTip = hand.GetIndex().TipPosition;

                transform.position = (thumbTip + indexTip) / 2;
                transform.forward = thumbTip - indexTip;

                pinchSphere.SetBlendShapeWeight(0, hand.PinchStrength * 100);

                if (!triggered)
                {
                    if(hand.PinchStrength > triggerThreshold)
                    {
                        OnShortcutTriggered?.Invoke();
                        triggered = true;
                    }
                }
                else
                {
                    if(hand.PinchStrength < releaseThreshold)
                    {
                        OnShortcutReleased?.Invoke();
                        triggered = false;
                    }
                }

                if(hand.PinchStrength > 0.95f)
                {
                    pinchSphere.enabled = false;
                }
                else
                {
                    pinchSphere.enabled = true;
                }
            }
        }

        if(!foundHand && triggered)
        {
            OnShortcutReleased?.Invoke();
            triggered = false;
        }
    }
}