using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.Stage;
using Com4Love.Qmax.Helper;
using Com4Love.Qmax.Net.Protocols.goods;
using UnityEngine.UI;
using System;

public class UISelectPropBehaviour : PopupEventCor
{
    List<UIPropItem> PassivePropItemList;
    List<UIPropItem> ActivePropItemList;
    PropCtr propCtr;

    public Transform PassiveParent;
    public Transform ActiveParent;
    public UIButtonBehaviour SureButton;
    public UIButtonBehaviour CloseButton;
    public Text GemNumText;
    public Text CoinNumText;
    public Text PowerNumText;

    public UIPropItem PropButtonPrefab;

    public StageConfig StageConfig;
    //Stage Stage;

    [HideInInspector]
    public List<int> UseUnitsList;

    /// <summary>
    /// 開始遊戲的一個幫助類///
    /// </summary>
    StartGameHelper StartHelper;
    // Use this for initialization

    /// <summary>
    /// 道具信息顯示時間//
    /// </summary>
    float InfoDisplayTime
    {
       get
        {
            return _infoDisplayTime;
        }

        set
        {
            if (_infoDisplayTime != value)
            {
                _infoDisplayTime = value;

                if (_infoDisplayTime < 0 && PropInfoText != null)
                    PropInfoText.gameObject.SetActive(false);
            }
        }
    }

    float _infoDisplayTime;

    public Text PropInfoText;

    private int CoinNum;
    private int GemNum;
    void Start () {

        propCtr = GameController.Instance.PropCtr;

        StartHelper = new StartGameHelper();

        ///清除所有被動道具的選中狀態///
        propCtr.ClearSelectAndUse();
        propCtr.ClearPropSelectAction();

        GameController.Instance.AtlasManager.AddAtlas(Atlas.UIComponent);

        PassivePropItemList = new List<UIPropItem>();
        ActivePropItemList = new List<UIPropItem>();

        CreatePropButton(propCtr.GetAllPassivePropIDList(), PassiveParent, PropButtonPrefab.gameObject, PassivePropItemList,null, false);
        //CreatePropButton(propCtr.GetAllActivePropIDList(),ActiveParent, PropButtonPrefab.gameObject, ActivePropItemList);

        foreach (var item in PassivePropItemList)
        {
            propCtr.PropSelectActionDic.Add(item.ItemPropType, SelectEvent);
            //propCtr.PropBuyActionDic.Add(item.ItemPropType,CountGemAndCoin);
        }
        propCtr.PropBuyCountAction += CountGemAndCoin;

        SureButton.onClick += OnClickSureButton;
        CloseButton.onClick += delegate (UIButtonBehaviour button) { Close(); };

        GameController.Instance.ModelEventSystem.OnBuyGoods += UpdateAndSelectCurItem;

        GameController.Instance.Client.GetAllGoodsList();
        GameController.Instance.ModelEventSystem.OnGetGoodsList += UpdateAllPropIcon;

    }

    void UpdateAllPropIcon(GoodsListResponse res)
    {

        for (int i = 0; i < PassivePropItemList.Count; i++)
        {
            PassivePropItemList[i].UpdateUI();
        }

        for (int i = 0; i < ActivePropItemList.Count; i++)
        {
            ActivePropItemList[i].UpdateUI();
        }
    }
	
	// Update is called once per frame
	void Update () {

        if (InfoDisplayTime >= 0)
            InfoDisplayTime -= Time.deltaTime;
	}
    public override void Close()
    {
        base.Close();

        GameController.Instance.Popup.Close(PopupID.UISelectProp);
    }

    /// <summary>
    /// 創建道具按鈕//
    /// 寫成static是想創建道具按鈕都可以調用這個方法//
    /// </summary>
    /// <param name="propidlist"></param>
    /// <param name="parent"></param>
    /// <param name="prefab"></param>
    /// <param name="propitemlist"></param>
    static public void CreatePropButton(List<int> propidlist,Transform parent,GameObject prefab
                                         ,List<UIPropItem> propitemlist = null
                                         ,Dictionary<PropType,UIPropItem> PropiteDic = null, bool inCombat = false)
    {
        if (propidlist == null || propidlist.Count == 0)
            return;

        GameController.Instance.AtlasManager.LoadAtlas(Atlas.UIComponent);
        foreach (var id in propidlist)
        {
            GameObject itemobject = Instantiate(prefab);

            itemobject.transform.SetParent(parent);
            itemobject.transform.localPosition = Vector3.zero;
            itemobject.transform.localScale = Vector3.one;
            itemobject.transform.localRotation = Quaternion.identity;


            UIPropItem buttonitem = itemobject.GetComponent<UIPropItem>();
            buttonitem.InCombat = inCombat;
            buttonitem.ItemPropType = (Com4Love.Qmax.PropType)id;

            string iconName = GameController.Instance.Model.GoodsConfigs[id].GoodsIcon;
            buttonitem.Icon.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIComponent, iconName);

            if (buttonitem.NameText != null)
            {
                int nameid = GameController.Instance.Model.GoodsConfigs[id].GoodsStringId;
                buttonitem.NameText.text = Utils.GetTextByID(nameid);
            }

            PropCtr propCtr = GameController.Instance.PropCtr;
            PropCtr.BuyMode mode = propCtr.GetBuyMode(id);

            ///設置價格//
            if (mode == PropCtr.BuyMode.Coin)
            {
                if (buttonitem.PriceNum != null)
                    buttonitem.PriceNum.text = propCtr.GetShopConfig(id).Coin.ToString();
            }
            else if (mode == PropCtr.BuyMode.Gem)
            {
                if (buttonitem.PriceNum != null)
                    buttonitem.PriceNum.text = propCtr.GetShopConfig(id).Gem.ToString();

                if(buttonitem.PriceIcon != null)
                    buttonitem.PriceIcon.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIComponent, "Icon2");
            }

