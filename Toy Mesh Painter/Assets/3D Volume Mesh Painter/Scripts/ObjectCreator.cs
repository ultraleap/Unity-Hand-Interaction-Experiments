using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public enum ObjectType
{
	CUBE,
	SPHERE,
	CYLINDER
}

public class ObjectCreator : MonoBehaviour
{
    public LeapProvider leapProvider;
    public Chirality chirality;

	public ObjectEditManager objManager;

	bool isPinching = false;

	Transform currentObject;
	Vector3 originalPos;

	public bool canCreate = false;

	Color currentObjectColor;

	public ObjectType objType;

	public Transform cubePrefab;
	public Transform spherePrefab;
	public Transform cylinderPrefab;

	private void Update()
    {
		Hand hand = leapProvider.CurrentFrame.GetHand(chirality);

		if (hand != null && canCreate)
		{
			if (isPinching)
			{
				if (hand.PinchStrength < 0.5f)
				{
					OnPinchEnd();
					isPinching = false;
				}
				else
				{
					OnPinch(hand.GetIndex().TipPosition);
				}
			}
			else
			{
				if (hand.PinchStrength > 0.8f)
				{
					OnPinchStart(hand.GetIndex().TipPosition);
					isPinching = true;
				}
			}
		}
		else
		{
			if (isPinching)
			{
				OnPinchEnd();
			}

			isPinching = false;
		}
	}

	void OnPinchStart(Vector3 pinchPos)
	{
		originalPos = pinchPos;

        switch (objType)
        {
            case ObjectType.CUBE:
				currentObject = Instantiate(cubePrefab, pinchPos, Quaternion.identity);
				break;
            case ObjectType.SPHERE:
				currentObject = Instantiate(spherePrefab, pinchPos, Quaternion.identity);
				break;
            case ObjectType.CYLINDER:
				currentObject = Instantiate(cylinderPrefab, pinchPos, Quaternion.identity);
				break;
        }

		currentObject.GetComponent<MeshRenderer>().material.color = currentObjectColor;
		currentObject.localScale = Vector3.one * 0.001f;
		objManager.AddObject(currentObject.gameObject);
	}

    void OnPinch(Vector3 pinchPos)
	{
		currentObject.position = originalPos + ((pinchPos - originalPos) / 2);
		currentObject.localScale = Vector3.one * ((pinchPos - originalPos).magnitude);
		currentObject.up = (pinchPos - originalPos).normalized;
	}

	void OnPinchEnd()
	{

	}

	public void SetObjectColor(Color color)
	{
		currentObjectColor = color;
	}
}