using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyConstrainer : MonoBehaviour
{
    public bool constrainable = true;

    public void Constrain()
    {
        if (constrainable)
        {
            Rigidbody rbod = GetComponent<Rigidbody>();
            rbod.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    public void Unconstrain()
    {
        Rigidbody rbod = GetComponent<Rigidbody>();
        rbod.constraints = RigidbodyConstraints.None;
    }
}
