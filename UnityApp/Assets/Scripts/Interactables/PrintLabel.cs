using Leap.Unity.PhysicalHands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PrintLabel : MonoBehaviour
{
    [SerializeField] private GameObject _label;

    public Action OnPrinted;

    private List<Label> _labels = new List<Label>();
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _label.SetActive(false);
    }

    public void Print()
    {
        for (int i = 0; i < _labels.Count; i++)
        {
            if (_labels[i] == null)
                continue;

            if (!_labels[i].HasBeenGrabbed)
                return;
        }

        GameObject newLabel = Instantiate(_label, this.transform);
        newLabel.SetActive(true);
        newLabel.transform.localScale = _label.transform.localScale;
        newLabel.transform.localPosition = _label.transform.localPosition;
        newLabel.transform.localRotation = _label.transform.localRotation;
        newLabel.GetComponent<Animation>().Play();
        _audioSource.Play();
        _labels.Add(newLabel.GetComponent<Label>());

        OnPrinted?.Invoke();
    }
}
