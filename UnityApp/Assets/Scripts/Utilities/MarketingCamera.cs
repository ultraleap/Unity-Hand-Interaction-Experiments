using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;

namespace Ultraleap.Aurora.Utils
{
    public class MarketingCamera : MonoBehaviour
    {
        [SerializeField] float maxPositionDelta = 1;
        [SerializeField] float damping = 15f;
        [SerializeField] private Camera _mainCam;
        [SerializeField] private Camera _marketingCamera;
        private bool _enableMarketingCam = false;
        public float delayedScreenshotTime = 2f;
        public bool linearColourSpace = true;

        void Awake()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            _enableMarketingCam = true;
#else
            _enableMarketingCam = false;
#endif
            _marketingCamera.gameObject.SetActive(_enableMarketingCam);
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            if (_enableMarketingCam)
            {
                if (_mainCam == null)
                {
                    _mainCam = Camera.main;
                }

                _marketingCamera.transform.position = _mainCam.transform.position;
                _marketingCamera.transform.rotation = _mainCam.transform.rotation;
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
           linearColourSpace = PlayerSettings.colorSpace == ColorSpace.Linear;
#endif
        }

        private void Update()
        {
            if (!_enableMarketingCam)
            {
                return;
            }
            FollowMainCam();
            CheckForScreenshot();
        }

        private void FollowMainCam()
        {
            float distance = Vector3.Distance(_marketingCamera.transform.position, _mainCam.transform.position);

            if (distance > maxPositionDelta)
            {
                _marketingCamera.transform.position = _mainCam.transform.position;
                _marketingCamera.transform.rotation = _mainCam.transform.rotation;
            }
            else
            {
                _marketingCamera.transform.position = Vector3.Lerp(_marketingCamera.transform.position, _mainCam.transform.position, Time.deltaTime * damping);
                _marketingCamera.transform.rotation = Quaternion.Slerp(_marketingCamera.transform.rotation, _mainCam.transform.rotation, Time.deltaTime * damping);
            }
        }

        private void CheckForScreenshot()
        {
            bool screenshotKeyPressed = false;

#if ENABLE_INPUT_SYSTEM
            screenshotKeyPressed = Keyboard.current.sKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            screenshotKeyPressed = Input.GetKeyDown(KeyCode.S);
#endif

            if (screenshotKeyPressed)
            {
                StartCoroutine(DelayedScreenshot());
            }
        }

        public IEnumerator DelayedScreenshot()
        {
            yield return new WaitForSeconds(delayedScreenshotTime);

            CaptureCamera(_marketingCamera, $"{Application.persistentDataPath}\\{DateTime.Now.ToString("yyyyMMMdd_HHmmss")}_screenshot.png", 1920, 1080);
        }

        void CaptureCamera(Camera camera, string path, int width, int height)
        {
            RenderTexture renderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
            renderTexture.antiAliasing = 8;
            Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, GraphicsFormat.R16G16B16A16_SFloat, TextureCreationFlags.None);


            RenderTexture target = camera.targetTexture;
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.targetTexture = target;

            RenderTexture active = RenderTexture.active;
            RenderTexture.active = renderTexture;

            texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            if (linearColourSpace)
            {
                var pixels = texture2D.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = pixels[i].gamma;
                }
                texture2D.SetPixels(pixels);
                texture2D.Apply();
            }


            RenderTexture.active = active;

            System.IO.File.WriteAllBytes(path, texture2D.EncodeToPNG());
        }
    }
}