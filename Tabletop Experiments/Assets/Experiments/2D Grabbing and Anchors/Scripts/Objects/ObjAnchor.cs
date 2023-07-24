using System;
using System.Collections.Generic;
using System.Linq;

//Anchor that a grabbable can be hooked to
public class ObjAnchor : Obj
{
    private static HashSet<ObjAnchor> _instances = new HashSet<ObjAnchor>();
    public static HashSet<ObjAnchor> Instances
    {
        get
        {
            return _instances;
        }
    }

    public new const JoyObjectType Type = JoyObjectType.ANCHOR;

    //TODO: we should just set this based on grabbable anchor when it is modified really
    public ObjGrabbable AnchoredGrabbable
    {
        get
        {
            return ObjGrabbable.Instances.FirstOrDefault(o => o.Anchor == this);
        }
    }
    public bool IsAnchored
    {
        get
        {
            return AnchoredGrabbable != null;
        }
    }

    protected void Awake()
    {
        _instances.Add(this);
    }

    private void OnDestroy()
    {
        _instances.Remove(this);
    }
}
