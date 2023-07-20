using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField]
    LeapProvider _inputProvider;

    [Header("Camera"), SerializeField]
    private bool _modifyCamera = true;
    [SerializeField]
    private bool _cameraZoom = true, _pullToZoom = false, _cameraRotate = false, _cameraRotateAround = false, _cameraPan = false;
    [SerializeField]
    private Camera _camera;
    [SerializeField, Range(0, 180)]
    private float _cameraFOVMin;
    [SerializeField, Range(0, 180)]
    private float _cameraFOVMax;
    [SerializeField, Range(-90, 90)]
    private float _cameraXRotMin, _cameraXRotMax;
    [SerializeField, Range(-90, 90)]
    private float _cameraXAroundRotMin, _cameraXAroundRotMax;
    [SerializeField, Range(-1, 2)]
    private float _cameraPanMin, _cameraPanMax;
    private float _currentFOV, _desiredFOV, _currentXRot, _desiredXRot, _currentXAroundRot, _desiredXAroundRot, _currentPan, _desiredPan;

    [Header("Object"), SerializeField]
    private bool _modifyObject = false;
    [SerializeField]
    private bool _objectSize = false, _objectRotation = false, _objectPan = false;
    [SerializeField]
    private Transform _panTransform, _scaleTransform;
    [SerializeField, Range(0, 4)]
    private float _objectSizeMin;
    [SerializeField, Range(0, 4)]
    private float _objectSizeMax;
    [SerializeField, Range(-1, 2)]
    private float _objectPanMin, _objectPanMax;
    private float _currentObjectSize, _desiredObjectSize, _currentObjectRot, _desiredObjectRot, _currentObjectPan, _desiredObjectPan;

    [Header("Interaction Settings"), SerializeField]
    private float _lerpSpeed = 0.5f;

    [SerializeField]
    private bool _usePinch = false, _useGrab = true;

    [SerializeField]
    private float _grabAmount = 0.5f, _pinchDistance = 0.02f;

    [SerializeField]
    private float _handDistanceScale = 0.5f, _pullScale = 0.5f, _rotScale = 0.5f, _rotateAroundScale = 0.5f, _cameraPanScale = 0.5f, _objectSizeScale = 0.5f, _objectRotationScale = 0.5f, _objectPanScale = 0.5f;

    private Hand _leftHand, _rightHand;
    private bool _wasGrabbing = false;
    private float _oldGrabDistance = 0f, _oldGrabHeight = 0f, _oldPullDistance = 0f, _oldHandDot = 0f;
    private Vector3 _originalPan, _originalObjectPan;

    private void OnValidate()
    {
        if (_camera == null)
        {
            _camera = FindObjectOfType<Camera>();
        }
        if (_inputProvider == null)
        {
            _inputProvider = FindObjectOfType<LeapProvider>();
        }
    }

    private void Awake()
    {
        _currentFOV = _camera.fieldOfView;
        _desiredFOV = _camera.fieldOfView;
        _currentXRot = _camera.transform.rotation.eulerAngles.x;
        _desiredXRot = _currentXRot;
        _currentXAroundRot = _camera.transform.parent.rotation.eulerAngles.x;
        _desiredXAroundRot = _currentXAroundRot;
        _currentPan = _camera.transform.parent.position.y;
        _originalPan = _camera.transform.parent.position;
        _desiredPan = _currentPan;
        if (_scaleTransform != null)
        {
            _currentObjectSize = _scaleTransform.localScale.x;
            _desiredObjectSize = _currentObjectSize;
            _currentObjectRot = _scaleTransform.rotation.eulerAngles.y;
            _desiredObjectRot = _currentObjectRot;
        }
        if (_panTransform != null)
        {
            _originalObjectPan = _panTransform.position;
            _currentObjectPan = _panTransform.position.y;
            _desiredObjectPan = _currentObjectPan;
        }
    }

    private void Update()
    {
        _leftHand = _inputProvider.GetHand(Chirality.Left);
        _rightHand = _inputProvider.GetHand(Chirality.Right);

        if (_leftHand != null && _rightHand != null &&
            ((_usePinch && (_leftHand.PinchDistance / 1000f) < _pinchDistance && (_rightHand.PinchDistance / 1000f) < _pinchDistance)
            || (_useGrab && _leftHand.GetFistStrength() > _grabAmount && _rightHand.GetFistStrength() > _grabAmount)))
        {
            ProcessHandData();
        }
        else
        {
            _wasGrabbing = false;
        }
        if (_modifyCamera)
        {
            if (_cameraZoom)
            {
                if (Mathf.Abs(_currentFOV - _desiredFOV) > 1e-5f)
                {
                    _currentFOV = Mathf.Lerp(_currentFOV, _desiredFOV, Time.deltaTime * (1.0f / _lerpSpeed));

                    _camera.fieldOfView = _currentFOV;
                }
            }
            if (_cameraRotate)
            {
                if (_cameraRotateAround)
                {
                    if (Mathf.Abs(_currentXAroundRot - _desiredXAroundRot) > 1e-7f)
                    {
                        _currentXAroundRot = Mathf.Lerp(_currentXAroundRot, _desiredXAroundRot, Time.deltaTime * (1.0f / _lerpSpeed));
                        _camera.transform.parent.rotation = Quaternion.Euler(_currentXAroundRot, 0, 0);
                    }
                }
                else
                {
                    if (Mathf.Abs(_currentXRot - _desiredXRot) > 1e-7f)
                    {
                        _currentXRot = Mathf.Lerp(_currentXRot, _desiredXRot, Time.deltaTime * (1.0f / _lerpSpeed));
                        _camera.transform.rotation = Quaternion.Euler(_currentXRot, 0, 0);
                    }
                }
            }
            if (_cameraPan)
            {
                if (Mathf.Abs(_currentPan - _desiredPan) > 1e-7f)
                {
                    _currentPan = Mathf.Lerp(_currentPan, _desiredPan, Time.deltaTime * (1.0f / _lerpSpeed));
                    _camera.transform.parent.position = new Vector3(_originalPan.x, _currentPan, _originalPan.z);
                }
            }
        }

        if (_modifyObject)
        {
            if (Mathf.Abs(_currentObjectSize - _desiredObjectSize) > 1e-5f)
            {
                _currentObjectSize = Mathf.Lerp(_currentObjectSize, _desiredObjectSize, Time.deltaTime * (1.0f / _lerpSpeed));

                _scaleTransform.localScale = Vector3.one * _currentObjectSize;
            }

            if (Mathf.Abs(_currentObjectPan - _desiredObjectPan) > 1e-7f)
            {
                _currentObjectPan = Mathf.Lerp(_currentObjectPan, _desiredObjectPan, Time.deltaTime * (1.0f / _lerpSpeed));
                _panTransform.transform.position = new Vector3(_originalObjectPan.x, _currentObjectPan, _originalObjectPan.z);
            }

            if (Mathf.Abs(_currentObjectPan - _desiredObjectPan) > 1e-7f)
            {
                _currentObjectRot = Mathf.Lerp(_currentObjectRot, _desiredObjectRot, Time.deltaTime * (1.0f / _lerpSpeed));
                _scaleTransform.rotation = Quaternion.Euler(0, _currentObjectRot, 0);
            }
        }
    }

    private void ProcessHandData()
    {
        float currentXDistance = _leftHand.PalmPosition.x - _rightHand.PalmPosition.x;
        float pullDistance = (_leftHand.PalmPosition.z + _rightHand.PalmPosition.z) / 2f;
        float currentYHeight = (_leftHand.PalmPosition.y + _rightHand.PalmPosition.y) / 2f;
        float currentDot = Vector3.Dot((_leftHand.PalmPosition - _rightHand.PalmPosition).normalized, Vector3.forward);
        if (!_wasGrabbing)
        {
            _oldHandDot = currentDot;
            _oldGrabDistance = currentXDistance;
            _oldPullDistance = pullDistance;
            _oldGrabHeight = currentYHeight;
            _wasGrabbing = true;
        }

        if (_cameraZoom)
        {
            if (_pullToZoom)
            {
                _desiredFOV += (pullDistance - _oldPullDistance) * _pullScale;
            }
            else
            {
                _desiredFOV += (currentXDistance - _oldGrabDistance) * _handDistanceScale;
            }
            _desiredFOV = Mathf.Clamp(_desiredFOV, _cameraFOVMin, _cameraFOVMax);
        }
        if (_cameraRotate)
        {
            if (_cameraRotateAround)
            {
                _desiredXAroundRot += (currentYHeight - _oldGrabHeight) * _rotateAroundScale;
            }
            else
            {
                _desiredXRot += (currentYHeight - _oldGrabHeight) * _rotScale;
            }
            _desiredXRot = Mathf.Clamp(_desiredXRot, _cameraXRotMin, _cameraXRotMax);
            _desiredXAroundRot = Mathf.Clamp(_desiredXAroundRot, _cameraXAroundRotMin, _cameraXAroundRotMax);
        }
        if (_cameraPan)
        {
            _desiredPan += (currentYHeight - _oldGrabHeight) * _cameraPanScale;
            _desiredPan = Mathf.Clamp(_desiredPan, _cameraPanMin, _cameraPanMax);
        }

        if (_objectSize)
        {
            if (_pullToZoom)
            {
                _desiredObjectSize += (pullDistance - _oldPullDistance) * _objectSizeScale;
            }
            else
            {
                _desiredObjectSize += (currentXDistance - _oldGrabDistance) * _objectSizeScale;
            }
            _desiredObjectSize = Mathf.Clamp(_desiredObjectSize, _objectSizeMin, _objectSizeMax);
        }

        if (_objectPan)
        {
            _desiredObjectPan += (currentYHeight - _oldGrabHeight) * _objectPanScale;
            _desiredObjectPan = Mathf.Clamp(_desiredObjectPan, _objectPanMin, _objectPanMax);
        }

        if (_objectRotation)
        {
            _desiredObjectRot += (currentDot - _oldHandDot) * _objectRotationScale;
        }

        _oldHandDot = currentDot;
        _oldGrabDistance = currentXDistance;
        _oldPullDistance = pullDistance;
        _oldGrabHeight = currentYHeight;
    }
}
