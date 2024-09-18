using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FlashItem))]
[RequireComponent(typeof(FadeUI))]
public class FadingFlashItem : MonoBehaviour
{
    public bool Shown => _shown;
    private bool _shown = false;

    private FlashItem _flash;
    private FadeUI _fade;

    void Awake()
    {
        _flash = GetComponent<FlashItem>();
        _fade = GetComponent<FadeUI>();

        _flash.DoFlash(false);
        _fade.FadeOut();
    }

    public void Show(bool show)
    {
        if (_shown == show)
            return;

        if (show)
        {
            _flash.DoFlash(true);
            _fade.FadeIn();
        }
        else
        {
            _flash.DoFlash(false);
            _fade.FadeOut();
        }

        _shown = show;
    }
}
