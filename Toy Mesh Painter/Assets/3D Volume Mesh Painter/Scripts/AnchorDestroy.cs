using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;
using System.Linq;

public class AnchorDestroy : MonoBehaviour
{
    Anchor anchor;

    private void Awake()
    {
        anchor = GetComponent<Anchor>();
    }

    public void OnAnchored()
    {
        StartCoroutine(RemoveAnchoredAfterWait());
    }

    IEnumerator RemoveAnchoredAfterWait()
    {
        yield return new WaitForEndOfFrame();

        var anchoredObjects = (anchor.anchoredObjects).ToArray();

        for (int i = 0; i < anchoredObjects.Length; i++)
        {
            anchoredObjects[i].Detach();
            Destroy(anchoredObjects[i].gameObject);
        }
    }
}