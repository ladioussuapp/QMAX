using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.Stage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com4Love.Qmax.Helper;
using Com4Love.Qmax.Tools;

public class UIUnitSelectWindowBehaviour : MonoBehaviour
{
    [HideInInspector]
    public MapUiBehaviour mapBehaviour;
    public Text TitleText;
    public Text LimitText;
    public RectTransform Goal;  //目标
    public Image bg;
    public Text energyText;
    public Transform Stars;
    protected StageCtr stageCtr;
    protected int star = 0;
    public UnitList[] UnitList;
    public GameObject Content;
    public UIButtonBehaviour EnterButton;
    public UIButtonBehaviour CloseButton;
    public Transform NomalContent;
    public Transform TreeActivityContent;
    public RectTransform Finger;
    public RectTransform Mask;
    public Text ScoreText;
    public Text LimitNameText;
    public Image LimitNameBg;
    public Text GoalNameText;
 
    private Transform fingerTag;
    private bool fingerDir;

    //邏輯修改。外部傳入此數據。有可能是正常的關卡，也有可能是大樹關卡
    private Stage Stage;
    private StageConfig StageConfig;


    /// <summary>
    /// 目標icon///
    /// </summary>
    List<GoalIcon> GoalIconList;
    /// <summary>
    /// 目標配置///
    /// </summary>
    List<StageConfig.Goal> GoalsConfig;

    /// <summary>
    /// 點擊按鈕是否直接進入遊戲//
    /// </summary>
    bool ClickToBattle = false;

    StartGameHelper StartHelper;


    public void Start()
    {
        if (GameController.Instance.StageCtr.IsActivityStage(StageConfig))
        {
            NomalContent.gameObject.SetActive(false);
            TreeActivityContent.gameObject.SetActive(true);
            Stars.gameObject.SetActive(false);
        }
        else
        {
            NomalContent.gameObject.SetActive(true);
            TreeActivityContent.gameObject.SetActive(false);
        }

        if (LimitNameText != null)
        {
            int nameid = 502;
            if (StageConfig.Mode == BattleMode.Normal)
                nameid = 502;
            else if (StageConfig.Mode == BattleMode.TimeLimit)
                nameid = 538;

            LimitNameText.text = Utils.GetTextByID(nameid);
        }
        Com4Love.Qmax.Tools.AtlasManager atls = GameController.Instance.AtlasManager;
        if (LimitNameBg != null)
        {
            LimitNameBg.sprite = StageConfig.Mode == BattleMode.TimeLimit ? atls.GetSprite(Atlas.UIComponent,"XR022a"):atls.GetSprite(Atlas.UIComponent, "XR022");
        }

        SkillMsgText.transform.parent.gameObject.SetActive(false);
        //GameController.Instance.ModelEventSystem.OnStageBeginEvent += ModelEventSystem_OnStageBeginEvent;
        mapBehaviour = FindObjectOfType<MapUiBehaviour>();
        stageCtr = GameController.Instance.StageCtr;

        TitleText.text = Utils.GetTextByStringID(StageConfig.NameStringID);
        int step = GameController.Instance.StageCtr.GetStageLimitStep(StageConfig.ID);
        LimitText.text = step.ToString();
        energyText.text = StageConfig.CostEnergy.ToString();

        LoadBg();

        if (Stage != null)
        {
            //有可能不需要顯示星數
            UpdateStar(Stage.star);
        }
 
        EnterButton.onClick += EnterButton_onClick;
        CloseButton.onClick += CloseButton_onClick;

        GameController.Instance.Popup.OnOpenComplete += onOpenWindow;

        Finger.gameObject.SetActive(false);
        StartCoroutine(DelayInit());

        StartHelper = new StartGameHelper();

        PropCtr propCtr = GameController.Instance.PropCtr;
        ///清除所有被動道具的選中狀態///
        propCtr.ClearSelectAndUse();

        /////加步數數字跳動效果///
        //if (propCtr.GetPropNumEffAction(PropType.AddState) == null)
        //    propCtr.AddPropNumEffAction(PropType.AddState, ChangeAddStatesUI);

        /////減少目標物數字跳動效果///
        //if (propCtr.GetPropNumEffAction(PropType.ReduceGoal) == null)
        //    propCtr.AddPropNumEffAction(PropType.ReduceGoal, ChangeGoalUI);

#if AUTO_FIGHT
        //延遲自動跳轉到戰鬥
        Invoke("Test", 2f);
#endif
    }

