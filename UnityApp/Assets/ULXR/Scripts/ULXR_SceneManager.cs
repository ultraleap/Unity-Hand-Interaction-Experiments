using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ULXR_SceneManager : MonoBehaviour
{

    public void LoadScene (Scene scene)
    {
        LoadScene(scene.buildIndex);
    }
    
    public void LoadScene (int buildIndex)
    {
        StartCoroutine(LoadSceneWithFade(buildIndex));
    }

    private IEnumerator LoadSceneWithFade(int buildIndex)
    {
        ULXR_Client.Instance.ViewBlocker.gameObject.SetActive(true);
        ULXR_Client.Instance.ViewBlocker.FadeIn();
        yield return new WaitForSeconds(ULXR_Client.Instance.ViewBlocker.BlockTimer);
        ULXR_MainController.Instance.SetInterfaceState(ULXR_InterfaceState.HIDING_UI);
        SceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
    }
}
