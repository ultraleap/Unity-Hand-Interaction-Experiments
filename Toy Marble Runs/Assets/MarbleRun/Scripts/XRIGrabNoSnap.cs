using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class XRIGrabNoSnap : MonoBehaviour
{
    XRGrabInteractable interactable;
    Transform grabParent;

    private void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();

        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelectEntering);
            interactable.selectExited.AddListener(OnSelectExiting);

            interactable.trackPosition = false;
            interactable.trackRotation = false;

            grabParent = new GameObject().transform;
        }
        else
        {
            Debug.Log("XRIGrabNoSnap requires a referenced XRGrabInteractable");
        }
    }

    private void OnSelectEntering(SelectEnterEventArgs args)
    {
        grabParent.parent = args.interactorObject.GetAttachTransform(interactable);
        interactable.transform.parent = grabParent;
    }

    private void OnSelectExiting(SelectExitEventArgs args)
    {
        interactable.transform.parent = null;
    }
}