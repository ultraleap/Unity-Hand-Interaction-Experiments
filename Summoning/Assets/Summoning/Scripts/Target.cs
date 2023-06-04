using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        RigidBodyConstrainer constrainer = collision.gameObject.GetComponent<RigidBodyConstrainer>();
        constrainer?.Constrain();
    }
}