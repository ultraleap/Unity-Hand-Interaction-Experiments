using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class TaskManager : MonoBehaviour
{
    [Header("Interactable Objects")]
    [SerializeField] private Cup _cup;
    [SerializeField] private PrintLabel _labelPrinter;
    [SerializeField] private PourCoffee _coffeeMachine;

    [Header("Prompts and Zones")]
    [SerializeField] private CupTrigger _underMachineZone;
    [SerializeField] private CupTrigger _onCounterZone;
    [SerializeField] private FadeUI _pressCoffeePrompt;
    [SerializeField] private FadeUI _pressMilkPrompt;
    [SerializeField] private FadeUI _printLabelPrompt;
    [SerializeField] private FadeUI _bringUnderMachinePrompt;

    [Header("UI")]
    [SerializeField] private List<TaskUI> _taskUIs;
    [SerializeField] private FadeUI _viewBlocker;

    [Header("SFX")]
    [SerializeField] private AudioClip _completeSFX;

    [Space]

    [SerializeField] private KeyCode _restartShortcutKey = KeyCode.Space;

    public Cup Cup => _cup;

    public Action<Task> OnCompletedTask;
    public Action OnCompletedAll;

    private bool _restarting = false;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _taskUIs.ForEach(o => o.Validate());

        _coffeeMachine.OnPourStart += OnCoffeePour;
        _labelPrinter.OnPrinted += OnLabelPrinted;

        _cup.OnFirstGrab += OnCupGrab;
        _cup.OnFirstLabelApplied += OnCupLabelApplied;

        _onCounterZone.OnEntered += OnFinalCounterZoneTriggered;

        _cup.Highlight(true);
        _coffeeMachine.SetCupFull(false);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        _viewBlocker.FadeOut();
        ShowNextObjectiveMarker();
    }

    //When cup is grabbed/labelled check other states
    private void OnCupGrab()
    {
        SetTaskComplete(Task.PICK_UP_CUP);
        _cup.Highlight(false);

        _pressCoffeePrompt.FadeIn();
        _bringUnderMachinePrompt.FadeIn();

        _underMachineZone.Activate();
    }
    private void OnCupLabelApplied()
    {
        _printLabelPrompt.FadeOut();
        SetTaskComplete(Task.PLACE_LABEL);
        CheckCoffeeProgression();
    }

    //When label is printed update the task checklist
    private void OnLabelPrinted()
    {
        SetTaskComplete(Task.PRINT_LABEL);
    }

    //When coffee pour happens, track if our cup is underneath the nozzle, then check other states
    private void OnCoffeePour(PourCoffee.OutputSize size, PourCoffee.OutputType type, float duration)
    {
        if (GetTaskComplete(Task.POUR_COFFEE))
            return;

        StartCoroutine(CoffeePourMonitor(size, type, duration));
    }
    private IEnumerator CoffeePourMonitor(PourCoffee.OutputSize size, PourCoffee.OutputType type, float duration)
    {
        float pourTime = 0.0f;
        while (true)
        {
            if (_coffeeMachine.CupIsUnder)
            {
                float val = Time.deltaTime * ((float)((int)size + 1) / (type == PourCoffee.OutputType.MILK ? 20.0f : 35.0f));
                AddTaskProgress(Task.POUR_COFFEE, val);
                switch (type)
                {
                    case PourCoffee.OutputType.MILK:
                        _cup.Feedback.AddMilk(val);
                        break;
                    case PourCoffee.OutputType.COFFEE:
                        _cup.Feedback.AddCoffee(val);
                        break;
                }

                if (_pressCoffeePrompt.IsVisible && _cup.Feedback.FillPercent > 0.5f && size == PourCoffee.OutputSize.LARGE && type == PourCoffee.OutputType.COFFEE)
                {
                    _pressCoffeePrompt.FadeOut();
                    _pressMilkPrompt.FadeIn();
                }

                if (GetTaskComplete(Task.POUR_COFFEE))
                {
                    CheckCoffeeProgression();
                    _cup.Feedback.DoSFX(false);
                    break;
                }
            }
            _cup.Feedback.DoSFX(_coffeeMachine.CupIsUnder);

            pourTime += Time.deltaTime;
            if (pourTime >= duration)
            {
                _cup.Feedback.DoSFX(false);
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    //Sanity checks task completion statuses and updates hinting/zones accordingly
    private void CheckCoffeeProgression()
    {
        if (!GetTaskComplete(Task.POUR_COFFEE))
            return;

        _coffeeMachine.SetCupFull(true);
        _pressCoffeePrompt.FadeOut();
        _pressMilkPrompt.FadeOut();

        _bringUnderMachinePrompt.FadeOut();

        _underMachineZone.Deactivate();

        if (!GetTaskComplete(Task.PLACE_LABEL))
        {
            _printLabelPrompt.FadeIn();
            return;
        }

        _onCounterZone.Activate();
    }

    //When cup is completed and placed into final zone, we're done!
    private void OnFinalCounterZoneTriggered()
    {
        SetTaskComplete(Task.SERVE_CUP);
        _onCounterZone.Deactivate();
    }

    private void FixedUpdate()
    {
        //Show the warning to bring cup under nozzle if that task is active and it isn't
        if (_underMachineZone.Activated && !_underMachineZone.IsInside && !GetTaskComplete(Task.POUR_COFFEE))
        {
            _bringUnderMachinePrompt.FadeIn();
        }
        else
        {
            _bringUnderMachinePrompt.FadeOut();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(_restartShortcutKey))
            Restart();
    }

    //Restart from the beginning
    public void Restart()
    {
        if (_restarting)
            return;

        StartCoroutine(RestartCoroutine());
    }
    private IEnumerator RestartCoroutine()
    {
        _restarting = true;

        _viewBlocker.FadeIn();
        yield return new WaitForSeconds(_viewBlocker.TransitionTime + 0.1f);
        SceneManager.LoadScene(0);
    }

    //Task completion management wrapper
    private void SetTaskComplete(Task task)
    {
        TaskUI taskUI = _taskUIs.FirstOrDefault(o => o.Type == task);
        if (!taskUI.Completed)
            _audioSource.PlayOneShot(_completeSFX);
        taskUI.Complete();

        ShowNextObjectiveMarker();
        OnCompletedTask?.Invoke(task);
    }
    private bool GetTaskComplete(Task task)
    {
        return _taskUIs.FirstOrDefault(o => o.Type == task).Completed;
    }
    private void AddTaskProgress(Task task, float progress)
    {
        TaskUI taskUI = _taskUIs.FirstOrDefault(o => o.Type == task);
        taskUI.AddProgress(progress);
        if (taskUI.Completed)
            _audioSource.PlayOneShot(_completeSFX);

        ShowNextObjectiveMarker();
    }
    private void ShowNextObjectiveMarker()
    {
        List<TaskUI> orderedTasks = _taskUIs.OrderBy(o => (int)o.Type).ToList();
        for (int i = 0; i < orderedTasks.Count; i++)
        {
            if (!orderedTasks[i].Completed)
            {
                orderedTasks[i].ShowObjectiveMarker();
                return;
            }
        }
        OnCompletedAll?.Invoke();
    }

    private void OnValidate()
    {
        _taskUIs.ForEach(o => o.Validate());
    }

    [System.Serializable]
    public class TaskUI
    {
        [SerializeField] private Task _type;
        [SerializeField] private string _description;

        [Space]

        [SerializeField] private FadeUI _objectiveFader;

        public Task Type => _type;

        private List<CompletionIndicator> _completionIndicators;

        public void Validate()
        {
            _completionIndicators = FindObjectsOfType<CompletionIndicator>().Where(o => o.Task == _type).ToList();
            _completionIndicators.ForEach(o => o.SetText((int)_type + 1, _description));
        }

        public void ShowObjectiveMarker()
        {
            _objectiveFader.FadeIn();
        }

        public void Complete()
        {
            AddProgress(999);
        }

        public void AddProgress(float progress)
        {
            _completionIndicators.ForEach(o => o.AddProgress(progress));

            if (Completed)
                _objectiveFader.FadeOut(false, 0, false, 1.5f);
        }

        public bool Completed => _completionIndicators[0].Completed;
    }

    public enum Task
    {
        PICK_UP_CUP,
        POUR_COFFEE,
        PRINT_LABEL,
        PLACE_LABEL,
        SERVE_CUP,
    }
}
