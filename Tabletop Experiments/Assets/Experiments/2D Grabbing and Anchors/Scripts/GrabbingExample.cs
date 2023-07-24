using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbingExample : MonoBehaviour
{
    [SerializeField] private List<AnchorPair> _defaultPairs;

    void Start()
    {
        for (int i = 0; i < _defaultPairs.Count; i++)
        {
            _defaultPairs[i].Grabbable.Anchor = _defaultPairs[i].Anchor;
        }
    }

    [System.Serializable]
    class AnchorPair
    {
        public ObjGrabbable Grabbable;
        public ObjAnchor Anchor;
    }
}
