using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Fade controller for UI CanvasGroup */
public class ULXR_FadeUI : MonoBehaviour
{
    public delegate void StateChange(bool isVisible);
    public StateChange FadeTriggered;
    public StateChange FadeComplete;

    [SerializeField] private bool startsInvisible = false;
    private CanvasGroup canvasGroup;

    private float lerpSpeed = 15;

    private float currentOpacity = 0.0f;
    private float targetOpacity = 0.0f;
    public float CurrentOpacity { get { return currentOpacity; } }

    private bool isVisible = false;
    public bool IsVisible { get { return isVisible; } }

    public float BlockTimer { get { return blockTimer; } set { blockTimer = value; } }
    private float blockTimer = 0.5f;
    private float blockTimerCurr = 1.0f;
    private bool triggeredCompletion = false;

    /* Fade all elements out */
    public void FadeOut(bool immediate = false, float opacity = 0.0f)
    {
        Setup();
        targetOpacity = opacity;
        isVisible = false;
        blockTimerCurr = 0.0f;

        triggeredCompletion = false;
        FadeTriggered?.Invoke(false);

        if (!immediate) return;
        canvasGroup.alpha = targetOpacity;
        currentOpacity = targetOpacity;
    }

    /* Fade all elements in */
    public void FadeIn()
    {
        Setup();
        targetOpacity = 1.0f;
        isVisible = true;
        blockTimerCurr = 0.0f;

        triggeredCompletion = false;
        FadeTriggered?.Invoke(true);
    }

    /* Toggle UI support */
    public void Toggle(Toggle toggle)
    {
        if (toggle.isOn) FadeIn();
        else FadeOut();
    }

    /* Grab references to all things that will be potentially modified by our fade */
    private void Start()
    {
        Setup();
        if (startsInvisible) FadeOut(true);
        else FadeIn();
    }
    private void Setup()
    {
        if (canvasGroup != null) return;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    /* Perform the fade if required */
    void Update()
    {
        if (blockTimerCurr >= blockTimer)
        {
            currentOpacity = targetOpacity;
            if (!triggeredCompletion)
            {
                triggeredCompletion = true;
                FadeComplete?.Invoke(targetOpacity == 1);
            }
        }
        else
        {
            blockTimerCurr += Time.deltaTime;
            currentOpacity = Mathf.Lerp(currentOpacity, targetOpacity, Time.deltaTime * lerpSpeed);
        }
        canvasGroup.alpha = currentOpacity;
        canvasGroup.blocksRaycasts = (canvasGroup.alpha >= 0.01f);
    }
}
