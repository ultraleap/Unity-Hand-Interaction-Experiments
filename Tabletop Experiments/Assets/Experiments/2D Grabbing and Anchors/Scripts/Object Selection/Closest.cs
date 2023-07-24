using Leap.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Work out the closest **generic joy object** to our hand in screenspace
public class Closest : MonoBehaviour
{
    [SerializeField] private Chirality _chirality;

    [Space]

    [SerializeField] private List<HandObjectWeight> _objectHandWeights; //todo: use dictionary

    protected Obj _closestObject = null;
    protected int _calculatedFrame = int.MinValue;

    protected Dictionary<Obj, float> _scores = new Dictionary<Obj, float>();
    protected bool _locked = false;

    private void OnValidate()
    {
        foreach (LeapPos position in Enum.GetValues(typeof(LeapPos)))
        {
            if (_objectHandWeights.FirstOrDefault(o => o.HandPosition == position) == null)
                _objectHandWeights.Add(new HandObjectWeight() { HandPosition = position, Weight = 1.0f });
        }
    }

    //Lock the current closest object as our selection
    public void Lock(bool locked = true)
    {
        _locked = locked;
    }

    //Calculate closest
    protected Obj CalculateInternal(JoyObjectType type)
    {
        if (_calculatedFrame == Time.frameCount || _locked)
        {
            return _closestObject;
        }

        _closestObject = null;
        _calculatedFrame = Time.frameCount;

        List<Obj> scored = CalculateListInternal(type);
        if (scored == null || scored.Count == 0) return null;

        _closestObject = scored[0];
        return _closestObject;
    }

    //Calculate closest as list sorted by score (ignores locks, and can be run multiple times a frame!)
    protected List<Obj> CalculateListInternal(JoyObjectType type)
    {
        _scores.Clear();

        Leap.Hand hand = HandExtensions.Get(_chirality);
        if (hand == null) return null;

        foreach (LeapPos position in Enum.GetValues(typeof(LeapPos)))
        {
            ScoreForPosition(hand, position, type);
        }

        List<KeyValuePair<Obj, float>> scoresOrdered = _scores.OrderBy(o => o.Value).ToList();
        List<Obj> toReturn = new List<Obj>();
        foreach (KeyValuePair<Obj, float> score in scoresOrdered) toReturn.Add(score.Key);
        return toReturn;
    }

    private void ScoreForPosition(Leap.Hand hand, LeapPos position, JoyObjectType type)
    {
        Vector3 pos = hand.ScreenSpace(position);
        switch (type)
        {
            case JoyObjectType.GRABBABLE:
                foreach (ObjGrabbable grabbable in ObjGrabbable.Instances)
                {
                    Score(pos, position, grabbable);
                }
                break;
            case JoyObjectType.ANCHOR:
                foreach (ObjAnchor anchor in ObjAnchor.Instances)
                {
                    Score(pos, position, anchor);
                }
                break;
        }
    }
    private void Score(Vector3 pos, LeapPos position, Obj grabbable)
    {
        //Don't score inactive objects
        if (!grabbable.IsActive)
        {
            if (_scores.ContainsKey(grabbable)) _scores.Remove(grabbable);
            return;
        }

        float distance = Mathf.Abs(Vector3.Distance(pos, grabbable.PositionScreenSpace));
        if (_scores.ContainsKey(grabbable)) _scores[grabbable] += distance * _objectHandWeights.FirstOrDefault(o => o.HandPosition == position).Weight;
        else _scores.Add(grabbable, distance * _objectHandWeights.FirstOrDefault(o => o.HandPosition == position).Weight);
    }

    [System.Serializable]
    public class HandObjectWeight
    {
        public LeapPos HandPosition;
        public float Weight;
    }
}