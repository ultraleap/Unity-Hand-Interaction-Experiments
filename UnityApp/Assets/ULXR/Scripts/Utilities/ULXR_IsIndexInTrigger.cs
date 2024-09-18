using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ULXR_IsIndexInTrigger : MonoBehaviour
{
    public bool IsIndexInside => _isIndexInside;
    private bool _isIndexInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name != "IndexFingerCol")
            return;

        _isIndexInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name != "IndexFingerCol")
            return;

        _isIndexInside = false;
    }
}
