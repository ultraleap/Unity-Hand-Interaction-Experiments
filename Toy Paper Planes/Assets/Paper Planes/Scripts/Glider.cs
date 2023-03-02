using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glider : MonoBehaviour
{
    public Rigidbody rbody;
    Vector3 prevPos;

    public bool onFlightPath = false;

    Vector3 startPos;

    // flight path vars

    float speed;
    Transform flightLeader;

    bool falling = false;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (onFlightPath && !falling)
        {
            // rotate towards flightLeader
            Quaternion lookRotation = Quaternion.LookRotation((flightLeader.position - transform.position).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 0.5f);

            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (!onFlightPath)
        {
            if (rbody.velocity.magnitude > 0f)
            {
                if (prevPos != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(rbody.position - prevPos);
                    rbody.rotation = Quaternion.Slerp(rbody.rotation, targetRotation, Time.deltaTime * 10);
                }

                prevPos = rbody.position;
            }

            if(falling)
            {
                rbody.AddForce(Vector3.down * 0.2f);
                rbody.AddForce(transform.forward * 0.5f);

                if (transform.position.y < -2f)
                {
                    Destroy(gameObject);
                    return;
                }    
            }

            if (!falling)
            {
                float dist = Vector3.Distance(startPos, transform.position);

                if(dist > 5)
                {
                    JoinFlightPath();
                }
                else if(dist > 0.5f)
                {
                    GetComponent<SphereCollider>().enabled = true;
                }
            }
        }
    }

    void JoinFlightPath()
    {
        rbody.velocity = Vector3.zero;

        onFlightPath = true;
        FlightLeader leader = FlightPathManager.Instance.CreateFlightPathLeader();
        flightLeader = leader.transform;
        speed = leader.speed;
    }

    private void OnDestroy()
    {
        if (flightLeader != null)
            Destroy(flightLeader.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        falling = true;
        onFlightPath = false;
    }
}