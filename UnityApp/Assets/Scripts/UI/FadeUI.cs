using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Fade controller for UI CanvasGroup */
public class FadeUI : MonoBehaviour
{
    [SerializeField] private bool _doVisionProAnim = false;
    [SerializeField] private Vector3 _visionProAnimOffset = new Vector3(0.0f, 0.0f , 100.0f);
    [SerializeField] private AnimationCurve _visionProCurve;

    private Vector3 _startPos = Vector3.zero;

    [Space]

    public Action<bool> FadeTriggered;
    public Action<bool> FadeComplete;

    [Tooltip("Optional: A mesh which will also fade its alpha colour channel in sync with the canvas group.")]
    [SerializeField] private MeshRenderer _linkedMesh = null;

    [SerializeField] private bool startsInvisible = true;
    private CanvasGroup _canvasGroup;

    public float TransitionTime { get { return _transitionTime; } set { _transitionTime = value; } }

    [SerializeField]
    private float _transitionTime = 0.1f;
    private float _transitionTimeCurrent = 0.0f;

    private float _currentOpacity = 0.0f;
    private float _targetOpacity = 0.0f;
    public float CurrentOpacity => _currentOpacity;

    private bool _isVisible = false;
    public bool IsVisible => _isVisible;

    private bool _fading = false;

    private bool _fadeInCalled = false;

    public bool disableOnFade = true;

    Coroutine _fadeOutCoroutine = null;
    Coroutine _fadeInCoroutine = null;

    [SerializeField]
    private float _disableOnFadeDelay = 0f;

    private void Start()
    {
        _startPos = this.transform.localPosition;

        if (_fadeInCalled)
            return;

        if (disableOnFade && startsInvisible)
        {
            if (!gameObject.activeInHierarchy || _disableOnFadeDelay == 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(DelayedDisable());
            }
        }
    }

