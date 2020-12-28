using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax;
using System;

/// <summary>
/// 玩家的基本信息顯示 體力 金幣 倒計時
/// </summary>
public class TopInfoView : MonoBehaviour
{
    public Text EnergyText;
    public Text GemText;
    public Text EnergyTimeText;
    public Text FreeTimeText;
    public Text UpgradAText;
    public Text UpgradBText;
    public Text KeyText;
    public Text CoinText;

    public Image GemIcon;
    public Image EnergyIcon;
    public Image CoinIcon;
    public Image UpgradeAIcon;
    public Image UpgradeBIcon;

    public bool isTweenText = false;

    protected PlayerCtr playerCtr;
    //private int currentEnergy;
    private double leftRecoveryTime;
    private int leftFreeTime;

    public UIButtonBehaviour GemButton;
    public UIButtonBehaviour EnergyButton;
    public UIButtonBehaviour CoinButton;
    private bool autoUpdate = true;

    public bool AutoUpdate
    {
        get
        {
            return autoUpdate;
        }
        set
        {
            autoUpdate = value;

            if (!autoUpdate)
            {
                RemovePlayerInfoRefEvent();
            }
        }
    }

    public void Start()
    {
        playerCtr = GameController.Instance.PlayerCtr;

        if (playerCtr == null)
        {
            return;
        }

        if (playerCtr.PlayerData != null)
        {
            // currentEnergy = playerCtr.PlayerData.energy;
            leftRecoveryTime = playerCtr.EnergyLeftTime;
            UpdateUi(false);
        }

        if (EnergyTimeText != null)
        {
            EnergyTimeText.gameObject.SetActive(false);
            GameController.Instance.ServerTime.AddTimeTick(EnergyTick);
        }

        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent += OnRechargeRefresh;

        if (FreeTimeText != null)
        {
            FreeTimeText.gameObject.SetActive(false);
            if (GameController.Instance.PlayerCtr.FreeLeftTime >= 0)
            {
                GameController.Instance.ServerTime.AddTimeTick(FreeTick);
            }
        }



        if (GemButton != null)
        {
            GemButton.onClick += GemButton_onClick;
        }

        if (EnergyButton != null)
        {
            EnergyButton.onClick += EnergyButton_onClick;
        }

        if (CoinButton != null)
        {
            CoinButton.onClick += CoinButton_onClick;
        }

        if (autoUpdate)
        {
            AddPlayerInfoRefEvent();
        }
    }

    private void OnRechargeRefresh()
    {
        StartCoroutine(Utils.DelayToInvokeDo(
                    delegate()
                    {
                        UpdateUi(false);
                    }, 0.1f
                ));
    }


    public void AddPlayerInfoRefEvent()
    {
        RemovePlayerInfoRefEvent();
        GameController.Instance.ModelEventSystem.OnPlayerInfoRef += ModelEventSystem_OnPlayerInfoRef;
    }

    /// <summary>
    /// 外部調用此方法會移除掉自動刷新功能
    /// </summary>
    public void RemovePlayerInfoRefEvent()
    {
        GameController.Instance.ModelEventSystem.OnPlayerInfoRef -= ModelEventSystem_OnPlayerInfoRef;
    }

