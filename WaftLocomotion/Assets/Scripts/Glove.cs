/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/
using Leap;
using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity
{

    public class Glove : MonoBehaviour
    {
        [Header("Leap Setup")]
        public LeapProvider leapProvider;
        public Chirality chirality;

        [Header("Glove Setup")]
        public GameObject head;
        public GameObject bar;
        public GameObject anchor;
        public GameObject pocketAnchor;

        [Header("Preferences")]
        public float k = 25.0f;
        public float damp = 8f,
                     dropoffDist = 0.3f,
                     cmOffset = 2f;

        [Header("State")]
        public bool gloveOn;
        public float gloveAmount;

        private Vector3 _localDisplacement, _localDisplacementPrev;
        private Vector3 _pocketDisplacement, _pocketDisplacementPrev, _pocketVelocityPrev;
        private Vector3 _barPosPrev, _localVelocityPrev;
        private float  _normalizedDistanceToHand;

        private float _gloveAmountPrev;
        private bool _gloveOnPrev;

        public UnityEvent<bool> OnGloveStateChange;
        public UnityEvent<float> WhileGloveChanging;

        private void Start()
        {
            if (leapProvider == null)
            {
                leapProvider = FindObjectOfType<LeapProvider>();
            }

            if (head == null)
            {
                head = Camera.main.gameObject;
            }

            _gloveOnPrev = gloveOn;
            _gloveAmountPrev = gloveAmount;

            OnGloveStateChange?.Invoke(gloveOn);
            WhileGloveChanging?.Invoke(gloveAmount);
        }

        void Update()
        {
            Hand swipingHand = HandUtils.Get(leapProvider, chirality == Chirality.Left ? Chirality.Right : Chirality.Left);
            Hand attachedHand = HandUtils.Get(leapProvider, chirality);

            Vector3 headPos = head.transform.position;

            if (swipingHand != null)
            {
                float distanceToHand = Vector3.Distance(swipingHand.GetMiddle().TipPosition, _barPosPrev);   // in global space
                _normalizedDistanceToHand = Mathf.Clamp01(
                                            Mathf.Pow((Mathf.Clamp(distanceToHand, 0f, dropoffDist) - dropoffDist) / dropoffDist,
                                                6f));  // calculates the nonlinear remapping of the distanceToHand where distance=0 outputs 1 and distance=.1 outputs 0
                // float distanceToBar = Mathf.Lerp(0, 1, Mathf.InverseLerp(0, 0.1f, leftFingerTipZPos));                   // alt
            }

            if (attachedHand != null)
            {
                // Gets a vector that points from the player's position to the target's.
                Vector3 headingToHead = attachedHand.WristPosition - headPos;

                // normalizes the vector
                float distToHead = headingToHead.magnitude;
                Vector3 dirToHead = headingToHead / distToHead; // This is now the normalized direction.
                gameObject.transform.SetPositionAndRotation(attachedHand.WristPosition + dirToHead * cmOffset * 0.01f, attachedHand.Rotation);
            }

            _barPosPrev = bar.transform.position;

            _localDisplacement.z = bar.transform.localPosition.z - anchor.transform.localPosition.z;
            _pocketDisplacement.z = bar.transform.localPosition.z - pocketAnchor.transform.localPosition.z;

            //Vector3 acceleration    = (localVelocity - localVelocityPrev) / Time.deltaTime;
            Vector3 localVelocity = (_localDisplacement - _localDisplacementPrev) / Time.deltaTime;
            Vector3 pocketVelocity = (_pocketDisplacement - _pocketDisplacementPrev) / Time.deltaTime;

            if (swipingHand != null)
            {
                Vector3 palmVelocityInLocalSpace = bar.transform.InverseTransformDirection(swipingHand.PalmVelocity);
                Vector3 palmVelocityRelativeToBar = Vector3.Project(palmVelocityInLocalSpace, Vector3.forward);
                localVelocity.z = Mathf.Lerp(localVelocity.z, palmVelocityRelativeToBar.z,
                                                   _normalizedDistanceToHand);
            }


            gloveAmount = (1 + Mathf.Cos(Mathf.PI *
                        (bar.transform.localPosition.z / (pocketAnchor.transform.localPosition.z - anchor.transform.localPosition.z))
                        )) / 2;

            if (_gloveAmountPrev != gloveAmount)
            {
                WhileGloveChanging?.Invoke(gloveAmount);
            }
            _gloveAmountPrev = gloveAmount;

            // alternative gloveAmount calculation using sigmoidFactory where k: steepness, going from 1 to 0
            //gloveAmount = 1 / (1 + Mathf.Exp(-2 * (bar.transform.localPosition.z - anchor.transform.localPosition.z)));
            //gloveAmount = 1 / (1 + Mathf.Exp(-k * (2*(bar.transform.localPosition.z / (pocketAnchor.transform.localPosition.z - anchor.transform.localPosition.z)) - 1)));

            Vector3 force = new Vector3(0, 0, (-k * _localDisplacement.z) - (damp * localVelocity.z));       // spring force
            Vector3 pocketForce = new Vector3(0, 0, (-k * _pocketDisplacement.z) - (damp * pocketVelocity.z));
            Vector3 sumForce = new Vector3(0, 0, Mathf.Lerp(pocketForce.z, force.z, gloveAmount));


            ////  kinematic equation solving for distance given a force and time
            // Vector3 distance = new Vector3(0, 0, (force.z * Time.deltaTime * Time.deltaTime) / 2);

            // bar.GetComponent<Rigidbody>().AddRelativeForce(force);
            // apply force to the bar without Rigidbody, instead setting position directly
            // bar.transform.localPosition += force * Time.deltaTime;

            bar.transform.localPosition += localVelocity * Time.deltaTime + (sumForce * Time.deltaTime * Time.deltaTime) / 2;

            if (Mathf.Min(bar.transform.localPosition.z - anchor.transform.localPosition.z,
                          pocketAnchor.transform.localPosition.z - bar.transform.localPosition.z)
                == bar.transform.localPosition.z - anchor.transform.localPosition.z)
            { gloveOn = true; }
            else
            { gloveOn = false; }

            if(_gloveOnPrev != gloveOn)
            {
                OnGloveStateChange?.Invoke(gloveOn);
            }
            _gloveOnPrev = gloveOn;


            _localDisplacementPrev = _localDisplacement;
            _localVelocityPrev = localVelocity;

            _pocketDisplacementPrev = _pocketDisplacement;
            _pocketVelocityPrev = pocketVelocity;
        }
    }
}