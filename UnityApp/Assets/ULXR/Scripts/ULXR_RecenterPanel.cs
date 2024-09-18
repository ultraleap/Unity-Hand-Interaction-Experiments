using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine;

/* Recenter the UI infront of the user */
public class ULXR_RecenterPanel : ULXR_Singleton<ULXR_RecenterPanel>
{
    [Header("Objects")]
    [SerializeField] private GameObject originDummy;
    [SerializeField] private GameObject userInterface;

    [Header("Panel Transform Data")]
    [SerializeField] public float uiForwardOffset = 0.32f;
    [SerializeField] public float uiVerticalOffset = 0.26f;
    [SerializeField] private float uiScale = 0.25f;
    [SerializeField] public float lerpSpeed = 3.11f;

    /* Position the grab ball in a nice place & align UI to it */
    public void Recenter()
    {
        userInterface.transform.localScale = new Vector3(uiScale, uiScale, uiScale);

        Vector3 newPos = Camera.main.transform.position + (Camera.main.transform.forward * uiForwardOffset);
        newPos.y = Camera.main.transform.position.y - uiVerticalOffset;
        userInterface.transform.position = newPos;
        userInterface.transform.rotation = CalculateLookAt(true);

        originDummy.transform.position = userInterface.transform.position;
        originDummy.transform.rotation = CalculateLookAt(false);
    }
    public Quaternion CalculateLookAt(bool clampToY = false)
    {
        if (!clampToY) return Quaternion.LookRotation(Camera.main.transform.forward);
        return Quaternion.AngleAxis(Quaternion.LookRotation(Camera.main.transform.forward).eulerAngles.y, Vector3.up);
    }
}
