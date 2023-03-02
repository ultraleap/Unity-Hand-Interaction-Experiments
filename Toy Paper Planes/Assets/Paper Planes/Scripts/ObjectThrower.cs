using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;

public class ObjectThrower : MonoBehaviour
{
    public Chirality chirality;

    public LeapProvider leapProvider;

    public Transform objectToSpawnPrefab;
    public Transform spawnAtTransform;

    const int VELOCITY_CACHE_SIZE = 10;

    private Queue<Vector3> velocityList = new Queue<Vector3>();
    Vector3 previousPalmPosition;
    Vector3 Velocity;
    Vector3 palmNormal;
    long prevTimestamp;

    public float throwVelocityMultiplier = 80;

    float throwDelay = 0.2f;

    float lastThrowTime = 0;

    private void OnEnable()
    {
        if (leapProvider == null)
            leapProvider = FindObjectOfType<LeapProvider>();

        leapProvider.OnUpdateFrame -= OnLeapFrame;
        leapProvider.OnUpdateFrame += OnLeapFrame;
    }

    private void OnDisable()
    {
        leapProvider.OnUpdateFrame -= OnLeapFrame;
    }

    private void OnLeapFrame(Leap.Frame _newFrame)
    {
        var hand = _newFrame.GetHand(chirality);

        if (hand != null)
        {
            if (previousPalmPosition == Vector3.zero)
            {
                prevTimestamp = _newFrame.Timestamp;
            }

            float deltaTime = (_newFrame.Timestamp - prevTimestamp) / 1e+6f;

            Vector3 palmPosition = hand.PalmPosition;
            Velocity = (palmPosition - previousPalmPosition) / deltaTime;
            if (velocityList.Count >= VELOCITY_CACHE_SIZE)
            {
                velocityList.Dequeue();
            }
            if (velocityList.Count < VELOCITY_CACHE_SIZE)
            {
                velocityList.Enqueue(Velocity);
            }

            previousPalmPosition = palmPosition;
            prevTimestamp = _newFrame.Timestamp;
            palmNormal = hand.PalmNormal;
        }
    }

    public void ThrowObject()
    {
        if(Time.realtimeSinceStartup - lastThrowTime <= throwDelay)
        {
            return;
        }

        lastThrowTime = Time.realtimeSinceStartup;

        var highestVelocity = Vector3.zero;
        foreach (Vector3 v in velocityList)
        {
            if (v.magnitude > highestVelocity.magnitude && Vector3.Dot(v.normalized, palmNormal) > 0.4f)
            {
                highestVelocity = v;
            }
        }

        if (highestVelocity.magnitude > 0.5f)
        {
            var rbody = Instantiate(objectToSpawnPrefab, spawnAtTransform.position, spawnAtTransform.rotation).GetComponent<Rigidbody>();
            rbody.velocity = highestVelocity;
        }
    }
}