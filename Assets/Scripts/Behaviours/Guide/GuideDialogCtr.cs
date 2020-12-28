/********************************************************************************
** auth： johnsonybq
** date： 2015/9/14 星期一 12:15:07
** FileName：GuideDialogCtr
** desc： 尚未编写描述
** Ver.:  V1.0.0
*********************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
public class GuideDialogCtr : GuideBaseCtr
{
    private int dialogID;
    private GameObject prefab;
    private Dictionary<int, DialogConfig> dialogData;
    private GameController gameCtr;
    private int stepIndex;
    private GuideDialogBehaviour GuideBeh;
    private Action m_callBack;
    private Action m_overCallBack;
    public GuideDialogCtr()
    {

    }

    public static GuideDialogCtr create(int dialogId,Action m_callBack, FingerPoint finger = null,bool showTips = false,Action callBack = null)
    {
        GuideDialogCtr ctr = new GuideDialogCtr();
        if (ctr.init(dialogId, m_callBack,finger,showTips, callBack))
        {
            return ctr;
        }
        else
        {
            ctr = null;
            return null;
        }
    }

    private bool init(int dialogId, Action callBack, FingerPoint finger = null ,bool showTips = false, Action overCallBack = null)
    {
        m_callBack = callBack;
        m_overCallBack = overCallBack;
        dialogID = dialogId;
        gameCtr = GameController.Instance;
        stepIndex = 1;
        dialogData = new Dictionary<int, DialogConfig>();
        foreach (DialogConfig item in gameCtr.Model.DialogConfigs.Values)
        {
            if (item.UID == dialogID)
            {
                item.finger = finger;
                item.ShowTips = showTips;
                dialogData.Add(item.ID,item);
            }
        }

        prefab = Resources.Load<GameObject>("Prefabs/Guide/GuideDialogPrefab");
        Transform target = GameObject.Instantiate(prefab).transform;
        GuideBeh = target.GetComponent<GuideDialogBehaviour>();

        return dialogData.Count != 0;
    }

    private bool showFinger = true;
    public bool ShowFinger
    {
        get { return showFinger; }
        set
        {
            showFinger = value;
        }
    }

    override public void execute()
    {
        DialogConfig item = dialogData[stepIndex];
        item.ShowFinger = showFinger;
        GuideBeh.setData(item, NextDialog);
    }

    private void NextDialog(bool skip)
    {
        if (skip)
        {
            EndGuide();
        }
        else
        {
            stepIndex++;
            if (stepIndex > dialogData.Count)
            {
                EndGuide();
            }
            else
            {
                execute();
            }
        }
    }

    override public void EndGuide()
    {
        if (m_callBack != null)
        {
            stepIndex = 1;
            m_callBack();
            GameObject.Destroy(GuideBeh.gameObject);
        }
        if(m_overCallBack !=null)
        {
            m_overCallBack();
        }
    }
}
