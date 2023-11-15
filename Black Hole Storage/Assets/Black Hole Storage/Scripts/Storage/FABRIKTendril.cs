using System.Collections;
using UnityEngine;

namespace Leap.Unity.Interaction.Storage
{
    [RequireComponent(typeof(LineRenderer))]
    public class FABRIKTendril : MonoBehaviour
    {
        [SerializeField] public GameObject _particlesPrefab;
        [SerializeField] private int _numberOfJoints = 10;
        [SerializeField] private float _showLineRendererTime = 0.4f;
        [SerializeField] private float tensionPopLineShrinkTime = 0.7f;

        private LineRenderer _lineRenderer;
        private Transform[] _childEffectors;
        private float _tensionPercentage;
        private float _maxWidth;
        public EasyIK easyIK;

        public AnimationCurve widthAnimation;
        public AnimationCurve shrinkAnimation;

        public bool transitioning { get { return transitionCoroutine != null; } }
        private Coroutine transitionCoroutine = null;
        private Coroutine fadeCoroutine = null;
        private float targetAlpha = 0;

        private bool isDestroying = false;

        private bool firstTimeGrowing = true;

        /// <summary>
        /// Sets up a fabrik tendril
        /// </summary>
        /// <param name="root">Where the tendril starts</param>
        /// <param name="maxDistance">The max distance the tendril needs to reach</param>
        /// <param name="tensionPercentage"> Changes the max distance to be maxDistance * (1-tensionPercentage).
        /// Any distance after this, the line will appear straight.
        /// This is partly used to create the illusion of tension in the tendril.</param>
        /// <param name="target">The target the tendril is trying to reach</param>
        public void SetupTendril(Vector3 root, Transform target, float maxDistance, float tensionPercentage)
        {
            _tensionPercentage = tensionPercentage;
            Vector3 endPoint = root - (target.position - root).normalized * (maxDistance - (maxDistance * tensionPercentage));

            Transform parent = transform;
            for (int i = 1; i <= _numberOfJoints; i++)
            {
                Transform child = new GameObject("Joint").transform;
                child.parent = parent;
                child.position = Vector3.Lerp(root, endPoint, (float)i / _numberOfJoints);
                child.localRotation = Quaternion.identity;
                parent = child;
            }

            easyIK = gameObject.AddComponent<EasyIK>();

            easyIK.tolerance = 0.0025f;
            easyIK.iterations = 5;
            easyIK.gizmoSize = 0.005f;
            easyIK.ikTarget = target;
            easyIK.numberOfJoints = _numberOfJoints;
            easyIK.Init();

            _lineRenderer = GetComponent<LineRenderer>();
            _maxWidth = _lineRenderer.widthMultiplier;
            FindLineRendererPoints();
            _lineRenderer.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (_lineRenderer == null || !_lineRenderer.enabled) { return; }
            if (!transitioning && !isDestroying)
            {
                UpdateLineRendererFABRIKPositions();
            }
        }

        void UpdateLineRendererFABRIKPositions()
        {
            Vector3[] lineRendererPositions = new Vector3[_childEffectors.Length];

            for (int i = 0; i < _childEffectors.Length - 1; i++)
            {
                lineRendererPositions[i] = _childEffectors[i].position;
            }

            _lineRenderer.positionCount = lineRendererPositions.Length;
            lineRendererPositions[_childEffectors.Length - 1] = easyIK.ikTarget.position;
            _lineRenderer.SetPositions(lineRendererPositions);
        }

        private void FindLineRendererPoints()
        {
            _childEffectors = GetComponentsInChildren<Transform>();
            _lineRenderer.positionCount = _childEffectors.Length;
        }

        public void Shrink(float tendrilStraightenTime, float shrinkStorageTime)
        {
            if (transitioning)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(ShrinkCoroutine(tendrilStraightenTime, shrinkStorageTime));
        }

        public void Grow(float tendrilStraightenTime, float growStorageTime)
        {
            if (transitioning)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(GrowCoroutine(tendrilStraightenTime, growStorageTime));
        }

        private IEnumerator ShrinkCoroutine(float tendrilStraightenTime, float shrinkStorageTime)
        {
            easyIK.enabled = false;
            Color colour = _lineRenderer.colorGradient.Evaluate(.2f);
            colour.a = 1;
            StartCoroutine(MorphToColor(colour, 0.2f));

            //TENDRIL TO STRAIGHT ANIMATION
            Vector3[] startPositions = new Vector3[_lineRenderer.positionCount];
            _lineRenderer.GetPositions(startPositions);

            Vector3[] endPositions = new Vector3[_lineRenderer.positionCount];
            int finalIndex = _lineRenderer.positionCount - 1;
            endPositions[0] = _lineRenderer.GetPosition(0);
            endPositions[finalIndex] = _lineRenderer.GetPosition(finalIndex);

            for (int i = 1; i < finalIndex; i++)
            {
                endPositions[i] = Vector3.Lerp(endPositions[0], endPositions[finalIndex], i / _lineRenderer.positionCount);
            }

            var t = 0f;

            while (t <= 1)
            {

                for (int i = 0; i < _lineRenderer.positionCount; i++)
                {
                    _lineRenderer.SetPosition(i, Vector3.Lerp(startPositions[i], endPositions[i], t));
                }

                yield return new WaitForEndOfFrame();
                t += Time.deltaTime / tendrilStraightenTime;
            }

            //SHRINK LINE RENDERER
            float startTime = Time.time;

            Vector3[] lineRendererPositions = new Vector3[2];
            _lineRenderer.positionCount = 2;
            while (Time.time - startTime < shrinkStorageTime)
            {
                lineRendererPositions[0] = transform.parent.position;
                lineRendererPositions[1] = easyIK.ikTarget.position;
                _lineRenderer.SetPositions(lineRendererPositions);
                yield return null;
            }

            transitionCoroutine = null;
            _lineRenderer.enabled = false;
        }

