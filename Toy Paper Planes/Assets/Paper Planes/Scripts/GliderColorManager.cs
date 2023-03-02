using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GliderColorManager : MonoBehaviour
{
    void Start()
    {
        Color randomColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.4f, 0.8f);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach(var rend in renderers)
        {
            rend.material.color = randomColor;
        }
    }
}
