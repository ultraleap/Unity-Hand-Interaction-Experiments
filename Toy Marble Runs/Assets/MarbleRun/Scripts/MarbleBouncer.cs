using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleBouncer : MonoBehaviour
{
    public float bounciness = 10;

    List<Marble> recentlyBounced = new List<Marble>();

    IEnumerator HandleRecentlyBounced(Marble marble)
    {
        recentlyBounced.Add(marble);

        yield return new WaitForSeconds(0.2f);

        if(marble != null)
            recentlyBounced.Remove(marble);
    }

    private void OnCollisionStay(Collision collision)
    {
        Marble marble = collision.gameObject.GetComponent<Marble>();

        if(marble != null && !recentlyBounced.Contains(marble))
        {
            marble.GetComponent<Rigidbody>().AddForce(transform.up * bounciness);

            StartCoroutine(HandleRecentlyBounced(marble));
        }
    }
}
