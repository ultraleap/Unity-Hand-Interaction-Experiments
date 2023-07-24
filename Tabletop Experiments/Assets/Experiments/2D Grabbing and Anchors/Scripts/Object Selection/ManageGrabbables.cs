using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//Management of hand object selection 
public class ManageGrabbables : MonoBehaviour
{
    [SerializeField] private List<HandObject> _hands;

    [Space]

    [SerializeField] private Color _grabbedColour = Color.yellow;
    [SerializeField] private Color _latchedColour = Color.white;
    [SerializeField] private Color _unlatchedColour = Color.black;

    [Space]

    [Tooltip("If enabled, grabbables will replace the object on their target anchor, if disabled, they will fall back to the next closest.")]
    [SerializeField] private bool _alwaysReplaceTargetAnchor = true;
    [SerializeField] private float _timeToConsiderGrabDropped = 0.1f;

    private Dictionary<ObjGrabbable, GrabMeta> _grabMeta = new Dictionary<ObjGrabbable, GrabMeta>();
    private Dictionary<Chirality, float> _timeSinceGrab = new Dictionary<Chirality, float>();

    private AudioSource _grabbingAudioSource;

    private bool _isActive = true;
    public bool IsActive
    {
        get
        {
            return _isActive;
        }
        set
        {
            if (!_isActive && value == true)
            {
                _grabsDisabled[Chirality.Left] = true;
                _grabsDisabled[Chirality.Right] = true;
            }
            _isActive = value;
        }
    }

    private Dictionary<Chirality, bool> _grabsDisabled = new Dictionary<Chirality, bool>();

    private void OnValidate()
    {
        foreach (Chirality chirality in Enum.GetValues(typeof(Chirality)))
        {
            if (_hands.FirstOrDefault(o => o.Chirality == chirality) == null)
                _hands.Add(new HandObject() { Chirality = chirality });
        }
    }

    private void Awake()
    {
        foreach (Chirality chirality in Enum.GetValues(typeof(Chirality)))
        {
            _timeSinceGrab.Add(chirality, float.PositiveInfinity);
        }

        _grabsDisabled.Add(Chirality.Left, false);
        _grabsDisabled.Add(Chirality.Right, false);

        _grabbingAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!IsActive) return;

        //Work out what's highlighted & store grab metadata
        _grabMeta.Clear();
        foreach (HandObject hand in _hands)
        {
            bool grabbing = hand.Grabbing;
            bool grabbingRAW = grabbing; //This is our un-filtered value - should only be used as ground truth

            //Don't allow a grab if we were just activated (prevents accidental inits)
            if (grabbing && _grabsDisabled[hand.Chirality]) 
                grabbing = false;

            //We override the current hand grab state if we're below the grab timeout threshold, to prevent dropouts
            if (grabbing) _timeSinceGrab[hand.Chirality] = 0.0f;
            else _timeSinceGrab[hand.Chirality] += Time.deltaTime;
            if (!grabbing) grabbing = _timeSinceGrab[hand.Chirality] <= _timeToConsiderGrabDropped;

            //We also lock the current closest object while "grabbing" in order to prevent unwanted re-selections
            hand.Closest.Grabbable?.Lock(grabbing);

            //Work out the closest grabbable
            ObjGrabbable closestGrabbable = hand.Closest.Grabbable?.ClosestObject;
            if (closestGrabbable != null)
            {
                if (!_grabMeta.ContainsKey(closestGrabbable)) _grabMeta[closestGrabbable] = new GrabMeta();
                if (hand.Chirality == Chirality.Left) _grabMeta[closestGrabbable].GrabbedByLeft = grabbing;
                if (hand.Chirality == Chirality.Right) _grabMeta[closestGrabbable].GrabbedByRight = grabbing;

                //We cannot be anchored if we are grabbed
                if (grabbing) closestGrabbable.Anchor = null; 
            }
            hand.GrabbableLine?.DrawTo(hand.Closest.Grabbable?.ClosestObject);

            //If we are have stopped grabbing in unfiltered data, reset the disable lock (from awake)
            if (!grabbingRAW) _grabsDisabled[hand.Chirality] = false;
        }

        bool anyObjectGrabbed = false;
        float objectVelocity = 0f;

