using Leap;
using Leap.Unity;
using Leap.Unity.Preview.Locomotion;
using UnityEngine;
using UnityEngine.VFX;

public class ParticlePainter : MonoBehaviour
{
    [SerializeField]
    private LightweightPinchDetector _leftPinch = null, _rightPinch = null;
    [SerializeField]
    private VisualEffect _vfx = null;
    [SerializeField]
    private GameObject _eraserVisual = null;
    [SerializeField]
    private float _eraserSize = 0.04f;

    private Vector3 _lastPosition;

    private void OnValidate()
    {
        _vfx = GetComponent<VisualEffect>();
    }

    private void Awake()
    {
        if (_leftPinch != null)
        {
            _leftPinch.OnPinch += OnPaintPinch;
            _leftPinch.OnUnpinch += OnPaintUnpinch;
            _leftPinch.OnPinching += OnPaintPinching;
        }
        if (_rightPinch != null)
        {
            _rightPinch.OnPinch += OnEraserPinch;
            _rightPinch.OnUnpinch += OnEraserUnpinch;
            _rightPinch.OnPinching += OnEraserPinching;
        }
        if (_eraserVisual != null)
        {
            _eraserVisual.SetActive(false);
        }
    }

    private void OnPaintPinch(Hand hand)
    {
        _vfx.SetVector3("PaintPosition", hand.GetPredictedPinchPosition());
        _lastPosition = hand.GetPredictedPinchPosition();
        _vfx.SetVector3("LastPaintPosition", _lastPosition);
        _vfx.SendEvent("OnPinch");
    }

    private void OnPaintUnpinch(Hand hand)
    {
        _vfx.SendEvent("OnUnpinch");
    }

    private void OnPaintPinching(Hand hand)
    {
        _vfx.SetVector3("LastPaintPosition", _lastPosition);
        _vfx.SetVector3("PaintPosition", hand.GetPredictedPinchPosition());
        _lastPosition = hand.GetPredictedPinchPosition();
    }

    private void OnEraserPinch(Hand hand)
    {
        _vfx.SetVector3("EraserPosition", hand.GetPredictedPinchPosition());
        _vfx.SetFloat("EraserSize", _eraserSize);
        if (_eraserVisual != null)
        {
            _eraserVisual.transform.position = hand.GetPredictedPinchPosition();
            _eraserVisual.transform.localScale = Vector3.one * _eraserSize;
            _eraserVisual.SetActive(true);
        }
    }

    private void OnEraserUnpinch(Hand hand)
    {
        _vfx.SetVector3("EraserPosition", Vector3.down);
        _vfx.SetFloat("EraserSize", 0f);
        if (_eraserVisual != null)
        {
            _eraserVisual.SetActive(false);
        }
    }

    private void OnEraserPinching(Hand hand)
    {
        _vfx.SetVector3("EraserPosition", hand.GetPredictedPinchPosition());
        if (_eraserVisual != null)
        {
            _eraserVisual.transform.position = hand.GetPredictedPinchPosition();
        }
    }
}
