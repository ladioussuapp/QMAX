using UnityEngine;
using System.Collections;

public abstract class SimpleAnimationBase : MonoBehaviour
{

    /// <summary>
    /// 总时间  秒
    /// </summary>
    public float Time = 1f;
    public bool Repeat = true;
    public bool Yoyo = false;

    float addElapsedTime = 0;
    float elapsedRate = 0f;
    int yoyoFlag = 1;

    // Use this for initialization
    void Start()
    {
        Reset();
    }

    public void LateUpdate()
    {
        addElapsedTime += UnityEngine.Time.deltaTime * yoyoFlag;
        elapsedRate = addElapsedTime / Time;

        if (yoyoFlag == 1 && elapsedRate >= 1 || yoyoFlag == -1 && elapsedRate <= 0)
        {
            if (Repeat)
            {
                Reset();

                if (Yoyo)
                {
                    yoyoFlag = -yoyoFlag;
                }
            }
        }

        UpdateAnimation(elapsedRate);
    }

    public void Reset()
    {
        if (Repeat && !Yoyo)
        {
            elapsedRate = 0;
            addElapsedTime = 0;
        }
    }

    virtual protected void UpdateAnimation(float elapsedRate_)
    {
        
    }
}
