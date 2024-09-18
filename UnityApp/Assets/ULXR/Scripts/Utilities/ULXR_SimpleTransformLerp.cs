using UnityEngine;

public class ULXR_SimpleTransformLerp : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;

    [Space]

    [SerializeField] private Transform _startTransform;
    [SerializeField] private Transform _endTransform;

    private float _deltaModifier = 20.0f;
    private bool _isPlayingForward = false;

    public void PlayForward()
    {
        _isPlayingForward = true;
    }

    public void PlayBackward()
    {
        _isPlayingForward = false;
    }

    private void Update()
    {
        _targetTransform.localRotation = Quaternion.Lerp(_targetTransform.localRotation, (_isPlayingForward) ? _endTransform.localRotation : _startTransform.localRotation, Time.deltaTime * _deltaModifier);
        _targetTransform.localPosition = Vector3.Lerp(_targetTransform.localPosition, (_isPlayingForward) ? _endTransform.localPosition : _startTransform.localPosition, Time.deltaTime * _deltaModifier);
        _targetTransform.localScale = Vector3.Lerp(_targetTransform.localScale, (_isPlayingForward) ? _endTransform.localScale : _startTransform.localScale, Time.deltaTime * _deltaModifier);
    }
}