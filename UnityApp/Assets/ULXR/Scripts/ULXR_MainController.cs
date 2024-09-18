using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity.InputModule;
using UnityEngine;

/* Main state controller */
public class ULXR_MainController : ULXR_Singleton<ULXR_MainController>
{
    [SerializeField] private ULXR_FadeUI InAppUI;

    private ULXR_InterfaceState currentInterfaceState;
    public ULXR_InterfaceState InterfaceState { get { return currentInterfaceState; } }

    public delegate void InterfaceStateChange(ULXR_InterfaceState val);
    public InterfaceStateChange OnInterfaceStateChanged;

    /* Setup */
    private void Start()
    {
        SetInterfaceState(ULXR_InterfaceState.HIDING_UI);
    }

    /* Set the state of the interface */
    public void SetInterfaceState(string state)
    {
        SetInterfaceState((ULXR_InterfaceState)Enum.Parse(typeof(ULXR_InterfaceState), state));
    }
    public void SetInterfaceState(ULXR_InterfaceState state)
    {
        if (currentInterfaceState == ULXR_InterfaceState.HIDING_UI) ULXR_RecenterPanel.Instance.Recenter();
        InAppUI.FadeOut();
        switch (state)
        {
            case ULXR_InterfaceState.SHOWING_UI:
                InAppUI.FadeIn();
                break;
            case ULXR_InterfaceState.FADED_UI:
                InAppUI.FadeOut(false, 0.4f);
                break;
        }
        currentInterfaceState = state;
        OnInterfaceStateChanged?.Invoke(currentInterfaceState);
    }
}

public enum ULXR_InterfaceState
{
    SHOWING_UI,
    FADED_UI, 
    HIDING_UI,
}