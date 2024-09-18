using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TabletScreenController : MonoBehaviour
{
    [SerializeField] private FadeUI _tasksFader;
    [SerializeField] private FadeUI _finaleFader;
    [SerializeField] private AudioSource _finaleSFX;

    [Space]

    [SerializeField] private GameObject _restartButton;

    [Header("Finale Texts")]
    [SerializeField] private TextMeshProUGUI _coffeeType;
    [SerializeField] private TextMeshProUGUI _coffeePercent;
    [SerializeField] private TextMeshProUGUI _milkPercent;
    [SerializeField] private TextMeshProUGUI _creationTime;
    [SerializeField] private bool _newLineForCraftTime = false;


    private TaskManager _taskManager;
    private float _taskStartTime = 0.0f;

    private void Awake()
    {
        _taskManager = FindObjectOfType<TaskManager>(); 

        _taskManager.OnCompletedTask += OnCompletedTask;
        _taskManager.OnCompletedAll += OnCompletedAllTasks;
        _restartButton?.SetActive(false);
    }

    private void OnCompletedTask(TaskManager.Task task)
    {
        if (task != TaskManager.Task.PICK_UP_CUP)
            return;
        _taskStartTime = Time.time;
    }

    private void OnCompletedAllTasks()
    {
        string completionTime = "";
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time - _taskStartTime);
            if (timeSpan.TotalSeconds < 60)
                completionTime = $"{timeSpan.Seconds} second{(timeSpan.Seconds == 1 ? "" : "s")}";
            else if (timeSpan.TotalMinutes < 60)
                completionTime = $"{timeSpan.Minutes} minute{(timeSpan.Minutes == 1 ? "" : "s")} {timeSpan.Seconds} second{(timeSpan.Seconds == 1 ? "" : "s")}";
            else if (timeSpan.TotalHours < 24)
                completionTime = $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours == 1 ? "" : "s")} {timeSpan.Minutes} minute{(timeSpan.Minutes == 1 ? "" : "s")}";
            else
                completionTime = $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays == 1 ? "" : "s")} {(int)timeSpan.Hours} hour{(timeSpan.Hours == 1 ? "" : "s")}";
        }
        float milkPercent = _taskManager.Cup.Feedback.MilkPercent * 100.0f;
        float coffeePercent = _taskManager.Cup.Feedback.CoffeePercent * 100.0f;
        string[] compliments = new string[] { "Excellent", "Wonderful", "Outstanding", "Impressive", "Brilliant", "Superb", "Remarkable", "Fantastic", "Exceptional", "Amazing", "Terrific", "Marvelous", "Stunning", "Spectacular", "Awesome", "Phenomenal", "Magnificent" };
        string coffeeType = "";
        {
            if (coffeePercent >= 95.0f)
                coffeeType = "Espresso";
            else if (coffeePercent >= 75.0f)
                coffeeType = "Macchiato";
            else if (coffeePercent >= 45.0f)
                coffeeType = "Cappuccino";
            else if (coffeePercent >= 35.0f)
                coffeeType = "Flat White";
            else if (coffeePercent >= 15.0f)
                coffeeType = "Latte";
            else
                coffeeType = "Babyccino";
        }

        _coffeeType.text = compliments[UnityEngine.Random.Range(0, compliments.Length)] + "\n" + coffeeType + "!";
        _coffeePercent.text = Mathf.RoundToInt(coffeePercent) + "%";
        _milkPercent.text = Mathf.RoundToInt(milkPercent) + "%";
        _creationTime.text = "masterfully crafted in " + (_newLineForCraftTime ? "\n" : "") + completionTime;

        StartCoroutine(DelayedFinale());
    }
    private IEnumerator DelayedFinale()
    {
        yield return new WaitForSeconds(1.0f);
        _tasksFader.FadeOut();
        _finaleSFX?.Play();
        yield return new WaitForSeconds(0.5f);
        _finaleFader.FadeIn();
        _restartButton?.SetActive(true);
    }

    public void RestartButtonPress()
    {
        _taskManager.Restart();
    }
}
