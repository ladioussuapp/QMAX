using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using System.Collections.Generic;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.goods;
using Com4Love.Qmax.Tools;
using System.Text.RegularExpressions;

public class UIShop : MonoBehaviour
{
    public UIButtonBehaviour CloseButton;
    public UISheetGroupBehaviour Tab;
    public Transform[] TabContents;      
    [System.NonSerialized]
    public int SelectIndex = 2;         //暫時  默認道具
    public ScrollRect scrollRect;
    public Text CoinText;
    public Text GemText;

    void Start()
    {
        CloseButton.onClick += CloseButton_onClick;
        Tab.OnChange += Tab_OnChange;
        GameController.Instance.ModelEventSystem.OnBuyGoods += OnBuyGoods;
        //購買物品後玩家信息會刷新
        //不直接監聽玩家信息改變信息   需要在動畫中改變信息顯示
        //GameController.Instance.ModelEventSystem.OnPlayerInfoRef += OnPlayerInfoRef;
        Tab.SetSelected(SelectIndex);
        CoinText.text = GameController.Instance.PlayerCtr.PlayerData.coin.ToString();
        GemText.text = GameController.Instance.PlayerCtr.PlayerData.gem.ToString();

        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent += OnRechargeRefresh;
    }

    private void OnRechargeRefresh()
    {
        GameController.Instance.Popup.HideLightLoading();
        RefPlayerInfo();
    }
 
    void RefPlayerInfo()
    {
        int gemOld = int.Parse(GemText.text);
        int coinOld = int.Parse(CoinText.text);

        if (GameController.Instance.PlayerCtr.PlayerData.gem != gemOld)
        {
            GameController.Instance.EffectProxy.ScrollText(GemText, gemOld, GameController.Instance.PlayerCtr.PlayerData.gem);
        }

        if (GameController.Instance.PlayerCtr.PlayerData.coin != coinOld)
        {
            GameController.Instance.EffectProxy.ScrollText(CoinText, coinOld, GameController.Instance.PlayerCtr.PlayerData.coin);
        }
    }
 
    void OnBuyGoods(int buyNum, GoodsItem res)
    {
         RefPlayerInfo();

        if (!GameController.Instance.Model.GoodsConfigs.ContainsKey(res.id))
        {
            return;
        }

        GoodsConfig goodsConfig = GameController.Instance.Model.GoodsConfigs[res.id];
        string goodsName = GameController.Instance.GoodsCtr.GetGoodsNameStr(goodsConfig);
        //int num = res.goodsItem.num;
        string msg = Utils.GetTextByID(20041, goodsName, buyNum);

        //購買物品成功  出提示
        GameController.Instance.Popup.ShowTextFloat(msg, LayerCtrlBehaviour.ActiveLayer.FloatLayer as RectTransform);
        RefPlayerInfo();
    }

    private void Tab_OnChange(UISheetBehaviour sheet)
    {
        SelectIndex = Tab.GetSelectIndex();

        for (int i = 0; i < TabContents.Length; i++)
        {
            TabContents[i].gameObject.SetActive(i == SelectIndex);
        }
 
        scrollRect.content = TabContents[SelectIndex] as RectTransform;
    }

    private void CloseButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Close(PopupID.UIShop);
    }

    public void OnDestroy()
    {
        GameController.Instance.ModelEventSystem.OnBuyGoods -= OnBuyGoods;
        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent -= OnRechargeRefresh;
        CloseButton.onClick -= CloseButton_onClick;
        Tab.OnChange -= Tab_OnChange;
    }
 
    public static void Open(TapIndex index = TapIndex.GOODS_PG_INDEX)
    {
        UIShop uiShop;

        if (GameController.Instance.Popup.IsPopup(PopupID.UIShop))
        {
            //已經彈出   則判斷頁簽是不是一致   如果不一致，則直接切換頁簽
            uiShop = GameController.Instance.Popup.GetPopup(PopupID.UIShop).GetComponent<UIShop>();

            if (uiShop.SelectIndex != (int)index)
            {
                uiShop.Tab.SetSelected((int)index);
            }
        }
        else
        {
            uiShop = GameController.Instance.Popup.Open(PopupID.UIShop, null, true, true).GetComponent<UIShop>();
            uiShop.SelectIndex = (int)index;
        }
    }

    public enum TapIndex
    {
        /// <summary>
        /// 充值  以前的鑽石充值界面
        /// </summary>
        GEM_INDEX,

        /// <summary>
        /// 金幣
        /// </summary>
        COIN_INDEX,

        /// <summary>
        /// 道具
        /// </summary>
        GOODS_INDEX,
        /// <summary>
        /// 套装
        /// </summary>
        GOODS_PG_INDEX
    }
 
}
