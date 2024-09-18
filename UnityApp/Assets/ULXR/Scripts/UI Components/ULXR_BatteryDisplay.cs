using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/* Displays battery info in UI */
public class ULXR_BatteryDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI percentText;
    [SerializeField] private GameObject percentIcon;
    [SerializeField] private Image batteryOverlay;

    private void Start()
    {
        ULXR_ValueTracker.Instance.battery.ValueChanged += UpdatePercentUI;
    }
    int tempBattery = 0;
    private void Update()
    {
        if (SystemInfo.batteryLevel == -1) return;
        tempBattery = (int)(SystemInfo.batteryLevel * 100.0f);
        if (tempBattery != ULXR_ValueTracker.Instance.battery.Get())
            ULXR_ValueTracker.Instance.battery.Set(tempBattery);
    }
    private void UpdatePercentUI(int percent)
    {
        percentIcon.transform.localScale = new Vector3((float)percent / 100.0f, 1, 1);
        percentText.text = percent + "%";
    }
}
