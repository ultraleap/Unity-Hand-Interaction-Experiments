using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GK;
using Leap;
using Leap.Unity;

public enum MeshPaintMode
{
	CREATE,
	MODIFY
}

public class MeshPainter : MonoBehaviour
{
	public LeapProvider leapProvider;
	public Chirality chirality;

	public ObjectEditManager objManager;

	ConvexHullCalculator calc;
	List<Vector3> verts = new List<Vector3>();
	List<int> tris = new List<int>();
	List<Vector3> normals = new List<Vector3>();
	List<Vector3> points = new List<Vector3>();

	bool isPinching = false;

	public Transform emptyPrefab;

	Transform currentObject;
	Vector3 lastPos;

	const float POINT_SPACING = 0.005f;

	Color currentObjectColor;

	public MeshPaintMode paintMode;
	public bool canPaint = false;

	private void Start()
    {
		calc = new ConvexHullCalculator();
		verts = new List<Vector3>();
		tris = new List<int>();
		normals = new List<Vector3>();
		points = new List<Vector3>();
	}

    private void Update()
    {
		Hand hand = leapProvider.CurrentFrame.GetHand(chirality);

		if(hand != null && canPaint)
        {
			if(isPinching)
            {
				if(hand.PinchStrength < 0.5f)
                {
					OnPinchEnd();
					isPinching = false;
				}
				else
                {
					if (Vector3.Distance(hand.GetIndex().TipPosition, lastPos) > POINT_SPACING)
					{
						OnPinch(hand.GetIndex().TipPosition);
					}
				}
            }
			else
            {
				if(hand.PinchStrength > 0.8f)
                {
					OnPinchStart(hand.GetIndex().TipPosition);
					isPinching = true;
				}
            }
        }
		else
        {
			if(isPinching)
            {
				OnPinchEnd();
			}

			isPinching = false;
        }
	}

    void OnPinchStart(Vector3 pinchPos)
    {
		lastPos = pinchPos;
		points.Clear();

		if (paintMode == MeshPaintMode.CREATE)
		{
			currentObject = Instantiate(emptyPrefab, pinchPos, Quaternion.identity);
			currentObject.GetComponent<MeshRenderer>().material.color = currentObjectColor;
			objManager.AddObject(currentObject.gameObject);
		}
		else if (paintMode == MeshPaintMode.MODIFY)
        {
			// find all nearby objects
			Collider[] colliders = Physics.OverlapSphere(pinchPos, 0.01f);
			currentObject = null;

			foreach(var collider in colliders)
            {
				// we found a suitable object
				if(objManager.objects.Contains(collider.gameObject))
                {
					// make a new empty
					currentObject = Instantiate(emptyPrefab, pinchPos, Quaternion.identity);
					currentObject.GetComponent<MeshRenderer>().material.color = currentObjectColor;
					objManager.AddObject(currentObject.gameObject);

					// get the mesh of the found object
					MeshFilter filter = collider.GetComponent<MeshFilter>();
                    foreach (var point in filter.sharedMesh.vertices)
                    {
						// move the mesh points to account for any scaling and align with the newly created object
						Vector3 transformedPoint = currentObject.InverseTransformPoint(collider.transform.TransformPoint(point));
                        points.Add(transformedPoint);
                    }

					// we no longer need the original object
					Destroy(collider.gameObject);

                    break;
                }
            }

			if (currentObject == null)
			{
				return;
			}
        }
		else
        {
			currentObject = null;
			return;
        }

		points.Add(Vector3.right * 0.01f);
		points.Add(Vector3.right * -0.01f);
		points.Add(Vector3.up * 0.01f);
		points.Add(Vector3.up * -0.01f);
		points.Add(Vector3.forward * 0.01f);
		points.Add(Vector3.forward * -0.01f);

		calc.GenerateHull(points, true, ref verts, ref tris, ref normals);

		var mesh = new Mesh();
		mesh.SetVertices(verts);
		mesh.SetTriangles(tris, 0);
		mesh.SetNormals(normals);

		currentObject.GetComponent<MeshFilter>().sharedMesh = mesh;
	}

	void OnPinch(Vector3 pinchPos)
    {
		if (currentObject == null)
			return;

		lastPos = pinchPos;
		points.Add(pinchPos - currentObject.position);

		calc.GenerateHull(points, true, ref verts, ref tris, ref normals);

		var mesh = new Mesh();
		mesh.SetVertices(verts);
		mesh.SetTriangles(tris, 0);
		mesh.SetNormals(normals);

		currentObject.GetComponent<MeshFilter>().sharedMesh = mesh;
	}

	void OnPinchEnd()
    {
		if (currentObject == null)
			return;

		for (var pointID1 = 0; pointID1 < points.Count; pointID1++)
        {
			for (var pointID2 = points.Count -1; pointID2 > pointID1; pointID2--)
			{
				if(pointID1 != pointID2 &&
					Vector3.Distance(points[pointID1], points[pointID2]) < POINT_SPACING)
                {
					points.RemoveAt(pointID2);
					pointID2--;
				}
			}
		}

		calc.GenerateHull(points, true, ref verts, ref tris, ref normals);

		RecenterAroundVertCenter();

		var mesh = new Mesh();
		mesh.SetVertices(verts);
		mesh.SetTriangles(tris, 0);
		mesh.SetNormals(normals);

		currentObject.GetComponent<MeshFilter>().sharedMesh = mesh;
		currentObject.GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	public void RecenterAroundVertCenter()
    {
		Vector3 centerPos = Vector3.zero;

		// get average pos
		foreach (var vert in verts)
		{
			centerPos += vert;
		}

		centerPos /= verts.Count;

		// apply average pos
		for (int vertID = 0; vertID < verts.Count; vertID++)
		{
			verts[vertID] -= centerPos;
		}

		//translate object to continue being in the origial position
		currentObject.transform.position += centerPos;
	}

	public void SetObjectColor(Color color)
    {
		currentObjectColor = color;
	}
}