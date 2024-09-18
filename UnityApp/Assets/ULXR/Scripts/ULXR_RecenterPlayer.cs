using System.Collections;
using UnityEngine;
using System;

public class ULXR_RecenterPlayer : ULXR_Singleton<ULXR_RecenterPlayer>
{
    public Action OnPlayerRecenter, OnPlayerRecenterComplete;

    public void RecentreWithFade(float fadeTime)
    {
        StartCoroutine(RecenterCoroutine(fadeTime));
    }

    /* Recenter button press */
    public void OnRecenterButtonPress()
    {
        StartCoroutine(RecenterCoroutine(ULXR_Client.Instance.ViewBlocker.BlockTimer));
    }

    /* Recenter after time with fade */
    private IEnumerator RecenterCoroutine(float fadeTime)
    {
        float oldBlockTimer = ULXR_Client.Instance.ViewBlocker.BlockTimer;
        ULXR_Client.Instance.ViewBlocker.BlockTimer = fadeTime;
        OnPlayerRecenter?.Invoke();
        ULXR_Client.Instance.ViewBlocker.FadeIn();
        yield return new WaitForSeconds(ULXR_Client.Instance.ViewBlocker.BlockTimer);

        RecenterPlayer();

        ULXR_Client.Instance.ViewBlocker.BlockTimer = oldBlockTimer;
        ULXR_MainController.Instance.SetInterfaceState(ULXR_InterfaceState.HIDING_UI);
        ULXR_RecenterPanel.Instance.Recenter();
        ULXR_Client.Instance.ViewBlocker.FadeOut();
        OnPlayerRecenterComplete?.Invoke();
    }



    /* Recenter player based on Leap cam and current GO */
    public void RecenterPlayer()
    {
        if (Camera.main.transform.parent == null)
        {
            GameObject parent = new GameObject("PlayerParent");
            Camera.main.transform.parent = parent.transform;
        }

        RotatePlayer(transform.rotation);
        MovePlayer(transform.position);
    }


    private void RotatePlayer(Quaternion newRotation)
    {
        Transform cameraOffset = Camera.main.transform.parent;
        float yRotation = (newRotation * Quaternion.Inverse(Camera.main.transform.rotation) * Camera.main.transform.parent.rotation).eulerAngles.y;
        cameraOffset.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void MovePlayer(Vector3 newPosition)
    {
        Transform cameraOffset = Camera.main.transform.parent;
        cameraOffset.position -= Camera.main.transform.position - gameObject.transform.transform.position;
    }
}