    private IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(_disableOnFadeDelay);
        gameObject.SetActive(false);
    }

    /* Fade all elements out */
    public void FadeOut() { FadeOut(false, 0.0f); }
    public void FadeOut(bool immediate = false, float opacity = 0.0f, bool force = false, float _delayedStart = 0.0f)
    {
        if (_currentOpacity == 0 && disableOnFade && !force)
        {
            return;
        }

        if (!_isVisible && !force)
            return;

        Setup();
        _targetOpacity = opacity;
        _isVisible = false;
        _transitionTimeCurrent = 0.0f;

        FadeTriggered?.Invoke(false);

        if (immediate || !gameObject.activeInHierarchy)
        {
            if (_fadeOutCoroutine != null )
                StopCoroutine(_fadeOutCoroutine);
            if (_fadeInCoroutine != null)
                StopCoroutine(_fadeInCoroutine);

            ApplySettings();
        }
        else
        {
            if (!_fading)
            {
                _fadeOutCoroutine = StartCoroutine(FadeCoroutine(false, _delayedStart));
            }
        }
    }

    /* Fade all elements in */
    public void FadeIn(bool immediate = false)
    {
        if (disableOnFade)
        {
            _fadeInCalled = true;
            gameObject.SetActive(true);
        }

        if (_isVisible)
            return;

        Setup();
        _targetOpacity = 1.0f;
        _isVisible = true;
        _transitionTimeCurrent = 0.0f;

        FadeTriggered?.Invoke(true);

        if (immediate || !gameObject.activeInHierarchy)
        {
            if (_fadeOutCoroutine != null)
                StopCoroutine(_fadeOutCoroutine);
            if (_fadeInCoroutine != null)
                StopCoroutine(_fadeInCoroutine);

            ApplySettings();
        }
        else
        {
            if (!_fading)
            {
                _fadeInCoroutine = StartCoroutine(FadeCoroutine(true));
            }
        }
    }

    /* Toggle UI support */
    public void Toggle(Toggle toggle)
    {
        if (toggle.isOn)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }

    /* Grab references to all things that will be potentially modified by our fade */
    private void Awake()
    {
        Setup();
        if (startsInvisible)
        {
            FadeOut(true, 0.0f, true);
        }
        else
        {
            _currentOpacity = 1.0f;
            FadeIn();
        }
    }

    private void OnEnable()
    {
        if (startsInvisible)
        {
            FadeOut(true, 0.0f, true);
        }
    }

    private void OnDisable()
    {
        FadeOut(true, 0.0f, true);
    }

    private void Setup()
    {
        if (_canvasGroup != null)
        {
            return;
        }
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    /* Perform the fade if required */
    private IEnumerator FadeCoroutine(bool fadingIn, float delay = 0.0f)
    {
        _fading = true;

        if (delay != 0.0f)
            yield return new WaitForSeconds(delay);

        if (_doVisionProAnim)
            this.transform.localPosition = fadingIn ? _startPos - _visionProAnimOffset : _startPos;

        while (_transitionTimeCurrent <= _transitionTime)
        {
            _transitionTimeCurrent += Time.deltaTime;
            _currentOpacity = Mathf.Lerp(_currentOpacity, _targetOpacity, Time.deltaTime / (1.0f * _transitionTime));
            _canvasGroup.alpha = _currentOpacity;

            if (_linkedMesh != null)
            {
                for (int i = 0; i < _linkedMesh.materials.Length; i++)
                {
                    if (_currentOpacity == 1.0f)
                        _linkedMesh.materials[i].SetFloat("_Surface", 0.0f);
                    else
                        _linkedMesh.materials[i].SetFloat("_Surface", 1.0f);

                    _linkedMesh.materials[i].color = new Color(_linkedMesh.materials[i].color.r, _linkedMesh.materials[i].color.g, _linkedMesh.materials[i].color.b, _currentOpacity < 0.25f ? 0.0f : _currentOpacity > 0.75f ? 1.0f : Unity.Mathematics.math.remap(0.25f, 0.75f, 0.0f, 1.0f, _currentOpacity));
                }
            }

            if (_doVisionProAnim)
            {
                if (fadingIn)
                    this.transform.localPosition = Vector3.Lerp(_startPos - _visionProAnimOffset, _startPos, _visionProCurve.Evaluate(_transitionTimeCurrent / _transitionTime));
                else
                    this.transform.localPosition = Vector3.Lerp(_startPos, _startPos - _visionProAnimOffset, _visionProCurve.Evaluate(_transitionTimeCurrent / _transitionTime));
            }

            yield return null;
        }
        ApplySettings();

        if (_doVisionProAnim)
            this.transform.localPosition = fadingIn ? _startPos : _startPos - _visionProAnimOffset;

        FadeComplete?.Invoke(_targetOpacity == 1);
        if (disableOnFade && _targetOpacity == 0)
        {
            gameObject.SetActive(false);
        }

        if (fadingIn)
            _fadeInCoroutine = null;
        else
            _fadeOutCoroutine = null;
    }

    private void ApplySettings()
    {
        _currentOpacity = _targetOpacity;

        _canvasGroup.alpha = _currentOpacity;
        _canvasGroup.blocksRaycasts = (_canvasGroup.alpha >= 0.01f);
        _canvasGroup.interactable = _canvasGroup.blocksRaycasts;

        if (_linkedMesh != null)
        {
            for (int i = 0; i < _linkedMesh.materials.Length; i++)
            {
                if (_currentOpacity == 1.0f)
                    _linkedMesh.materials[i].SetFloat("_Surface", 0.0f);
                else
                    _linkedMesh.materials[i].SetFloat("_Surface", 1.0f);

                _linkedMesh.materials[i].color = new Color(_linkedMesh.materials[i].color.r, _linkedMesh.materials[i].color.g, _linkedMesh.materials[i].color.b, _currentOpacity);
            }
        }

        _fading = false;
    }

    public void SetStartsInvisible(bool startsInvisible)
    {
        this.startsInvisible = startsInvisible;
    }
}
