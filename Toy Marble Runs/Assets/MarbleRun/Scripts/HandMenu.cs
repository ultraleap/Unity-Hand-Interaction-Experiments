using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenu : MonoBehaviour
{
    public Transform[] pieces;

    public void SpawnPiece(int pieceIndex)
    {
        Instantiate(pieces[pieceIndex], Camera.main.transform.position + (Camera.main.transform.forward * 0.3f), pieces[pieceIndex].rotation);
    }
}
