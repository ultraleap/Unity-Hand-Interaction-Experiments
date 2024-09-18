using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompletionIndicator : MonoBehaviour
{
    public float Progress => _progress;
    private float _progress = 0.0f;

    public bool Completed => _completed;
    private bool _completed = false;

    public TaskManager.Task Task => _task;

    public Action OnCompleted;

    [Tooltip("This is the task which the indicator will show progression info for")]
    [SerializeField] private TaskManager.Task _task;

    [Space]

    [SerializeField] private Image _progression;
    [SerializeField] private Animator _animator;

    [Space]

    [SerializeField] private TextMeshProUGUI _indexText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    //Set the associated text elements, if they exist
    public void SetText(int index, string description)
    {
        if (_indexText != null)
            _indexText.text = index.ToString();
        if (_descriptionText != null)
            _descriptionText.text = description;
    }

    //Set the current progress towards task completion: valid range 0-1
    public void SetProgress(float progress)
    {
        if (_completed)
            return;

        _progress = Mathf.Clamp(progress, 0.0f, 1.0f);

        if (_progress == 1.0f)
        {
            _completed = true;
            _animator.SetBool("completed", true);
            OnCompleted?.Invoke();
        }
    }

    //Add progress towards the current task completion: valid range 0-1
    public void AddProgress(float progress)
    {
        SetProgress(_progress + progress);
    }

    //Remove progress from the current task completion: valid range 0-1
    public void RemoveProgress(float progress)
    {
        if (progress < 0)
            progress *= -1;

        SetProgress(_progress - progress);
    }

    //Insta-complete the progress
    public void SetAsComplete()
    {
        SetProgress(1.0f);
    }

    private void Update()
    {
        _progression.fillAmount = Mathf.Lerp(_progression.fillAmount, _progress, Time.deltaTime * 10.0f);
    }
}
