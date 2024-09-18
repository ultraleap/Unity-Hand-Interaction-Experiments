using Leap.Unity;
using Leap.Unity.PhysicalHands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HybridMarkerObject : MonoBehaviour
{
    [Tooltip("The tracked marker")]
    [SerializeField] private Transform _markerObject;
    [Tooltip("The physics object associated with the marker which we use to detect grabs")]
    [SerializeField] private PhysicalHandEvents _physicalObject;

    [Tooltip("This object gets set to the most up to date position and rotation we want to use")]
    public Transform dataObject;

    [Tooltip("This object smoothly follows the data object's position")]
    public Transform visualObject;

    private bool _usingPhysicalHandsPos = false;

    public float lerpAmount = 15f;

    private Vector3 _markerObjectPos;
    private Quaternion _markerObjectRot;

    private bool _markerObjectUpdatedThisFrame = false;
    private bool _waitForUpdatedMarkerPos = false;
    private GrabHelper grabHelper;

    public bool usePhysicalHands = true;
    
    // Start is called before the first frame update
    void Start()
    {
        if (dataObject == null)
        {
            dataObject = transform;
        }

        grabHelper = FindObjectOfType<GrabHelper>();

        _physicalObject.onGrabEnter.AddListener(OnGrab);
        _physicalObject.onGrabExit.AddListener(OnUngrab);
    }

    private void UpdateMarkerPos()
    {
        if (_markerObjectPos != _markerObject.transform.position || _markerObjectRot != _markerObject.transform.rotation)
        {
            //tracked marker transform changed

            _markerObjectPos = _markerObject.transform.position;
            _markerObjectRot = _markerObject.transform.rotation;
            _markerObjectUpdatedThisFrame = true;
        }
        else
        {
            _markerObjectUpdatedThisFrame = false;
        }
    }


    private void Update()
    {
        UpdateMarkerPos();

        if (usePhysicalHands && _usingPhysicalHandsPos) 
        {
            bool objectsAligned = AlignObjects();

            if (!objectsAligned)
            {
                SetDataObjectPosRot(_physicalObject.transform.position, _physicalObject.transform.rotation);
            }
        }
        else
        {
            if (_waitForUpdatedMarkerPos)
            {
                if (_markerObjectUpdatedThisFrame)
                {
                    _waitForUpdatedMarkerPos = false;
                }
                else
                {
                    return;
                }
            }
            SetDataObjectPosRot(_markerObjectPos, _markerObjectRot);

            _physicalObject.rigidBody.position = dataObject.position;
            _physicalObject.rigidBody.rotation = dataObject.rotation;
        }

        float lerp = _usingPhysicalHandsPos ? 60 : lerpAmount;

        visualObject.position = Vector3.Lerp(visualObject.position, dataObject.position, Time.deltaTime * lerpAmount);
        visualObject.rotation = Quaternion.Slerp(visualObject.rotation, dataObject.rotation, Time.deltaTime * lerpAmount);


    }

    private void OnGrab(ContactHand hand)
    {
        _usingPhysicalHandsPos = true;
    }

    private void OnUngrab(ContactHand hand)
    {
        _usingPhysicalHandsPos = false;
        _waitForUpdatedMarkerPos = true;
    }

    private void SetDataObjectPosRot(Vector3 pos, Quaternion rot)
    {
        dataObject.position = pos;
        dataObject.rotation = rot;
    }


    [SerializeField] float misalignedPositionCheck = 0.02f, misalignedRotCheck = 15;
    private bool AlignObjects()
    {
        if (!_markerObjectUpdatedThisFrame) return false;

        bool positionMisaligned = Vector3.Distance(_markerObjectPos, _physicalObject.transform.position) > misalignedPositionCheck;
        bool angleMisaligned = Quaternion.Angle(_markerObjectRot, _physicalObject.transform.rotation) > misalignedRotCheck;

        if (positionMisaligned || angleMisaligned)
        {
            if(positionMisaligned)
            {
            //    Debug.Log("POSITION");
            }

            if (angleMisaligned)
            {
             //   Debug.Log("ANGLE");
            }

         //   grabHelper.ResetHelper();

            _physicalObject.rigidBody.position = _markerObjectPos;
            _physicalObject.rigidBody.rotation = _markerObjectRot;

            SetDataObjectPosRot(_markerObjectPos, _markerObjectRot);
            return true;
        }

        return false;
    }
}
