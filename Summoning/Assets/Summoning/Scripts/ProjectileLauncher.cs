using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class ProjectileLauncher : MonoBehaviour
{
    Transform origin;
    Transform target;
    Rigidbody rigid;

    public float initialAngle = 50;
    public float targetHeightOffset = 0.2f;

    void Launch()
    {
        Vector3 p = target.position + (Vector3.up * targetHeightOffset);

        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = initialAngle * Mathf.Deg2Rad;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(origin.position.x, 0, origin.position.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = origin.position.y - p.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        if (initialVelocity == float.NaN)
        {
            return;
        }

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);
        velocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        if (origin.position.x > p.x)
        {
            velocity.x = -velocity.x;
        }

        // Fire!
        rigid.AddForce(velocity, ForceMode.VelocityChange);
        rigid.AddTorque(Vector3.right * 10);
    }

    public void RaySelect(SelectEnterEventArgs args)
    {
        target = (args.interactorObject as XRRayInteractor).attachTransform;

        origin = args.interactableObject.transform;
        rigid = origin.GetComponent<Rigidbody>();

        args.manager.CancelInteractableSelection(args.interactableObject);

        Launch();
    }
}