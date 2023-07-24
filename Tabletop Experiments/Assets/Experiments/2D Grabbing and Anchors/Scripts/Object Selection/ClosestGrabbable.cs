using Leap.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Work out what grabbable is closest to our hand
public class ClosestGrabbable : Closest
{
    public ObjGrabbable ClosestObject
    {
        get
        {
            return Calculate();
        }
    }

    //This is a more expensive call and should not be utilised often.
    public List<ObjGrabbable> ClosestObjects
    {
        get
        {
            return CalculateListInternal(JoyObjectType.GRABBABLE)?.Cast<ObjGrabbable>()?.ToList();
        }
    }

    public ObjGrabbable Calculate()
    {
        return (ObjGrabbable)CalculateInternal(JoyObjectType.GRABBABLE);
    }
}
