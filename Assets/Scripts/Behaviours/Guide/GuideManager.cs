/********************************************************************************
** auth： johnsonybq
** date： 2015/7/1 星期三 17:18:58
** FileName：GuideManager
** desc： 尚未编写描述
** Ver.:  V1.0.0
*********************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;

enum GuideType
{
    GUIDE_MOVIE = 1,
    GUIDE_DIALOG,
    GUIDE_CLICK,
    GUIDE_LINE,
    GUIDE_SHOWPIC
};

public class GuideNode
{
    public Component TargetNode;
    public Camera TarCamera;
    public Action CallBackFirst;
    public Action CallBack;
    public float Delay = 0;
    public int index;
    public bool ShowMask = true;
    public bool ShowTips = false;
    public bool CheckAnim = false;
    public bool ExecuteCallBack = true;
}

public class FingerPoint
{
    public bool IsShow;
    public Vector3 Pos;
}

public enum GuideVersion
{
    None,
    Version_1
}
public enum GuideModel
{
    ModelStep,
    ModelTrigger
}

class GuideManager
{
    private static GuideManager m_instance;

    public bool isOpenGuide;
    /// <summary>
    /// 教學步驟
    /// </summary>
    public int guideIndex;
    public string guideName = null;
    public GuideVersion version = GuideVersion.Version_1;
    public static string[] GuideStepArr = { "guideStep1", "guideStep2", "guideStep3", "guideStep4", "guideStep5", "guideStep6" };
    public static string[] GuideTriggerArr = { "guideTrigger1", "guideTrigger2"};
    public bool IsGuideRunning;
    private Dictionary<string, GuideNode> guideNodeDict;

    /// <summary>
    /// 與guideIndex對應 升級第一隻夥伴的引導
    /// </summary>
    public int UPGRADE_FIRST_UNIT = 3;

    public const int GuideStageID = 3;

    public List<int> GuideOverIDList;

    public static GuideManager getInstance()
    {
        if (m_instance == null)
        {
            m_instance = new GuideManager();
            m_instance.IsGuideRunning = false;
        }
        return m_instance;
    }

    private GuideManager()
    {
        GuideUtility.initGuideFightData();
        guideIndex = 0;
        guideNodeDict = new Dictionary<string, GuideNode>();
        GuideOverIDList = new List<int>();
    }

    public Dictionary<string, GuideNode> GuideNodeDict
    {
        get
        {
            return guideNodeDict;
        }
    }

    public void addGuideNode(string key, GuideNode node)
    {
        if (!guideNodeDict.ContainsKey(key))
        {
            guideNodeDict.Add(key, node);
        }
    }

    public void removeGuideNode(string key)
    {
        if (guideNodeDict.ContainsKey(key))
        {
            guideNodeDict.Remove(key);
        }
    }

    public void initGuideData()
    {

    }

    public void StartGuide(int index = 1)
    {
        if (isOpenGuide)
        {
            //string stepN = GuideStepArr[guideIndex - 1];
            //Dictionary<int, GuideConfig> data = getGuideConfigByStepName(stepN);
            Dictionary<int, GuideConfig> data = GetGuideConfigByStepID();
            if (data != null)
            {
                GuideLayerModeBehCtr ctr = GuideLayerModeBehCtr.create(data, NextGuide, index, guideIndex);
                IsGuideRunning = true;
                ctr.execute();
            }
        }
        {

        }
    }

    public void StartPropGuide(int stageId)
    {
        if (isOpenGuide)
        {
            for (int i = 1; i <= GuideTriggerArr.Length; i++)
            {
                Dictionary<int, GuideConfig> data = getGuideConfigByStepName(GuideTriggerArr[i - 1]);

                if (data != null)
                    foreach (var item in data)
                    {
                        int indexNum = item.Value.HasData(stageId);

                        if (indexNum != -1)
                        {
                            //string savename = item.Value.GuideName + stageId;
                            string savename = string.Format("{0}_{1}", item.Value.GuideName, stageId);

                            if (!IsGuideKeySave(savename))
                            {
                                SetGuideConfigSaveName(data, savename, indexNum);
                                GuideLayerModeBehCtr ctr = GuideLayerModeBehCtr.create(data, NextGuide, 1, guideIndex, GuideModel.ModelTrigger);
                                IsGuideRunning = true;
                                ctr.execute();
                                break;
                            }

                        }
                    }
            }
        }
    }

    void SetGuideConfigSaveName(Dictionary<int, GuideConfig> data,string savename,int indexNum)
    {
        foreach (var da in data)
        {
            da.Value.IndexNum = indexNum;
            da.Value.SaveName = savename;
        }
    }

    public bool IsGuideOver()
    {
        //return guideIndex > GuideStepArr.Length;

        return IndexOfGuideStep() > GameController.Instance.Model.GuideStepIDConfigs.Count;
    }

    public bool IsGuideKeySave(string name)
    {
        return PlayerPrefsTools.GetBoolValue(name,true);
    }

    public void GuideKeySave(string name)
    {
        PlayerPrefsTools.SetBoolValue(name, true, true);
    }


    private void NextGuide()
    {
        IsGuideRunning = false;
        //guideIndex++;
        AddStep();
    }


    public void LocationGuide()
    {
        int guideid = CurrentGuideID();
        GameController.Instance.Client.SaveGuideIndex(guideIndex, guideid);
        GuideManager.getInstance().GuideOverIDList.Add(guideid);
        NextGuide();
    }

    private int m_stageId;
    private int m_step;
    public void StartFightGuide(int stageId, int step,Action callBack = null,bool timeLimit = false)
    {
        //GuideLayerBehCtr fight = GuideLayerBehCtr.create(stageId, step, EndGuide);
        //fight.execute();

        if (IsGuideRunning)
        {
            return;
        }

        m_stageId = stageId;
        m_step = step;
        FingerPoint finger = null;
        if (GuideUtility.contentName.ContainsKey(m_stageId))
        {
            Dictionary<int, int> temp = GuideUtility.contentName[m_stageId];
            if (GuideUtility.fingerPos.ContainsKey(m_stageId))
            {
                Dictionary<int, Vector3> fingerStep = GuideUtility.fingerPos[m_stageId];
                if (fingerStep.ContainsKey(step))
                {
                    finger = new FingerPoint();
                    finger.Pos = fingerStep[step];
                    finger.IsShow = true;
                }
            }
            if (temp.ContainsKey(m_step))
            {
                int dialogId = temp[m_step];
                GuideDialogCtr ctr = GuideDialogCtr.create(dialogId, EndGuide, finger,true, callBack);
                ctr.ShowFinger = false;
                IsGuideRunning = true;
                ctr.execute();
            }
        }
        else if(timeLimit)
        {
            if (callBack != null)
                callBack();
        }
    }

    public void EndGuide()
    {
        IsGuideRunning = false;
        if (GuideUtility.contentName.ContainsKey(m_stageId))
        {
            Dictionary<int, int> temp = GuideUtility.contentName[m_stageId];
            if (temp.ContainsKey(m_step))
            {
                temp.Remove(m_step);
            }
            if (temp.Count == 0)
            {
                GuideUtility.contentName.Remove(m_stageId);
            }

        }
    }


    public Dictionary<int, GuideConfig> getGuideConfigByStepName(string name)
    {
        Dictionary<int, GuideConfig> result = new Dictionary<int, GuideConfig>();
        int count = 1;
        foreach (GuideConfig item in GameController.Instance.Model.GuideConfigs.Values)
        {
            if (item.GuideName == name)
            {
                result.Add(count, item);
                count++;
            }
        }

        return result;
    }


    public Dictionary<int, GuideConfig> GetGuideConfigByStepID()
    {
        Dictionary<int, GuideConfig> result = null;

        Dictionary<int, GuideStepIdConfig> guideStepId = GameController.Instance.Model.GuideStepIDConfigs;

        if (!guideStepId.ContainsKey(guideIndex))
            return result;

        int guideid = guideStepId[guideIndex].GuideID;

        /////這裡缺一個驗證該教學id是否已經通過的邏輯待添加
        /////該驗證要從服務器取
        //if (false)
        //{
        //    AddStep();

        //    return GetGuideConfigByStepID();
        //}
        int count = 1;
        result = new Dictionary<int, GuideConfig>();

        foreach (GuideConfig item in GameController.Instance.Model.GuideConfigs.Values)
        {
            if (item.GuideID == guideid)
            {
                result.Add(count, item);
                count++;
            }
        }

        return result;
    }

    /// <summary>
    /// 找到正確的步數位置
    /// </summary>
    public void FindRightStep()
    {
        Dictionary<int, GuideStepIdConfig> guideStepId = GameController.Instance.Model.GuideStepIDConfigs;

        if (!guideStepId.ContainsKey(guideIndex))
        {
            while (guideIndex < MaxGuideStep())
            {
                if (!guideStepId.ContainsKey(guideIndex))
                    AddStep();
                else
                    break;
            }

        }

    }

    public Dictionary<int, GuideConfig> GetGuideConfigByGuideID(int id)
    {
        Dictionary<int, GuideConfig> result = new Dictionary<int, GuideConfig>();

        return result;
    }

    /// <summary>
    /// 增加教學步數
    /// </summary>
    void AddStep()
    {
        guideIndex = GetNextGuideStep();
    }

    /// <summary>
    /// 獲取下個教學步驟
    /// </summary>
    /// <returns></returns>
    int GetNextGuideStep()
    {
        int guidestep = guideIndex;

        List<int> keys = new List<int>(GameController.Instance.Model.GuideStepIDConfigs.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            ///驗證是否完成了該教學ID
            ///該驗證要從服務器取
            int guideid =  GameController.Instance.Model.GuideStepIDConfigs[keys[i]].GuideID;

            if (GuideOverIDList.Contains(guideid))
                continue;

            if (guidestep > keys[i])
                continue;
            else if (guidestep < keys[i])
                return keys[i];
            else if (guidestep == keys[i])
            {
                if (i < keys.Count - 1)
                    guidestep = keys[i + 1];
                else
                    guidestep = MaxGuideStep() + 1;

                return guidestep;
            }

        }

        guidestep = MaxGuideStep()+1;

        return guidestep;
    }

    int GetCurrentGuideStep()
    {
        int guidestep = guideIndex;

        List<int> keys = new List<int>(GameController.Instance.Model.GuideStepIDConfigs.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (guidestep < keys[i])
            {
                if (i == 0)///有過教學，但是之前的教學步驟刪了
                    guidestep = 0;
                else
                    guidestep = keys[i-1];

                break;

            }else if(guidestep > keys[i] && i==keys.Count-1)
                guidestep = keys[i - 1];
        }

        return guidestep;
    }

    int MaxGuideStep()
    {
        int step = 0;

        foreach (var guide in GameController.Instance.Model.GuideStepIDConfigs)
        {
            if (guide.Key > step)
                step = guide.Key;
        }

        return step;
    }

    /// <summary>
    /// 獲取當前教學的教學id
    /// </summary>
    /// <returns></returns>
    public int CurrentGuideID()
    {
        int id = 0;

        if (GameController.Instance.Model.GuideStepIDConfigs.ContainsKey(guideIndex))
            id = GameController.Instance.Model.GuideStepIDConfigs[guideIndex].GuideID;

        return id;
    }

    public int IndexOfGuideStep()
    {
        int step = GetCurrentGuideStep();
        ///初始化默認第一步教學
        if (step == 0)
            return 1;

        int id = 0;
        List<int> keys = new List<int>(GameController.Instance.Model.GuideStepIDConfigs.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (step == keys[i])
            {
                id = i + 1;
                break;
            }
        }

        return id;
    }

    public void SaveGuideIndex()
    {
        int guideid = CurrentGuideID();
        GameController.Instance.Client.SaveGuideIndex(guideIndex, guideid);
        GuideManager.getInstance().GuideOverIDList.Add(guideid);
    }

    public void SaveAndGotoNext()
    {
        SaveGuideIndex();
        AddStep();
        IsGuideRunning = false;
    }
}

