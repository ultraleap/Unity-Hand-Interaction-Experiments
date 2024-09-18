using Leap.Unity.Attachments;
using Leap.Unity.PhysicalHands;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PalmUIManager : MonoBehaviour
{
    private Dictionary<string, bool> _triggeredObjects;

    private AttachmentHands attachmentHands;

    // Start is called before the first frame update
    void Start()
    {
        attachmentHands = FindObjectOfType<ULXR_HandUI>().gameObject.GetComponentInParent<AttachmentHands>();
        _triggeredObjects = new Dictionary<string, bool>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddTriggeredObject(string objectName)
    {
        if (_triggeredObjects.ContainsKey(objectName))
        {
            _triggeredObjects[objectName] = true;
        }
        else
        {
            _triggeredObjects.Add(objectName, true);
        }

        UpdateHandUI();
    }

    public void RemoveTriggeredObject(string objectName)
    {
        if (_triggeredObjects.ContainsKey(objectName))
        {
            _triggeredObjects[objectName] = false;
        }
        else
        {
            _triggeredObjects.Add(objectName, false);
        }
        UpdateHandUI();
    }


    private void UpdateHandUI()
    {
        int triggeringObjects = 0;

        foreach (var triggeredObject in _triggeredObjects)
        {
            if (triggeredObject.Value) triggeringObjects++;
        }

        attachmentHands?.gameObject.SetActive(triggeringObjects <= 0);
    }
}