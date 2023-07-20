using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleGameobjects : MonoBehaviour
{
    public List<GameObject> cyclableGameobjects;
    public KeyCode goBackKey = KeyCode.LeftArrow;
    public KeyCode goForwardKey = KeyCode.RightArrow;

    private int _activeGameobjectIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        SetActiveGameObject();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(goBackKey))
        {
            _activeGameobjectIndex--;
            if (_activeGameobjectIndex < 0)
            {
                _activeGameobjectIndex = cyclableGameobjects.Count - 1;
            }
            SetActiveGameObject();
        }

        else if (Input.GetKeyDown(goForwardKey))
        {
            _activeGameobjectIndex++;
            if (_activeGameobjectIndex > cyclableGameobjects.Count - 1)
            {
                _activeGameobjectIndex = 0;
            }
            SetActiveGameObject();
        }
    }

    private void SetActiveGameObject()
    {
        for (int i = 0; i < cyclableGameobjects.Count; i++)
        {
            cyclableGameobjects[i].SetActive(i == _activeGameobjectIndex);
        }
    }
}
