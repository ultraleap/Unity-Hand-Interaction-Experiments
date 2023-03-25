using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPiece : MonoBehaviour
{
    public Transform spawnPrefab;

    public Transform spawnPoint;

    public float spawnTimer;

    float lastSpawn;

    bool canSpawn = true;

    void Update()
    {
        if(Time.realtimeSinceStartup - lastSpawn > spawnTimer)
        {
            lastSpawn = Time.realtimeSinceStartup;
            Spawn();
        }
    }

    public void Spawn(bool force = false)
    {
        if(canSpawn || force)
            Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    public void ToggleSpawn()
    {
        canSpawn = !canSpawn;
    }
}
