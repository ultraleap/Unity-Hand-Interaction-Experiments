using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ULXR_Client : ULXR_Singleton<ULXR_Client>
{
    [SerializeField] ULXR_FadeUI launchFader;
    public ULXR_FadeUI ViewBlocker { get { return launchFader; } }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;   
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        launchFader.gameObject.SetActive(true);
        yield return new WaitForSeconds(launchFader.BlockTimer);
        launchFader.FadeOut();
    }

    private void Update()
    {
        launchFader.transform.position = Camera.main.transform.position;
    }

    public Action OnReturnToLauncher;
    public void ReturnToLauncher()
    {
        Application.Quit();
    }
}
