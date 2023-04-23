using Leap.Unity;
using UnityEngine;

public class HeadMovementIndicators : MonoBehaviour
{
    [Header("Setup")]
    public WaftLocomotion waft;
    public Transform head;
    public GameObject dot;
    public GameObject dotSecondary;
    public GameObject beam;
    public GameObject column;

    [Header("Toggles")]
    public bool dotOn = true;
    public bool dotSecondaryOn = false;
    public bool beamOn = false;
    public bool columnOn = false;
    public bool headTargetOn = true;

    [Header("Preferences")]
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

    public Vector3 headTargetLocalOffset = new Vector3(0, -0.27f, 0.63f);

    private void Start()
    {
        if(waft == null)
        {
            waft = FindObjectOfType<WaftLocomotion>();
        }
        if(head == null)
        {
            head = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 headTargetPos = head.TransformPoint(headTargetLocalOffset);
        UpdateDot(headTargetPos);
        UpdateDotSecondary(headTargetPos);
        UpdateBeam(headTargetPos);
        UpdateColumn(headTargetPos);
    }

    private void UpdateDot(Vector3 headTargetPos)
    {
        if (dotOn)
        {
            dot.SetActive(true);
            dot.transform.SetPositionAndRotation(headTargetPos - (waft.currentVel * headDotMagnitude), Quaternion.identity);
        }
        else { dot.SetActive(false); }
    }

    private void UpdateDotSecondary(Vector3 headTargetPos)
    {
        if (dotSecondaryOn)
        {
            dotSecondary.SetActive(true);
            dotSecondary.transform.SetPositionAndRotation(headTargetPos - (waft.currentVel * headDotSecondaryMagnitude), Quaternion.identity);
        }
        else { dotSecondary.SetActive(false); }
    }

    private void UpdateBeam(Vector3 headTargetPos)
    {
        if (beamOn)
        {
            beam.SetActive(true);
            beam.transform.SetPositionAndRotation((headTargetPos + (headTargetPos - (waft.currentVel * headBeamMagnitude))) * 0.5f,
                                                        Quaternion.FromToRotation(Vector3.up, headTargetPos - (headTargetPos + (waft.currentVel * beamMagnitude))));
            beam.transform.localScale = new Vector3(beam.transform.localScale.x,
                                                        (headTargetPos - (headTargetPos - (waft.currentVel * headBeamMagnitude))).magnitude * 0.5f,
                                                        beam.transform.localScale.z);
        }
        else { beam.SetActive(false); }
    }

    private void UpdateColumn(Vector3 headTargetPos)
    {
        if (columnOn)
        {
            column.SetActive(true);
            column.transform.SetPositionAndRotation(headTargetPos - (waft.currentVel * headDotMagnitude), Quaternion.identity);
        }
        else { column.SetActive(false); }
    }
}
