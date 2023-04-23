using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using static MovementIndicatorPresets;
using UnityEngine.UI;
using TMPro;

//[ExecuteInEditMode ] //Uncomment this to see UI updates when in Editor mode, but be careful it can overwrite values
public class GUIControls : MonoBehaviour
{
    public WaftLocomotion waft;
    public MovementIndicatorPresets movementIndicatorPresets;

    public Slider dampingSlider, pinchDampingSlider, amplificationSlider;
    public TextMeshProUGUI dampingValue, pinchDampingValue, amplificationValue;

    public Toggle defaultToggle, marionetteToggle, dragIndicatorToggle;

    public Button ShowHideUI;
    public GameObject SettingsRoot;

    private void Start()
    {
        if (waft == null)
        {
            waft = FindObjectOfType<WaftLocomotion>();
        }

        if (movementIndicatorPresets == null)
        {
            movementIndicatorPresets = FindObjectOfType<MovementIndicatorPresets>();
        }

        switch (movementIndicatorPresets.currentPreset)
        {
            case MovementIndicatorPreset.MARIONETTE:
                marionetteToggle.isOn = true;
                break;
            case MovementIndicatorPreset.DRAG_INDICATOR:
                dragIndicatorToggle.isOn = true;
                break;
            default:
            case MovementIndicatorPreset.DEFAULT:
                defaultToggle.isOn = true;
                break;
        }

        dampingValue.text = "" + waft.damping;
        pinchDampingValue.text = "" + waft.pinchDamping;
        amplificationValue.text = "" +  waft.amplification;

        dampingSlider.value = waft.damping;
        pinchDampingSlider.value = waft.pinchDamping;
        amplificationSlider.value = waft.amplification;

        dampingSlider.onValueChanged.AddListener(delegate { OnDampingChange(); });
        pinchDampingSlider.onValueChanged.AddListener(delegate { OnPinchDampingChange(); });
        amplificationSlider.onValueChanged.AddListener(delegate { OnAmplificationChange(); });

        defaultToggle.onValueChanged.AddListener(delegate { movementIndicatorPresets.SetPreset(MovementIndicatorPreset.DEFAULT); });
        marionetteToggle.onValueChanged.AddListener(delegate { movementIndicatorPresets.SetPreset(MovementIndicatorPreset.MARIONETTE); });
        dragIndicatorToggle.onValueChanged.AddListener(delegate { movementIndicatorPresets.SetPreset(MovementIndicatorPreset.DRAG_INDICATOR); });

        ShowHideUI.onClick.AddListener(delegate { SettingsRoot.SetActive(!SettingsRoot.activeInHierarchy); }) ;
    }

    void OnDampingChange()
    {
        waft.damping = dampingSlider.value;
        dampingValue.text = "" + dampingSlider.value;
    }
    
    void OnPinchDampingChange()
    {
        waft.pinchDamping = pinchDampingSlider.value;
        pinchDampingValue.text = "" + pinchDampingSlider.value;
    }
    
    void OnAmplificationChange()
    {
        waft.amplification = amplificationSlider.value;
        amplificationValue.text = "" + amplificationSlider.value;
    }
}