    IEnumerator DelayInit()
    {
        yield return new WaitForSeconds(0.1f);
        //if (GuideManager.getInstance().version == GuideVersion.Version_1 && GuideManager.getInstance().guideIndex == 6)
        if (GuideManager.getInstance().CurrentGuideID() == 5)
        {
            for (int i = 1; i < (int)ColorType.All; i++)
            {
                List<UnitConfig> units = GameController.Instance.UnitCtr.GetOwnUnits((ColorType)i);
                if (units.Count >= 2)
                {
                    UnitList[i - 1].OnSelected = this.OnItemSelected;
                    fingerTag = this.transform.Find("Canvas").Find("FingerTag" + i);

                    if (UnitList[i - 1].ArrowUp.activeInHierarchy)
                        fingerDir = true;
                    if (UnitList[i - 1].ArrowDown.activeInHierarchy)
                        fingerDir = false;

                    if (fingerDir)
                        Finger.position = new Vector3(fingerTag.transform.position.x, fingerTag.transform.position.y + 0.4f, fingerTag.transform.position.z);
                    else
                        Finger.position = new Vector3(fingerTag.transform.position.x, fingerTag.transform.position.y , fingerTag.transform.position.z);

                    
                    Material m_ImageMaterial = Mask.gameObject.GetComponent<Image>().material;

                    Vector2 targetLocalPos = new Vector2(fingerTag.localPosition.x - 40, fingerTag.localPosition.y + 130);
                    Vector2 tempV = m_ImageMaterial.GetTextureScale("_MainTex");
                    m_ImageMaterial.SetTextureScale("_Mask", tempV);
                    m_ImageMaterial.SetTextureOffset("_Mask", new Vector2(-targetLocalPos.x / (Mask.sizeDelta.x), -targetLocalPos.y / Mask.sizeDelta.y));
                    GuideManager.getInstance().StartGuide();

                    break;
                }
            }
        }
    }

#if AUTO_FIGHT
    void Test()
    {
        EnterButton_onClick(EnterButton);
    }
#endif


    public void OnDestroy()
    {
        EnterButton.onClick -= EnterButton_onClick;
        CloseButton.onClick -= CloseButton_onClick;
        GameController.Instance.Popup.OnOpenComplete -= onOpenWindow;

        switch (GuideManager.getInstance().version)
        {
            case GuideVersion.Version_1:
                removeFliter_v1();
                break;
            default:
                break;
        }
        StartHelper.Clear();

        GameController.Instance.PropCtr.ClearPropNumEffAction(PropType.AddState);
        GameController.Instance.PropCtr.ClearPropNumEffAction(PropType.ReduceGoal);
    }

    private void onOpenWindow(PopupID obj)
    {
        if (obj == PopupID.UISelectHero)
        {
            GameController.Instance.Popup.OnOpenComplete -= onOpenWindow;
            switch (GuideManager.getInstance().version)
            {
                case GuideVersion.Version_1:
                    addFliter_v1();
                    break;
                default:
                    break;
            }
        }
    }

