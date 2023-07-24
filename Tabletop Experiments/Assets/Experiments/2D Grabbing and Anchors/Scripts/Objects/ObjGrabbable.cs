using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

//A grabbable object able to be highlighted by hand and placed on an anchor
public class ObjGrabbable : Obj
{
    private static HashSet<ObjGrabbable> _instances = new HashSet<ObjGrabbable>();
    public static HashSet<ObjGrabbable> Instances
    {
        get
        {
            return _instances;
        }
    }

    public new const JoyObjectType Type = JoyObjectType.GRABBABLE;

    private List<Material> _materials = null;
    public List<Material> Materials
    {
        get
        {
            if (_materials == null)
            {
                _materials = new List<Material>();
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    _materials.AddRange(renderer.materials);
                }
            }
            return _materials;
        }
    }

    //Anchored info
    private ObjAnchor _anchor = null;
    public ObjAnchor Anchor
    {
        get
        {
            return _anchor;
        }
        set
        {
            if (value == _anchor) return;
            if (value != null && (value.IsAnchored || !value.IsActive)) return; //Only one object per anchor

            _anchor = value;
            this.transform.parent = _anchor?.transform;
            OnAnchored?.Invoke(this, _anchor);
        }
    }

    public bool IsAnchored
    {
        get
        {
            return _anchor != null;
        }
    }
    public Action<ObjGrabbable, ObjAnchor> OnAnchored;

    //Grabbed info
    private GrabMeta _grabMeta = new GrabMeta();
    public bool GrabbedLeft
    {
        get
        {
            return _grabMeta.GrabbedByLeft;
        }
    }
    public bool GrabbedRight
    {
        get
        {
            return _grabMeta.GrabbedByRight;
        }
    }
    public bool Grabbed
    {
        get
        {
            return GrabbedLeft || GrabbedRight;
        }
    }
    public Action<ObjGrabbable> OnGrabbed;

    private Vector3 _lastPos;
    private float _velocity;
    public float Velocity => _velocity;

    public void SetGrabMeta(GrabMeta meta)
    {
        bool wasGrabbed = _grabMeta != null && _grabMeta.Grabbed;
        _grabMeta = meta;
        if (!wasGrabbed && _grabMeta != null && _grabMeta.Grabbed) OnGrabbed?.Invoke(this);
    }

    private void Update()
    {
        if (GrabbedLeft) MoveToHand(Chirality.Left);
        if (GrabbedRight) MoveToHand(Chirality.Right);
        if (IsAnchored) MoveToAnchor();

        _velocity = Vector3.Distance(transform.position, _lastPos) / Time.deltaTime;
        _lastPos = transform.position;
    }

    private void MoveToHand(Chirality chirality)
    {
        Leap.Hand hand = HandExtensions.Get(chirality);
        if (hand == null) return;
        this.transform.position = Vector3.Lerp(this.transform.position, hand.PalmPosition + (hand.PalmNormal * 0.1f), Time.deltaTime * 10.0f);
    }

    private void MoveToAnchor()
    {
        this.transform.position = Vector3.Lerp(this.transform.position, Anchor.transform.position, Time.deltaTime * 10.0f);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Anchor.transform.rotation, Time.deltaTime * 10.0f);
        this.transform.localScale = Vector3.Lerp(this.transform.localScale, Vector3.one, Time.deltaTime * 10.0f);
    }

    protected void Awake()
    {
        _instances.Add(this);
    }

    private void OnDestroy()
    {
        _instances.Remove(this);
    }
}

public class GrabMeta
{
    public bool Grabbed
    {
        get
        {
            return GrabbedByLeft || GrabbedByRight;
        }
    }

    public bool GrabbedByLeft = false;
    public bool GrabbedByRight = false;
}