using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;

public class UIShopGoodsWindow : MonoBehaviour
{
    public Text NameText;
    public Text InfoText;
    public Text PriceText;
    public Image PriceIcon;
    public Button TipButton;
    public RectTransform AnimationT;
    public ShopConfig Data;
    public UIButtonBehaviour CloseButton;
    public UIButtonBehaviour BuyButton;
    public Image Icon;
    public Image TipIcon;
    public Text CountText;
    public Sprite PriceGemSprite;
    public Sprite PriceCoinSprite;

    public Text BuyButtonText;

    bool IsDisplayTips = true;

    bool IsWantBuy = true;

    UIShop.TapIndex DisplayTap = UIShop.TapIndex.GEM_INDEX;

    Animator tipAnimator;
    GoodsConfig goodsConfig;

    // Use this for initialization
    void Start()
    {
        CloseButton.onClick += CloseButton_onClick;
        BuyButton.onClick += BuyButton_onClick;

        goodsConfig = GameController.Instance.Model.GoodsConfigs[Data.GoodsId];
        NameText.text = GameController.Instance.GoodsCtr.GetGoodsNameStr(goodsConfig);
        InfoText.text = GameController.Instance.GoodsCtr.GetGoodsContentStr(goodsConfig);

        Icon.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIComponent, Data.ShopIcon);
        Icon.SetNativeSize();
        CountText.text = string.Format("x{0}", Data.ByNum);

        if (Data.Gem == 0)
        {
            //使用金幣購買
            PriceText.text = Data.Coin.ToString();
            PriceIcon.sprite = PriceCoinSprite;
        }
        else
        {
            PriceText.text = Data.Gem.ToString();
            PriceIcon.sprite = PriceGemSprite;
        }

        if (goodsConfig.GoodsTipsGif != "")
        {
            //需要顯示物品使用動畫
            TipIcon.gameObject.SetActive(true);
            TipButton.onClick.AddListener(OnTipClick);
            RectTransform tipT = (RectTransform)GameController.Instance.QMaxAssetsFactory.CreatePrefab("Prefabs/Ui/UIShop/UITips02");
            tipT.SetParent(AnimationT);
            tipT.localScale = new Vector3(1, 1, 1);
            tipT.anchoredPosition3D = Vector3.zero;
            tipAnimator = tipT.GetChild(0).GetComponent<Animator>();
            Q.Assert(tipAnimator != null, "物品動畫狀態機未找到，請保持根目錄下只有一層動畫");
        }
        else
        {
            TipIcon.gameObject.SetActive(false);
        }
    }

    private void BuyButton_onClick(UIButtonBehaviour button)
    {
        ///想要購買//
        ///不想直接購買的情況是要這個彈窗，點擊進入充值//
        if (IsWantBuy)
        {
            bool res = GameController.Instance.GoodsCtr.BuyGoods(Data.UID);
            if (res)
            {
                GameController.Instance.Popup.Close(PopupID.UIShopGoodsWindow, false);
            }
            else
            {
                if (IsDisplayTips)
                {
                    ///提示金幣不足還是鑽石不足//
                    string info = Data.Gem > 0 ? Utils.GetTextByID(20042) : Utils.GetTextByID(20047);
                    //不夠錢 出提示
                    UIAlertBehaviour alert = UIAlertBehaviour.Alert(info, null, null, 2, 0, 0
                        , (byte)UIAlertBehaviour.ButtonStates.ButtonOk | (byte)UIAlertBehaviour.ButtonStates.ButtonCancel);

                    //OnClickOKButton 等事件在alert 被銷毀時會被自動清理
                    alert.OnClickOKButton += Alert_OnClickOKButton;
                }
                else
                {
                    if (GameController.Instance.Popup.IsPopup(PopupID.UIShopGoodsWindow))
                        GameController.Instance.Popup.Close(PopupID.UIShopGoodsWindow, false);

                    //打開對應商店頁簽
                    UIShop.Open(DisplayTap);
                }


            }
        }
        else
        {
            if (GameController.Instance.Popup.IsPopup(PopupID.UIShopGoodsWindow))
                GameController.Instance.Popup.Close(PopupID.UIShopGoodsWindow, false);

            //打開對應商店頁簽
            UIShop.Open(DisplayTap);
        }


    }

    public void SetDisplay(bool isenough = true, bool istips = false , bool isbuy = true, UIShop.TapIndex tap = UIShop.TapIndex.GEM_INDEX)
    {
        if (!isenough)
        {
            BuyButtonText.text = Utils.GetTextByID(20043);
            PriceText.color = Color.red;
        }
        IsDisplayTips = istips;
        DisplayTap = tap;
        IsWantBuy = isbuy;
    }

    private void Alert_OnClickOKButton(UIButtonBehaviour obj)
    {
        GameController.Instance.Popup.Close(PopupID.UIShopGoodsWindow, false);
        //錢不夠 確定去充值介面
        ///DisplayTap默認值就是充值介面
        UIShop.Open(DisplayTap);
    }

    private void CloseButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Close(PopupID.UIShopGoodsWindow);
    }

    public void OnDestroy()
    {
        BuyButton.onClick -= BuyButton_onClick;
        CloseButton.onClick -= CloseButton_onClick;
        TipButton.onClick.RemoveAllListeners();
    }

    void OnTipClick()
    {
        AnimationT.gameObject.SetActive(!AnimationT.gameObject.activeSelf);

        if (tipAnimator != null && tipAnimator.isActiveAndEnabled)
        {
            tipAnimator.Play(goodsConfig.GoodsTipsGif);
        }
    }
}