    void CloseButton_onClick(UIButtonBehaviour button)
    {
        if (Finger.gameObject.activeSelf)
            return;

        GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.MAP_SCENE_BACK);
        GameController.Instance.Popup.Close(PopupID.UISelectHero);
    }

    //List<int> units;

    void EnterButton_onClick(UIButtonBehaviour button)
    {
        if (ClickToBattle)
        {
            if (Finger.gameObject.activeSelf)
                return;
           
            StartHelper.GoToGame(StageConfig, GetSelectUnits());
        }
        else
        {
            UISelectPropBehaviour uiprop = GameController.Instance.Popup.Open(PopupID.UISelectProp, null, true, true).GetComponent<UISelectPropBehaviour>();
            uiprop.SetData(StageConfig, GetSelectUnits());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="config"></param>
    /// <param name="clickToBattle">是否直接進入遊戲，還是需要彈道具窗口</param>
    public void SetData(StageConfig config,bool clickToBattle = false)
    {
        StageConfig = config;
        Stage = GameController.Instance.StageCtr.GetStageData(config.ID);

        //限定道具出現關卡之後 並且不是活動關卡
        if (config.ID > GuideManager.GuideStageID && !GameController.Instance.StageCtr.IsActivityStage(config))
            ClickToBattle = clickToBattle;
        else
            ClickToBattle = true;
    }

    public Image SkillIcon;
    public Text SkillMsgText;

    public void ShowSkillTip(UnitConfig config, Sprite icon)
    {
        SkillMsgText.transform.parent.gameObject.SetActive(true);
        SkillMsgText.text = string.Format("{0} - 等級{1} - 攻擊力：{2}", Utils.GetTextByStringID(config.NameStringId), config.Level, config.UnitAtk);
        SkillIcon.sprite = icon;

        CancelInvoke("HideSkillTip");
        Invoke("HideSkillTip", 3f);
    }

    private void HideSkillTip()
    {
        SkillMsgText.transform.parent.gameObject.SetActive(false);
    }

    protected void LoadBg()
    {
        Sprite sprite = GameController.Instance.QMaxAssetsFactory.CreateUiSelectUnitBg(StageConfig, new Vector2(.5f, .5f));
        bg.sprite = sprite;
    }

    protected List<int> GetSelectUnits()
    {
        List<int> selects = new List<int>();
        UnitConfig select;

        foreach (UnitList list in UnitList)
        {
            select = list.GetSelectUnit();

            if (select != null)
                selects.Add(select.ID);
        }

        return selects;
    }

    protected void UpdateStar(int star_)
    {
        star = star_;
        Transform t;
        Transform img;

        for (int i = 0; i < Stars.childCount; i++)
        {
            t = Stars.GetChild(i);
            img = t.GetChild(0);

            if (i < star)
            {
                img.gameObject.SetActive(true);
            }
            else
            {
                img.gameObject.SetActive(false);
            }
        }

        UpdateGoal();
    }

    //目標信息更新
    protected void UpdateGoal()
    {
        //List<StageConfig.Goal> goals;
        StageConfig.Goal goal;
        Stage stageData = GameController.Instance.StageCtr.GetStageData(StageConfig.ID);
        switch (star)
        {
            case 0:
                if (stageData.targets.ContainsKey(1))
                {
                    GoalsConfig = BattleTools.ParseGoal(stageData.targets[1]);
                }
                else
                {
                    //GoalsConfig = StageConfig.Goal1;
                }
                //GoalsConfig = StageConfig.Goal1;
                break;
            case 1:
                if (stageData.targets.ContainsKey(2))
                {
                    GoalsConfig = BattleTools.ParseGoal(stageData.targets[2]);
                }
                else
                {
                    //GoalsConfig = StageConfig.Goal2;
                }
                //GoalsConfig = StageConfig.Goal2;
                break;
            default:
                if (stageData.targets.ContainsKey(3))
                {
                    GoalsConfig = BattleTools.ParseGoal(stageData.targets[3]);
                }
                else
                {
                    //GoalsConfig = StageConfig.Goal3;
                }
                //GoalsConfig = StageConfig.Goal3;
                break;
        }

        bool hasScore = false;
        bool hasGoals = false;

        //1顯示分數很目標，2顯示目標，3顯示分數//
        switch (StageConfig.InterfaceType)
        {
            case 1:
                hasScore = true;
                hasGoals = true;
                break;
            case 2:
                hasScore = false;
                hasGoals = true;
                break;
            case 3:
                hasScore = true;
                hasGoals = false;
                break;

        }

        if (hasScore)
        {

            ///正常關卡過關分數為一星分數，分數關卡為對應星級分數
            List<StageConfig.Goal> scorecon;
            if (!GameController.Instance.StageCtr.IsAllScoreGoal(GoalsConfig))
                scorecon = BattleTools.ParseGoal(stageData.targets[3]);
            else
                scorecon = GoalsConfig;

            foreach (var con in scorecon)
            {
                if (con.Type == BattleGoal.Score && con.Num != 0)
                {
                    ScoreText.text = string.Format("{0:N0}", con.Num);
                    hasScore = true;
                    break;
                }
            }
        }

        ///有分數目標顯示分數,沒有分數顯示消除目標//
        Goal.gameObject.SetActive(hasGoals);
        ScoreText.gameObject.SetActive(hasScore);

        if (hasScore)
            GoalNameText.text = Utils.GetTextByID(539);

        ///沒有分數目標計算顯示消除目標//
        if (hasGoals)
        {
            int goalItemCount = GoalsConfig.Count;

            Transform goalItem;
            GoalIcon icon;
            GoalIconList = new List<GoalIcon>();

            for (int i = 0; i < Goal.childCount; i++)
            {
                goalItem = Goal.GetChild(i);
                goalItem.gameObject.SetActive(false);
                icon = goalItem.GetComponent<GoalIcon>();

                if (i < goalItemCount)
                {
                    goal = GoalsConfig[i];

                    if (goal.Type == BattleGoal.Score)
                        continue;

                    icon.Data = goal;
                    goalItem.gameObject.SetActive(true);

                    GoalIconList.Add(icon);
                }
            }
        }
        

    }

    public void ChangeGoalUI(bool isuse,float point)
    {
        ///只顯示分數目標//
        if (StageConfig.InterfaceType == 3)
            return;

        for (int i =0; i< GoalIconList.Count;i++)
        {
            GoalIcon icon = GoalIconList[i];

            if (icon.isActiveAndEnabled && i < GoalsConfig.Count)
            {
                float delaytime = .1f;
                int min = (int)(GoalsConfig[i].Num * (1 - point/100));
                int max = (int)(GoalsConfig[i].Num);
                int from = isuse ? max : min;
                int to = isuse ? min : max;

                delaytime = delaytime + .05f * (max - min);
                if (delaytime > 0.3f)
                    delaytime = 0.3f;

                GameController.Instance.EffectProxy.ScrollText(icon.amountText, from, to, delaytime);
            }
        }
    }

    void ChangeAddStatesUI(bool isuse, float point)
    {
        int step = GameController.Instance.StageCtr.GetStageLimitStep(StageConfig.ID);
        int from = isuse ? step : step + (int)point;
        int to = isuse ? step + (int)point : step;

        GameController.Instance.EffectProxy.ScrollText(LimitText, from, to, .3f);
    }

    private void addFliter_v1()
    {
        //if (GuideManager.getInstance().guideIndex == 2)
        if(GuideManager.getInstance().CurrentGuideID() == 1)
        {
            GuideNode node = new GuideNode();
            node.TargetNode = EnterButton;
            node.TarCamera = Camera.main;
            node.index = 2;
            node.CallBack = delegate ()
            {
                EnterButton_onClick(null);
            };
            GuideManager.getInstance().addGuideNode("UIUnitSelectEnterBtn", node);
        }
        //else if (GuideManager.getInstance().guideIndex == 6)
        else if (GuideManager.getInstance().CurrentGuideID() == 5)
        {
            Action moveUp = null;
            Action moveDown = null;
            moveUp = delegate ()
            {
                LeanTween.moveLocalY(Finger.gameObject, -50, 1).setOnComplete(delegate ()
                {
                    Finger.position = new Vector3(fingerTag.position.x, fingerTag.position.y, fingerTag.position.z);
                    moveUp();
                });
            };
            moveDown = delegate ()
            {                
                LeanTween.moveLocalY(Finger.gameObject, -300, 1).setOnComplete(delegate ()
                {
                    //為什麼這個坐標這樣設，我也不知道
                    Finger.position = new Vector3(fingerTag.position.x, fingerTag.position.y + 2, fingerTag.position.z);
                    moveDown();
                });
            };

            GuideNode node1 = new GuideNode();
            node1.TargetNode = null;
            node1.TarCamera = Camera.main;
            node1.index = 2;
            node1.ShowMask = false;
            node1.ExecuteCallBack = false;
            node1.CallBackFirst = delegate ()
            {
                //手指;
                Finger.GetComponent<Animator>().SetTrigger("Slide");
                Finger.gameObject.SetActive(true);
                Mask.gameObject.SetActive(true);
                if (fingerDir)
                    moveDown();
                else
                    moveUp();
            };
            node1.CallBack = delegate ()
            {

            };
            GuideManager.getInstance().addGuideNode("UIUnitSelectOther", node1);
        }

    }
    private void OnItemSelected(UnitListItem item)
    {
        Finger.gameObject.SetActive(false);
        Mask.gameObject.SetActive(false);
    }

    private void removeFliter_v1()
    {

        GuideManager.getInstance().removeGuideNode("UIUnitSelectEnterBtn");
        GuideManager.getInstance().removeGuideNode("UIUnitSelectOther");

    }


}
