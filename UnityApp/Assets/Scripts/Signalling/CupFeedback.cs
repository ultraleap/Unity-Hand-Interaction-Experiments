using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class CupFeedback : MonoBehaviour
{
    [Header("Cup settings")]
    [SerializeField] private float _bottomWidth;
    [SerializeField] private float _topWidth;
    [SerializeField] private float _height;

    [Header("Coffee settings")]
    [SerializeField] private Color _coffeeColour;
    [SerializeField] private Color _milkColour;

    [Space] 

    [Range(0, 1)]
    [SerializeField] private float _max;

    [Space]

    [SerializeField] private Transform _fill;

    private float _targetSFXVol = 0.0f;

    public float FillPercent
    {
        get
        {
            return Mathf.Clamp(_milkProgress + _coffeeProgress, 0.0f, 1.0f);
        }
    }

    public float MilkPercent
    {
        get
        {
            return Mathf.Clamp(_milkProgress / 1.0f, 0.0f, 1.0f);
        }
    }

    public float CoffeePercent
    {
        get
        {
            return Mathf.Clamp(_coffeeProgress / 1.0f, 0.0f, 1.0f);
        }
    }

    private AudioSource _audioSource;
    private MeshRenderer _fillRenderer;
    private float _milkProgress = 0.0f;
    private float _coffeeProgress = 0.0f;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _fillRenderer = _fill.GetComponent<MeshRenderer>();
    }

    public void DoSFX(bool underNozzle)
    {
        if (underNozzle && !_audioSource.isPlaying)
            _targetSFXVol = 1.0f;

        if (!underNozzle && _audioSource.isPlaying)
            _targetSFXVol = 0.0f;
    }

    public void AddMilk(float val)
    {
        if (FillPercent >= 1.0f)
            return;

        _milkProgress += val;
        UpdateVisual();
    }

    public void AddCoffee(float val)
    {
        if (FillPercent >= 1.0f)
            return;

        _coffeeProgress += val;
        UpdateVisual();
    }

    private float ComputeDiameter()
    {
        return ((_topWidth - _bottomWidth) * FillPercent * _max) + _bottomWidth;
    }

    private void UpdateVisual()
    {
        Vector3 scale = _fill.localScale;
        scale.x = ComputeDiameter() / 2;
        scale.z = ComputeDiameter() / 2;
        _fill.localScale = scale;

        Vector3 position = _fill.localPosition;
        position.y = FillPercent * _height * _max;
        _fill.localPosition = position;

        _fillRenderer.material.color = ((_coffeeColour * CoffeePercent) + (_milkColour * MilkPercent)) / FillPercent;
    }

    private void Update()
    {
        if (_audioSource.volume != _targetSFXVol)
        {
            if (_targetSFXVol == 1.0f && _audioSource.volume == 0.0f && !_audioSource.isPlaying)
            {
                _audioSource.Play();
            }

            _audioSource.volume = Mathf.Lerp(_audioSource.volume, _targetSFXVol, Time.deltaTime * 20.0f);

            if (_targetSFXVol == 0.0f && _audioSource.volume <= 0.01f && _audioSource.isPlaying)
            {
                _audioSource.volume = 0.0f;
                _audioSource.Pause();
            }
            else if (_targetSFXVol == 1.0f && _audioSource.volume >= 0.99f)
            {
                _audioSource.volume = 1.0f;
            }
        }
    }
}
