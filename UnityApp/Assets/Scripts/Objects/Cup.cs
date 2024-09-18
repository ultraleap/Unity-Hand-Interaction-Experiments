using Leap.Unity.PhysicalHands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FlashItem))]
[RequireComponent(typeof(AudioSource))]
public class Cup : MonoBehaviour
{
    [SerializeField] private GameObject _label;
    [SerializeField] private FlashItem _flash;
    [SerializeField] private PhysicalHandEvents _events;
    [SerializeField] private CupFeedback _feedback;

    [Space]

    [SerializeField] private List<AudioClip> _stickTicketSFX;

    public CupFeedback Feedback => _feedback;

    public bool IsGrabbed => _isGrabbed;
    private bool _isGrabbed = false;

    public bool HasLabel => _hasLabel;
    private bool _hasLabel = false;

    public Action OnFirstLabelApplied;
    private bool _hasDoneFirstLabelAppliedEvent;
    public Action OnFirstGrab;
    private bool _hasDoneFirstGrabEvent;

    private bool _highlighted = false;
    private int _stickSFX = 0;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _label.SetActive(false);
        _flash.DoFlash(_highlighted);

        if (_events != null)
        {
            _events.onGrabEnter.AddListener(OnGrabEnter);
            _events.onGrabExit.AddListener(OnGrabExit);
        }
    }

    //Flash the cup
    public void Highlight(bool highlight)
    {
        _highlighted = highlight;
        _flash.DoFlash(_highlighted);
    }

    //Grab state tracking
    private void OnGrabEnter(ContactHand hand)
    {
        _isGrabbed = true;

        if (!_hasDoneFirstGrabEvent)
        {
            OnFirstGrab?.Invoke();
            _hasDoneFirstGrabEvent = true;
        }
    }
    private void OnGrabExit(ContactHand hand)
    {
        _isGrabbed = false;
    }

    //Label placement
    void OnTriggerEnter(Collider col)
    {
        if (col.tag != "Label") return;

        GameObject newLabel = Instantiate(_label, this.transform);
        newLabel.SetActive(true);
        newLabel.transform.localScale = _label.transform.localScale;
        newLabel.transform.localPosition = _label.transform.localPosition;
        newLabel.transform.LookAt(col.transform.position);
        newLabel.transform.localRotation = Quaternion.Euler(new Vector3(0, newLabel.transform.localRotation.eulerAngles.y, 0));
        Destroy(col.gameObject);

        _hasLabel = true;

        _audioSource.PlayOneShot(_stickTicketSFX[_stickSFX]);

        _stickSFX++;
        if (_stickSFX >= _stickTicketSFX.Count)
            _stickSFX = 0;

        if (!_hasDoneFirstLabelAppliedEvent)
        {
            OnFirstLabelApplied?.Invoke();
            _hasDoneFirstLabelAppliedEvent = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }
}
