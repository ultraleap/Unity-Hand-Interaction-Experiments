using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapPiece : MonoBehaviour
{
    public List<AttachPoint> attachPoints = new List<AttachPoint>();

    public float attachDist = 0.05f;

    public float snapAngleX = 10;
    public float snapAngleY = 10;
    public float snapAngleZ = 10;

    public void CheckForAttach()
    {
        AttachPoint[] allAttachPoints = FindObjectsOfType<AttachPoint>();

        float nearestSnap = attachDist;

        int nearestAllIndex = 0;
        int nearestIndex = 0;

        for (int allIndex = 0; allIndex < allAttachPoints.Length; allIndex++)
        {
            for (int i = 0; i < attachPoints.Count; i++)
            {
                if (attachPoints.Contains(allAttachPoints[allIndex]))
                    continue;

                float dist = Vector3.Distance(attachPoints[i].transform.position, allAttachPoints[allIndex].transform.position);

                if (dist < nearestSnap)
                {
                    // snap to the point!
                    nearestSnap = dist;
                    nearestAllIndex = allIndex;
                    nearestIndex = i;
                }
            }
        }

        transform.rotation = Quaternion.Euler(Mathf.Round(transform.eulerAngles.x / snapAngleX) * snapAngleX,
                                        Mathf.Round(transform.eulerAngles.y / snapAngleY) * snapAngleY,
                                        Mathf.Round(transform.eulerAngles.z / snapAngleZ) * snapAngleZ);


        if (nearestSnap != attachDist)
        {
            transform.position += allAttachPoints[nearestAllIndex].transform.position - attachPoints[nearestIndex].transform.position;
        }
    }
}