        //Update grabbables with stored grab metadata
        foreach (ObjGrabbable grabbable in ObjGrabbable.Instances)
        {
            bool isHighlighted = _grabMeta.ContainsKey(grabbable); //This defines if we are highlighted OR grabbed
            GrabMeta grabMeta = isHighlighted ? _grabMeta[grabbable] : new GrabMeta();

            //Forcibly clear grab meta out if hand has been lost
            if (grabbable.GrabbedLeft && HandExtensions.Get(Chirality.Left) == null) grabMeta = new GrabMeta();
            if (grabbable.GrabbedRight && HandExtensions.Get(Chirality.Right) == null) grabMeta = new GrabMeta();

            //We have just been dropped - check anchors
            if (grabbable.Grabbed && !grabMeta.Grabbed)
            {
                HandObject hand = grabbable.GrabbedLeft ? _hands.FirstOrDefault(o => o.Chirality == Chirality.Left) : grabbable.GrabbedRight ? _hands.FirstOrDefault(o => o.Chirality == Chirality.Right) : null;
                ObjAnchor anchor = hand.Closest.Anchor.ClosestObject;
                if (anchor == null) anchor = hand.Closest.AnchorLastClosest; //Try fallback to last frame closest (helps for hand dropouts)
                if (anchor != null)
                {
                    //We have found a suitable anchor near our drop location - attach!
                    grabbable.Anchor = anchor;

                    //If we didn't attach for whatever reason...
                    List<ObjAnchor> nextBest = hand?.Closest?.Anchor?.ClosestObjects;
                    if (grabbable.Anchor != anchor && nextBest != null)
                    {
                        if (_alwaysReplaceTargetAnchor)
                        {
                            //...replace the object on the anchor
                            ObjGrabbable toReplace = anchor.AnchoredGrabbable;
                            for (int i = 1; i < nextBest.Count; i++)
                            {
                                toReplace.Anchor = nextBest[i];
                                if (toReplace.Anchor == nextBest[i]) break;
                            }
                            grabbable.Anchor = anchor;
                        }
                        else
                        {
                            //...find the next closest suitable
                            for (int i = 1; i < nextBest.Count; i++)
                            {
                                grabbable.Anchor = nextBest[i];
                                if (grabbable.Anchor == nextBest[i]) break;
                            }
                        }
                    }
                }
            }
            grabbable.SetGrabMeta(grabMeta);

            //Update the grabbable materials with the correct colour based on highlight/grab state
            foreach (Material mat in grabbable.Materials)
            {
                Color colour = !isHighlighted ? _unlatchedColour : grabbable.Grabbed ? _grabbedColour : _latchedColour;
                mat.color = colour;
            }

            if (grabbable.Grabbed)
            {
                anyObjectGrabbed = true;
                objectVelocity = grabbable.Velocity;
            }
        }

        //Update grab audio
        if (anyObjectGrabbed && !_grabbingAudioSource.isPlaying) _grabbingAudioSource.Play();
        else if (!anyObjectGrabbed && _grabbingAudioSource.isPlaying) _grabbingAudioSource.Stop();
        if (_grabbingAudioSource.isPlaying) _grabbingAudioSource.volume = Mathf.InverseLerp(0f, 1f, objectVelocity);

        //Store the closest anchor this frame so we can potentially use it next frame
        foreach (HandObject hand in _hands)
        {
            hand.Closest.AnchorLastClosest = hand.Closest.Anchor.ClosestObject;
        }
    }

    //Start our update logic again when we are enabled
    private void OnEnable()
    {
        _isActive = true;
        _grabsDisabled[Chirality.Left] = true;
        _grabsDisabled[Chirality.Right] = true;
    }
}

[System.Serializable]
public class HandObject
{
    public Leap.Hand Hand
    {
        get
        {
            return HandExtensions.Get(Chirality);
        }
    }
    public ObjGrabbable Grabbable
    {
        get
        {
            return Closest.Grabbable?.ClosestObject;
        }
    }

    public bool Grabbing
    {
        get
        {
            return Hand == null ? false : Hand.IsGrabbing();
        }
    }

    public Chirality Chirality;

    public ClosestData Closest = new ClosestData();
    public LineToJoy GrabbableLine = null;

    [System.Serializable]
    public class ClosestData
    {
        public ClosestGrabbable Grabbable = null;
        public ClosestAnchor Anchor = null;

        public ObjAnchor AnchorLastClosest = null; 
    }
}