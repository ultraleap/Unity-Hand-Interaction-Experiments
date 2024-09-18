using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupTrigger : MonoBehaviour
{
    public bool IsInside => _isInside;
    private bool _isInside = false;

    public Action OnEntered;
    public Action OnExited;

    public bool Activated => _activated;
    private bool _activated = false;

    [SerializeField] private Location _triggerLocation;
    public Location TriggerLocation => _triggerLocation;

    [SerializeField] private CirclePrompt _prompt;

    public enum Location
    {
        COFFEE_MACHINE,
        FINAL_DESTINATION,

        NOWHERE,
    }

    private void Awake()
    {
        _prompt.ShowPrompt(false);
    }

    public void Activate()
    {
        _prompt.ShowPrompt(true);
        _activated = true;
    }

    public void Deactivate()
    {
        _prompt.ShowPrompt(false);
        _activated = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_activated)
            return;
        if (other.gameObject.tag != "Cup")
            return;

        OnEntered?.Invoke();
        _isInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_activated)
            return;
        if (other.gameObject.tag != "Cup")
            return;

        OnExited?.Invoke();
        _isInside = false;
    }
}
