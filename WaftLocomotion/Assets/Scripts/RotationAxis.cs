using Leap;
using Leap.Unity;
using UnityEngine;

/// <summary>
/// A visual gizmo that guides a user towards a specific pinch rotation axis
/// </summary>
public class RotationAxis : MonoBehaviour
{
    [Header("Leap Setup")]
    public LeapProvider leapProvider;
    public Chirality chirality;

    [Header("Rotation Axis Setup")]
    public GameObject pinchAxis;
    public GameObject worldAxis;
    public GameObject alignmentRadius;
    public GameObject alignmentTube;
    public GameObject alignmentAxisDot;

    // Start is called before the first frame update
    void Start()
    {
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

        worldAxis.SetActive(true);
        worldAxis.transform.position = hand.GetPredictedPinchPosition();
        pinchAxis.SetActive(true);
        pinchAxis.transform.position = hand.GetPredictedPinchPosition();

        // pinchAxis.transform.rotation = Quaternion.FromToRotation(
        //     Vector3.up, Vector3.Cross(
        //         hand.GetThumb().Bone(Bone.BoneType.TYPE_PROXIMAL).NextJoint
        //         - hand.GetPredictedPinchPosition(),
        //         hand.GetIndex().Bone(Bone.BoneType.TYPE_METACARPAL).NextJoint
        //         - hand.GetPredictedPinchPosition()
        //     ).normalized);
        // alternate method for pinchAxis rotation

        pinchAxis.transform.rotation = Quaternion.FromToRotation(Vector3.up, hand.RadialAxis());

        // alignment radius stays centered on worldAxis but slides up and down to stay at the world-altitude of the top of the pinchAxis, and changes radius to always touch the top of pinchAxis
        alignmentRadius.SetActive(true);
        float lAngle = Vector3.Angle(pinchAxis.transform.up, worldAxis.transform.up) * Mathf.Deg2Rad;
        float radius = pinchAxis.transform.localScale.y * Mathf.Sin(lAngle) * 2f;
        float height = pinchAxis.transform.localScale.y * Mathf.Cos(lAngle);
        alignmentRadius.transform.position = worldAxis.transform.position + worldAxis.transform.up * height;
        alignmentRadius.transform.localScale = new Vector3(radius, 0.001f, radius);

        alignmentTube.SetActive(true);
        alignmentTube.transform.position = worldAxis.transform.position;

        alignmentAxisDot.SetActive(true);
        alignmentAxisDot.transform.position = alignmentRadius.transform.position;

        float dotProduct = Vector3.Dot(worldAxis.transform.up, hand.RadialAxis()); // not happy with this angle, might try perpendicular to the palm normal in the future
        float clampedDotProduct = Mathf.Clamp(dotProduct, 0f, 1f);
        clampedDotProduct = Mathf.Pow(clampedDotProduct, 4f);  // ramp between 0 and 1 exponentially


        // ramp material's emission color to full HDR white:
        Material worldAxisMat = worldAxis.GetComponent<Renderer>().material;
        Material pinchAxisMat = pinchAxis.GetComponent<Renderer>().material;

        worldAxisMat.color = new Color(worldAxisMat.color.r, worldAxisMat.color.g, worldAxisMat.color.b,
                                        clampedDotProduct);
        pinchAxisMat.color = new Color(pinchAxisMat.color.r, pinchAxisMat.color.g, pinchAxisMat.color.b,
                                        clampedDotProduct);
        worldAxisMat.SetColor("_EmissionColor", new Color(1, 1, 1, 1) * clampedDotProduct);
        pinchAxisMat.SetColor("_EmissionColor", new Color(1, 1, 1, 1) * clampedDotProduct);
    }
}
