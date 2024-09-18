using Leap.Unity;
using Leap.Unity.Attachments;
using Leap.Unity.PhysicalHands;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlinthSyncth : MonoBehaviour
{

    [SerializeField] private Transform _plinth, _markerAnchor, _plinthAnchor;
    [SerializeField] private float _stillTime = 3;
    [SerializeField] private LeapProvider _leapProvider;

    [SerializeField] private Image _loadingBar;
    [SerializeField] private TrackingMarkerObject _markerObject;

    [SerializeField] private ULXR_SceneManager ulxrSceneManager;

    [SerializeField] private AttachmentHands ULXRAttachmentHands;
    private bool _synced = false;
    private Pose _playerPlinthPlose;
    private float _timeMarkerFound = float.MaxValue;
    private Pose _playerWorldPose;


    // Start is called before the first frame update
    void Start()
    {
        //Prevent this being called OnSceneLoad... just incase... this might not be a problem but im a wimp 
        if (_synced) return;

        DontDestroyOnLoad(gameObject);

        _loadingBar.fillAmount = 0;
        _markerObject.OnTrackingStart += OnMarkerTrackingStart;
        _markerObject.OnTrackingLost += OnMarkerTrackingLost;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    // Update is called once per frame
    void Update()
    {
        if (_synced) return;

        float newProgress = 0;

        if (_markerObject.Tracked)
        {
            newProgress = Mathf.Clamp01((Time.time - _timeMarkerFound) / _stillTime);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _markerAnchor.position = _leapProvider.GetHand(Chirality.Right).PalmPosition;
            newProgress = 1;
        }

        _loadingBar.fillAmount = Mathf.Lerp(_loadingBar.fillAmount, newProgress, Time.deltaTime * 15f);

        if (newProgress == 1)
        {
            //TODO calibration success screen + happy ui 
            SyncPlayer();
        }
    }

    private void SyncPlayer()
    {

        //we know the marker up is gonna be straight up, so 0 out the x rotation to reflect this
        Vector3 markerRot = _markerAnchor.rotation.eulerAngles;
        markerRot.x = 0;
        _markerAnchor.rotation = Quaternion.Euler(markerRot);
        _plinth.rotation = _markerAnchor.rotation;
        _plinth.position = _markerAnchor.position + (_plinth.position - _plinthAnchor.position);

        // Save the player pos and rot relative to the plinth
        _playerWorldPose = Camera.main.transform.GetPose();
        _playerPlinthPlose.position = _plinth.InverseTransformPoint(Camera.main.transform.position);
        _playerPlinthPlose.rotation = _plinth.InverseTransformRotation(Camera.main.transform.rotation);        

        _synced = true;

        if (ulxrSceneManager == null)
        {
            ulxrSceneManager = FindObjectOfType<ULXR_SceneManager>();
        }

        ulxrSceneManager?.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!_synced) return;


        if (ULXRAttachmentHands == null)
        {
            ULXRAttachmentHands = FindObjectOfType<AttachmentHands>();
        }

        // Quick hack to solve a bug - sorry this is gross & shouldn't belong in this script :( 
        ULXRAttachmentHands?.SetLeapProvider(FindObjectOfType<PhysicalHandsManager>());

        StartCoroutine(RecentrePlayer());
    }

    private IEnumerator RecentrePlayer()
    {
        yield return new WaitForSeconds(0.25f);
        // Find the plinth in the scene
        _plinth = GameObject.FindGameObjectWithTag("Plinth").transform;
        ULXR_RecenterPlayer recenterPlayer = FindObjectOfType<ULXR_RecenterPlayer>();


        Vector3 translationSinceCalibration =  Camera.main.transform.position - _playerWorldPose.position;
        Quaternion rotationSinceCalibration = Camera.main.transform.rotation * Quaternion.Inverse(_playerWorldPose.rotation);

        // Set the pos and rot relative to it
        recenterPlayer.transform.position = _plinth.TransformPoint(_playerPlinthPlose.position)+ translationSinceCalibration;
        recenterPlayer.transform.rotation = _plinth.TransformRotation(_playerPlinthPlose.rotation) * rotationSinceCalibration;

        recenterPlayer.RecentreWithFade(0.1f);

        DestroyImmediate(gameObject);
    }

    private void OnMarkerTrackingStart()
    {
        _timeMarkerFound = Time.time;
        // TODO - nicer ui reactions when tracked
    }

    private void OnMarkerTrackingLost()
    {
        // TODO - nicer ui reactions when untracked
        // prompt to user to move closer to the marker
    }
}
