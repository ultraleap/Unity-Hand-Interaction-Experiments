using System;
using UnityEngine;

namespace Leap.Unity.Interaction.Storage
{
    /// <summary>
    /// Interaction Behaviour extended to allow for Tap events.
    /// Note that an Interaction engine bug means that if used with a collider
    /// where IsTrigger is set to false, you will experience unwanted behaviour. 
    /// </summary>
    public class TappableInteractionBehaviour : InteractionBehaviour
    {
        /// <summary>
        /// Called when a TappableInteractionBehaviour is tapped or pinched without being moved by a grasp
        /// Useful for activation/deactivation.
        /// </summary>
        public Action OnTap;

        protected float _timeoutAfterGrasp = 0.25f;
        protected float _timeLastGrasped = -1;

        /// <summary>
        /// The position of the storage when it was grasped
        /// </summary>
        protected Vector3 _positionOnContact;

        // Start is called before the first frame update
        protected override void OnEnable()
        {
            base.OnEnable();
            OnContactEnd += CheckForTap;
            OnGraspEnd += SetTimeLastGrasped;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnContactEnd -= CheckForTap;
            OnGraspEnd -= SetTimeLastGrasped;
        }

        protected void CheckForTap()
        {
            // If other controllers are still in contact with this IB, return
            if (contactingControllers.Count > 1)
            {
                return;
            }

            //If the object is being grasped, return
            foreach (InteractionController interactionController in contactingControllers)
            {
                if (interactionController.isGraspingObject)
                {
                    return;
                }
            }

            if (Time.time - _timeLastGrasped > _timeoutAfterGrasp)
            {
                OnTap?.Invoke();
            }
        }

        protected virtual void SetTimeLastGrasped()
        {
            _timeLastGrasped = Time.time;
        }
    }
}