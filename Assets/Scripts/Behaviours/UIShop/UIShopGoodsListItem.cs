using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Data.Config;
using UnityEngine.UI;
using Com4Love.Qmax;

public class UIShopGoodsListItem : MonoBehaviour
{
    public struct Data
    {
        public ShopConfig Config;

    }

    public Data ItemData;
    public Image Icon;
    public Text InfoText;
    public Text NameText;
    public Text Price;
    public Image PriceIcon;
    public UIButtonBehaviour EnterButton;
    public Button IconButton;
    public Text CountText;

    public Sprite PriceGemSprite;
    public Sprite PriceCoinSprite;

    // Use this for initialization
    void Start()
    {
        EnterButton.onClick += EnterButton_onClick;
        IconButton.onClick.AddListener(OnIconClick);

        UpdateData();
    }

    public void UpdateData()
    {
        ShopConfig shopConfig = ItemData.Config;
        GoodsConfig goodsConfig = GameController.Instance.Model.GoodsConfigs[shopConfig.GoodsId];

        NameText.text = GameController.Instance.GoodsCtr.GetGoodsNameStr(goodsConfig);           
        InfoText.text = GameController.Instance.GoodsCtr.GetGoodsContentStr(goodsConfig);
        Icon.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIComponent, shopConfig.ShopIcon);
        Icon.SetNativeSize();
        CountText.text = string.Format("x{0}", shopConfig.ByNum);

        if (shopConfig.Gem == 0)
        {
            //使用金幣購買
            Price.text = shopConfig.Coin.ToString();
            PriceIcon.sprite = PriceCoinSprite;
        }
        else
        {
            Price.text = shopConfig.Gem.ToString();
            PriceIcon.sprite = PriceGemSprite;
        }
    }

    void OnIconClick()
    {
        UIShopGoodsWindow uIShopGoodsWindow  = GameController.Instance.Popup.Open(PopupID.UIShopGoodsWindow , null, true , true).GetComponent< UIShopGoodsWindow>();
        uIShopGoodsWindow.Data = ItemData.Config;
    }

    private void EnterButton_onClick(UIButtonBehaviour button)
    {
        //商城的商品ID
        //窗口直接監聽物品刷新事件
        bool res = GameController.Instance.GoodsCtr.BuyGoods(ItemData.Config.UID);

        if (!res)
        {
            ///提示金幣不足還是鑽石不足//
            string info =  ItemData.Config.Gem > 0 ? Utils.GetTextByID(20042) : Utils.GetTextByID(20047);
            //不夠錢 出提示
            UIAlertBehaviour alert = UIAlertBehaviour.Alert(info, null,null,2,0,0
                ,(byte)UIAlertBehaviour.ButtonStates.ButtonOk | (byte)UIAlertBehaviour.ButtonStates.ButtonCancel);

            //OnClickOKButton 等事件在alert 被銷毀時會被自動清理
            alert.OnClickOKButton += Alert_OnClickOKButton;
        }
    }
 
    private void Alert_OnClickOKButton(UIButtonBehaviour obj)
    {
        //錢不夠 確定去充值介面
        UIShop.Open(UIShop.TapIndex.GEM_INDEX);
    }

    public void OnDestroy()
    {
        EnterButton.onClick -= EnterButton_onClick;
        IconButton.onClick.RemoveAllListeners();
    }
}