    void EnergyButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Open(PopupID.UIPowerShop, null, true, true);
    }

    void GemButton_onClick(UIButtonBehaviour button)
    {
        UIShop.Open(UIShop.TapIndex.GEM_INDEX);
    }


    private void CoinButton_onClick(UIButtonBehaviour button)
    {
        UIShop.Open(UIShop.TapIndex.COIN_INDEX);
    }

    public void OnDestroy()
    {
        RemovePlayerInfoRefEvent();
        //CancelInvoke("EnergyTick");
        GameController.Instance.ServerTime.RemoveTimeTick(EnergyTick);
        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent -= OnRechargeRefresh;
        if (GemButton != null)
        {
            GemButton.onClick -= GemButton_onClick;
        }

        if (EnergyButton != null)
        {
            EnergyButton.onClick -= EnergyButton_onClick;
        }

        if (CoinButton != null)
        {
            CoinButton.onClick -= CoinButton_onClick;
        }
    }

    void ModelEventSystem_OnPlayerInfoRef(System.Collections.Generic.List<RewardType> arg1, ActorGameResponse arg2)
    {
        UpdateUi(true);
    }

    protected void EnergyTick(double duration)
    {
        leftRecoveryTime = playerCtr.EnergyLeftTime;
        if (playerCtr != null && playerCtr.PlayerData == null || playerCtr.PlayerData.energy >= playerCtr.PlayerData.energyMax)
        {
            EnergyTimeText.gameObject.SetActive(false);
        }
        else
        {
            EnergyTimeText.gameObject.SetActive(true);

            //int seconds = (int)Utils.DateTimeToUnixTime(DateTime.UtcNow) - GameController.Instance.PlayerCtr.PlayerData.fixEnergyTime;
            if (!EnergyTimeText.gameObject.activeInHierarchy)
            {
                EnergyTimeText.gameObject.SetActive(true);
            }

            if (leftRecoveryTime <= 0)
            {
                leftRecoveryTime = 0;
            }

            int sec = (int)Math.Floor(leftRecoveryTime);
            int minute = sec / 60;
            int seconds = sec % 60;
            EnergyTimeText.text = string.Format("{0}{1}:{2}{3}", (minute < 10 ? "0" : ""), minute, (seconds < 10 ? "0" : ""), seconds);

            if (leftRecoveryTime <= 0)
            {
                UpdateEnergyUi(playerCtr.PlayerData.energy, playerCtr.PlayerData.energyMax);

                if (playerCtr.Model != null)
                {
                    leftRecoveryTime = playerCtr.EnergyLeftTime;
                }

                if (playerCtr.PlayerData.energy >= playerCtr.PlayerData.energyMax)
                {
                    EnergyTimeText.gameObject.SetActive(false);
                }
                else
                {
                    leftRecoveryTime = playerCtr.EnergyLeftTime;
                    EnergyTimeText.gameObject.SetActive(true);
                }
            }

            //leftRecoveryTime--;
        }
    }

    protected void FreeTick(double duration)
    {
        leftFreeTime = GameController.Instance.PlayerCtr.FreeLeftTime;
        if (FreeTimeText != null)
        {
            if (leftFreeTime <= 0)
            {
                FreeTimeText.gameObject.SetActive(false);

                if (leftFreeTime != -1)
                {
                    ///無限體力時間到了要刷新當前體力
                    ///等於-1表示不需要再次請求刷新體力了
                    GameController.Instance.Client.RefreshEnergy();
                    GameController.Instance.PlayerCtr.FreeLeftTime = -1;
                }
            }
            else
            {
                FreeTimeText.gameObject.SetActive(true);

                int hourse = leftFreeTime / 3600;
                int minute = (leftFreeTime % 3600) / 60;
                int seconds = (leftFreeTime % 3600) % 60;

                FreeTimeText.text = string.Format("{0}{1}:{2}{3}:{4}{5}", (hourse < 10 ? "0" : ""), hourse, (minute < 10 ? "0" : ""), minute, (seconds < 10 ? "0" : ""), seconds);
            }
        }
    }

    void UpdateEnergyUi(int energy, int energyMax)
    {
        if (Mathf.Abs(energy - playerCtr.PlayerData.energy) >= 2)
        {
            return;
        }
        if (energy >= energyMax)
        {
            energy = energyMax;
        }

        //currentEnergy = energy;
        if (energyMax == -1)
        {
            EnergyText.fontSize = 128;
            EnergyText.text = string.Format("{0} / {1}", "∞", "∞");

        }
        else
        {
            EnergyText.text = energy + "/" + energyMax;
        }
    }

    public void UpdateUi(bool isTweenText_)
    {
        leftRecoveryTime = playerCtr.EnergyLeftTime;
        UpdateEnergyText();
        UpdateGemText(PlayerData.gem, isTweenText_);
        UpdateCoinText(PlayerData.coin, isTweenText_);
        UpdateUiUpgradeAText(PlayerData.upgradeA, isTweenText_);
        UpdateUiUpgradeBText(PlayerData.upgradeB, isTweenText_);
        UpdateUiUpgradeKeyText();
    }

    public ActorGameResponse PlayerData
    {
        get
        {
            return GameController.Instance.PlayerCtr.PlayerData;
        }
    }

    public void UpdateEnergyText()
    {
        if (EnergyText != null)
        {
            if (PlayerData.energyMax == -1)
            {
                EnergyText.text = string.Format("{0} / {1}", "∞", "∞");
            }
            else
            {
                EnergyText.text = PlayerData.energy + "/" + PlayerData.energyMax;
            }

        }
    }

    public void UpdateCoinText(int coin, bool tween = false)
    {
        if (CoinText == null)
        {
            return;
        }

        if (tween)
        {
            GameController.Instance.EffectProxy.ScrollText(CoinText, int.Parse(CoinText.text), coin, .8f);
        }
        else
        {
            CoinText.text = PlayerData.coin.ToString();
        }
    }

    public void UpdateGemText(int gem, bool tween = false)
    {
        if (GemText != null)
        {
            if (tween)
            {
                GameController.Instance.EffectProxy.ScrollText(GemText, int.Parse(GemText.text), gem, .8f);
            }
            else
            {
                GemText.text = PlayerData.gem.ToString();
            }
        }
    }

    public void UpdateUiUpgradeAText(int upgradeA, bool tween = false)
    {
        if (UpgradAText != null)
        {
            if (tween)
            {
                GameController.Instance.EffectProxy.ScrollText(UpgradAText, int.Parse(UpgradAText.text), upgradeA, .8f);
            }
            else
            {
                UpgradAText.text = PlayerData.upgradeA.ToString();
            }
        }
    }

    public void UpdateUiUpgradeBText(int upgradeB, bool tween = false)
    {
        if (UpgradBText != null)
        {
            if (tween)
            {
                GameController.Instance.EffectProxy.ScrollText(UpgradBText, int.Parse(UpgradBText.text), upgradeB, .8f);
            }
            else
            {
                UpgradBText.text = GameController.Instance.PlayerCtr.PlayerData.upgradeB.ToString();
            }
        }
    }

    public void UpdateUiUpgradeKeyText()
    {
        if (KeyText != null)
        {
            string keyStr = PlayerData.key > 99 ? "N" : PlayerData.key.ToString();

            KeyText.text = keyStr;
        }
    }

}
