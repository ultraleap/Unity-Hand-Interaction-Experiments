using System;
using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

public class PourCoffee : MonoBehaviour
{
    public bool IsPouring => _isPouring;
    private bool _isPouring = false;

    public bool CupIsUnder
    {
        get
        {
            if (_cupIsUnderLastFrameCheck == Time.frameCount)
                return _cupIsUnder;
            bool firstHit = Physics.OverlapSphere(_liquidRaycastPos.position, _widthOfLiquidRaycast, _liquidColliderLayersInclude).Length != 0;
            bool hit = Physics.SphereCast(_liquidRaycastPos.position, _widthOfLiquidRaycast, Vector3.down, out RaycastHit info, _lengthOfLiquidRaycast, _liquidColliderLayersInclude);
            _cupIsUnder = (firstHit || hit) && info.collider.gameObject.layer == _liquidColliderLayersSucceed.GetFirstLayerIndex();
            _cupIsUnderLastFrameCheck = Time.frameCount;

            return _cupIsUnder;
        }
    }
    private bool _cupIsUnder = false;
    private int _cupIsUnderLastFrameCheck = -1;

    public Action<OutputSize, OutputType, float> OnPourStart;
    public Action<OutputSize, OutputType, float> OnPourEnd;

    [SerializeField] TextMeshProUGUI _interfaceText;

    [Space]

    [SerializeField] ParticleSystem _coffeeParticle;
    [SerializeField] ParticleSystem _milkParticle;

    [Space]

    [SerializeField] Transform _liquidRaycastPos;
    [SerializeField] float _lengthOfLiquidRaycast;
    [SerializeField] float _widthOfLiquidRaycast;
    [SerializeField] LayerMask _liquidColliderLayersInclude;
    [SerializeField] LayerMask _liquidColliderLayersSucceed;

    [Space]

    [SerializeField]
    [Range(0, 1)] float _maxSfxVol = 1.0f;
    [SerializeField] AudioSource _audioSource;

    private Coroutine _warningCoroutine = null, _pouringCoroutine = null;
    private float _targetSFXVol = 0.0f;

    private OutputSize _activeSize;
    private OutputType _activeType;
    private float _activeDuration;
    private ParticleSystem _activeParticles;

    private bool _cupFull = false;

    private string idleText = "Ready...";

    public void OnMachineButtonPress(string buttonID)
    {
        switch (buttonID)
        {
            case "coffee_small":
                StartPour(OutputSize.SMALL, OutputType.COFFEE, 2.5f);
                break;
            case "coffee_medium":
                StartPour(OutputSize.MEDIUM, OutputType.COFFEE, 5.0f);
                break;
            case "coffee_large":
                StartPour(OutputSize.LARGE, OutputType.COFFEE, 7.5f);
                break;
            case "milk_small":
                StartPour(OutputSize.SMALL, OutputType.MILK, 1.5f);
                break;
            case "milk_medium":
                StartPour(OutputSize.MEDIUM, OutputType.MILK, 2.5f);
                break;
            case "milk_large":
                StartPour(OutputSize.LARGE, OutputType.MILK, 3.5f);
                break;
        }
    }

    public void SetCupFull(bool full)
    {
        if (_cupFull == full) return;

        _cupFull = full;

        idleText = _cupFull ? "Coffee Poured!" : "Ready...";

        if (_warningCoroutine == null && _pouringCoroutine == null)
        {
            _interfaceText.text = idleText;
        }
    }

    private void StartPour(OutputSize size, OutputType type, float duration)
    {
        if (_cupFull) return;

        if (_isPouring)
        {
            if (_warningCoroutine == null)
                _warningCoroutine = StartCoroutine(WarningCoroutine());
            return;
        }

        if (_warningCoroutine != null)
        {
            StopCoroutine(_warningCoroutine);
            _warningCoroutine = null;
        }

        _pouringCoroutine = StartCoroutine(PourCoroutine(size, type, duration));
    }


    private IEnumerator PourCoroutine(OutputSize size, OutputType type, float duration)
    {
        _activeSize = size;
        _activeType = type;
        _activeDuration = duration;

        _isPouring = true;
        _interfaceText.text = "Pouring " + type + "...";

        _activeParticles = type == OutputType.MILK ? _milkParticle : _coffeeParticle;
        var main = _activeParticles.main;
        main.duration = duration;
        _activeParticles.Play();
        _targetSFXVol = _maxSfxVol;

        OnPourStart?.Invoke(size, type, duration);
        yield return new WaitForSeconds(duration);
        _pouringCoroutine = StartCoroutine(StopPourCoroutine());
    }

    private void StopPour()
    {
        if (_isPouring)
        {
            StopCoroutine(_pouringCoroutine);
            _pouringCoroutine = null;
            _pouringCoroutine = StartCoroutine(StopPourCoroutine());
        }
    }

    private IEnumerator StopPourCoroutine()
    {
        _activeParticles.Stop();
        _targetSFXVol = 0.0f;
        yield return new WaitForSeconds(0.01f);

        _isPouring = false;
        _interfaceText.text = idleText;

        OnPourEnd?.Invoke(_activeSize, _activeType, _activeDuration);
        _pouringCoroutine = null;
    }

    private IEnumerator WarningCoroutine()
    {
        string prevText = _interfaceText.text;
        for (int i = 0; i < 2; i++)
        {
            _interfaceText.text = "Already Pouring!";
            yield return new WaitForSeconds(0.7f);
            _interfaceText.text = "Please Wait!";
            yield return new WaitForSeconds(0.7f);
        }

        if (_isPouring)
            _interfaceText.text = prevText;
        else
            _interfaceText.text = idleText;

        _warningCoroutine = null;
    }

    private void Update()
    {
        if (_audioSource.volume != _targetSFXVol)
        {
            if (_targetSFXVol == _maxSfxVol && _audioSource.volume == 0.0f && !_audioSource.isPlaying)
            {
                _audioSource.time = 0.0f;
                _audioSource.Play();
            }

            _audioSource.volume = Mathf.Lerp(_audioSource.volume, _targetSFXVol, Time.deltaTime * 10.0f);

            if (_targetSFXVol == 0.0f && _audioSource.volume <= 0.01f && _audioSource.isPlaying)
            {
                _audioSource.volume = 0.0f;
                _audioSource.Stop();
            }
            else if (_targetSFXVol == _maxSfxVol && _audioSource.volume >= _maxSfxVol - 0.01f)
            {
                _audioSource.volume = _maxSfxVol;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_liquidRaycastPos.position, _liquidRaycastPos.position + (Vector3.down * _lengthOfLiquidRaycast));
        Gizmos.DrawSphere(_liquidRaycastPos.position, _widthOfLiquidRaycast);
        Gizmos.DrawSphere(_liquidRaycastPos.position + (Vector3.down * _lengthOfLiquidRaycast), _widthOfLiquidRaycast);
    }

    public enum OutputSize
    {
        SMALL, MEDIUM, LARGE
    }

    public enum OutputType
    {
        MILK, COFFEE
    }
}
