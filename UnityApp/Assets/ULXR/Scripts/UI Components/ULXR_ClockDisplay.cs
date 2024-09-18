using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* Displays current time on text UI */
public class ULXR_ClockDisplay : MonoBehaviour
{
    private TextMeshProUGUI uiText;
    private void Start()
    {
        uiText = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        uiText.text = DateTime.Now.ToString("t").ToUpper();
    }
}
