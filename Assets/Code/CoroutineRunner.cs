using UnityEngine;
using System.Collections;
using System;

public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Inst;

    void Awake()
    {
        if (!Inst)
        {
            Inst = this;
        }
    }

    public static void WaitForEndOfFrame(Action action)
    {
        Inst.StartCoroutine(WaitForEndOfFrameRoutine(action));
    }

    static IEnumerator WaitForEndOfFrameRoutine(Action action)
    {
        if (action != null)
        {
            yield return new WaitForEndOfFrame();
            action();
        }
    }
}
