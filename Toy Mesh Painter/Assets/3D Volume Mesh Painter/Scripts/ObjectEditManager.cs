using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;
using Leap;
using Leap.Unity;

public class ObjectEditManager : MonoBehaviour
{
	public LeapProvider leapProvider;

	public List<GameObject> objects = new List<GameObject>();

    bool canScale = false;

    public void AddObject(GameObject obj)
    {
        objects.Add(obj);
    }

    public void EnableInteraction()
    {
        foreach(var obj in objects)
        {
            if (obj != null)
                obj.GetComponent<InteractionBehaviour>().ignoreGrasping = false;
        }
    }

    public void DisableInteraction()
    {
        foreach (var obj in objects)
        {
            if(obj != null)
                obj.GetComponent<InteractionBehaviour>().ignoreGrasping = true;
        }
    }

    public void EnableScaling()
    {
        canScale = true;
    }

    public void DisableScaling()
    {
        canScale = false;
    }

	#region scaling

	bool leftPinching = false;
	bool rightPinching = false;

	float lastDist = 0;

	Transform scalingObject;

	public float scalingSpeed = 10;

	private void Update()
	{
		Hand leftHand = leapProvider.CurrentFrame.GetHand(Chirality.Left);
		Hand rightHand = leapProvider.CurrentFrame.GetHand(Chirality.Right);

		if(leftHand != null && canScale)
        {
			if (leftPinching)
			{
				if (leftHand.PinchStrength < 0.5f)
				{
					leftPinching = false;
					lastDist = 0;
					scalingObject = null;
				}
			}
			else
			{
				if (leftHand.PinchStrength > 0.8f)
				{
					leftPinching = true;
				}
			}
		}
		else
        {
			leftPinching = false;
			lastDist = 0;
			scalingObject = null;
		}

		if (rightHand != null && canScale)
		{
			if (rightPinching)
			{
				if (rightHand.PinchStrength < 0.5f)
				{
					rightPinching = false;
					lastDist = 0;
					scalingObject = null;
				}
			}
			else
			{
				if (rightHand.PinchStrength > 0.8f)
				{
					rightPinching = true;
				}
			}
		}
		else
		{
			rightPinching = false;
			lastDist = 0;
			scalingObject = null;
		}

		if (leftHand != null && rightHand != null)
        {
			float dist = (leftHand.GetIndex().TipPosition - rightHand.GetIndex().TipPosition).magnitude;

			if (leftPinching && rightPinching)
			{

				if (scalingObject == null)
				{
					// find an object if both are pinching and on the object
					Collider[] leftObjects = Physics.OverlapSphere(leftHand.GetIndex().TipPosition, 0.01f);
					Collider[] rightObjects = Physics.OverlapSphere(rightHand.GetIndex().TipPosition, 0.01f);

					bool found = false;

					foreach (var leftCol in leftObjects)
                    {
						foreach(var rightCol in rightObjects)
                        {
							if(leftCol == rightCol && objects.Contains(leftCol.gameObject))
                            {
								scalingObject = leftCol.transform;
								found = true;
								break;
							}
                        }

						if (found)
							break;
                    }
				}
				else
                {
					if (lastDist != 0)
                    {
						scalingObject.localScale += Vector3.one * Mathf.Max(scalingObject.localScale.x, 1) * ((dist - lastDist) * Time.deltaTime * scalingSpeed);
                    }
                }
			}

			lastDist = dist;
        }


		if(leftPinching && rightPinching)
        {
			if(!scalingHappening)
            {
				DisableInteraction();
				scalingHappening = true;
			}
		}
		else
        {
			if (scalingHappening)
			{
				EnableInteraction();
				scalingHappening = false;
			}
		}
	}

	bool scalingHappening = false;

    #endregion
}