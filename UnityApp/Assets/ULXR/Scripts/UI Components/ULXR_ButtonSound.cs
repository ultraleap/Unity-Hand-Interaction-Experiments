using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* Button sound player */
public class ULXR_ButtonSound : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private AudioClip buttonSFX;
    private AudioSource audioPlayer = null;

    private Button button;

    private Image img;

    /* Setup */
    private void Start()
    {
        audioPlayer = GetComponent<AudioSource>();
        if (audioPlayer == null) 
            audioPlayer = gameObject.AddComponent<AudioSource>();

        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("ButtonSound applied to object with no Button");
            return;
        }

        Transform childContainer = transform.GetChild(0);
        if (childContainer != null)
        {
            for (int i = 0; i < childContainer.childCount; i++)
            {
                img = childContainer.GetChild(i).GetComponent<Image>();
                if (img != null)
                    break;
            }
        }

        ULXR_ValueTracker.Instance.volume_ui.ValueChanged += OnValueChanged;
        OnValueChanged(ULXR_ValueTracker.Instance.volume_ui.Get());
    }

    /* Update SFX volume */
    private void OnValueChanged(int val)
    {
        audioPlayer.volume = (float)val / 100.0f;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        audioPlayer.PlayOneShot(buttonSFX);
    }
}
