using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using System;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Net;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Tools;
using Com4Love.Qmax.Data.Config;

public class UIPowerShopBehaviour : MonoBehaviour
{
    public UIButtonBehaviour ButtonClose;
    public UIButtonBehaviour ButtonBuyPower;
    public UIButtonBehaviour ButtonBuyMax;

    public GameObject TimeTips;
    public Text PowerCount;
    public GameObject PowerCountImage;
    public Animator BuyTipsAnimator;
    public Animator EffectAnimator;
    public Slider progress;
    public Transform ProgressEffect;

    //购买体力之后的文字提示
    public Text BuyTipsText;
    public Text HasTipsText;

    /// <summary>
    /// 剩余秒数
    /// </summary>
    private double leftSeconds;
    private PlayerCtr playerCtr;

    public Text m_fullEnergyCost;
    public Text m_incrEnergyMaxCost;

    private int fullEneCost;
    private int incrMaxEnergyCost;

    public void Awake()
    {
        ProgressEffect.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start()
    {
        playerCtr = GameController.Instance.PlayerCtr;
        leftSeconds = playerCtr.EnergyLeftTime;
        progress.enabled = false;
        progress.minValue = -0.158f;
        fullEneCost = playerCtr.Model.GameSystemConfig.ShopEnergyAddFull;
        string incMaxEnergyCost = playerCtr.Model.GameSystemConfig.shopEnergyMaxUp;
        string[] strArr = incMaxEnergyCost.Split(',');

        if (strArr.Length >= 2)
        {
            incrMaxEnergyCost = int.Parse(strArr[1]);
        }

        m_fullEnergyCost.text = fullEneCost.ToString();
        m_incrEnergyMaxCost.text = incrMaxEnergyCost.ToString();
        UpdateEnergyPorgress(false);
        AddEvent();
        UpView(1);
        // 能量低于二点时，播放台词音效
        if (GameController.Instance.PlayerCtr.PlayerData.energy < 2)
        {
            GameController.Instance.AudioManager.PlayAudio("Vo_accompany_10");
        }

    }


    void UpdateEnergyPorgress(bool tween = true)
    {
        float value = (playerCtr.PlayerData.energy) / (float)playerCtr.PlayerData.energyMax;

        if (value != progress.value)
        {
            if (tween)
            {
                ProgressEffect.gameObject.SetActive(true);

                LeanTween.value(progress.gameObject, progress.value, value, .3f).setOnUpdate(delegate(float val)
                {
                    progress.value = val;
                }).setOnComplete(delegate()
                {
                    ProgressEffect.gameObject.SetActive(false);
                });
            }
            else
            {
                progress.value = value;
            }
        }
    }

    void UpTimeTips(double duration)
    {
        if (TimeTips == null)
            return;

        if (GameController.Instance.PlayerCtr.PlayerData.energy < GameController.Instance.PlayerCtr.PlayerData.energyMax)
        {
            Text TextTime = TimeTips.transform.Find("Text_Time").GetComponent<Text>();
            if (!TimeTips.activeInHierarchy)
            {
                TimeTips.SetActive(true);
            }

            int sec = (int)Math.Floor(leftSeconds);
            int minute = sec / 60;
            int seconds = sec % 60;
            string timeFormat = (minute < 10 ? "0" : "") + minute.ToString() + ":" + (seconds < 10 ? "0" : "") + seconds.ToString();
            TextTime.text = Utils.GetText("将在{0}秒后恢复", timeFormat);
            if (leftSeconds <= 0)
            {
                TimeTips.SetActive(false);
                UpdateEnergyPorgress();
                leftSeconds = playerCtr.EnergyLeftTime;
            }
            leftSeconds -= duration;
        }
        else
        {
            if (TimeTips.activeInHierarchy)
            {
                UpdateEnergyPorgress();
                TimeTips.SetActive(false);
            }
        }
    }

    void UpPowerCount()
    {
        int energy = GameController.Instance.PlayerCtr.PlayerData.energy;
        int energyMax = GameController.Instance.PlayerCtr.PlayerData.energyMax;
        StageConfig nextStage = GameController.Instance.StageCtr.GetNextStage();
        nextStage = nextStage == null ? GameController.Instance.StageCtr.GetStage(GameController.Instance.PlayerCtr.PlayerData.passStageId) : nextStage;
        int energyNeed = nextStage == null ? energyMax : nextStage.CostEnergy;

        //如果体力值小于下一关卡的需要体力则显示成红色
        if (energy == -1 && energyMax == -1)
        {
            PowerCount.text = string.Format("{0} / {1}", "∞", "∞");
        }
        else
        {
            if (energy < energyNeed)
            {
                PowerCount.text = RichtextUtil.ColorText(energy.ToString(), 0xff0000) + "/" + energyMax.ToString();
            }
            else
            {
                PowerCount.text = energy.ToString() + "/" + energyMax.ToString();
            }
        }

        PowerCountImage.GetComponent<RectTransform>().sizeDelta.Set(Math.Max(72, Math.Min(440, 72 + (440 - 72) * (GameController.Instance.PlayerCtr.PlayerData.energy / GameController.Instance.PlayerCtr.PlayerData.energyMax))), 90);
    }

    void UpButtonEnabled()
    {
        //ButtonBuyPower.interactable = GameController.Instance.PlayerCtr.PlayerData.gem >= fullEneCost && GameController.Instance.PlayerCtr.PlayerData.energy < GameController.Instance.PlayerCtr.PlayerData.energyMax;
        //ButtonBuyMax.interactable = GameController.Instance.PlayerCtr.PlayerData.gem >= incrMaxEnergyCost;

        int maxNum = GameController.Instance.Model.PlayerData.energyMax;
        int configNum = GameController.Instance.Model.GameSystemConfig.gemMaxEnergy;
        int gem = GameController.Instance.PlayerCtr.PlayerData.gem;

        if (playerCtr.JudgeNoLimitEnergy())
        {
            ButtonBuyPower.interactable = false;
            ButtonBuyMax.interactable = false;
        }
        else
        {
            ButtonBuyMax.interactable = (maxNum < configNum && gem >= incrMaxEnergyCost);
            ButtonBuyPower.interactable = GameController.Instance.PlayerCtr.PlayerData.energy < GameController.Instance.PlayerCtr.PlayerData.energyMax;
        }
    }

    void AddEvent()
    {
        
        ButtonClose.onClick += OnClose;
        ButtonBuyPower.onClick += OnBuyPower;
        ButtonBuyMax.onClick += OnBuyMax;
        GameController.Instance.ModelEventSystem.OnBuyEnergyEvent += OnBuyEnergyEvent;
        //StartCoroutine(AutoUpdateView());
        //InvokeRepeating("UpView", 0f, 1f);
        //InvokeRepeating("UpView", 0.5f, 1.0f);  
        GameController.Instance.ServerTime.AddTimeTick(UpView);
    }

    void RemoveEvent()
    {
        ButtonClose.onClick -= OnClose;
        ButtonBuyPower.onClick -= OnBuyPower;
        ButtonBuyMax.onClick -= OnBuyMax;
        GameController.Instance.ModelEventSystem.OnBuyEnergyEvent -= OnBuyEnergyEvent;
        //CancelInvoke("UpView");
        GameController.Instance.ServerTime.RemoveTimeTick(UpView);
    }

    IEnumerator AutoUpdateView()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            UpView(1);
        }
    }

    void UpView(double duration)
    {
        UpButtonEnabled();
        UpPowerCount();
        UpTimeTips(duration);
    }

    void OnBuyEnergyEvent(bool isMaxUp)
    {
        //购买体力返回
        BuyTipsAnimator.Play("BuyTips");
        EffectAnimator.Play("PowerDuang");

        //改为动画
        UpView(1);
        UpdateEnergyPorgress();

        if (isMaxUp)
        {
            BuyTipsText.text = Utils.GetTextByID(1560);
        }
        else
        {
            BuyTipsText.text = Utils.GetTextByID(1552);
        }
    }


    private void OnBuyMax(UIButtonBehaviour button)
    {
        GameController.Instance.Client.BuyMaxEnergy(1);
    }

    private void OnBuyPower(UIButtonBehaviour button)
    {
        if (GameController.Instance.PlayerCtr.PlayerData.gem >= fullEneCost)
            GameController.Instance.Client.BuyEnergy(0);
        else if(!GameController.Instance.Popup.IsPopup(PopupID.UIShop))
            UIShop.Open(UIShop.TapIndex.GEM_INDEX);

    }

    private void OnClose(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Close(PopupID.UIPowerShop);
    }

    void OnDestroy()
    {
        RemoveEvent();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
