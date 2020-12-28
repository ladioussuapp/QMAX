using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.goods;
using Com4Love.Qmax.Net.Protocols.Stage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUiBehaviour : MonoBehaviour
{
    public Text KeyText;
    public Image KeyTipImg;
    public Image UpgradeCountTipImg;
    public Text UpgradeCountText;
    public Image AchieveCountImg;
    public Text AchieveCountText;

    public UIButtonBehaviour KeyButton;
    public UIButtonBehaviour CJButton;
    public UIButtonBehaviour UpgradeButton;
    public UIButtonBehaviour SettingButton;
    /// <summary>
    /// 大樹按鈕 現在的活動關卡按鈕
    /// </summary>
    public UIButtonBehaviour TreeButton;
    public UIButtonBehaviour LoginAwardButton;
    public UIButtonBehaviour GoodsButton;
    public Image AtlasSprite;
    public Transform TreeButtonStateNomal;  //沒有活動可以進行時狀態
    public Transform TreeButtonStateLight;  //有活動可以進行時狀態

    //大樹相關
    //大樹倒計時開啟倒計時與關閉倒計時的顏色
    public Text TALeftTime;
    public Color TATextColorStart;  //红色    表示正在進行中， 表示活動快結束
    public Color TATextColorEnd;
    public Image TAStateImg;        //大樹圖標

    StageConfig TargetConfig;
    protected ActorGameResponse playerData;
    protected PlayerCtr playerCtr;
    //DateTime TreeActivityTipDT;
    //TimeSpan TreeActivityTipSpan;
    private bool goodsWinOpenFlag;

    /// <summary>
    /// 选人场景关闭回调
    /// </summary>
    Action SelectUnitCloseHandler = null;
    Action<StageConfig> UnLockCloseHandler = null;

    ///// <summary>
    ///// 地圖場景的物體 在彈出全屏界面後禁用以提高性能
    ///// </summary>
    //[HideInInspector]
    //public MapView MapViewRoot = null;

    public void Awake()
    {
        GameController.Instance.AtlasManager.AddAtlas(Atlas.UIMap, AtlasSprite.sprite.texture);
    }

    public void Start()
    {
        playerCtr = GameController.Instance.PlayerCtr;

        if (playerCtr.PlayerData != null)
        {
            UpdateUi(playerCtr.PlayerData);
            CheckButtonVisibles();

            if (GameController.Instance.PlayerCtr.PlayerData.passStageId >= 5)
            {
                ///過關關卡達到5以上開啟登錄獎勵//
                SignInfo();
            }

            PlayButtonAni();
        }

        KeyButton.onClick += KeyButton_onClick;
        CJButton.onClick += CJButton_onClick;
        UpgradeButton.onClick += UpgradeButton_onClick;
        SettingButton.onClick += SettingButton_onClick;
        TreeButton.onClick += TreeButton_onClick;
        LoginAwardButton.onClick += LoginAwardButton_OnClick;
        GoodsButton.onClick += GoodsButton_OnClick;

        GameController gCtr = GameController.Instance;
        gCtr.ViewEventSystem.ClickMapBtnEvent += ViewEventSystem_MapButtonClickEvent;
        gCtr.ModelEventSystem.OnPlayerInfoRef += ModelEventSystem_OnPlayerInfoRef;
        gCtr.ModelEventSystem.OnStageUnlocked += ModelEventSystem_OnStageUnlocked;
        //gCtr.ModelEventSystem.OnTreeActInfoUpdate += UpdateActivitingState;
        gCtr.ModelEventSystem.OnPushDataSerialized += OnCheckPushData;
        gCtr.ModelEventSystem.OnReachAchieveCountUpdate += OnAchieveCountUpdate;
        gCtr.ModelEventSystem.OnGetGoodsList += OnGoodsReflash;
        gCtr.ViewEventSystem.TryToStartTreeAct += OnTryToStartTreeAct;
        GameController.Instance.Popup.OnCloseComplete += Popup_OnCloseComplete;


        StartCoroutine(endFrameExecute());

        //有可能是由推送進入的遊戲，需要在一開始檢測是否有推送的數據
        OnCheckPushData();

        //請求已達成的成就數量 
        GameController.Instance.Client.GetReachAchieveCount();
        //請求最新的背包數據
        GameController.Instance.Client.GetAllGoodsList();
    }

    public void OnDestroy()
    {
        KeyButton.onClick -= KeyButton_onClick;
        CJButton.onClick -= CJButton_onClick;
        UpgradeButton.onClick -= UpgradeButton_onClick;
        SettingButton.onClick -= SettingButton_onClick;
        TreeButton.onClick -= TreeButton_onClick;
        GoodsButton.onClick -= GoodsButton_OnClick;
        removeFliter();
        //GameController.Instance.ModelEventSystem.OnSignEvent -= ModelEventSystem_OnSignEvent;
        GameController.Instance.ModelEventSystem.OnSignInfoEvent -= ModelEventSystem_OnSignInfoEvent;
        GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UIMap);
        GameController.Instance.ModelEventSystem.OnPlayerInfoRef -= ModelEventSystem_OnPlayerInfoRef;
        GameController.Instance.ModelEventSystem.OnStageUnlocked -= ModelEventSystem_OnStageUnlocked;
        GameController.Instance.Popup.OnCloseComplete -= Popup_OnCloseComplete;
        //GameController.Instance.ModelEventSystem.OnTreeActInfoUpdate -= UpdateActivitingState;            //大树信息刷新   目前弃用
        GameController.Instance.ViewEventSystem.ClickMapBtnEvent -= ViewEventSystem_MapButtonClickEvent;
        GameController.Instance.ViewEventSystem.TryToStartTreeAct -= OnTryToStartTreeAct;
        GameController.Instance.ModelEventSystem.OnPushDataSerialized -= OnCheckPushData;
        GameController.Instance.ModelEventSystem.OnReachAchieveCountUpdate -= OnAchieveCountUpdate;
        GameController.Instance.ModelEventSystem.OnGetGoodsList -= OnGoodsReflash;
    }

    void UpdateUi(ActorGameResponse data)
    {
        playerData = data;

        int keyCount = data.key;
        KeyText.text = keyCount > 99 ? "N" : keyCount.ToString();
        KeyTipImg.gameObject.SetActive(keyCount > 0);
        int upgradeCount = GameController.Instance.UnitCtr.GetUpgradeAbleCount();
        UpgradeCountTipImg.gameObject.SetActive(upgradeCount > 0);
        UpgradeCountText.text = upgradeCount.ToString();
    }

    void SignInfo()
    {
        ///Days == -1 即沒有獲取到服務器數據需要再次請求///
        if (GameController.Instance.Model.LoginGiveData.Days == -1)
        {
            GameController.Instance.ModelEventSystem.OnSignInfoEvent -= ModelEventSystem_OnSignInfoEvent;
            GameController.Instance.ModelEventSystem.OnSignInfoEvent += ModelEventSystem_OnSignInfoEvent;
            GameController.Instance.PlayerCtr.SignInfo();
        }
    }

    void OpenUILoginGiveEveryDay()
    {
        if (GameController.Instance.Model.LoginGiveData.IsCanAward)
        {
            Action saveAndOpen = delegate
            {
                string saveTime = Utils.DateTimeToUnixTime(System.DateTime.Now).ToString();
                PlayerPrefsTools.SetStringValue(OnOff.OpenUILoginGive, saveTime, true);
                OpenUILoginGive();
            };

            if (!PlayerPrefsTools.HasKey(OnOff.OpenUILoginGive, true))
            {
                saveAndOpen();
                return;
            }

            double openTime = 0;
            double.TryParse(PlayerPrefsTools.GetStringValue(OnOff.OpenUILoginGive, true), out openTime);

            DateTime befTime = Utils.UnixTimeToDateTime(openTime);

            ///判定是否過了一年，再判定天數///
            if (System.DateTime.Now.Year > befTime.Year || System.DateTime.Now.DayOfYear > befTime.DayOfYear)
            {
                saveAndOpen();
            }
        }
    }

    void ModelEventSystem_OnSignInfoEvent(int day, bool iscanaward)
    {
        GameController.Instance.Model.LoginGiveData.Days = day;
        GameController.Instance.Model.LoginGiveData.IsCanAward = iscanaward;
        ///成功獲取到了登陸信息，如果沒有打開登陸獎勵窗口則打開///

        if (iscanaward)
        {
            OpenUILoginGiveEveryDay();
        }
    }

    void OpenUILoginGive()
    {
        if (GameController.Instance.Popup.IsPopup(PopupID.UISelectHero))
        {
            //已經打開了選人界面的情況下，等待選人界面關閉後再彈出。
            SelectUnitCloseHandler = OpenUILoginGive;
            return;
        }

        if (!GameController.Instance.Popup.IsPopup(PopupID.UILoginGive))
        {
            GameController.Instance.Popup.Open(PopupID.UILoginGive, null, true, true);
        }
    }

    void OpenAchievement()
    {
        if (!GameController.Instance.Popup.IsPopup(PopupID.UIAchievement))
        {
            GameController.Instance.Client.OpenAchieveData();
        }
    }

    /// <summary>
    ///start里手動調用， 然後在 OnPushDataSerialized 事件中調用
    /// </summary>
    void OnCheckPushData()
    {
        if (GameController.Instance.Model.PushData != null)
        {
            //        string s = null;
            //GameController.Instance.Model.PushData.ContentOptional("action", ref s);

            //        if (s == "tree")
            //        {
            //            if (GameController.Instance.TreeActivityCtr.CanBeEntered())
            //            {
            //                //可以進入大樹活動
            //                TreeButton_onClick(TreeButton);
            //GameController.Instance.ClearPushData();
            //            }
            //        }
        }
    }

    /// <summary>
    /// 更新已達成的成就數量
    /// </summary>
    /// <param name="count"></param>
    void OnAchieveCountUpdate(int count)
    {
        if (AchieveCountImg == null || AchieveCountText == null)
            return;

        if (count > 0)
        {
            AchieveCountImg.gameObject.SetActive(true);
            if (count > 99)
                AchieveCountText.text = "N";
            else
                AchieveCountText.text = count.ToString();
        }
        else
        {
            AchieveCountImg.gameObject.SetActive(false);
        }
    }

    void LoginAwardButton_OnClick(UIButtonBehaviour button)
    {
        if (GameController.Instance.Model.LoginGiveData.Days == -1)
        {
            SignInfo();
        }
        else
        {
            OpenUILoginGive();
        }
    }

    void OnGoodsReflash(GoodsListResponse res)
    {
        if (!GameController.Instance.Popup.IsPopup(PopupID.UIGoodsWin) && goodsWinOpenFlag)
        {
            GameController.Instance.Popup.Open(PopupID.UIGoodsWin, null, true, false).GetComponent<GoodsWinBehaviour>();
            goodsWinOpenFlag = false;
        }
    }

    void GoodsButton_OnClick(UIButtonBehaviour button)
    {
        if (GameController.Instance.Popup.IsPopup(PopupID.UIGoodsWin))
        {
            return;
        }
        goodsWinOpenFlag = true;
        GameController.Instance.Client.GetAllGoodsList();
    }

    private void OnTryToStartTreeAct()
    {
        Q.Log("通過大樹通知打開大樹 2");
        TreeButton_onClick(TreeButton);
    }

    void TreeButton_onClick(UIButtonBehaviour button)
    {
        StageConfig activeStageConfig = GameController.Instance.StageCtr.GetMinimumUnlockActiveStage();

        if (GameController.Instance.ViewEventSystem.MoveToMapLvl != null)
        {
            GameController.Instance.ViewEventSystem.MoveToMapLvl(activeStageConfig.ID);
        }
    }

    IEnumerator endFrameExecute()
    {
        yield return new WaitForEndOfFrame();
        addFliter();
    }

    void ModelEventSystem_OnStageUnlocked(int obj)
    {


        //有關卡被鑽石解鎖   
        //查看解鎖窗口是否存在，存在則關閉它，並打開進入關卡窗口
        //監聽改到start裡
        if (GameController.Instance.Popup.IsPopup(PopupID.UIUnlock))
        {
            UnLockCloseHandler = OpenSelectUnitWin;
        }
    }

    void Popup_OnCloseComplete(PopupID obj)
    {
        if (obj == PopupID.UIUnlock && UnLockCloseHandler != null)
        {
            //解鎖關卡觸發解鎖窗口關閉
            UnLockCloseHandler(TargetConfig);
            GameController.Instance.SceneCtr.WantToScene = Scenes.BattleScene;
            UnLockCloseHandler = null;
        }
        else if (obj == PopupID.UISelectHero)
        {
            if (SelectUnitCloseHandler != null)
            {
                //應該新增判斷，如果執行了場景跳轉就不再執行這個回調。
                SelectUnitCloseHandler();
                SelectUnitCloseHandler = null;
            }
        }
    }

    void SettingButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Open(PopupID.UISetting, null, true, true);
    }

    void UpgradeButton_onClick(UIButtonBehaviour button)
    {
        //GameController.Instance.SceneCtr.LoadLevel(Scenes.UpgradScene, null);
        GameController.Instance.Popup.Open(PopupID.UIUpgrad, null, true, false, -1, "Prefabs/Ui/UIPopUpNoBg");
    }

    void KeyButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Open(PopupID.UIGetChance, null, true, false, -1, "Prefabs/Ui/UIPopUpNoBg");
    }

    void CJButton_onClick(UIButtonBehaviour button)
    {
        OpenAchievement();
    }

    void CheckButtonVisibles()
    {
        if (playerData == null)
        {
            return;
        }

        //應該是逐步開放功能
        KeyButton.gameObject.SetActive(GameController.Instance.PlayerCtr.CheckGetChanceAble());

        switch (GuideManager.getInstance().version)
        {
            case GuideVersion.Version_1:
                UpgradeButton.gameObject.SetActive(GameController.Instance.PlayerCtr.PlayerData.passStageId >= 1);
                break;
            default:
                break;
        }

        LoginAwardButton.gameObject.SetActive(GameController.Instance.PlayerCtr.PlayerData.passStageId >= 5);
        CJButton.gameObject.SetActive(GameController.Instance.PlayerCtr.PlayerData.passStageId >= 6);

        CheckTreeButtonVisible();
    }



    //大樹按鈕的顯示隱藏邏輯 現在邏輯為
    void CheckTreeButtonVisible()
    {
        StageConfig activeStageConfig = GameController.Instance.StageCtr.GetMinimumUnlockActiveStage();
        TreeButton.gameObject.SetActive(activeStageConfig != null);
 
    }

    void ModelEventSystem_OnPlayerInfoRef(System.Collections.Generic.List<RewardType> arg1, ActorGameResponse arg2)
    {
        UpdateUi(GameController.Instance.PlayerCtr.PlayerData);
    }

    void OpenSelectUnitWin(StageConfig config)
    {
        if (GameController.Instance.Popup.IsPopup(PopupID.UISelectHero))
        {
            return;
        }

#if TREEACTIVITY_DEBUG
        Debug.Log("測試大樹：");
        config = GameController.Instance.StageCtr.GetActivityStage();
#endif

        UIUnitSelectWindowBehaviour window = GameController.Instance.Popup.Open(PopupID.UISelectHero, null, true, true).GetComponent<UIUnitSelectWindowBehaviour>();

        window.SetData(config);
    }

    void OpenUnLockWin(StageConfig sConfig, bool needGem)
    {
        if (GameController.Instance.Popup.IsPopup(PopupID.UIUnlock))
        {
            return;
        }

        Transform rt = GameController.Instance.Popup.Open(PopupID.UIUnlock, null, true, true);
        List<StageLockInfo> lockInfos = GameController.Instance.StageCtr.GetStageLockInfo(sConfig);
        UIUnlock uiUnlock = rt.GetComponent<UIUnlock>();
        uiUnlock.SetData(lockInfos, needGem, sConfig);
    }

    public void OpenLvl(int lvl)
    {
        if (GameController.Instance.Popup.HasPopup)
        {
            return;
        }

        int lockState = GameController.Instance.StageCtr.GetStageLockState(lvl);

        StageConfig sConfig = GameController.Instance.StageCtr.GetStage(lvl);
        TargetConfig = sConfig;

        if (sConfig == null)
        {
            //沒有此關卡
            return;
        }

        if (lockState != 0)
        {
            OpenUnLockWin(sConfig, lockState == 2);
        }
        else
        {
            OpenSelectUnitWin(sConfig);
            GameController.Instance.SceneCtr.WantToScene = Scenes.BattleScene;
        }
    }

    void ViewEventSystem_MapButtonClickEvent(int lvl)
    {
        //Debug.LogWarning("MapView.Instance.IsBusy :" + MapView.Instance.IsBusy);

        //地圖在滾動的時候不讓點擊
        if (!MapView.Instance.IsBusy)
        {
            OpenLvl(lvl);
        }
    }

    private void addFliter()
    {
        GuideNode node = new GuideNode();
        node.TargetNode = KeyButton;
        node.TarCamera = Camera.main;
        node.index = 1;
        node.ShowTips = true;
        node.CallBack = delegate ()
        {
            KeyButton_onClick(null);
        };
        GuideManager.getInstance().addGuideNode("MapViewKeyIcon", node);

        GuideNode node1 = new GuideNode();
        node1.TargetNode = UpgradeButton;
        node1.TarCamera = Camera.main;
        node1.index = 1;
        node1.ShowTips = true;
        node1.CallBack = delegate ()
        {
            UpgradeButton_onClick(null);
        };
        GuideManager.getInstance().addGuideNode("MapViewUnitIcon", node1);
    }

    private void removeFliter()
    {
        GuideManager.getInstance().removeGuideNode("MapViewKeyIcon");
        GuideManager.getInstance().removeGuideNode("MapViewUnitIcon");
    }

    void SaveButtonAniInfo(OnOff kind)
    {
        PlayerPrefsTools.SetIntValue(kind, 1, true);
    }

    bool GetButtonCanPlayAni(OnOff kind)
    {
        return PlayerPrefsTools.GetIntValue(kind, true) == 0;
    }

    void PlayButtonAni()
    {
        bool state = false;
        UIButtonBehaviour button = null;
        OnOff buttonKind = OnOff.None;
        ///第五關抽獎///
        if (KeyButton.isActiveAndEnabled && GetButtonCanPlayAni(OnOff.ChangeButtonMark))
        {
            button = KeyButton;
            state = true;
            buttonKind = OnOff.ChangeButtonMark;
        }

        if (UpgradeButton.isActiveAndEnabled && GetButtonCanPlayAni(OnOff.UpgradeButtonMark))
        {
            button = UpgradeButton;
            state = true;
            buttonKind = OnOff.UpgradeButtonMark;
        }

        if (state)
        {
            SaveButtonAniInfo(buttonKind);

            StartCoroutine(Utils.DelayToInvokeDo(delegate ()
            {
                button.PlayHintAni();
            }, .8f
            ));
        }

    }

}
