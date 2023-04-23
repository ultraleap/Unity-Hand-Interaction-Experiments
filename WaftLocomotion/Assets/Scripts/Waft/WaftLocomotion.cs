/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace Leap.Unity
{

    /// <summary>Ensure this script is on your player object and 
    /// set to execute before the LeapXRServiceProvider</summary>
    public class WaftLocomotion : MonoBehaviour
    {
        [Tooltip("Your Leap Hand Provider.  Ensure the Pinch Locomotion script " +
                 "is set to execute before this provider in the 'Script Execution Order'")]
        public LeapServiceProvider leapProvider;

        [Tooltip("How far apart the fingers need to be to count as a pinch [mm].")]
        [Range(0.00f, 50f)]
        public float pinchThreshold = 25f;

        [Tooltip("How much your movements affect the current momentum")]
        [Range(0.000f, 1f)]
        public float coupling = 0.144f;

        [Tooltip("How much your momentum dampens over time.")]
        [Range(0.0f, 0.025f)]
        public float damping = 0.007f;

        [Tooltip("How much your momentum dampens over time while pinching")]
        [Range(0.0f, 0.025f)]
        public float pinchDamping = 0f;

        [Tooltip("Multiplies the hand velocity by this number to accelerate faster speeds than hands")]
        [Range(1, 30)]
        public float amplification = 10f;

        [HideInInspector]
        public Vector3 currentVel, currentRotVel;

        public struct WaftHandData
        {
            public Vector3 worldVel, localVel, localRotVel, pinchPosition, palmPos, localPos, posPrev, localPosPrev, posPrevWhenLost;
            public bool activeLastFrame, pinching;
        }
        public WaftHandData[] handData = new WaftHandData[2];

        float velSum;

        public AnimationCurve velocityCouplingGraph;

        private float _waftStrength = 0;
        private bool _locomotionActive = true;


        void Start()
        {
            for (int i = 0; i < handData.Length; i++)
            {
                handData[i].posPrev = Vector3.zero;
                handData[i].localPosPrev = Vector3.zero;
                handData[i].posPrevWhenLost = Vector3.zero;
                handData[i].activeLastFrame = false;
            }

            if (leapProvider == null)
            {
                leapProvider = FindObjectOfType<LeapServiceProvider>();
            }
        }

        void Update()
        {

            // Set the localspace transform of the new previous frame so we have the provider to be set with the previous provider state
            // leftLocalPosPrev = leftLocalPos; //provider.transform.InverseTransformPoint(leftPos);
            //         rightLocalPosPrev = rightLocalPos; //provider.transform.InverseTransformPoint(rightPos);
            // leftPosPrev = leftPos;
            //rightPosPrev = rightPos;

            UpdateHandData();

            ///////////////////////////////
            // gravity
            //             RaycastHit hit;
            //             Ray downRay = new Ray(transform.root.position, -Vector3.up);
            // ​
            //             if (!gravityOn) {transform.root.position -= currentVel / Time.deltaTime;}
            //             else{
            //                 if (Physics.Raycast(downRay, out hit) && hit.distance > hoverHeight) {
            //                     currentVel.y -= gravitationalAcceleration * Time.deltaTime;
            //                     transform.root.position -= currentVel / Time.deltaTime;
            //                     }
            //                 else { transform.root.position -= currentVel / Time.deltaTime;} 
            //                 }
            //             currentVel *= 1.0f - damping;

            // coupling =1 produces 1:1 ("step") locomotion. coupling <1 produces "waft" locomotion
            // this is probably worth changing so that there isn't a discontinuity,
            // maybe lerping between an output velocity from direct and waft woud give the continuous 0-1 behavior ?
            // try again with a lerp between two separate currentVels

            if (_locomotionActive)
            {
                if (coupling == 1) { DirectLocomotion(); }
                else { Waft(); }
            }

            //currentVel *= 1.0f - ((leftPinching || rightPinching )? pinchDamping : damping);
            // consider a squared damping to emulate drag over surface area
            currentVel *= 1.0f - damping;  // reverted as pinchDamping=0 was forcing the system to maintain velocity without naturally decelerating from global drag

            transform.root.position -= currentVel * Time.deltaTime;
            //transform.root.position -= new Vector3 (currentVel.x * Time.deltaTime, 0, currentVel.z * Time.deltaTime); // xz only
            leapProvider.RetransformFrames();
        }

        private void UpdateHandData()
        {
            for (int i = 0; i < handData.Length; i++)
            {

                Hand hand = leapProvider.CurrentFrame.GetHand((Chirality)i);
                handData[i].pinching = hand != null && hand.PinchDistance < pinchThreshold;

                if (hand != null)
                {
                    handData[i].palmPos = hand.PalmPosition;
                    handData[i].pinchPosition = hand.GetPinchPosition();
                    handData[i].localPos = transform.root.InverseTransformPoint(handData[i].pinchPosition); //provider.transform.InverseTransformPoint(leftPos);                    
                    if (handData[i].activeLastFrame)
                    {         // this makes sure we have a previous position to calculate a velocity from
                        handData[i].localVel = (handData[i].localPos - handData[i].localPosPrev) / Time.deltaTime;
                        handData[i].worldVel = transform.root.TransformVector(handData[i].localVel);   // Vel      = (leftPos      - leftPosPrev)      / Time.deltaTime;
                    }
                    else
                    {
                        handData[i].localVel = Vector3.zero;
                        handData[i].worldVel = Vector3.zero;

                        // Use the last valid position when tracking was lost as the previous position
                        handData[i].posPrev = handData[i].posPrevWhenLost;
                        handData[i].localPosPrev = transform.root.InverseTransformPoint(handData[i].posPrev);
                    }

                    // Set the localspace transform of the new previous frame so we have the provider to be set with the previous provider state
                    handData[i].localPosPrev = handData[i].localPos; //provider.transform.InverseTransformPoint(leftPos);
                    handData[i].posPrev = handData[i].pinchPosition;

                    handData[i].activeLastFrame = true;
                }
                else
                {
                    // Store the last valid position when tracking is lost
                    if (handData[i].activeLastFrame)
                    {
                        handData[i].posPrevWhenLost = handData[i].posPrev;
                    }
                    handData[i].activeLastFrame = false;
                }
            }
        }

        void DirectLocomotion()
        {
            //during "step" locomotion, the head should be static if the hands aren't moving
            // this should mean that direct locomotion can't coast. you only move if your pinch moves
            // Vector3 totalVelocity = Vector3.zero;  

            // // flo original
            // //Locomotion velocity is average of hand velocities if both pinching:
            // if (leftPinching && rightPinching)
            //     totalVelocity = (left.PalmVelocity.ToVector3() +  right.PalmVelocity.ToVector3()) * 0.5f;
            // else if (leftPinching && !rightPinching)
            //     totalVelocity = left.PalmVelocity.ToVector3();
            // else if (!leftPinching && rightPinching)
            //     totalVelocity = right.PalmVelocity.ToVector3();
            // currentVel = totalVelocity; //Vector3.LerpUnclamped(currentVel,  totalVelocity,  coupling); //Since coupling is 1 simplify this line

            // priming up some changes to how coupling works to enable gradual dropoff of direct coupling
            // currently this is in effect equivalent to the prior method until coupling is <1 at which point
            // at which point we should change the if coupling=1 then direct else waft 

            WaftHandData left = handData[0];
            WaftHandData right = handData[1];

            if (left.pinching && right.pinching)
            {
                currentVel = ((left.worldVel + right.worldVel) * 0.5f);
                // currentVel = Vector3.LerpUnclamped( currentVel, ((leftWorldVel + rightWorldVel) * 0.5f), coupling );
            }
            if (left.pinching && !right.pinching)
            {
                currentVel = left.worldVel;
                // currentVel = Vector3.LerpUnclamped( currentVel, leftWorldVel, coupling );
            }
            else if (!left.pinching && right.pinching)
            {
                currentVel = right.worldVel;
                // currentVel = Vector3.LerpUnclamped( currentVel, rightWorldVel, coupling );
            }
        }

        void Waft()
        {

            Vector3 localVel = Vector3.zero;
            Vector3 worldVel = Vector3.zero;

            WaftHandData left = handData[0];
            WaftHandData right = handData[1];

            if (left.pinching && right.pinching)
            {
                localVel = (left.localVel + right.localVel); // * 0.5f;
                worldVel = (left.worldVel + right.worldVel); // * 0.5f;     // worldVel = (left.PalmVelocity.ToVector3() + right.PalmVelocity.ToVector3()) * 0.5f;
            }
            else if (left.pinching && !right.pinching)
            {
                localVel = left.localVel;
                worldVel = left.worldVel;     // worldVel = left.PalmVelocity.ToVector3();
                currentRotVel = left.localRotVel;
            }
            else if (!left.pinching && right.pinching)
            {
                localVel = right.localVel;
                worldVel = right.worldVel;    // worldVel = right.PalmVelocity.ToVector3();
            }
            else if (!left.pinching && !right.pinching) { }

            localVel *= _waftStrength;
            worldVel *= _waftStrength;

            // flo's approach 
            // float maxHandVelocity   = maxHandSpeed; //10 m/s is fastest Gray's hand moves
            // float velocityCoupling  = Mathf.Clamp01(localVel.magnitude / maxHandVelocity); //0-1 value representing how close hand is moving at maximum velocity
            // float maxHeadVelocity   = amplification; //e.g. 3x faster than max hand velocity would be value of 3

            // //in order to accelerate to high velocities, we add extra velocity as a function of how fast hand is moving
            // Vector3 extraVelocity = worldVel.normalized * maxHandVelocity * Mathf.Pow(velocityCoupling,velocityRampPower) * maxHeadVelocity; 

            //in waft mode, we're adding the local velocity of hand (relative to head) to head velocity
            velSum = Vector3.LerpUnclamped(Vector3.zero,
                worldVel.normalized * localVel.magnitude * amplification * Time.deltaTime, velocityCouplingGraph.Evaluate(localVel.magnitude)).magnitude;

            currentVel += Vector3.LerpUnclamped(Vector3.zero,
                                                worldVel.normalized * localVel.magnitude * amplification * Time.deltaTime,    // (worldVel + extraVelocity) * conversionfactor * Time.deltaTime,
                                                    velocityCouplingGraph.Evaluate(localVel.magnitude)                                 //  Mathf.Pow(coupling,0.5f)
            );
            /*
            TODO:
            (DONE) -during waft locomotion, if the head is in motion, a locally-static pinching hand does not brake the head and instead allows the head to coast normally because there is no hand velocity to append to the head velocity (the current script causes the head to brake as soon as the hand pinches)
            (DONE)-during waft locomotion, if the pinching hand is locally in motion, that velocity influences the head velocity. if the pinching hand is locally static, the velocity appended to the head is zero (NOT causing the head velocity to tend towards zero)
            (DONE)-during waft locomotion, the only way to brake the body is through 1. dampening or 2. moving a pinch point in the opposite direction as the head movement (think of the hand as if it is a retrothruster. only by shooting a reaction mass in the opposite direction can you decelerate)
            (DONE)-during waft locomotion, slow hand velocity applies a lesser velocity to the head. fast hand velocity applies a magnified velocity to the head (this allows you to move through space faster than your hands can move (this means you can run and accrue speed and traverse distances far greater than otherwise)) (this was previously accomplished with a velocityCoupling animationCurve that ramped from a slope <1 to >1 driven by hand velocity
            -during waft locomotion, dual pinching should average the velocities of each pinchpoint
            */
        }

        public void UpdateWaftStrength(float strength)
        {
            _waftStrength = strength;
        }

        public void SetLocomotionActive(bool isActive)
        {
            _locomotionActive = isActive;
        }
    }
}