using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 时间通知  当存货时间超过指定值时  调用 OnTimeOut 方法通知外部做处理
/// </summary>
public class DurationTimeDispatcher : MonoBehaviour
{
    [Tooltip("持续时间")]
    public float TimeDuration = 1f;
    float timeDuration;

    public event Action OnTimeOut;

    void AutoDes()
    {
        GameObject.Destroy(this.gameObject);
    }

    public void Start()
    {
        if (OnTimeOut == null)
        {
            OnTimeOut = AutoDes;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timeDuration > 0)
        {
            timeDuration -= Time.deltaTime;

            if (timeDuration <= 0)
            {
                if (OnTimeOut != null)
                {
                    OnTimeOut();
                }
            }
        }
    }

    public void Reset()
    {
        timeDuration = TimeDuration;
    }

    public void OnEnable()
    {
        Reset();
    }
}
