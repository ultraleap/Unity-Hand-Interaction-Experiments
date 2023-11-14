using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace Leap.Unity.Interaction.Storage
{
    public class Storage : MonoBehaviour
    {
        public class StorageObject
        {
            internal Action cachedOnGraspBegin;
            internal Action cachedOnGraspEnd;
            internal Action cachedOnGraspStay;

            public string guid;
            public GameObject gameObject;
            public InteractionBehaviour interactionBehaviour;
            public Vector3 storageLocalPosition;
            public PositionConstraint positionConstraint;
            public bool isStorage = false;

            // details about the object when it was stored
            public Vector3 lossyScale;
            public Transform originalParent;
            public bool wasKinematic;

            /// <summary>
            /// Enable/Disable physics for the object. Useful to disable whilst you're animating the storage object
            /// </summary>
            /// <param name="isEnabled"></param>
            public void EnablePhysics(bool isEnabled)
            {
                gameObject.GetComponent<InteractionBehaviour>().enabled = isEnabled;
                gameObject.GetComponent<Collider>().enabled = isEnabled;
                gameObject.GetComponent<Rigidbody>().detectCollisions = isEnabled;
            }
        }

        public enum PrestoredStorageSetup
        {
            MiddleOfStorageRadius,
            RandomOutOfBounds,
            RandomAll,
        }

        [Header("Setup")]
        [SerializeField] protected TappableInteractionBehaviour _tappableInteractionBehaviour;
        [SerializeField] protected AnchorableBehaviour _anchorableBehaviour;
        [SerializeField] protected Transform _meshTransform;

        [Tooltip("The logic in which Interaction Objects already childed to storage on Start get positioned." +
            "\n- Middle Of Storage Radius:  If an object is positioned outside of the storage's radius, move to the middle of the storage radius" +
            "\n- Random Out Of Bounds: Randomly position all objects outside the radius of the storage" +
            "\n- AllRandom: Randomly position all objects")]
        public PrestoredStorageSetup prestoredStorageSetup = PrestoredStorageSetup.MiddleOfStorageRadius;

        [Header("Storage Tweaks")]
        [Tooltip("The radius around the storage transform, for which objects are still in storage")]
        public float storageRadius = 0.4f;

        [Tooltip("The percentage of the storage for which objects will be stored between." +
            " E.g. if set to 0.05, objects will be stored up to 95% of the radius")]
        [Range(0, 1)] public float tensionPercentage = 0.05f;

        [Tooltip("If enabled, the storage will start open")]
        public bool startOpen = false;

        protected SphereCollider _sphereCollider;

        /// <summary>
        /// Objects considered for adding to the storage
        /// </summary>
        protected HashSet<InteractionBehaviour> _graspedObjectsInTrigger = new HashSet<InteractionBehaviour>();

        /// <summary>
        /// An unordered list of all objects in the storage
        /// </summary>
        public List<StorageObject> storageObjects = new List<StorageObject>();

        /// <summary>
        /// True if the storage is open
        /// </summary>
        protected bool _isStorageShowing = false;

        protected float timeLastEaten = 0.0f;
        protected float timeLastTapped = 0.0f;
        protected float timeLastGrasped = 0.0f;

        /// <summary>
        /// The amount in seconds that you must wait before performing another tap, or eat another object.
        /// </summary>
        public float actionCooldown = 0.4f;

        /// <summary>
        /// Number of storage objects currently grasped.
        /// </summary>
        public int NumberOfGraspedStorageObjects { get; protected set; }

        public bool drawDebugGizmos = false;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            if (_tappableInteractionBehaviour == null) { _tappableInteractionBehaviour = GetComponentInChildren<TappableInteractionBehaviour>(); }
            _tappableInteractionBehaviour.OnTap += OnTap;
            _tappableInteractionBehaviour.OnGraspBegin += OnGrasp;
            _tappableInteractionBehaviour.OnGraspEnd += OnGrasp;

            if (_anchorableBehaviour == null) { _anchorableBehaviour = GetComponentInChildren<AnchorableBehaviour>(); }

            if (_anchorableBehaviour != null)
            { _anchorableBehaviour.WhileAttachedToAnchor += AttachedToAnchor; }
            if (_sphereCollider == null) { _sphereCollider = GetComponentInChildren<SphereCollider>(); }
            if (_meshTransform == null) { _meshTransform = transform; }

            PositionPrestoredStorage();

            if (startOpen)
            {
                ShowStorage();
            }
            else
            {
                HideStorage();
            }
        }

        protected virtual void AttachedToAnchor()
        {
            if (this.enabled != _anchorableBehaviour.gameObject.activeInHierarchy)
            {
                return;
            }

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(_anchorableBehaviour.anchor.gameObject.activeInHierarchy);
            }

            this.enabled = _anchorableBehaviour.gameObject.activeInHierarchy;
        }

        protected virtual void OnGrasp()
        {
            timeLastGrasped = Time.time;
        }

        protected virtual void PositionPrestoredStorage()
        {
            List<InteractionBehaviour> children = GetComponentsInChildren<InteractionBehaviour>().Where(ib => ib.gameObject != gameObject).ToList();

            foreach (InteractionBehaviour child in children)
            {
                if (prestoredStorageSetup == PrestoredStorageSetup.RandomAll)
                {
                    AddToStorage(child.gameObject, true);
                }
                else
                {
                    if (Vector3.Distance(child.transform.position, gameObject.transform.position) > storageRadius)
                    {
                        switch (prestoredStorageSetup)
                        {
                            case PrestoredStorageSetup.MiddleOfStorageRadius:
                                child.transform.position = transform.position + (child.transform.position - transform.position).normalized * (storageRadius / 2);
                                AddToStorage(child.gameObject, false);
                                break;
                            default:
                            case PrestoredStorageSetup.RandomOutOfBounds:
                                AddToStorage(child.gameObject, true);
                                break;
                        }
                    }
                    else
                    {
                        AddToStorage(child.gameObject, false);
                    }
                }
            }
        }

        public void OnTap()
        {
            timeLastTapped = Time.time;

            if (Time.time - timeLastEaten < actionCooldown
                || Time.time - timeLastGrasped < actionCooldown
                || NumberOfGraspedStorageObjects > 0)
            {
                return;
            }

            if (!_isStorageShowing)
            {
                ShowStorage();
                _isStorageShowing = true;
            }
            else
            {
                HideStorage();
                _isStorageShowing = false;
            }

        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            // Register the object entering the trigger as potential for storage 

            if (isValidObjectToAddToStorage(other.gameObject))
            {
                _graspedObjectsInTrigger.Add(other.GetComponent<InteractionBehaviour>());
            }
        }

        private void OnTriggerStay(Collider other)
        {
            // If a potential object is ungrasped, 
            if (other.GetComponent<InteractionBehaviour>() != null && _graspedObjectsInTrigger.Contains(other.GetComponent<InteractionBehaviour>()))
            {
                if (!other.GetComponent<InteractionBehaviour>().isGrasped)
                {
                    _graspedObjectsInTrigger.Remove(other.GetComponent<InteractionBehaviour>());

                    AddToStorage(other.gameObject, true);
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            // Register the object entering the trigger as potential for storage 

            if (other.GetComponent<InteractionBehaviour>() != null)
            {
                _graspedObjectsInTrigger.Remove(other.GetComponent<InteractionBehaviour>());
            }
        }

        protected virtual StorageObject AddToStorage(GameObject objectToStore, bool randomPosition)
        {
            timeLastEaten = Time.time;
            PositionConstraint posConstraint = objectToStore.AddComponent<PositionConstraint>();
            ConstraintSource constraintSrc = new ConstraintSource();
            constraintSrc.sourceTransform = transform;
            constraintSrc.weight = 1;
            posConstraint.AddSource(constraintSrc);
            posConstraint.weight = 1;
            posConstraint.constraintActive = false;

            Rigidbody rigidbody = objectToStore.GetComponent<Rigidbody>();

            StorageObject storageObject = new StorageObject()
            {
                gameObject = objectToStore,
                interactionBehaviour = objectToStore.GetComponent<InteractionBehaviour>(),
                lossyScale = objectToStore.transform.lossyScale,
                originalParent = objectToStore.transform.parent,
                positionConstraint = posConstraint,
                guid = Guid.NewGuid().ToString(),
                wasKinematic = rigidbody.isKinematic,
                isStorage = objectToStore.GetComponent<Storage>() != null
            };

            rigidbody.isKinematic = true;
            objectToStore.transform.parent = transform;
            //Setup grasp callbacks
            storageObject.cachedOnGraspBegin += () =>
            {
                OnStorageObjectGraspBegin(storageObject);
            };
            storageObject.interactionBehaviour.OnGraspBegin += storageObject.cachedOnGraspBegin;

            storageObject.cachedOnGraspStay += () =>
            {
                OnStorageObjectGraspStay(storageObject);
            };
            storageObject.interactionBehaviour.OnGraspStay += storageObject.cachedOnGraspStay;

            storageObject.cachedOnGraspEnd += () =>
            {
                OnStorageObjectGraspEnd(storageObject);
            };
            storageObject.interactionBehaviour.OnGraspEnd += storageObject.cachedOnGraspEnd;

            if (randomPosition)
            {
                storageObject.gameObject.transform.position = RandomStartingPosition();
            }
            storageObject.storageLocalPosition = storageObject.gameObject.transform.localPosition;
            storageObject.positionConstraint.translationOffset = storageObject.storageLocalPosition;

            storageObjects.Add(storageObject);

            if (!_isStorageShowing)
            {
                storageObject.gameObject.SetActive(false);
            }

            return storageObject;
        }

        protected bool isValidObjectToAddToStorage(GameObject objectToStore)
        {
            if (objectToStore.GetComponent<InteractionBehaviour>() == null)
            {
                return false;
            }

            if (!objectToStore.GetComponent<InteractionBehaviour>().isGrasped)
            {
                return false;
            }

            if (storageObjects.Any(storageObject => storageObject.gameObject == objectToStore))
            {
                //Item already in this storage
                return false;
            }

            // Don't add if already in another storage
            List<Storage> storages = FindObjectsOfType<Storage>().Where(storage => storage != this).ToList();
            foreach (Storage storage in storages)
            {
                StorageObject s = storage.storageObjects.FirstOrDefault(storageObject => storageObject.gameObject == objectToStore);
                if (s != null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// A random position inside the storage sphere, between sphereColliderRadius + 0.05 & tension percentage
        /// </summary>
        /// <returns></returns>
        protected virtual Vector3 RandomStartingPosition()
        {
            Vector3 direction = UnityEngine.Random.insideUnitCircle.normalized;
            float randomVal = UnityEngine.Random.Range(_sphereCollider.radius * _meshTransform.lossyScale.CompMax() + 0.05f, (storageRadius * 1 - tensionPercentage));
            return transform.position + (direction * randomVal);
        }

        protected virtual void OnStorageObjectGraspBegin(StorageObject storageObject)
        {
            storageObject.positionConstraint.constraintActive = false;
            storageObject.gameObject.transform.parent = null;
            NumberOfGraspedStorageObjects++;
        }

        protected virtual void OnStorageObjectGraspStay(StorageObject storageObject)
        {
            float distanceFromStorage = Vector3.Distance(transform.position, storageObject.gameObject.transform.position);
            if (distanceFromStorage > storageRadius)
            {
                RemoveFromStorage(storageObject);
            }
        }

        protected virtual void OnStorageObjectGraspEnd(StorageObject storageObject)
        {
            storageObject.gameObject.transform.parent = transform;
            storageObject.storageLocalPosition = storageObject.gameObject.transform.localPosition;
            storageObject.positionConstraint.translationOffset = storageObject.storageLocalPosition;
            storageObject.positionConstraint.constraintActive = true;
            NumberOfGraspedStorageObjects--;
        }

        protected virtual void RemoveFromStorage(StorageObject storageObject)
        {
            storageObject.gameObject.GetComponent<InteractionBehaviour>().OnGraspBegin -= storageObject.cachedOnGraspBegin;
            storageObject.gameObject.GetComponent<InteractionBehaviour>().OnGraspStay -= storageObject.cachedOnGraspStay;
            storageObject.gameObject.GetComponent<InteractionBehaviour>().OnGraspEnd -= storageObject.cachedOnGraspEnd;

            storageObject.gameObject.transform.parent = storageObject.originalParent;
            storageObject.gameObject.GetComponent<InteractionBehaviour>().SetKinematicWithoutGrasp(storageObject.wasKinematic);
            NumberOfGraspedStorageObjects--;

            Destroy(storageObject.positionConstraint);
            storageObjects.Remove(storageObject);
        }

        protected virtual void ShowStorage()
        {
            foreach (StorageObject storageObj in storageObjects)
            {
                ShowStorageObject(storageObj);
            }
        }

        protected virtual void ShowStorageObject(StorageObject storageObject)
        {
            storageObject.gameObject.SetActive(true);
        }

        protected virtual void HideStorage()
        {
            foreach (StorageObject storageObj in storageObjects)
            {
                HideStorageObject(storageObj);
            }
        }

        protected virtual void HideStorageObject(StorageObject storageObject)
        {
            List<InteractionController> graspingControllers = new List<InteractionController>();
            foreach (InteractionController interactionController in storageObject.interactionBehaviour.graspingControllers)
            {
                graspingControllers.Add(interactionController);
            }
            storageObject.interactionBehaviour.EndGrasp(graspingControllers);
            storageObject.gameObject.SetActive(false);
        }

        /// <summary>
        /// Use this if the storage is attached to an anchor on a hand menu, to time the storage along with the hand menu.
        /// </summary>
        /// <param name="isFacing"></param>
        public virtual void PalmFacingCameraCallback(bool isFacing)
        {
            if (_anchorableBehaviour == null || !_anchorableBehaviour.isAttached)
            {
                return;
            }

            if (isFacing && _isStorageShowing)
            {
                StartCoroutine(DelayedShowStorage());
            }

            if (!isFacing && _isStorageShowing)
            {
                HideStorage();
            }
        }

        protected virtual IEnumerator DelayedShowStorage()
        {
            yield return new WaitForSeconds(0.5f);
            ShowStorage();
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmos)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, storageRadius);
            Gizmos.color = Color.Lerp(Color.white, Color.green, 0.5f);
            Gizmos.DrawWireSphere(transform.position, storageRadius * (1 - tensionPercentage));
        }
    }
}