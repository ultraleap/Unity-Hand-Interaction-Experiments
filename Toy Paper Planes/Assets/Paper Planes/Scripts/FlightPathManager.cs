using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightPathManager : MonoBehaviour
{
    public static FlightPathManager Instance;

    public Transform[] playerPositions;

    public Transform flightLeaderPrefab;

    public float minSpeed = 1f;
    public float maxSpeed = 2f;

    public Vector3 maxOffset;

    private void Start()
    {
        Instance = this;
    }

    public FlightLeader CreateFlightPathLeader()
    {
        var leader = Instantiate(flightLeaderPrefab).GetComponent<FlightLeader>();

        leader.Setup(Random.Range(0, playerPositions.Length),
            this,
            Random.Range(minSpeed, maxSpeed),
            new Vector3(Random.Range(-maxOffset.x, maxOffset.x), Random.Range(-maxOffset.y, maxOffset.y), Random.Range(-maxOffset.z, maxOffset.z)));

        return leader;
    }
}