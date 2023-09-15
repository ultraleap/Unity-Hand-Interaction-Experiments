using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Marble marble = collision.gameObject.GetComponent<Marble>();

        if (marble != null)
        {
            Destroy(marble.gameObject);
        }
    }
}
