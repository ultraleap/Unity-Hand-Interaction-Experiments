using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObjectFromFloor : MonoBehaviour
{
    public List<Transform> newObjectPrefab = new List<Transform>();

    public void CreateObjectFromHandPos(Vector3 _pos)
    {
        StartCoroutine(CreateAfterWait(_pos));


    }

    IEnumerator CreateAfterWait(Vector3 _atPos)
    {
        yield return new WaitForSeconds(0.2f);
        Rigidbody rbod = Instantiate(GetItemToSpawn(), _atPos + Vector3.down * 1.5f, Quaternion.identity).GetComponent<Rigidbody>();
        rbod.AddForce(Vector3.up * 300);
    }

    public Transform GetItemToSpawn()
    {
        return newObjectPrefab[Random.Range(0, newObjectPrefab.Count)];
    }
}
