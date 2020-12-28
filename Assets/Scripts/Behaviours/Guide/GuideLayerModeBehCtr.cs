/********************************************************************************
** auth： johnsonybq
** date： 2015/9/6 星期日 12:03:18
** FileName：GuideLayerModeBehCtr
** desc： 尚未编写描述
** Ver.:  V1.0.0
*********************************************************************************/


using Com4Love.Qmax;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GuideLayerModeBehCtr : GuideBaseCtr
{
    private GameObject prefab;
    private GuideLayerModeBehaviour GuideBeh;
    private Dictionary<int, GuideConfig> GuideData;
    private int stepIndex = 1;
    private Action m_callBack;
    private GuideGetObject getObject;
    private int guideIndex;
    private GuideDialogCtr dialogCtr;
    private bool dialogOver;
    private GuideModel model;
    public static GuideLayerModeBehCtr create(Dictionary<int, GuideConfig> data, Action callBack, int index, int guideIndex, GuideModel _mode = GuideModel.ModelStep)
    {
        GuideLayerModeBehCtr ctr = new GuideLayerModeBehCtr();
        ctr.guideIndex = guideIndex;
        ctr.GuideData = data;
        ctr.stepIndex = index;
        ctr.m_callBack = callBack;
        ctr.dialogOver = false;
        ctr.model = _mode;
        ctr.init();
        return ctr;
    }

    void init()
    {

    }

    override public void execute()
    {
        GuideConfig item = GuideData[stepIndex];
        Q.Log(LogTag.Test, "GuideLayerModeBehCtr exec 1 item={0}, dialogOver={1}",
            item.NodeName,
            dialogOver);
        float delay = 0;
        if (dialogOver)
        {
            delay = 0;
        }
        else if (GuideManager.getInstance().GuideNodeDict.ContainsKey(item.NodeName))
        {
            delay = GuideManager.getInstance().GuideNodeDict[item.NodeName].Delay;
        }

        Action delayCall = delegate()
        {
            if (GuideBeh == null)
            {
                prefab = Resources.Load<GameObject>("Prefabs/Guide/GuideLayerCanvasMode");
                Transform target = GameObject.Instantiate(prefab).transform;
                GuideBeh = target.GetComponent<GuideLayerModeBehaviour>();
            }
            else
            {
                if (!GuideBeh.gameObject.activeInHierarchy)
                {
                    GuideBeh.gameObject.SetActive(true);
                }
            }

            getObject = GuideBeh.gameObject.AddComponent<GuideGetObject>();
            Q.Log(LogTag.Test, "GuideLayerModeBehCtr exec 2");
            getObject.init(item.NodeName, findObjectOver);
        };
        if (delay > 0)
            LeanTween.delayedCall(delay, delayCall);
        else
            delayCall();

    }

    void overDialog()
    {
        dialogOver = true;
        dialogCtr = null;
        execute();
    }

    /// <summary>
    /// 找到節點回調函數
    /// </summary>
    private void findObjectOver()
    {
        Q.Log(LogTag.Test, "GuideLayerModeBehCtr:findObjectOver 0");
        GuideConfig item = GuideData[stepIndex];
        if (!dialogOver)
        {
            Q.Log(LogTag.Test, "GuideLayerModeBehCtr:findObjectOver 1");
            if (item.StepDialogConfigs.Count > item.IndexNum && item.StepDialogConfigs[item.IndexNum].Count !=0)
            {
                Q.Log(LogTag.Test, "GuideLayerModeBehCtr:findObjectOver 1.1");
                if (dialogCtr == null)
                {
                    bool showTips = GuideManager.getInstance().GuideNodeDict[item.NodeName].ShowTips;
                    dialogCtr = GuideDialogCtr.create(item.DialogID[item.IndexNum], overDialog, null, showTips);
                }

                Q.Log(LogTag.Test, "GuideLayerModeBehCtr:findObjectOver 1.11");
                dialogCtr.execute();
                if (GuideBeh != null && GuideBeh.gameObject.activeInHierarchy)
                {
                    Q.Log(LogTag.Test, "GuideLayerModeBehCtr:findObjectOver 1.12");
                    GameObject.Destroy(GuideBeh.gameObject);
                    GuideBeh = null;
                }
            }
            else
            {
                Q.Log(LogTag.Test, "GuideLayerModeBehCtr:findObjectOver 1.2");
                dialogOver = true;
                execute();
            }
        }
        else
        {
            Q.Log(LogTag.Test, "GuideLayerModeBehCtr:findObjectOver 2");
            GuideNode gNode = getObject.getObject();

            if (gNode.TargetNode != null)
            {
                Vector3 screenPoint = gNode.TarCamera.WorldToScreenPoint(gNode.TargetNode.transform.position);
                GuideBeh.SetData(screenPoint, StepNext, gNode.ShowMask);
            }
            else //當純對話外理
            {
                StepNext();
            }


            if (gNode.CallBackFirst != null)
            {
                gNode.CallBackFirst();
            }

        }

    }

    private void StepNext()
    {
        Q.Log(LogTag.Test, "GuideLayerModeBehCtr:StepNext 1");
        GuideConfig item = GuideData[stepIndex];
        if (item.IsSave)
        {
            if (model == GuideModel.ModelStep)
            {
                int guideid = GuideManager.getInstance().CurrentGuideID();
                GameController.Instance.Client.SaveGuideIndex(guideIndex, guideid);
                GuideManager.getInstance().GuideOverIDList.Add(guideid);
            }
            else if (model == GuideModel.ModelTrigger)
            {
                GuideManager.getInstance().GuideKeySave(item.SaveName);
            }

        }
        stepIndex++;
        dialogOver = false;
        GuideNode gNode = getObject.getObject();
        GameObject.Destroy(getObject);

        if (stepIndex > this.GuideData.Count)
        {
            Q.Log(LogTag.Test, "GuideLayerModeBehCtr:StepNext 2");
            GameObject.Destroy(GuideBeh.gameObject);

            if (model == GuideModel.ModelStep)
            {
                Q.Log(LogTag.Test, "GuideLayerModeBehCtr:StepNext 3");
                if (m_callBack != null)
                {
                    m_callBack();
                }
            }
            else if (model == GuideModel.ModelTrigger)
            {
                Q.Log(LogTag.Test, "GuideLayerModeBehCtr:StepNext 4");
                if (m_callBack != null)
                {
                    GuideManager.getInstance().IsGuideRunning = false;
                }
            }

            Q.Log(LogTag.Test, "GuideLayerModeBehCtr:StepNext 5 {0}, {1}", gNode != null, gNode.CallBack != null);
            if (gNode.CallBack != null)
            {
                gNode.CallBack();
            }
            Q.Log(LogTag.Test, "GuideLayerModeBehCtr:StepNext 6");
            return;
        }
        else
        {
            Q.Log(LogTag.Test, "GuideLayerModeBehCtr:StepNext 7");
            if (gNode.CallBack != null)
            {
                gNode.CallBack();
            }
            execute();
        }
    }
    override public void EndGuide()
    {

    }
}