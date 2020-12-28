/********************************************************************************
** auth： johnsonybq
** date： 2015/9/8 星期二 16:57:31
** FileName：GuideGetObject
** desc： 尚未编写描述
** Ver.:  V1.0.0
*********************************************************************************/



using Com4Love.Qmax.Tools;
using System;
using System.Timers;
using UnityEngine;
using Com4Love.Qmax;

public class GuideGetObject : MonoBehaviour
{

    private string objectName;

    private GuideNode _object;

    private Action m_callBack;

    private bool _isScheduled;
    public void init(string objectName, Action callBack)
    {
        this.objectName = objectName;
        m_callBack = callBack;
        _isScheduled = false;
    }

    public GuideNode getObject()
    {
        return this._object;
    }


    void Start()
    {
        if (!this._isScheduled)
        {
            this._isScheduled = true;
            InvokeRepeating("onUpdate", .01f, VectorManager.distanceCheckFrequency);
        }
    }

    void onUpdate()
    {

        if (GuideManager.getInstance().GuideNodeDict.ContainsKey(this.objectName))
        {
            _object = GuideManager.getInstance().GuideNodeDict[this.objectName];
            findOverCallBack();
        }
    }

    void findOverCallBack()
    {
        Q.Log(LogTag.Test, "GuideGetObject:findOverCallBack 1");
        Q.Assert(m_callBack != null, "GuideGetObject:findOverCallBack assert 1");
        CancelInvoke("onUpdate");
        if (this.m_callBack != null)
        {
            this.m_callBack();
        }
    }
}
