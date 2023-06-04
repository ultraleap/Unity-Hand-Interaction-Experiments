using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractorManager : MonoBehaviour
{
    public LeapProvider prov;

    public GameObject rayInteractorObjectRight;
    public GameObject directInteractorObjectRight;

    public GameObject rayInteractorObjectLeft;
    public GameObject directInteractorObjectLeft;

    public void Awake()
    {
        prov.OnUpdateFrame += LeapFrame;
    }

    private void LeapFrame(Frame _frame)
    {
        Hand hand = _frame.GetHand(Chirality.Left);

        HandleInteractors(hand, rayInteractorObjectLeft, directInteractorObjectLeft);

        hand = _frame.GetHand(Chirality.Right);

        HandleInteractors(hand, rayInteractorObjectRight, directInteractorObjectRight);
    }

    void HandleInteractors(Hand _hand, GameObject _rayInteractor, GameObject _directInteractor)
    {
        if (_hand != null)
        {
            if (Vector3.Dot(_hand.PalmNormal, Vector3.up) > 0.6f)
            {
                _rayInteractor.SetActive(true);
                _directInteractor.SetActive(false);
            }
            else
            {
                _rayInteractor.SetActive(false);
                _directInteractor.SetActive(true);
            }
        }
    }
}