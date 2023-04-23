using Leap;
using Leap.Unity;
using UnityEngine;

public class HandMovementIndicators : MonoBehaviour
{
    public enum BeamType { PINCH, PALM };

    [Header("Leap Setup")]
    public LeapProvider leapProvider;
    public Chirality chirality;

    [Header("Hand Movement Indicator Setup")]
    public WaftLocomotion waft;
    public GameObject dot;
    public GameObject pinchDot;
    public GameObject beam;
    public GameObject column;

    [Header("Toggles")]
    public bool pinchDotOn = false;
    public bool dotOn = false;
    public bool beamOn = true;
    public BeamType beamType = BeamType.PINCH;

    public bool columnsOn = true;


    [Header("Preferences")]
    [Tooltip("This value amplifies the hands' dot positions based on your current velocity vector . ~3.15 places the dots close to your hands' final locations after coasting to a stop. Negative values have the dots trailing behind.")]
    public float dotMagnitude = 3.2f;
    [Tooltip("This value stretches the hands' beam length based on your current velocity vector. ~0.25 dampens the beam's length to trail behind the hands like a ribbon. Negative values stretch the beam towards the endpoint.")]
    public float beamMagnitude = 0.25f;
    public float dotSize = 1f, dotSizeOffset = 0.01f;

    private void Start()
    {
        if (waft == null)
        {
            waft = FindObjectOfType<WaftLocomotion>();
        }

        if (leapProvider == null)
        {
            leapProvider = FindObjectOfType<LeapProvider>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Hand hand = leapProvider.CurrentFrame.GetHand(chirality);

        if (hand == null) { return; }

        WaftLocomotion.WaftHandData handData = waft.handData[chirality == Chirality.Left ? 0 : 1];

        UpdateDot(hand, handData);
        UpdatePinchDot(hand);
        UpdateBeam(handData);
        UpdateColumn(handData);
    }

    private void UpdateDot(Hand hand, WaftLocomotion.WaftHandData handData)
    {
        if (dotOn)
        {
            dot.SetActive(true);
            dot.transform.SetPositionAndRotation((handData.pinchPosition + (handData.pinchPosition - (waft.currentVel * dotMagnitude))) * 0.5f, Quaternion.identity);
            dot.transform.localScale = new Vector3(hand.PinchDistance + dotSizeOffset,
                                                        hand.PinchDistance + dotSizeOffset,
                                                        hand.PinchDistance + dotSizeOffset) * 0.001f * dotSize;
        }
        else
        {
            dot.SetActive(false);
        }
    }

    private void UpdatePinchDot(Hand hand)
    {
        if (pinchDotOn)
        {
            pinchDot.SetActive(true);
            pinchDot.transform.position = hand.GetPredictedPinchPosition();
        }
        else
        {
            pinchDot.SetActive(false);
        }
    }

    private void UpdateBeam(WaftLocomotion.WaftHandData handData)
    {
        if (beamOn)
        {
            beam.SetActive(true);

            Vector3 basePos;
            switch (beamType)
            {
                case BeamType.PALM:
                    basePos = handData.palmPos;
                    break;
                default:
                case BeamType.PINCH:
                    basePos = handData.pinchPosition;
                    break;
            }

            beam.transform.SetPositionAndRotation((basePos + (basePos + (waft.currentVel * beamMagnitude))) * 0.5f,
                                                        Quaternion.FromToRotation(Vector3.up, basePos - (basePos + (waft.currentVel * beamMagnitude))));
            beam.transform.localScale = new Vector3(beam.transform.localScale.x,
                                                        (basePos - (basePos - (waft.currentVel * beamMagnitude))).magnitude * 0.5f,
                                                        beam.transform.localScale.z);               // scale cylinder radius with currentVel?
        }
        else
        {
            beam.SetActive(false);
        }
    }

    private void UpdateColumn(WaftLocomotion.WaftHandData handData)
    {
        if (columnsOn)
        {
            column.transform.SetPositionAndRotation((handData.pinchPosition + (handData.pinchPosition - (waft.currentVel * dotMagnitude))) * 0.5f, Quaternion.identity);
            column.SetActive(true);
        }
        else { column.SetActive(false); }
    }
}