        private IEnumerator GrowCoroutine(float tendrilTransitionTime, float growStorageTime)
        {
            easyIK.enabled = firstTimeGrowing;
            Color colour = _lineRenderer.colorGradient.Evaluate(.2f);
            colour.a = 1;
            StartCoroutine(MorphToColor(colour, 0.2f));

            //GROW LINE RENDERER
            float startTime = Time.time;

            Vector3[] lineRendererPositions = new Vector3[2];
            _lineRenderer.positionCount = 2;
            while (Time.time - startTime < growStorageTime)
            {
                lineRendererPositions[0] = transform.parent.position;
                lineRendererPositions[1] = easyIK.ikTarget.position;
                _lineRenderer.SetPositions(lineRendererPositions);
                yield return null;
            }


            //STRAIGHT TO TENDRIL ANIMATION

            Vector3[] startPositions = new Vector3[_childEffectors.Length];
            int finalIndex = _childEffectors.Length - 1;
            startPositions[0] = _lineRenderer.GetPosition(0);
            startPositions[finalIndex] = _lineRenderer.GetPosition(1);

            for (int i = 1; i < finalIndex; i++)
            {
                startPositions[i] = Vector3.Lerp(startPositions[0], startPositions[finalIndex], i / _childEffectors.Length);
            }

            _lineRenderer.positionCount = startPositions.Length;
            _lineRenderer.SetPositions(startPositions);

            float t = 0;
            while (t <= 1)
            {

                for (int i = 0; i < _lineRenderer.positionCount; i++)
                {
                    _lineRenderer.SetPosition(i, Vector3.Lerp(startPositions[i], _childEffectors[i].position, t));
                }

                yield return new WaitForEndOfFrame();
                t += Time.deltaTime / tendrilTransitionTime;
            }
            transitionCoroutine = null;

            colour = _lineRenderer.colorGradient.Evaluate(.2f);
            colour.a = targetAlpha;

            easyIK.enabled = true;
            yield return new WaitForEndOfFrame();
            easyIK.enabled = false;

            if (firstTimeGrowing)
            {
                firstTimeGrowing = false;
            }

            StartCoroutine(MorphToColor(colour, 0.2f));
        }

        public void FadeAlpha(float alpha)
        {
            targetAlpha = alpha;

            if (transitioning)
            {
                return;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            Color colour = _lineRenderer.colorGradient.Evaluate(.2f);
            colour.a = alpha;
            fadeCoroutine = StartCoroutine(MorphToColor(colour, _showLineRendererTime));
        }

        internal void UpdateTendrilTension(float tension)
        {
            float minTensionRange = tension - _tensionPercentage;
            if (tension < tension - _tensionPercentage) { return; }

            float t = Mathf.InverseLerp(minTensionRange, 1, tension);

            _lineRenderer.widthMultiplier = Mathf.Lerp(_maxWidth, 0, widthAnimation.Evaluate(t));
        }

        public IEnumerator MorphToColor(Color color, float morphTime)
        {
            _lineRenderer.enabled = true;

            var c1 = _lineRenderer.colorGradient.Evaluate(.2f);
            var c2 = _lineRenderer.colorGradient.Evaluate(.8f);

            var fade = 0f;

            while (fade <= 1)
            {
                var gradient = new Gradient();
                gradient.mode = GradientMode.Blend;
                var gradientColorKeys = new GradientColorKey[2]
                {
                new GradientColorKey(Color.Lerp(c1, color, fade), .2f),
                new GradientColorKey(Color.Lerp(c2, color, fade), .8f)
                };

                var alphaKeys = new GradientAlphaKey[2]
                {
                new GradientAlphaKey(Mathf.Lerp(c1.a, color.a, fade), .2f),
                new GradientAlphaKey(Mathf.Lerp(c2.a, color.a, fade), .8f)
                };

                gradient.SetKeys(gradientColorKeys, alphaKeys);


                _lineRenderer.colorGradient = gradient;

                yield return new WaitForEndOfFrame();

                fade += Time.deltaTime / morphTime;
            }

            _lineRenderer.enabled = color.a != 0;
            fadeCoroutine = null;

        }

        public void SpawnParticlesAtIKTarget()
        {
            Instantiate(_particlesPrefab, easyIK.ikTarget.position, easyIK.ikTarget.rotation);
        }

        public void AnimatedDestroy()
        {
            SpawnParticlesAtIKTarget();

            if (transitioning)
            {
                StopCoroutine(transitionCoroutine);
            }

            isDestroying = true;
            StartCoroutine(DestroyTendril());
        }


        private IEnumerator DestroyTendril()
        {
            isDestroying = true;
            float startTime = Time.time;
            Vector3 lineRendererEndAtStartOfShrink = _lineRenderer.GetPosition(_lineRenderer.positionCount - 1);
            Vector3[] lineRendererPositions = new Vector3[2];

            lineRendererPositions[0] = _lineRenderer.GetPosition(0);
            lineRendererPositions[1] = lineRendererEndAtStartOfShrink;

            _lineRenderer.positionCount = 2;
            while (Time.time - startTime < tensionPopLineShrinkTime)
            {
                float t = (Time.time - startTime) / tensionPopLineShrinkTime;

                _lineRenderer.widthMultiplier = Mathf.Lerp(_maxWidth, 0, widthAnimation.Evaluate(1 - t));

                lineRendererPositions[1] = Vector3.Lerp(lineRendererEndAtStartOfShrink, _lineRenderer.GetPosition(0), shrinkAnimation.Evaluate(t));

                _lineRenderer.SetPositions(lineRendererPositions);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
