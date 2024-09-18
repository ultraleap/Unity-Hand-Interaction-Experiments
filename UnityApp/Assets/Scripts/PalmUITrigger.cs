using Leap;
using Leap.Unity;
using Leap.Unity.PhysicalHands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmUITrigger : MonoBehaviour
{
    private PalmUIManager _palmUIManager;
    [SerializeField] private float _maxRadius = 0.1f;

    private bool _triggered = false;

    private LeapProvider _serviceProvider;

    private void Awake()
    {
        _palmUIManager = FindAnyObjectByType<PalmUIManager>();
        _serviceProvider = Hands.Provider;
    }

    private void Update()
    {
        if (_triggered)
        {
            Hand leftHand = _serviceProvider.GetHand(Chirality.Left);

            if (leftHand == null || Vector3.Distance(leftHand.PalmPosition, transform.position) < _maxRadius)
            {
                _palmUIManager.RemoveTriggeredObject("objectName");
                _triggered = false;
            }
        }
    }



    [SerializeField] private string objectName;
    private void OnTriggerEnter(Collider col)
    {
        HardContactBone bone = col.GetComponent<HardContactBone>();

        // If the left palm is intersecting, hide the palm ui
        if (bone != null && bone.contactHand.Handedness == Leap.Unity.Chirality.Left && col.gameObject.name.ToLower().Contains("palm"))
        {
            _palmUIManager.AddTriggeredObject("objectName");
            _triggered = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        HardContactBone bone = col.GetComponent<HardContactBone>();

        // If the left palm is intersecting, hide the palm ui
        if (bone != null && bone.contactHand.Handedness == Leap.Unity.Chirality.Left && col.gameObject.name.ToLower().Contains("palm"))
        {
            _palmUIManager.RemoveTriggeredObject("objectName");
            _triggered = false;
        }
    }
}
