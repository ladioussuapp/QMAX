using Com4Love.Qmax;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIUpgradGoleTipWin : MonoBehaviour
{
    public struct Data
    {
        public int UpgradeA;
        public int UpgradeB;
        public int Gold;
    }

    public UIButtonBehaviour EnterButton;
    public UIButtonBehaviour CloseButton;
    public Text TxtUpgradeA;
    public Text TxtUpgradeB;
    public Text TxtGold;
    public event Action OnBuyUpgrade;

    public Image ImgUpgradeA;
    public Image ImgUpgradeB;

    public Image ImgCountUpgradeA;
    public Image ImgCountUpgradeB;

    public Text Title;

    Data data;

    public void Start()
    {
        EnterButton.onClick += EnterButton_onClick;
        CloseButton.onClick += CloseButton_onClick;

        GameController.Instance.Popup.OnOpenComplete += onOpenWindow;
        // title text
        Title.text = Utils.GetTextByID(1707);
    }

    private void onOpenWindow(PopupID obj)
    {
        if (PopupID.UIUpgradGoleTipWin == obj)
        {
            GameController.Instance.Popup.OnOpenComplete -= onOpenWindow;
            addFliter();
        }
    }

    void CloseButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Close(PopupID.UIUpgradGoleTipWin);
    }

    void EnterButton_onClick(UIButtonBehaviour button)
    {
        bool res = GameController.Instance.UnitCtr.BuyUpgrade(data.UpgradeA, data.UpgradeB);

        if (res)
        {
            //夠錢
            GameController.Instance.ModelEventSystem.OnBuyUpgrade += ModelEventSystem_OnBuyUpgrade;
            EnterButton.onClick -= EnterButton_onClick;
        }
        else
        {
            //錢不夠，彈出購買鑽石窗口
            UIShop.Open(UIShop.TapIndex.GEM_INDEX);
        }
    }

    void ModelEventSystem_OnBuyUpgrade()
    {
        GameController.Instance.ModelEventSystem.OnBuyUpgrade -= ModelEventSystem_OnBuyUpgrade;

        if (OnBuyUpgrade != null)
        {
            OnBuyUpgrade();
        }

        GameController.Instance.Popup.Close(PopupID.UIUpgradGoleTipWin);
    }

    public void OnDestroy()
    {
        OnBuyUpgrade = null;
        removeFliter();
        EnterButton.onClick -= EnterButton_onClick;
        CloseButton.onClick -= CloseButton_onClick;
        GameController.Instance.ModelEventSystem.OnBuyUpgrade -= ModelEventSystem_OnBuyUpgrade;
    }

    public void SetData(Data data_)
    {
        data = data_;

        TxtUpgradeA.text = data.UpgradeA.ToString();
        TxtUpgradeB.text = data.UpgradeB.ToString();
        TxtGold.text = data.Gold.ToString();

        // 只要A材料，只顯示A材料
        if (data.UpgradeA > 0 && data.UpgradeB <= 0)
        {
            //位置設置為中間
            Vector3 a_img_old_pos = ImgUpgradeA.transform.localPosition;
            ImgUpgradeA.transform.localPosition = new Vector3(0, a_img_old_pos.y, a_img_old_pos.z);

            Vector3 a_count_old_pos = ImgCountUpgradeA.transform.localPosition;
            ImgCountUpgradeA.transform.localPosition = new Vector3(0, a_count_old_pos.y, a_count_old_pos.z);
            // 不顯示材料B
            ImgUpgradeB.gameObject.SetActive(false);
            ImgCountUpgradeB.gameObject.SetActive(false);
        }
        // 只要B材料，只顯示B材料
        else if (data.UpgradeB > 0 && data.UpgradeA <= 0)
        {
            //位置設置為中間
            Vector3 b_img_old_pos = ImgUpgradeB.transform.localPosition;
            ImgUpgradeB.transform.localPosition = new Vector3(0, b_img_old_pos.y, b_img_old_pos.z);

            Vector3 b_count_old_pos = ImgCountUpgradeB.transform.localPosition;
            ImgCountUpgradeB.transform.localPosition = new Vector3(0, b_count_old_pos.y, b_count_old_pos.z);
            // 不顯示材料A
            ImgUpgradeA.gameObject.SetActive(false);
            ImgCountUpgradeA.gameObject.SetActive(false);
        }
    }

    private void addFliter()
    {
        GuideNode node = new GuideNode();
        node.TargetNode = EnterButton;
        node.TarCamera = Camera.main;
        node.index = 4;
        node.ShowMask = false;
        node.CallBack = delegate()
        {
            EnterButton_onClick(null);
        };
        GuideManager.getInstance().addGuideNode("BuySutffUpgradeBtn", node);
    }

    private void removeFliter()
    {
        GuideManager.getInstance().removeGuideNode("BuySutffUpgradeBtn");
    }
 
}
