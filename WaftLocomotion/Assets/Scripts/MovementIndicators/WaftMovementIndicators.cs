using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaftMovementIndicators : MonoBehaviour
{
    public WaftLocomotion waft;

    [Header("Hand Visualisations")]
    public bool handPinchDotsOn = false;
    public bool handDotsOn = false;
    public bool handBeamsOn = true;
    public bool palmBeamsInstead = true;

    [Header("Head Visualisations")]
    public bool handColumnsOn = false;
    public bool headDotOn = true;
    public bool headDotSecondaryOn = false;
    public bool headBeamOn = false;
    public bool headColumnOn = false;
    public bool headTargetOn = true;

    [Tooltip("This value amplifies the hands' dot positions based on your current velocity vector . ~3.15 places the dots close to your hands' final locations after coasting to a stop. Negative values have the dots trailing behind.")]
    public float dotMagnitude = 3.2f;
    [Tooltip("This value amplifies the head's dot position based on your current velocity vector. ~1.57 places the dot close to your head target's final location after coasting to a stop. Negative values has the dot trailing behind.")]
    public float headDotMagnitude = 1.6f;
    [Tooltip("This value amplifies the head's secondary-dot position based on your current velocity vector. ~0.1 dampens the dot's movement to just the visible area around your head target as a sort of elastic deceleration minimap HUD. Negative values has the dot trailing behind.")]
    public float headDotSecondaryMagnitude = 0.1f;
    [Tooltip("This value stretches the hands' beam length based on your current velocity vector. ~0.25 dampens the beam's length to trail behind the hands like a ribbon. Negative values stretch the beam towards the endpoint.")]
    public float beamMagnitude = 0.25f;
    [Tooltip("This value stretches the head target's beam length based on your current velocity vector. ~0.25 dampens the beam's length to trail behind the head target like a ribbon. Negative values stretch the beam towards the endpoint as if it is a section of an elastic connector.")]
    public float headBeamMagnitude = 0.25f;
    public float dotSize = 1f, dotSizeOffset = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
