using System;
using UnityEngine;

public class ResendAnimEventBehaviour : MonoBehaviour
{
    public Action<Animator, string> EventDelegate;

    public void OnEvent(string param)
    {
        //Debug.Log(string.Format("{0} OnEvent:param={1}", gameObject.name, param));
        if (EventDelegate != null)
            EventDelegate(GetComponent<Animator>(), param);
    }

    void OnDestroy()
    {
        EventDelegate = null;
    }

    void OnDisable()
    {
        EventDelegate = null;
    }
}
