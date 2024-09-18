using Leap.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* ULXR change synced value on button or toggle press */
public class ULXR_EnableDisableButton : MonoBehaviour
{
    [SerializeField] private GameObject enabledIcon;
    [SerializeField] private GameObject disabledIcon;
    [SerializeField] string syncValue;

    private Toggle toggle;
    private Button button;
    private CompressibleUI compressibleUI;

    /* Setup */
    void Start()
    {
        toggle = GetComponent<Toggle>();
        button = GetComponent<Button>();
        compressibleUI = GetComponent<CompressibleUI>();

        ULXR_ValueTracker.Instance.GetSyncBoolByName(syncValue).ValueChanged += UpdateUI;
        Set(ULXR_ValueTracker.Instance.GetSyncBoolByName(syncValue).Get());
    }

    /* Update button UI */
    private void UpdateUI(bool isOn)
    {
        if (enabledIcon) enabledIcon.SetActive(isOn);
        if (disabledIcon) disabledIcon.SetActive(!isOn);

        if (toggle != null)
        {
            toggle.isOn = isOn;
            //if (isOn) ((Image)toggle.targetGraphic).sprite = toggle.spriteState.pressedSprite;
            //else ((Image)toggle.targetGraphic).sprite = toggle.spriteState.disabledSprite;
            //compressibleUI.DivideLayerHeightsOnToggle(toggle);
        }
    }

    /* Set value */
    public void Toggle()
    {
        Set(!ULXR_ValueTracker.Instance.GetSyncBoolByName(syncValue).Get());
    }
    public void Set(bool val)
    {
        ULXR_ValueTracker.Instance.GetSyncBoolByName(syncValue).Set(val);
    }
}
