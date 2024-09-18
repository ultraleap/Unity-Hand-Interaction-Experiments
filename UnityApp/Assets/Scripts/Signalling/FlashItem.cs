using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashItem : MonoBehaviour
{
    [SerializeField] [ColorUsage(false, true)] private Color _glow = new Color(0, 21.36125f, 2.23678f);
    [SerializeField] [ColorUsage(false, true)] private Color _noglow = new Color(0, 0, 0);

    private Color _target = new Color(0, 0, 0);
    private Material _mat;
    private Coroutine _coroutine;

    [SerializeField] private bool _flashOnStart = false;

    public bool Flashing => _flashing;
    private bool _flashing = false;

    private void Awake()
    {
        _mat = GetComponent<MeshRenderer>().material;
        _noglow = GetEmission();

        if (_flashOnStart)
            DoFlash(true);
    }

    private void OnDestroy()
    {
        DoFlash(false);
    }

    public void DoFlash(bool active)
    {
        if (_flashing == active)
            return;

        _flashing = active;

        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        if (active)
            _coroutine = StartCoroutine(GlowCoroutine());
        else
            _target = _noglow;
    }

    private IEnumerator GlowCoroutine()
    {
        while (true)
        {
            _target = _glow;
            yield return new WaitForSeconds(1.0f);
            _target = _noglow;
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void Update()
    {
        Color c = GetEmission();
        if (c != _target)
            _mat.SetColor("_EmissionColor", Color.Lerp(c, _target, Time.deltaTime * 10.0f));
    }

    private Color GetEmission()
    {
        return _mat.GetColor("_EmissionColor");
    }
}
