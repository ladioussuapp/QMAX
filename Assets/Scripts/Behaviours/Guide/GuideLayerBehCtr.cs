/********************************************************************************
** auth： johnsonybq
** date： 2015/9/2 星期三 14:19:31
** FileName：GuideLayerBehCtr
** desc： 尚未编写描述
** Ver.:  V1.0.0
*********************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
public class GuideLayerBehCtr : GuideBaseCtr
{
    private GuideLayerBehaviour m_guideLayerBeh;
    private GameObject prefab;
    private int stageId;
    private int step;
    private Action<int, int> m_callBack;
    public GuideLayerBehCtr(int stageId, int step)
    {
        this.stageId = stageId;
        this.step = step;
    }

    public static GuideLayerBehCtr create(int stageId, int step, Action<int, int> callBack)
    {
        GuideLayerBehCtr ctr = new GuideLayerBehCtr(stageId, step);
        ctr.m_callBack = callBack;
        return ctr;
    }

    override public void execute()
    {
        //if (GuideUtility.contentName.ContainsKey(stageId) && GuideUtility.contentName[stageId].ContainsKey(step))
        //{
        //    prefab = Resources.Load<GameObject>("Prefabs/Guide/GuideLayerCanvas");
        //    Transform target = GameObject.Instantiate(prefab).transform;
        //    GuideLayerBehaviour GuideBeh = target.GetComponent<GuideLayerBehaviour>();
        //    string contentUrl = GuideUtility.contentName[stageId][step];
        //    string audioUrl = "";
        //    if (GuideUtility.audioName.ContainsKey(stageId) && GuideUtility.audioName[stageId].ContainsKey(step))
        //    {
        //        audioUrl = GuideUtility.audioName[stageId][step];
        //    }
        //    GuideBeh.setData(contentUrl, audioUrl, EndGuide);
        //}
    }

    override public void EndGuide()
    {
        if (m_callBack != null)
        {
            m_callBack(this.stageId, this.step);
        }
    }
}

    
