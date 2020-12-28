using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using UnityEngine.UI;
using Com4Love.Qmax.Ctr;
using System;
using System.Collections.Generic;

public class UIPropItem : MonoBehaviour {

    public PropType ItemPropType = PropType.None;
    public Image Icon;
    public Image TickImage;
    public Image AddImage;
    public Text NumText;
    public Transform NumTextAndBG;
    public Text PriceNum;
    public Sprite ButtonSelectedSprite;
    public Sprite ButtonNormalSprite;
    public Image ButtonBg;
    public Image PriceIcon;
    public Text NameText;
    public Image FreeImage;
    public GameObject PriceParent;
    [HideInInspector]
    public bool InCombat;
    private PropCtr propCtr;

    [HideInInspector]
    public UIButtonBehaviour BuyButton;

    GoodsConfig.SType _kind = GoodsConfig.SType.None;
    public GoodsConfig.SType Kind
    {
        get {
            if (_kind == GoodsConfig.SType.None)
                _kind = GameController.Instance.PropCtr.GetPropSType((int)ItemPropType);

            return _kind;
        }
    }
    // Use this for initialization
    void Awake()
    {
        propCtr = GameController.Instance.PropCtr;

        BuyButton = gameObject.GetComponent<UIButtonBehaviour>();
        if (BuyButton == null)
        {
            BuyButton = gameObject.AddComponent<UIButtonBehaviour>();
        }

        BuyButton.onClick += OnClickPropButton;
    }
    public virtual void Start () {

        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent += OnRechargeRefresh;
 
        UpdateUI();
    }
	
	public virtual void UpdateUI()
    {

        int Num = propCtr.GetPropNum(ItemPropType);

        SetNumText(Num);

        SetPriceDisplay();

        //Color changecolor = Num > 0 ? Color.white: new Color(1, 1, 1, 0.5f);

        bool isselected = propCtr.GetPropSelect(ItemPropType);

        if (ButtonBg != null)
        {
            //ButtonBg.color = changecolor;
            ButtonBg.sprite = isselected ? ButtonSelectedSprite : ButtonNormalSprite;
        }

        //if(NumText != null)
        //    NumText.color = changecolor;

        //if(Icon!= null)
        //    Icon.color = changecolor;
     
        SetTickImageActive(isselected);

        
    }
    public void SetTickImageActive(bool tickActive)
    {
        if (TickImage == null)
            return;
        TickImage.gameObject.SetActive(tickActive);
    }

    public void SetNumText(int num)
    {
        if (NumTextAndBG != null)
            NumTextAndBG.gameObject.SetActive(num > 0);

        if (AddImage != null)
            AddImage.gameObject.SetActive(num <= 0);

        if (NumText != null)
            NumText.text = num > 99 ? "N" : string.Format("{0}", num);       

    }
    void SetPriceDisplay()
    {
        int num = propCtr.GetPropNum(ItemPropType);

        if(PriceParent!= null)
            PriceParent.SetActive(num <= 0);

        FreeImage.gameObject.SetActive(num>0);
    }

    public void OnClickPropButton()
    {
        OnClickPropButton(BuyButton);
    }

    private void OnRechargeRefresh()
    {
        if (GameController.Instance.PropCtr.TemSelectedProp == ItemPropType)
        {
            int num = GameController.Instance.PropCtr.GetPropNum(ItemPropType);
            if (num > 0)
            {
                GameController.Instance.PropCtr.SetPropSelect(ItemPropType,true);
                UpdateUI();
            }
        }
    }

