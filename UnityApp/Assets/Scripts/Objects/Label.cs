using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.PhysicalHands;

[RequireComponent(typeof(PhysicalHandEvents))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FlashItem))]
public class Label : MonoBehaviour
{
    public bool HasBeenGrabbed => _hasBeenGrabbed;
    private bool _hasBeenGrabbed = false;

    private Rigidbody _rigidbody;
    private PhysicalHandEvents _phe;
    private FlashItem _flash;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _phe = GetComponent<PhysicalHandEvents>();
        _flash = GetComponent<FlashItem>();

        _phe.onGrabEnter.AddListener(OnGrabEnter);
        _phe.onGrabExit.AddListener(OnGrabExit);

        _flash.DoFlash(true);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnGrabEnter(ContactHand hand)
    {
        _hasBeenGrabbed = true;
        _flash.DoFlash(false);
    }

    private void OnGrabExit(ContactHand hand)
    {
        _rigidbody.isKinematic = false;
        StartCoroutine(KeepNonKinematic()); //override grab helper
    }
    private IEnumerator KeepNonKinematic()
    {
        while (true)
        {
            if (_rigidbody.isKinematic)
            {
                _rigidbody.isKinematic = false;
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
