using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap;
using UnityEngine.Animations;

public class SockPuppet : MonoBehaviour
{
    [SerializeField] private Transform _headTransform;
    [SerializeField] private Transform _upperMouthTransform;
    [SerializeField] private Transform _lowerMouthTransform;

    [Header("Restrictions")]
    [SerializeField, Range(0, 3)] private float _indexRotationMultiplier = 1.5f;
    [SerializeField, Range(0, 180)] private float _maxHeadYRot = 90f;
    [SerializeField, Range(0, 180)] private float _maxLowerMouthYRot = 25f;

    [Header("Other Settings")]
    [SerializeField, Range(0, 50)] private float _lerpFactor = 30f;
    [SerializeField] private Chirality _chirality = Chirality.Right;

    private LeapServiceProvider _leapProvider;

    private Quaternion _startingHeadRot = Quaternion.identity;
    private Quaternion _startingUpperRot = Quaternion.identity;
    private Quaternion _startingLowerRot = Quaternion.identity;

    private Quaternion _desiredHeadRot;
    private Quaternion _desiredUpperRot;
    private Quaternion _desiredLowerRot;

    private bool handTracked = false;


    // Start is called before the first frame update
    void Start()
    {
        _leapProvider = FindObjectOfType<LeapServiceProvider>();
        _leapProvider.OnUpdateFrame -= OnUpdateFrame;
        _leapProvider.OnUpdateFrame += OnUpdateFrame;

        _startingHeadRot = _headTransform.localRotation;
        _desiredHeadRot = _startingHeadRot;

        _startingUpperRot = _upperMouthTransform.localRotation;
        _desiredUpperRot = _startingUpperRot;

        _startingLowerRot = _lowerMouthTransform.localRotation;
        _desiredLowerRot = _startingLowerRot;
    }

    public void ResetPuppet()
    {
        if (_startingHeadRot == null || _startingHeadRot == Quaternion.identity) return;

        _desiredHeadRot = _startingHeadRot;
        _desiredUpperRot = _startingUpperRot;
        _desiredLowerRot = _startingLowerRot;

        _headTransform.localRotation = _desiredHeadRot;
        _upperMouthTransform.localRotation = _desiredUpperRot;
        _lowerMouthTransform.localRotation = _desiredLowerRot;

        enabled = false;
    }

    private void OnEnable()
    {
        foreach (ConstraintSystem c in GetComponentsInChildren<ConstraintSystem>(true))
        {
            c.gameObject.SetActive(true);
        }
    }

    private void OnUpdateFrame(Frame currentFrame)
    {
        Hand hand = currentFrame.GetHand(_chirality);
        if (hand == null)
        {
            _desiredHeadRot = _startingHeadRot;
            _desiredUpperRot = _startingUpperRot;
            _desiredLowerRot = _startingLowerRot;

            handTracked = false;
            return;
        }
        handTracked = true;

        var palmRot = hand.GetPalmPose().rotation;

        _desiredHeadRot = _startingHeadRot * Quaternion.Euler(0, 20f, 0) * palmRot;

        var upperRotDif = Quaternion.Euler(-20f, 0, 0) * Quaternion.Inverse(palmRot) * hand.GetIndex().Bone(Bone.BoneType.TYPE_PROXIMAL).Basis.rotation;
        _desiredUpperRot = _startingUpperRot * Quaternion.LerpUnclamped(Quaternion.identity, upperRotDif, _indexRotationMultiplier);

        // the following magic values work well, because the thumb has some extra rotations on it that we don't want here. Theyre not currently exposed because I don't think theyll need to change again.
        _desiredLowerRot = _startingLowerRot * Quaternion.Euler(-20f, 10f, 0) * Quaternion.Inverse(palmRot) * hand.GetThumb().Bone(Bone.BoneType.TYPE_PROXIMAL).Basis.rotation
            * Quaternion.Euler(0, 0, -65f);
    }


    // Update is called once per frame
    void Update()
    {
        if (_leapProvider == null)
        {
            _leapProvider = FindObjectOfType<LeapServiceProvider>();
            _leapProvider.OnUpdateFrame += OnUpdateFrame;
        }

        Quaternion lastHeadRot = _headTransform.localRotation;

        float handTrackedLerp = handTracked ? 1 : 1 / 10f;

        // TODO: find a lerp value that feels good..
        Quaternion newHeadRot = Quaternion.Lerp(_headTransform.localRotation, _desiredHeadRot, _lerpFactor * Time.deltaTime * handTrackedLerp);
        Quaternion newUpperRot = Quaternion.Lerp(_upperMouthTransform.localRotation, _desiredUpperRot, _lerpFactor * Time.deltaTime * handTrackedLerp);
        Quaternion newLowerRot = Quaternion.Lerp(_lowerMouthTransform.localRotation, _desiredLowerRot, _lerpFactor * Time.deltaTime * handTrackedLerp);

        // restrict head rotation
        while (Vector3.Angle(newHeadRot * Vector3.up, _startingHeadRot * Vector3.up) > _maxHeadYRot)
        {
            newHeadRot = Quaternion.RotateTowards(newHeadRot, _startingHeadRot, 1f);
        }

        // restrict lower mouth rotation
        while (Vector3.Angle(newLowerRot * Vector3.up, _startingLowerRot * Vector3.up) > _maxLowerMouthYRot)
        {
            newLowerRot = Quaternion.RotateTowards(newLowerRot, _startingLowerRot, 1f);
        }

        // if lower mouth is further up than upper mouth, reverse that:
        if (Vector3.SignedAngle(newLowerRot * Vector3.forward, newUpperRot * Vector3.forward, newLowerRot * Vector3.right) > 0)
        {
            newUpperRot = newLowerRot;
        }

        
        _headTransform.localRotation = newHeadRot;
        _upperMouthTransform.localRotation = newUpperRot;
        _lowerMouthTransform.localRotation = newLowerRot;
    }
}
