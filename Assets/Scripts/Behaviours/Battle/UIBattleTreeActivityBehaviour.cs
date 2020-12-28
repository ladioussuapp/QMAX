
using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.counterpart;
using Com4Love.Qmax.Net.Protocols.tree;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleTreeActivityBehaviour : MonoBehaviour
{
    struct Award
    {
        public int UpgradeA;
        public int UpgradeB;
        public int Gem;
    }

    StageConfig stage;
    Award award;
    TreeFightCtr fightCtr;
    Animator animator;

    //public Image[] AwardImgs;
    public Text[] AwardTexts;       //左上角的獎勵text
    public TreeActivityHpProgressBar enemyProgressBar;
    public Text TimeLeftText;       //右邊倒計時的text
    public Text[] AwardPanelTexts;      //結算面板裡的text
    public event Action<object> OnClickPauseButton;
    public EffectSpawn ProgressChangeEffect;
    public EffectSpawn HitEffect1;
    public EffectSpawn LeavesEffectSpawn;
    public UIButtonBehaviour PauseButton;
    public TreeBattleTipTimeEffect TipTimeEffect;
    public TreeActivityAwardPanel awardPanel;
    //時間快到的提示
    public Image FlashImg;
    BaseStateMachineBehaviour stateMachine;

    /// <summary>
    /// 增加獎勵 不應該被外部直接調用
    /// 
    /// 1連線過程中，連線數量超過3會有連線獎勵。
    /// 2傷害值每累積到一定得程度會有獎勵
    /// 3戰鬥結束後根據總傷害獲得獎勵
    /// </summary>
    void AddAward(int upgradeA = 0, int upgradeB = 0, int gem = 0)
    {
        int upgradeANew = 0;
        int upgradeBNew = 0;
        int gemNew = 0;

        //不能立刻與當前的獎勵同步
        if (upgradeA != 0)
        {
            upgradeANew = award.UpgradeA + upgradeA;
            GameController.Instance.EffectProxy.ScrollText(AwardTexts[0], award.UpgradeA, upgradeANew);
            award.UpgradeA = upgradeANew;
            animator.SetTrigger("AwardUpgradeATrigger");

            //AwardTexts[0].text = award.UpgradeA.ToString();
        }

        if (upgradeB != 0)
        {
            upgradeBNew = award.UpgradeB + upgradeB;
            GameController.Instance.EffectProxy.ScrollText(AwardTexts[1], award.UpgradeB, upgradeBNew);

            award.UpgradeB = upgradeBNew;
            animator.SetTrigger("AwardUpgradeBTrigger");

            //AwardTexts[1].text = award.UpgradeB.ToString();
        }

        if (gem != 0)
        {
            gemNew = award.Gem + upgradeB;
            GameController.Instance.EffectProxy.ScrollText(AwardTexts[2], award.Gem, gemNew);
            award.Gem = gemNew;
            animator.SetTrigger("AwardGemTrigger");

            //AwardTexts[2].text = award.Gem.ToString();
        }

        //Q.Log("view層addaward顯示獎勵:" + upgradeANew + "|" + upgradeBNew + "|" + gemNew);
        //Q.Log("view層顯示獎勵:" + award.UpgradeA + "|" + award.UpgradeB + "|" + award.Gem);
        //Q.Log("數據層獎勵:" + fightCtr.TreeFightData.AwardData.UpgradeA + "|" + fightCtr.TreeFightData.AwardData.UpgradeB + "|" + fightCtr.TreeFightData.AwardData.Gem);
    }

    /// <summary>
    /// 恢復大樹的血條的血量 
    /// </summary>
    /// <param name="atk"></param>
    public void HpBackup()
    {
        fightCtr.TreeFightData.LoseHpInGroupPreView = 0;
        enemyProgressBar.PreViewBack();
        SetProgressVal();
    }

    public void SetCamera(Camera camera)
    {
        Canvas[] canvas = GetComponentsInChildren<Canvas>();

        for (int i = 0; i < canvas.Length; i++)
        {
            canvas[i].worldCamera = camera;
        }
    }
 
    // Use this for initialization
    void Start()
    {
        fightCtr = GameController.Instance.TreeFightCtr;

        Award award = new Award();
        award.Gem = award.UpgradeA = award.UpgradeB = 0;
        animator = GetComponent<Animator>();
        stateMachine = animator.GetBehaviour<BaseStateMachineBehaviour>();

        enemyProgressBar.gameObject.SetActive(false);   //先不顯示進度條
        AwardTexts[0].text = AwardTexts[1].text = AwardTexts[2].text = "0";
        AwardPanelTexts[0].text = AwardPanelTexts[1].text = AwardPanelTexts[2].text = "0";

        int timeLeft = Mathf.FloorToInt((float)fightCtr.TreeFightData.TimeLeft);
        TimeLeftText.text = timeLeft.ToString();

        GameController.Instance.ModelEventSystem.OnTreeFightBegin += OnTreeFightBegin;
        GameController.Instance.ModelEventSystem.OnTreeFightComplete += OnTreeFightComplete;
        GameController.Instance.ModelEventSystem.OnTreeFightHPAward += OnTreeFightHPAward;
        GameController.Instance.ModelEventSystem.OnTreeFightLineAward += OnTreeFightLineAward;
        GameController.Instance.ModelEventSystem.OnTreeFightDamaged += OnTreeFightDamaged;
        GameController.Instance.ModelEventSystem.OnTreeFightTimeTick += OnTreeFightTimeTick;
        GameController.Instance.ModelEventSystem.OnTreeFightHPConfigChange += OnTreeFightHPConfigChange;
        GameController.Instance.ModelEventSystem.OnTreeFightDamageAward += OnTreeFightDamageAward;
        GameController.Instance.ViewEventSystem.BattleFirstMoveCompleteEvent += OnBattleFirstMoveComplete;
        PauseButton.onClick += PauseButton_OnClickPauseButton;
        GameController.Instance.Popup.OnOpenComplete += Popup_OnOpenComplete;
        GameController.Instance.Popup.OnCloseComplete += Popup_OnCloseComplete;
        //GameController.Instance.ModelEventSystem.OnTreeFightSubmitRequest += OnTreeFightSubmitRequest;            //改为活动关卡提交回调
        GameController.Instance.ModelEventSystem.OnSubmitCounterpart += OnTreeFightSubmitRequest;

        enemyProgressBar.OnAwardFlyIn += enemyProgressBar_OnAwardFlyIn;
        GameController.Instance.ModelEventSystem.OnTreeFightPause += OnTreeFightPause;

        stateMachine.StateExitEvent += StateMachine_StateExitEvent;
    }

    private void StateMachine_StateExitEvent(Animator arg1, AnimatorStateInfo stateInfo, int layer)
    {
        if (stateInfo.IsName("Start"))
        {
            //開始的ReadyGo 播放完成
            if (GameController.Instance.ViewEventSystem.BattleInitAnimCompleteEvent != null)
            {
                GameController.Instance.ViewEventSystem.BattleInitAnimCompleteEvent(null);
            }

            fightCtr.StageBegin();
        }
    }

    void OnDestroy()
    {
        fightCtr.Clear();

        GameController.Instance.ModelEventSystem.OnTreeFightBegin -= OnTreeFightBegin;
        GameController.Instance.ModelEventSystem.OnTreeFightComplete -= OnTreeFightComplete;
        GameController.Instance.ModelEventSystem.OnTreeFightHPAward -= OnTreeFightHPAward;
        GameController.Instance.ModelEventSystem.OnTreeFightLineAward -= OnTreeFightLineAward;
        GameController.Instance.ModelEventSystem.OnTreeFightDamaged -= OnTreeFightDamaged;
        GameController.Instance.ModelEventSystem.OnTreeFightTimeTick -= OnTreeFightTimeTick;
        GameController.Instance.ModelEventSystem.OnTreeFightHPConfigChange -= OnTreeFightHPConfigChange;
        GameController.Instance.ModelEventSystem.OnTreeFightDamageAward -= OnTreeFightDamageAward;
        GameController.Instance.ViewEventSystem.BattleFirstMoveCompleteEvent -= OnBattleFirstMoveComplete;
        PauseButton.onClick -= PauseButton_OnClickPauseButton;
        GameController.Instance.Popup.OnOpenComplete -= Popup_OnOpenComplete;
        GameController.Instance.Popup.OnCloseComplete -= Popup_OnCloseComplete;
        //GameController.Instance.ModelEventSystem.OnTreeFightSubmitRequest -= OnTreeFightSubmitRequest;                
        GameController.Instance.ModelEventSystem.OnSubmitCounterpart -= OnTreeFightSubmitRequest;

        enemyProgressBar.OnAwardFlyIn -= enemyProgressBar_OnAwardFlyIn;
        GameController.Instance.ModelEventSystem.OnTreeFightPause -= OnTreeFightPause;

        stateMachine.StateExitEvent -= StateMachine_StateExitEvent;
    }

    void enemyProgressBar_OnAwardFlyIn(int id, int award)
    {
        if (id == 0)
        {
            AddAward(award, 0, 0);
        }
        else if (id == 1)
        {
            AddAward(0, award, 0);
        }
        else if (id == 2)
        {
            AddAward(0, 0, award);
        }
    }


    void OnTreeFightPause()
    {
        StopFlashEffect();
    }

    void OnTreeFightSubmitRequest(SubmitCounterpartResponse res)
    {
        
        OpenUIWin(res);
    }

    //自己打開勝利窗口
    void OpenUIWin(SubmitCounterpartResponse res)
    {
        ActorGameResponse player = GameController.Instance.PlayerCtr.PlayerData;

        UIWinBehaviour winBeh = GameController.Instance.Popup.Open(PopupID.UIWin, null, false).GetComponent<UIWinBehaviour>();
        winBeh.OnClickOKButton += OnClickResultBackButton;

        StageConfig sConfig = GameController.Instance.Model.StageConfigs[fightCtr.TreeFightData.cutStageId];
 
        int starNum = res.stage.star;

        int gem = 0;
        int key = 0;
        int upgradeA = 0;
        int upgradeB = 0;
        int coin = 0;

        for (int i = 0; i < res.valueResultResponse.list.Count; i++)
        {
            ValueResult vr = res.valueResultResponse.list[i];

            if (vr.changeType == 0)
            {
                continue;
            }

            switch (vr.valuesType)
            {
                case (int)RewardType.Gem:
                    gem = vr.changeValue;
                    break;
                case (int)RewardType.Key:
                    key = vr.changeValue;
                    break;
                case (int)RewardType.UpgradeA:
                    upgradeA = vr.changeValue;
                    break;
                case (int)RewardType.UpgradeB:
                    upgradeB = vr.changeValue;
                    break;
                case (int)RewardType.Coin:
                    coin = vr.changeValue;
                    break;
                default:
                    break;
            }
        }

        winBeh.SetCollected(gem, key, upgradeA, upgradeB , coin);

        //for test end
        winBeh.SetCrtStatus(
            player.energy, player.energyMax,
            player.key, player.gem,
            player.upgradeA, player.upgradeB , coin);

        winBeh.SetStageInfo(0, Utils.GetTextByStringID(sConfig.NameStringID), starNum);
    }

    void OnClickResultBackButton(object sender)
    {
        GameController.Instance.SceneCtr.LoadLevel(Scenes.MapScene, null, true, true);
    }

    void Popup_OnCloseComplete(PopupID obj)
    {
        if (obj == PopupID.UIPause)
        {
            //暫停界面被關閉後繼續倒計時
            fightCtr.GoOn();
        }
    }

    void Popup_OnOpenComplete(PopupID obj)
    {
        if (obj == PopupID.UIPause)
        {
            //暫停界面被打開後停止倒計時
            fightCtr.Pause();
        }
    }

    void PauseButton_OnClickPauseButton(object obj)
    {
        if (OnClickPauseButton != null)
        {
            OnClickPauseButton(obj);
        }
    }

    void OnTreeFightTimeTick()
    {
        //倒計時
        int timeLeft = Mathf.RoundToInt((float)fightCtr.TreeFightData.TimeLeft);
        TimeLeftText.text = timeLeft.ToString();
        animator.SetTrigger("TimeLeftTrigger");

        if (timeLeft <= 5)
        {
            //5秒開始出倒計時TIP
            TipTimeEffect.SetVal(timeLeft);
            animator.SetTrigger("TipTimeLeftTrigger");

            StartFlashEffect();
        }
    }

    //戰鬥準備好了 出現進度條，出現Ready go 動畫
    void OnBattleFirstMoveComplete()
    {
        animator.SetTrigger("ReadyTrigger");
    }

    void OnTreeFightBegin()
    {
        enemyProgressBar.gameObject.SetActive(true);
        enemyProgressBar.InitMaxHp(fightCtr.TreeFightData.MaxHp);
    }

    void OnTreeFightDamageAward(int upgradeA, int upgradeB, int gem)
    {
        //傷害值獎勵 戰鬥完成之後才會給 此事件之後為OnTreeFightComplete事件
        awardPanel.SetAward(upgradeA, upgradeB, gem);
    }

    void OnTreeFightComplete(ModelEventSystem.BattleResultEventArgs args)
    {
        GameController.Instance.ModelEventSystem.OnTreeFightTimeTick -= OnTreeFightTimeTick;
        animator.SetTrigger("CompleteTrigger");
        TimeLeftText.text = "0";

        //Q.Log("view層顯示獎勵:" + award.UpgradeA + "|" + award.UpgradeB + "|" + award.Gem);
        //Q.Log("數據層獎勵:" + fightCtr.TreeFightData.AwardData.UpgradeA + "|" + fightCtr.TreeFightData.AwardData.UpgradeB + "|" + fightCtr.TreeFightData.AwardData.Gem);
    }


    void OnTreeFightHPAward(TimeLimitedHPConfig hpConfig)
    {
        //HP獎勵通知
        //血條圖標飛入左上角後再控制動畫 暫時直接加上測試
        //AddAward(hpConfig.UpgradeA, hpConfig.UpgradeB, 0);
        enemyProgressBar.HpReward(hpConfig, fightCtr.CutHpConfigIndex);
        HitEffect1.Spawn();
    }

    void OnTreeFightLineAward(int upgradeA, int upgradeB)
    {
        //連線獎勵 棋盤出往上飛動畫
        AddAward(upgradeA, upgradeB, 0);
    }

    void OnTreeFightDamaged(int newDamage, bool preCare)
    {
        //大樹受到傷害
        if (preCare)
        {
            SetProgressValPreView(newDamage);   
        }
        else
        {
            //大樹受擊就播放。。。
            LeavesEffectSpawn.Spawn();
            SetProgressVal();
        }
    }

    //3個配置切換 就是3管血被打光 進度條需要切換 播放切換進度條特效
    void OnTreeFightHPConfigChange()
    {
        int[] maxHps = new int[3];
        int lockedIndex = -1;

        for (int i = 0; i < 3; i++)
        {
            TimeLimitedHPConfig hpConfig = fightCtr.NextHPConfigs[i];

            if (hpConfig != null)
            {
                maxHps[i] = hpConfig.Hp;

                if (hpConfig.Level > fightCtr.TreeFightData.MaxLevel)
                {
                    //等级限制  鎖定
                    lockedIndex = i;
                    break;
                }
            }
            else
            {
                //到最後一個了 需要鎖定
                lockedIndex = i;
                maxHps[i] = 0;
                break;
            }
        }

        enemyProgressBar.Ref(maxHps[0], maxHps[1], maxHps[2]);
        enemyProgressBar.Lock(lockedIndex);
        SetProgressVal();
        ProgressChangeEffect.Spawn();
    }

    //暫時用作真實攻擊          
    void SetProgressVal()
    {
        enemyProgressBar.SetValHp(fightCtr.TreeFightData.LoseHpInGroup, fightCtr.CutGroupHpTotal, fightCtr.TreeFightData.DamageTotal, fightCtr.CutHPConfig.hpTotal);
    }

    /// <summary>
    /// LoseHpInGroupPreView 為當前整條血的傷害值，當刷新到新的三個配置時，會置0
    /// </summary>
    /// <param name="newDamage">當前傷害值</param>
    void SetProgressValPreView(int newDamage)
    {
        int loseHpInGroup = fightCtr.TreeFightData.LoseHpInGroupPreView;
        enemyProgressBar.SetValPreview(loseHpInGroup, newDamage);
    }
 
    void StartFlashEffect()
    {
        if (LeanTween.isTweening(FlashImg.gameObject))
        {
            return;   
        }

        LeanTween.value(FlashImg.gameObject, 0f, 1f, .4f).setLoopType(LeanTweenType.pingPong).setOnUpdate(delegate(float val) {
            FlashImg.color = new Color(FlashImg.color.r, FlashImg.color.g, FlashImg.color.b, val);
        });
    }

    void StopFlashEffect()
    {
        LeanTween.cancel(FlashImg.gameObject);
        //FlashImg.gameObject.SetActive(false);
    }

    //最後出現的傷害值獎勵面板效果
    IEnumerator DemagePanelEffect()
    {
        StopFlashEffect();

        animator.SetTrigger("CompleteTrigger");
        yield return new WaitForSeconds(.5f);

        bool waitForFly = false;
 
        if (awardPanel.upgradeA > 0)
        {
            animator.SetTrigger("DemagePlaneTrigger1");
            waitForFly = true;
        }

        if (awardPanel.upgradeB > 0)
        {
            animator.SetTrigger("DemagePlaneTrigger2");
            waitForFly = true;
        }

        if (awardPanel.gem > 0)
        {
            animator.SetTrigger("DemagePlaneTrigger3");
            waitForFly = true;
        }

        if (waitForFly)
        {
            yield return new WaitForSeconds(1f);
        }

        yield return StartCoroutine(awardPanel.ScrollText());
        yield return new WaitForSeconds(1f);
        //戰鬥獎勵播放到最後一幀
        fightCtr.SubmitFightRequest();
    }

    #region 幀尾事件 待改成動畫狀態機
    ///幀尾事件 待改成動畫狀態機
    void OnAnimationDemagePlaneFrameEnd1()
    {
        AddAward(awardPanel.upgradeA, 0, 0);
    }

    void OnAnimationDemagePlaneFrameEnd2()
    {
        AddAward(0, awardPanel.upgradeB, 0);
    }

    void OnAnimationDemagePlaneFrameEnd3()
    {
        AddAward(0, 0, awardPanel.gem);
    }

    //傷害值面板動畫播放完成
    void OnAnimationCompleteFrameEnd()
    {
        StartCoroutine(DemagePanelEffect());
    }

    void OnAnimationTipTimeFrameStart()
    {
        //倒計時切換紋理
        //目前不需要調用
    }
    #endregion

    ////用來測試戰鬥流程
    //IEnumerator Test()
    //{
    //    bool fight = true;

    //    while (fight)
    //    {
    //        fightCtr.AtkDamage(250, true);
    //        yield return new WaitForSeconds(.3f);
    //        fightCtr.AtkDamage(250, false);
    //        yield return new WaitForSeconds(.3f);
    //    }
    //}
}

