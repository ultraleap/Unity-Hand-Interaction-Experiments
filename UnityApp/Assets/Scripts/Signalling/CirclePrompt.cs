using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePrompt : MonoBehaviour
{
    [SerializeField] List<GameObject> _objects = new List<GameObject>();
    [SerializeField] FlashItem _flash;

    public bool Shown => _shown;
    private bool _shown;

    private void Awake()
    {
        ShowPrompt(false);
    }

    public void ShowPrompt(bool show)
    {
        _shown = show;

        //todo: fade
        _objects.ForEach(o => o.SetActive(show));

        _flash.DoFlash(show);
    }
}
