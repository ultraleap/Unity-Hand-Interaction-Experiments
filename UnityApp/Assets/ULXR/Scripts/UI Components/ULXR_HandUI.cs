using Leap.Unity;
using Leap.Unity.Interaction;
using Leap.Unity.PhysicalHands;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/* Handler for all hand UI */
public class ULXR_HandUI : ULXR_Singleton<ULXR_HandUI>
{
    [SerializeField] private GameObject ui;

    [Space]

    [SerializeField] private PhysicalHandsButton handButton;
    [SerializeField] private Image handButton_BG;
    [SerializeField] private Color handButton_Hover;
    [SerializeField] private Color handButton_Press;
    [SerializeField] private Color handButton_Default;

    [Space]

    [SerializeField] private Sprite _openIcon;
    [SerializeField] private Sprite _closeIcon;
    [SerializeField] private Image _icon;

    [Space]

    [SerializeField] private LeapProvider _provider = null;
    [SerializeField] private ULXR_IsIndexInTrigger _triggerCheck;

    /* Setup */
    private void Start()
    {
        if (_provider == null)
            _provider = FindObjectOfType<LeapProvider>();
        if (_provider == null)
            Debug.LogWarning("Failed to find a LeapProvider in scene! Please assign one via ULXR_HandUI.");

        ULXR_MainController.Instance.OnInterfaceStateChanged += UpdateHandIcons;
        UpdateHandIcons(ULXR_MainController.Instance.InterfaceState);
    }

    private void UpdateHandIcons(ULXR_InterfaceState state)
    {
        _icon.sprite = state == ULXR_InterfaceState.HIDING_UI ? _openIcon : _closeIcon;
    }

    /* Show/hide the actual UI based on hand visibility - stops floating UI in worldspace when hands are null */
    void Update()
    {
        if (_provider != null)
        {
            bool leftHandVisible = _provider.CurrentFrame?.Hands.FirstOrDefault(o => o.IsLeft) != null;
            if (ui.activeInHierarchy != leftHandVisible)
            {
                ui.SetActive(leftHandVisible);
                UpdateHandIcons(ULXR_MainController.Instance.InterfaceState);
            }
        }
        UpdateButtonHover();
    }

    /* Change sprite of hand button on hover & press */
    private void UpdateButtonHover()
    {
        if (handButton.IsPressed)
        {
            handButton_BG.color = handButton_Press;
        }
        else if (_triggerCheck.IsIndexInside)
        {
            handButton_BG.color = handButton_Hover;
        }
        else
        {
            handButton_BG.color = handButton_Default;
        }
    }

    /* Hand button press */
    public void OnHandButtonPress()
    {
        switch (ULXR_MainController.Instance.InterfaceState)
        {
            case ULXR_InterfaceState.FADED_UI:
            case ULXR_InterfaceState.SHOWING_UI:
                ULXR_MainController.Instance.SetInterfaceState(ULXR_InterfaceState.HIDING_UI);
                break;
            case ULXR_InterfaceState.HIDING_UI:
                ULXR_MainController.Instance.SetInterfaceState(ULXR_InterfaceState.SHOWING_UI);
                break;
        }
    }
}