    void OnClickPropButton(UIButtonBehaviour button)
    {
        int Num = propCtr.GetPropNum(ItemPropType);
        GameController.Instance.PropCtr.TemSelectedProp = ItemPropType;
        bool select = propCtr.GetPropSelect(ItemPropType);

        ///为拥有该道具///
        if (Num <= 0)
        {
            if (Kind == GoodsConfig.SType.PassiveProp)
            {
                ///被动道具//
                ///当前该道具是选中状态//
                if (select)
                {
                    AdvanceSelectPassive(select);
                }
                else
                {
                    ///货币足够支付直接买，不够去商店///
                    if (propCtr.CheckBuyProp(ItemPropType))
                    {
                        AdvanceSelectPassive(select);

                    }
                    else
                    {
                        ///是否要人民币直购//
                        if (GameController.Instance.Model.IsPaymentChan())
                        {
                            ///人名币购买
                            string paymentId = GameController.Instance.PropCtr.GetPropPaymentID(Convert.ToInt32(ItemPropType), InCombat);
                            GameController.Instance.PlayerCtr.BuyGem(paymentId);
                        }
                        else
                        {
                            ////选择当前道具所需游戏币不够//
                            UIShopGoodsWindow uIShopGoodsWindow = GameController.Instance.Popup.Open(PopupID.UIShopGoodsWindow, null, true, true).GetComponent<UIShopGoodsWindow>();
                            uIShopGoodsWindow.Data = propCtr.GetShopConfig(ItemPropType);

                            ///钻石足够买道具所需金币进入钻石买金币页面，否则进入充值界面//
                            ///被动道具有购物车，所以不能在购买弹窗中点击判定直接购买单个道具//
                            if (propCtr.IsGemToCoin(ItemPropType))
                                uIShopGoodsWindow.SetDisplay(false,false,false,UIShop.TapIndex.COIN_INDEX);
                            else
                                uIShopGoodsWindow.SetDisplay(false, false, false);

                            AddEvent();
                        }


                    }
                }

            }
            else if (Kind == GoodsConfig.SType.ActiveProp)
            {

                /// 货币足够支付直接买，不够去商店///
                /// 暂用逻辑，每次购买都提示弹窗//
                //if (propCtr.BuySingleGoods(ItemPropType))
                if(propCtr.CheckBuyProp(ItemPropType,false))
                {
                    //主动道具购买一定有弹窗//
                    UIShopGoodsWindow uIShopGoodsWindow = GameController.Instance.Popup.Open(PopupID.UIShopGoodsWindow, null, true, true).GetComponent<UIShopGoodsWindow>();
                    uIShopGoodsWindow.Data = propCtr.GetShopConfig(ItemPropType);

                }
                else
                {
                    ///平台走直购流程//
                    if (GameController.Instance.Model.IsPaymentChan())
                    {
                        ///人民币直接购买//
                        string paymentId = GameController.Instance.PropCtr.GetPropPaymentID(Convert.ToInt32(ItemPropType), InCombat);
                        GameController.Instance.PlayerCtr.BuyGem(paymentId);
                    }
                    else
                    {
                        UIShopGoodsWindow uIShopGoodsWindow = GameController.Instance.Popup.Open(PopupID.UIShopGoodsWindow, null, true, true).GetComponent<UIShopGoodsWindow>();
                        uIShopGoodsWindow.Data = propCtr.GetShopConfig(ItemPropType);

                        ///设置游戏币不足//
                        ///主动道具点击则为需要购买//
                        uIShopGoodsWindow.SetDisplay(false, false);

                        AddEvent();
                    }

                }
            }///prop kind


        }
        else
        {
            SelectProp(select);
        }///Num

    }


    /// <summary>
    /// select 表示当前的选中状态
    /// 预先选择被动道具，当前被动道具数量可能为0
    /// 设置成与当前相反状态
    /// </summary>
    /// <param name="select"></param>
    void AdvanceSelectPassive(bool select)
    {
        propCtr.SetPropSelect(ItemPropType, !select);
        UpdateUI();

        SetNumText( select? 0 : 1 );

        if (propCtr.PropBuyCountAction != null)
            propCtr.PropBuyCountAction(ItemPropType, !select);

        ///如果有选择事件，则要执行选择事件///
        if (propCtr.PropSelectActionDic.ContainsKey(ItemPropType)
            && propCtr.PropSelectActionDic[ItemPropType] != null)
        {
            propCtr.PropSelectActionDic[ItemPropType](ItemPropType);
        }

    }

    /// <summary>
    /// 添加购买事件//
    /// </summary>
    void AddEvent()
    {
        ///添加关闭窗口去除购买事件//
        GameController.Instance.Popup.OnCloseComplete -= CloseShopEvent;
        GameController.Instance.Popup.OnCloseComplete += CloseShopEvent;
    }

    /// <summary>
    /// 反向选择，例如当前是选中状态则变成为选择状态///
    /// </summary>
    /// <param name="select"></param>
    public void SelectProp(bool select)
    {
        if (Kind == GoodsConfig.SType.PassiveProp)
        {
            ///被动道具选择后进游戏就要扣除//
            propCtr.SetPropUseNum(ItemPropType, select ? 0 : 1);
        }
        else if (Kind == GoodsConfig.SType.ActiveProp)
        {
            //int num = propCtr.GetPropUseNum(ItemPropType);
        }

        propCtr.SetPropSelect(ItemPropType, !select);

        ///有数字效果时候，显示数字跳动效果///
        Action<bool, float> propAc = propCtr.GetPropNumEffAction(ItemPropType);

        if (propAc != null)
            propAc(!select, propCtr.GetPropValue(ItemPropType));

        ///如果有选择事件，则要执行选择事件///
        if (propCtr.PropSelectActionDic.ContainsKey(ItemPropType)
            && propCtr.PropSelectActionDic[ItemPropType] != null)
        {
            propCtr.PropSelectActionDic[ItemPropType](ItemPropType);
        }

        UpdateUI();
    }

    void OnDestroy()
    {
        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent -= OnRechargeRefresh;
        GameController.Instance.Popup.OnCloseComplete -= CloseShopEvent;
    }

    void CloseShopEvent(PopupID ID)
    {
        ///关闭商店要把购买道具事件去除
        if (ID == PopupID.UIShop)
        {
            GameController.Instance.Popup.OnCloseComplete -= CloseShopEvent;

            if (Kind == GoodsConfig.SType.PassiveProp)
            {
                ///充值游戏币足够支付，预选中被动道具
                if (propCtr.CheckBuyProp(ItemPropType))
                    AdvanceSelectPassive(false);
            }
            else if (Kind == GoodsConfig.SType.ActiveProp)
            {
                ///充值游戏币足够支付，预选中被动道具直接购买主动道具
                if (propCtr.CheckBuyProp(ItemPropType, false))
                    propCtr.BuySingleGoods(ItemPropType);

            }

        }
    }

}
