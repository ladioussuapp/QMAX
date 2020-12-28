using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.goods;

public class UIGetChanceAnimationBehaviour : MonoBehaviour {
    public TopInfoView InfoView;
    public TmpPlayerData Data;

    public class TmpPlayerData
    {
        public TmpPlayerData(int _UpgradeA, int _UpgradeB, int _Gem , int _Coin)
        {
            UpgradeA = _UpgradeA;
            UpgradeB = _UpgradeB;
            Gem = _Gem;
            Coin = _Coin;
        }

        public int UpgradeA;
        public int UpgradeB;
        public int Gem;
        public int Coin;
    }

    public void init(ActorGameResponse playerData)
    {
        Data = new TmpPlayerData(playerData.upgradeA, playerData.upgradeB, playerData.gem , playerData.coin);
    }

    public void Upgrow(int _UpgradeA = 0, int _UpgradeB = 0, int _Gem = 0 , int _Coin = 0)
    {
        Data.UpgradeA += _UpgradeA;
        Data.UpgradeB += _UpgradeB;
        Data.Gem += _Gem;
        Data.Coin += _Coin;
    }

	// Use this for initialization
	void Start () {
        InfoView.AutoUpdate = false;
        GameController.Instance.ModelEventSystem.OnBuyKeys += ModelEventSystem_OnBuyKeys;
        GameController.Instance.ModelEventSystem.OnEnergyTimeGrowEvent += ModelEventSystem_OnEnergyTimeGrowEvent;
        GameController.Instance.ModelEventSystem.OnBuyGems += OnBuyGems;
        GameController.Instance.ModelEventSystem.OnBuyGoods += OnBuyGoods;
	}

    void OnBuyGoods(int num , GoodsItem ite)
    {
        InfoView.UpdateUi(true);
    }

    void OnBuyGems(int num)
    {
        InfoView.UpdateUi(true);
    }

    void ModelEventSystem_OnEnergyTimeGrowEvent()
    {
        //體力自動增長 刷新
        InfoView.UpdateEnergyText();
    }

    void ModelEventSystem_OnBuyKeys(Com4Love.Qmax.Net.Protocols.ValueResultListResponse obj)
    {
        Data.Gem = GameController.Instance.PlayerCtr.PlayerData.gem;
        //購買了鑰匙 鑽石會減少 刷新鑽石
        InfoView.UpdateGemText(Data.Gem, true);
    }

    public void OnDestroy()
    {
        GameController.Instance.ModelEventSystem.OnBuyKeys -= ModelEventSystem_OnBuyKeys;
        GameController.Instance.ModelEventSystem.OnEnergyTimeGrowEvent -= ModelEventSystem_OnEnergyTimeGrowEvent;
        GameController.Instance.ModelEventSystem.OnBuyGems -= OnBuyGems;
        GameController.Instance.ModelEventSystem.OnBuyGoods -= OnBuyGoods;
    }

    /// <summary>
    /// 動畫播放事件 由動畫事件觸發
    /// </summary>
	public void OnAnimationAni01Start()
    {
        //橘子
        InfoView.UpdateUiUpgradeAText(Data.UpgradeA, true);
    }

    public void OnAnimationAni02Start()
    {
        //桃子
        InfoView.UpdateUiUpgradeBText(Data.UpgradeB, true);
    }

    public void OnAnimationAni03Start()
    {
        //鑽石
        InfoView.UpdateGemText(Data.Gem,true);
    }

    public void OnAnimationAni04Start()
    {
        //體力
        InfoView.UpdateEnergyText();
    }

    public void OnAnimationAni05Start()
    {
        //金幣
        InfoView.UpdateCoinText(Data.Coin, true);
    }
}
