using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Com4Love.Qmax;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;

public class UIAddMove : PopupEventCor
{
    public UIButtonBehaviour AddButton;
    public UIButtonBehaviour EndButton;
    public Text TitleText;
    public Text PriceText;
    public Text TitleConstText;
    public Text CountText;

    public Transform ItemGoal;
    public Transform TimeGoal;

    public UIAddMoveGoalItem[] GoalItems;

    public event Action<UIAddMove> OnEndLevel;
    public event Action<UIAddMove> OnBuyMoves;

    /// <summary>
    /// 是否限時模式
    /// </summary>
    private bool isTimeLimit;

    // Use this for initialization
    void Start()
    {
        AddButton.onClick += AddButton_onClick;
        EndButton.onClick += EndButton_onClick;

    }

    void EndButton_onClick(UIButtonBehaviour button)
    {
        EndButton.onClick -= EndButton_onClick;

        if (OnEndLevel != null)
        {
            OnEndLevel(this);
        }
    }

    public override void Close()
    {
        base.Close();
    }

    void AddButton_onClick(UIButtonBehaviour button)
    {
        //購買步數
        if (!isTimeLimit && GameController.Instance.PlayerCtr.CheckBuyMovesAble())
        {
            //夠錢
            AddButton.onClick -= AddButton_onClick;
            GameController.Instance.Popup.ShowLightLoading();
            GameController.Instance.ModelEventSystem.OnBuyMoves += ModelEventSystem_OnBuyMoves;
            GameController.Instance.PlayerCtr.BuyMoves();       //監聽購買結果消息
        }
        // 購買時間
        else if (isTimeLimit && GameController.Instance.PlayerCtr.CheckBuyTimeAble())
        {
            //夠錢
            AddButton.onClick -= AddButton_onClick;
            GameController.Instance.Popup.ShowLightLoading();
            GameController.Instance.ModelEventSystem.OnBuyTime += ModelEventSystem_OnBuyTime;
            GameController.Instance.PlayerCtr.BuyTime();
        }
        else
        {
            //不夠錢，彈出鑽石窗口
            UIShop.Open(UIShop.TapIndex.GEM_INDEX);
        }
    }

    void ModelEventSystem_OnBuyTime(int obj)
    {
        GameController.Instance.Popup.HideLightLoading();
        GameController.Instance.ModelEventSystem.OnBuyTime -= ModelEventSystem_OnBuyTime;
    }

    void ModelEventSystem_OnBuyMoves(int obj)
    {
        if (GameController.Instance.ViewEventSystem.AddSteps != null)
            GameController.Instance.ViewEventSystem.AddSteps();

        GameController.Instance.Popup.HideLightLoading();
        GameController.Instance.ModelEventSystem.OnBuyMoves -= ModelEventSystem_OnBuyMoves;
    }

    public void OnDestroy()
    {
        AddButton.onClick -= AddButton_onClick;
        EndButton.onClick -= EndButton_onClick;
        GameController.Instance.ModelEventSystem.OnBuyMoves -= ModelEventSystem_OnBuyMoves;
        GameController.Instance.ModelEventSystem.OnBuyTime -= ModelEventSystem_OnBuyTime;
    }

    public void SetData(BattleModel data)
    {
        isTimeLimit = data.CrtStageConfig.Mode == BattleMode.TimeLimit;
        TitleText.text = Utils.GetTextByStringID(data.CrtStageConfig.NameStringID);
        PriceText.text = GameController.Instance.Model.GameSystemConfig.buyMoves.ToString();

        if (isTimeLimit)
        {
            // 時間模式，替換圖片
            //TitleImage.sprite = TimeSprite1;
            //TitleImage.SetNativeSize();
            //CountImage.sprite = TimeSprite2;
            //CountImage.SetNativeSize();
            TitleConstText.text = Utils.GetTextByID(544);
            CountText.text = Utils.GetTextByID(543);
        }

        StageConfig.Goal goal;

        if (data.CrtStageConfig.InterfaceType == 3)
        {
            // 顯示分數
            ItemGoal.gameObject.SetActive(false);
            TimeGoal.gameObject.SetActive(true);
            UIAddMoveTimeGoal tGoal = TimeGoal.gameObject.GetComponent<UIAddMoveTimeGoal>();
            if (tGoal != null)
            {
                int s = 0;
                for (int i = 0; i < data.CurrentGoal.Count; i++)
                {
                    goal = data.CurrentGoal[i];
                    s += goal.Num;
                }
                tGoal.setData(s);
            }
            return;
        }

        UIAddMoveGoalItem item;
        int left = -1;

        for (int i = 0; i < GoalItems.Length; i++)
        {
            left = -1;
            item = GoalItems[i];

            if (i < data.CurrentGoal.Count)
            {
                item.gameObject.SetActive(true);
                goal = data.CurrentGoal[i];

                if (goal.Type == BattleGoal.Unit)
                {
                    //怪
                    left = goal.Num - data.UnitGoal;
                    left = Mathf.Max(0, left);
                }
                else if (goal.Type == BattleGoal.Object)
                {
                    left = goal.Num - data.ObjectGoal[goal.RelativeID];
                    left = Mathf.Max(0, left);
                }
                if (left != -1)
                {
                    item.SetData(left, goal);
                    item.gameObject.SetActive(true);
                }
                else
                {
                    item.gameObject.SetActive(false);
                }
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    public struct Data
    {
        public string Title;
        public int price;
    }
}
