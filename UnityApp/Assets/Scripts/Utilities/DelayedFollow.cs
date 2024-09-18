using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedFollow : MonoBehaviour
{
    [SerializeField] private Transform _toFollow;
    [SerializeField] private float _speed;

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, _toFollow.position, Time.deltaTime * _speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, _toFollow.rotation, Time.deltaTime * _speed);
    }
}
