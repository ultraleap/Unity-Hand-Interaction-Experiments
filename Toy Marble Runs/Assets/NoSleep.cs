using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoSleep
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