            if (propitemlist != null)
                propitemlist.Add(buttonitem);

            if (PropiteDic!= null)
            {
                PropiteDic.Add(buttonitem.ItemPropType, buttonitem);
            }

        }
    }

    void OnClickSureButton(UIButtonBehaviour button)
    {
        ///添加購買成功事件//
        GameController.Instance.ModelEventSystem.OnBuyGoodsList -= BuyPropSuccess;
        GameController.Instance.ModelEventSystem.OnBuyGoodsList += BuyPropSuccess;

        List<int> noneAndSelectpas = propCtr.GetNoneAndSelectList();

        ///如果不夠支付購買//
        if (!propCtr.BuyPropList(noneAndSelectpas))
        {
            ///清除購買成功事件//
            GameController.Instance.ModelEventSystem.OnBuyGoodsList -= BuyPropSuccess;
            Q.Log("購買道具所需遊戲幣不足");
        }

    }

    void BuyPropSuccess(BuyGoodsResponse buygood)
    {
        GameController.Instance.ModelEventSystem.OnBuyGoodsList -= BuyPropSuccess;

        List<int> allselectpassive = propCtr.GetPassiveSelectList();

        ///設置所有選中的被動道具使用數量為1，在進入遊戲是傳給服務器消耗//
        foreach (int id in allselectpassive)
        {
            if (propCtr.GetPropNum(id) > 0)
                propCtr.SetPropUseNum((PropType)id, 1);
            else
                Q.Log("沒有道具 -- ", id);
        }

        StartHelper.GoToGame(StageConfig, UseUnitsList);
    }

    /// <summary>
    /// 設置道歉選擇道具界面數據信息//
    /// </summary>
    public void SetData(StageConfig config,List<int> useUnit)
    {
        StageConfig = config;
        //Stage = GameController.Instance.StageCtr.GetStageData(StageConfig.ID);
        UseUnitsList = useUnit;

        PowerNumText.text = config.CostEnergy.ToString();
    }

    /// <summary>
    /// 根據id查找到對應的道具按鈕///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    UIPropItem GetUIPropItem( int id)
    {
        ///先從主動道具中找///
        foreach (var prop in PassivePropItemList)
        {
            int proid = (int)prop.ItemPropType;

            if (proid == id)
                return prop;
        }

        ///從被動道具中查找//
        foreach (var prop in ActivePropItemList)
        {
            int proid = (int)prop.ItemPropType;

            if (proid == id)
                return prop;
        }

        return null;
    }

    public void OnDestroy()
    {
        StartHelper.Clear();
        GameController.Instance.ModelEventSystem.OnBuyGoods -= UpdateAndSelectCurItem;
        GameController.Instance.ModelEventSystem.OnBuyGoodsList -= BuyPropSuccess;
        GameController.Instance.ModelEventSystem.OnGetGoodsList -= UpdateAllPropIcon;
        propCtr.ClearPropSelectAction();
        propCtr.ClearBuyCountAction();

    }

    /// <summary>
    /// 更新道具按鈕的狀態//
    /// </summary>
    /// <param name="num"></param>
    /// <param name="res"></param>
    public void UpdateAndSelectCurItem(int num, GoodsItem res)
    {
        int id = res.id;

        UIPropItem item = GetUIPropItem(id);

        ///item不為null則在道具列表中///
        ///並且擁有數量大於0
        if (item != null && propCtr.GetPropNum(item.ItemPropType) > 0)
        {
            ///道具選取事件///
            ///參數false是要從沒被選中到被選中狀態//
            item.SelectProp(false);
        }
    }

    void SelectEvent(PropType proptype)
    {
        SetPropInfoText(proptype);
    }

    /// <summary>
    /// 顯示道具信息
    /// </summary>
    /// <param name="proptype"></param>
    void SetPropInfoText(PropType proptype)
    {
        InfoDisplayTime = 3f;

        if (PropInfoText != null)
        {
            PropInfoText.gameObject.SetActive(true);
            PropInfoText.text = GameController.Instance.GoodsCtr.GetGoodsContentStr((int)proptype);
        }
            
    }

    void CountGemAndCoin(PropType proptype,bool isadd)
    {
        int befcoin = CoinNum;
        int befgem = GemNum;

        ShopConfig config = propCtr.GetShopConfig(proptype);

        int changecoin = isadd ? config.Coin : -config.Coin;
        int changegem = isadd ? config.Gem : -config.Gem;

        CoinNum += changecoin;
        GemNum += changegem;

        float detime = 0.5f;
        GameController.Instance.EffectProxy.ScrollText(CoinNumText, befcoin, CoinNum, detime);
        GameController.Instance.EffectProxy.ScrollText(GemNumText, befgem, GemNum, detime);
    }

 }
