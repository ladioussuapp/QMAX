using Com4Love.Qmax;
using System;
using System.Collections;
using UnityEngine;

public class EventDispatcher : MonoBehaviour
{
    public event Action StartEvent;
    public event Action UpdateEvent;
    //public event Action FixedUpdateEvent;
    public event Action ApplicationQuitEvent;
    public event Action DestroyEvent;
    public event Action<bool> AppLicationPauseEvent;

    void Start()
    {
        //isPause = false;
        DontDestroyOnLoad(gameObject);
        Q.Log("GameController Start");
        if (StartEvent != null)
            StartEvent();

        //StartCoroutine(PrintAppStatus());
    }


    private IEnumerator PrintAppStatus()
    {
        while(true)
        {
            Utils.LogStatus();
            yield return new WaitForSeconds(3);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (UpdateEvent != null)
            UpdateEvent();
    }

    //public void FixedUpdate()
    //{
    //    if (FixedUpdateEvent != null)
    //    {
    //        FixedUpdateEvent();
    //    }
    //}

    void OnApplicationQuit()
    {
        if (ApplicationQuitEvent != null)
            ApplicationQuitEvent();
    }

    void OnApplicationPause(bool isPause)
    {
        //this.isPause = isPause;
        if (AppLicationPauseEvent != null)
        {
            AppLicationPauseEvent(isPause);
        }
    }

    void OnDestroy()
    {
        if (DestroyEvent != null)
            DestroyEvent();
    }
}
