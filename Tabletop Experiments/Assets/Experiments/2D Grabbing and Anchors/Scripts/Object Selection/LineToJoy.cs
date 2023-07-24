using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Draw a line from our hand to a joy object
[RequireComponent(typeof(LineRenderer))]
public class LineToJoy : MonoBehaviour
{
    [SerializeField] private bool _doLine = true;

    LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public bool DrawTo(Obj obj)
    {
        if (obj == null) 
        {
            _lineRenderer.enabled = false;
            return false;
        }
        if (!_doLine) return false;
        _lineRenderer.enabled = true;
        _lineRenderer.SetPositions(new Vector3[] { this.transform.position, obj.PositionWorldSpace });
        return true;
    }
}
