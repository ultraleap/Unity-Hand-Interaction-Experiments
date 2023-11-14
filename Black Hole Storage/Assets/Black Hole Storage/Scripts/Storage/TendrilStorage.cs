using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Interaction.Storage
{
    public class TendrilStorage : Storage
    {
        [Header("Setup")]
        public GameObject FABRIKTendrilPrefab;

        [Header("Visuals")]

        [SerializeField, Tooltip("Used when the storage is closed")]
        private Material _idleMat;

        [SerializeField, Tooltip("Used when the storage is open")]
        private Material _openMat;

        [SerializeField, Tooltip("Evaluated when an object is being \"eaten\"")]
        private AnimationCurve _eatStorageAnimationCurve;

        [SerializeField, Tooltip("Evaluated when the storage itself is shrinking whilst one of its object has high tension")]
        private AnimationCurve _storageTensionCurve;

        [SerializeField, Tooltip("Evaluated to move an object in/out of shown/hidden")]
        private AnimationCurve _showHideStorageAnimationCurve;
        [SerializeField, Tooltip("The amount of time it takes for a storage object to be shown/hidden")]
        private float _showHideStorageAnimationDuration = 0.25f;

        [SerializeField, Tooltip("The amount of time it takes for an object to be eaten")]
        private float _eatStorageAnimationDuration = 0.15f;

        [SerializeField]
        [Range(0, 1), Tooltip("The amount the mesh will shrink at max tension")]
        private float _meshVisualTensionReductionPercentage = 0.3f;

        [SerializeField]
        [Range(0, 1), Tooltip("The amount the mesh will grow when a valid object is held in it.")]
        private float _meshReadyToEatGrowthPercentage = 0.5f;

        [SerializeField, Tooltip("How fast the mesh scales")]
        private float _meshScaleLerpSpeed = 15f;

        [SerializeField, Tooltip("How fast it takes for a tendril to go from straight to curled.")]
        private float _tendrilToStraightTime = 0.2f;

        /// <summary>
        /// All fabrik tendrils associated with the storage. Accessed by passing in a storage object's guid.
        /// </summary>
        public Dictionary<string, FABRIKTendril> fabrikTendrils = new Dictionary<string, FABRIKTendril>();

        private Vector3 _meshLocalScaleOnStart;
        private Vector3 _targetLocalMeshScale;
        private Dictionary<string, Coroutine> transitioningCoroutines = new Dictionary<string, Coroutine>();

        private const float TRANSPARENT = 0.0f;
        private const float TRANSLUCENT = 0.2f;
        private const float OPAQUE = 1.0f;

        protected override void Start()
        {
            base.Start();
            _meshLocalScaleOnStart = _meshTransform.localScale;
            _targetLocalMeshScale = _meshLocalScaleOnStart;

            _tappableInteractionBehaviour.OnHoverBegin += OnHoverBegin;
            _tappableInteractionBehaviour.OnHoverEnd += OnHoverEnd;
        }

        private void OnEnable()
        {
            if (_tappableInteractionBehaviour.isHovered)
            {
                foreach (StorageObject so in storageObjects)
                {
                    if (!so.interactionBehaviour.isGrasped && fabrikTendrils[so.guid].isActiveAndEnabled)
                    {
                        fabrikTendrils[so.guid].FadeAlpha(TRANSLUCENT);
                    }
                }
            }
            else
            {
                foreach (StorageObject so in storageObjects)
                {
                    if (!so.interactionBehaviour.isGrasped && fabrikTendrils[so.guid].isActiveAndEnabled && !so.isStorage)
                    {
                        fabrikTendrils[so.guid].FadeAlpha(TRANSPARENT);
                    }
                }
            }
        }

        protected void OnHoverBegin()
        {
            if (_isStorageShowing)
            {
                foreach (StorageObject so in storageObjects)
                {
                    if (!so.interactionBehaviour.isGrasped && fabrikTendrils[so.guid].isActiveAndEnabled)
                    {
                        fabrikTendrils[so.guid].FadeAlpha(TRANSLUCENT);
                    }
                }
            }
        }

        protected void OnHoverEnd()
        {
            if (_isStorageShowing)
            {
                foreach (StorageObject so in storageObjects)
                {
                    if (!so.interactionBehaviour.isGrasped && fabrikTendrils[so.guid].isActiveAndEnabled && !so.isStorage)
                    {
                        fabrikTendrils[so.guid].FadeAlpha(TRANSPARENT);
                    }
                }
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            if (isValidObjectToAddToStorage(other.gameObject))
            {
                _targetLocalMeshScale = _meshLocalScaleOnStart * (1 + _meshReadyToEatGrowthPercentage);
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);

            if (_graspedObjectsInTrigger.Count == 0)
            {
                _targetLocalMeshScale = _meshLocalScaleOnStart;
            }
        }

        private void Update()
        {
            if (_targetLocalMeshScale != _meshTransform.localScale)
            {
                LerpMeshScale();
            }
        }

        void LerpMeshScale()
        {
            _meshTransform.localScale = Vector3.Lerp(_meshTransform.localScale, _targetLocalMeshScale, Time.deltaTime * _meshScaleLerpSpeed);

            if (Vector3.Distance(_meshTransform.localScale, _targetLocalMeshScale) < 0.001f)
            {
                _meshTransform.localScale = _targetLocalMeshScale;
            }
        }

        protected override StorageObject AddToStorage(GameObject objectToStore, bool randomPosition = true)
        {
            timeLastEaten = Time.time;

            Vector3 localStartingPos = transform.InverseTransformPoint(objectToStore.transform.position);

            StorageObject addedStorageObject = base.AddToStorage(objectToStore, randomPosition);

            //If there aren't any other objects ready to be added, return the storage to its default scale
            if (_graspedObjectsInTrigger.Count == 0)
            {
                _targetLocalMeshScale = _meshLocalScaleOnStart;
            }

            //Setup fabrik tendril 
            string guid = addedStorageObject.guid;

            fabrikTendrils.Add(guid, Instantiate(FABRIKTendrilPrefab).GetComponent<FABRIKTendril>());
            fabrikTendrils[guid].transform.parent = transform;
            fabrikTendrils[guid].transform.localPosition = Vector3.zero;
            fabrikTendrils[guid].SetupTendril(transform.position, addedStorageObject.gameObject.transform, storageRadius, tensionPercentage);

            objectToStore.gameObject.SetActive(true);
            AddToStorageAnimation(addedStorageObject, localStartingPos);

            return addedStorageObject;
        }

        /// <summary>
        /// Spawns in a random starting position on the extremities of the storage radius, rather than anywhere in the storage radius.
        /// This is because the tendrils look nicer when initialised to an extreme value!
        /// </summary>
        protected override Vector3 RandomStartingPosition()
        {
            Vector3 direction = Random.insideUnitSphere.normalized;
            float randomVal = Random.Range(storageRadius * (0.90f - tensionPercentage), storageRadius * (1 - tensionPercentage));
            return transform.position + (direction * randomVal);
        }

        protected override void OnStorageObjectGraspBegin(StorageObject storageObject)
        {
            if (!_isStorageShowing) return;
            base.OnStorageObjectGraspBegin(storageObject);
            fabrikTendrils[storageObject.guid].FadeAlpha(OPAQUE);
            fabrikTendrils[storageObject.guid].easyIK.enabled = true;
        }

        protected override void OnStorageObjectGraspStay(StorageObject storageObject)
        {
            if (!_isStorageShowing) return;
            base.OnStorageObjectGraspStay(storageObject);
            float distanceFromStorage = Vector3.Distance(transform.position, storageObject.gameObject.transform.position);

            float tension = distanceFromStorage / storageRadius;
            fabrikTendrils[storageObject.guid].UpdateTendrilTension(tension);
            UpdateStorageTension(tension);
        }

        protected override void OnStorageObjectGraspEnd(StorageObject storageObject)
        {
            storageObject.gameObject.transform.parent = transform;
            fabrikTendrils[storageObject.guid].easyIK.enabled = false;
            if (!_isStorageShowing) return;

            if (!fabrikTendrils[storageObject.guid].transitioning)
            {
                storageObject.storageLocalPosition = storageObject.gameObject.transform.localPosition;
            }
            storageObject.positionConstraint.translationOffset = storageObject.storageLocalPosition;
            storageObject.positionConstraint.constraintActive = true;


            if (_tappableInteractionBehaviour.isHovered || storageObject.isStorage)
            {
                fabrikTendrils[storageObject.guid].FadeAlpha(TRANSLUCENT);
            }
            else
            {
                fabrikTendrils[storageObject.guid].FadeAlpha(TRANSPARENT);
            }

            _targetLocalMeshScale = _meshLocalScaleOnStart;
            NumberOfGraspedStorageObjects--;
        }

        protected override void RemoveFromStorage(StorageObject storageObject)
        {
            fabrikTendrils[storageObject.guid].AnimatedDestroy();
            base.RemoveFromStorage(storageObject);
            //Wait for callbacks to be removed before reseting the mesh scale
            StartCoroutine(ResetMeshScaleAfterOneFrame());
        }

        private IEnumerator ResetMeshScaleAfterOneFrame()
        {
            yield return null;
            _targetLocalMeshScale = _meshLocalScaleOnStart;
        }

        #region Storage Animation

        private void AddToStorageAnimation(StorageObject storageObject, Vector3 startingPos)
        {
            StartCoroutine(EatObject(storageObject, startingPos));
        }

        private IEnumerator EatObject(StorageObject storageObject, Vector3 startingPos)
        {
            TendrilStorage tendrilStorage = storageObject.gameObject.GetComponent<TendrilStorage>();

            if (tendrilStorage != null)
            {
                foreach (StorageObject so in tendrilStorage.storageObjects)
                {
                    tendrilStorage.fabrikTendrils[so.guid].easyIK.enabled = false;
                }
            }

            float startTime = Time.time;

            while (Time.time - startTime < _eatStorageAnimationDuration)
            {
                float t = (Time.time - startTime) / _eatStorageAnimationDuration;

                storageObject.gameObject.transform.localPosition = Vector3.Lerp(startingPos, Vector3.zero, _eatStorageAnimationCurve.Evaluate(t));
                storageObject.gameObject.transform.SetLossyScale(Vector3.Lerp(storageObject.lossyScale, Vector3.zero, _eatStorageAnimationCurve.Evaluate(t)));

                yield return null;
            }

            if (_isStorageShowing)
            {
                ShowStorageObject(storageObject);
            }
            else
            {
                storageObject.gameObject.SetActive(false);
            }
        }

        private void UpdateStorageTension(float tension)
        {
            float minTensionRange = tension - tensionPercentage;
            if (tension < tension - tensionPercentage) { return; }

            float t = Mathf.InverseLerp(minTensionRange, 1, tension);

            //Grasped objects in trigger take priority over showing tension on the storage
            if (_graspedObjectsInTrigger.Count == 0)
            {
                _targetLocalMeshScale = Vector3.Lerp(_meshLocalScaleOnStart, _meshLocalScaleOnStart * (1 - _meshVisualTensionReductionPercentage), _storageTensionCurve.Evaluate(t));
            }
        }

        protected override void ShowStorage()
        {
            _meshTransform.GetComponent<MeshRenderer>().material = _openMat;
            base.ShowStorage();
        }

        protected override void ShowStorageObject(StorageObject storageObject)
        {
            if (transitioningCoroutines.ContainsKey(storageObject.guid))
            {
                StopCoroutine(transitioningCoroutines[storageObject.guid]);
            }

            transitioningCoroutines[storageObject.guid] = StartCoroutine(AnimatedShowStorageObject(storageObject));
            fabrikTendrils[storageObject.guid].Grow(_tendrilToStraightTime, _showHideStorageAnimationDuration);
        }

        protected override void HideStorage()
        {
            _meshTransform.GetComponent<MeshRenderer>().material = _idleMat;
            base.HideStorage();
        }

        protected override void HideStorageObject(StorageObject storageObject)
        {
            if (transitioningCoroutines.ContainsKey(storageObject.guid))
            {
                StopCoroutine(transitioningCoroutines[storageObject.guid]);
            }

            transitioningCoroutines[storageObject.guid] = StartCoroutine(AnimatedHideStorageObject(storageObject));
            fabrikTendrils[storageObject.guid].Shrink(_tendrilToStraightTime, _showHideStorageAnimationDuration);
        }

        private IEnumerator AnimatedShowStorageObject(StorageObject storageObject)
        {
            yield return null;

            float startTime = Time.time;
            storageObject.EnablePhysics(false);

            fabrikTendrils[storageObject.guid].FadeAlpha(TRANSLUCENT);

            storageObject.gameObject.transform.localPosition = Vector3.zero;
            storageObject.gameObject.transform.SetLossyScale(Vector3.zero);

            storageObject.positionConstraint.constraintActive = false;
            storageObject.gameObject.SetActive(true);

            while (Time.time - startTime < _showHideStorageAnimationDuration)
            {
                float t = (Time.time - startTime) / _showHideStorageAnimationDuration;

                storageObject.gameObject.transform.localPosition = Vector3.Lerp(Vector3.zero, storageObject.storageLocalPosition, _showHideStorageAnimationCurve.Evaluate(t));
                storageObject.gameObject.transform.SetLossyScale(Vector3.Lerp(Vector3.zero, storageObject.lossyScale, _showHideStorageAnimationCurve.Evaluate(t)));
                yield return null;
            }

            storageObject.EnablePhysics(true);
            storageObject.positionConstraint.constraintActive = true;
            storageObject.gameObject.transform.localPosition = storageObject.storageLocalPosition;
            storageObject.gameObject.transform.SetLossyScale(storageObject.lossyScale);
            transitioningCoroutines.Remove(storageObject.guid);
        }

        private IEnumerator AnimatedHideStorageObject(StorageObject storageObject)
        {
            yield return new WaitForSeconds(_tendrilToStraightTime);
            List<InteractionController> graspingControllers = new List<InteractionController>();
            foreach (InteractionController interactionController in storageObject.interactionBehaviour.graspingControllers)
            {
                graspingControllers.Add(interactionController);
            }
            storageObject.interactionBehaviour.EndGrasp(graspingControllers);

            yield return null;

            float startTime = Time.time;
            storageObject.positionConstraint.constraintActive = false;
            storageObject.gameObject.SetActive(true);
            storageObject.EnablePhysics(false);

            while (Time.time - startTime < _showHideStorageAnimationDuration)
            {
                float t = (Time.time - startTime) / _showHideStorageAnimationDuration;

                storageObject.gameObject.transform.localPosition = Vector3.Lerp(storageObject.storageLocalPosition, Vector3.zero, _showHideStorageAnimationCurve.Evaluate(t));
                storageObject.gameObject.transform.SetLossyScale(Vector3.Lerp(storageObject.lossyScale, Vector3.zero, _showHideStorageAnimationCurve.Evaluate(t)));
                yield return null;
            }

            storageObject.EnablePhysics(true);
            storageObject.gameObject.SetActive(false);
            transitioningCoroutines.Remove(storageObject.guid);
        }
        #endregion
    }
}