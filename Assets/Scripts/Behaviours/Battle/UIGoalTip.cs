using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Data.Config;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIGoalTip : MonoBehaviour {
    public struct Data
    {
        public string Title;
        public List<StageConfig.Goal> goals;
    }

    public UIGoalTipListItem[] items;
    Data data;

    /// <summary>
    /// 分數提示父節點
    /// </summary>
    public Transform ScoreTipsParent;

    public Text ScoreTipsNum;

    public void SetData(Data value)
    {
        data = value;
        DataChange();
    }

    public void OnEnable()
    {
        //Invoke("Hide", 2f);
    }

    public void OnDestroy()
    {

    }

    //void Hide()
    //{
    //    Destroy(gameObject);
    //}

    void DataChange()
    {
        int NumMax = 4;
        for (int i = 0; i < data.goals.Count; i++)
        {
            if(i<NumMax)
                items[i].SetData(data.goals[i]);
        }

    }
}
 
