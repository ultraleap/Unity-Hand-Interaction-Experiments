using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base object that anchors and grabbables inherit from
public class Obj : MonoBehaviour
{
    public Vector3 PositionWorldSpace
    {
        get
        {
            return this.transform.position;
        }
    }
    public Vector3 PositionScreenSpace
    {
        get
        {
            return Camera.main.WorldToScreenPoint(this.transform.position);
        }
    }

    private bool _isActive = true;
    public bool IsActive
    {
        get
        {
            return !gameObject.activeInHierarchy ? false : _isActive;
        }
        set
        {
            _isActive = value;
        }
    }

    public const JoyObjectType Type = JoyObjectType.NOT_ASSIGNED;
}

public enum JoyObjectType
{
    GRABBABLE,
    ANCHOR,

    NOT_ASSIGNED, //Logical error if we ever use this
}