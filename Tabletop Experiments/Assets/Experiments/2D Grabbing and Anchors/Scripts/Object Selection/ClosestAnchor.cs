using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

//Work out the closest anchor to hand
public class ClosestAnchor : Closest
{
    public ObjAnchor ClosestObject
    {
        get
        {
            return Calculate();
        }
    }

    //This is a more expensive call and should not be utilised often.
    public List<ObjAnchor> ClosestObjects
    {
        get
        {
            return CalculateListInternal(JoyObjectType.ANCHOR)?.Cast<ObjAnchor>()?.ToList();
        }
    }

    public ObjAnchor Calculate()
    {
        return (ObjAnchor)CalculateInternal(JoyObjectType.ANCHOR);
    }
}
