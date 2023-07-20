using UnityEngine;
using UnityEditor;

public class DistanceConstraint : MonoBehaviour {
    public Transform anchorObject;
    public bool anchorIsKinematic = false;
    [Tooltip("How far transform stays from anchor")]
    public float Distance = 0.5f;
    [Tooltip("Overwrite distance to anchor on start")]
    public bool autoDistance = false;
    [Tooltip("Limit swing symmetrically, centered on z-axis (forward). 0 is no swing, 180 is full spherical rotations")]
    public float maxConeAngle = 90;
    [Tooltip("Resistance to movement, 0 is no resistance, 50 is like moving through molasses [0:100]")]
    public float drag = 0;

    [HideInInspector] public SphereCollider headCollider;

    Vector3 lastPos;

    private void OnEnable()
    {
        lastPos = transform.position;

        if(autoDistance)
        {
            Distance = Vector3.Distance(anchorObject.position,transform.position);
        }
    }

    void OnValidate()
    {
        drag = Mathf.Clamp(drag, 0,100);
    }

    void Update() {
    Debug.DrawLine(transform.position, anchorObject.position);
  }

  public void ResolveConstraint() {


        if (anchorIsKinematic) {
            transform.position = transform.position.ConstrainDistance(anchorObject.transform.position, Distance);

        } else {

          Vector3 tempPos = transform.position;

            transform.position += (transform.position.ConstrainDistance(anchorObject.transform.position, Distance) - transform.position) / 2f ;
            anchorObject.position += (anchorObject.position.ConstrainDistance(tempPos, Distance) - anchorObject.position) / 2f;

        }

        float newMaxConeAngle = maxConeAngle;

        // constraint to sphere collider
        Vector3 sphereCentre = headCollider.transform.TransformPoint(headCollider.center);
        float sphereRadius = headCollider.radius * headCollider.transform.lossyScale.x;

        // get tangent point:
        float angleBetweenMiddleAndSphereDir = Vector3.Angle(anchorObject.forward, Vector3.Normalize(sphereCentre - anchorObject.transform.position));
        float angleBetweenSphereDirAndTangent = Mathf.Asin(sphereRadius / Vector3.Magnitude(sphereCentre - anchorObject.transform.position)) * Mathf.Rad2Deg;

        newMaxConeAngle = Mathf.Min(maxConeAngle, angleBetweenMiddleAndSphereDir - angleBetweenSphereDirAndTangent);


        newMaxConeAngle = Mathf.Max(newMaxConeAngle, 0);

        transform.position = transform.position.ConstrainToCone(anchorObject.transform.position, anchorObject.forward, newMaxConeAngle);

        
    }

    public void PostProcessConstraint()
    {

            transform.position -= (transform.position - lastPos) * Time.deltaTime * (drag+1);
            lastPos = transform.position;

        transform.LookAt(2 * transform.position - anchorObject.position, anchorObject.up); //rotate joint to align with anchor, this prevents weird rotation flipping


    }

}