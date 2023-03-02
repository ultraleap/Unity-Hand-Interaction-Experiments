using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightLeader : MonoBehaviour
{
    public FlightPathManager flightPath;

    int currentTargetID = 0;

    public float speed = 1;

    Vector3 offset;

    Vector3 targetPos;

    public void Setup(int _targetID, FlightPathManager _path, float _speed, Vector3 _offset)
    {
        flightPath = _path;
        currentTargetID = _targetID;
        speed = _speed;
        offset = _offset;

        targetPos = flightPath.playerPositions[currentTargetID].position + offset;
        transform.position = targetPos;
        transform.LookAt(targetPos);
    }

    void Update()
    {
        transform.position += (targetPos - transform.position).normalized * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            MoveToNextTarget();
        }
    }

    void MoveToNextTarget()
    {
        if (currentTargetID == flightPath.playerPositions.Length - 1)
        {
            currentTargetID = 0;
        }
        else
        {
            currentTargetID++;
        }

        targetPos = flightPath.playerPositions[currentTargetID].position + offset;
        transform.LookAt(targetPos);
    }
}