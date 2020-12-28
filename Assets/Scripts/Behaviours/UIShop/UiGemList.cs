using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com4Love.Qmax;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;

public class UiGemList : MonoBehaviour
{
    [HideInInspector]
    public List<UiGemListItem> items;
    UiGemListItem cutItem;

    public void Start()
    {
        GameController.Instance.AtlasManager.LoadAtlas(Atlas.UIGem);
        GameController.Instance.PlayerCtr.GetRechargeInfo();
        GameController.Instance.ModelEventSystem.OnPaymentInfo += ModelEventSystem_OnPaymentInfo;
        GameController.Instance.ModelEventSystem.OnBuyGems += ModelEventSystem_OnBuyGems;

        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent += OnRechargeRefresh;

        GameController.Instance.Popup.ShowLightLoading();
        //下面新增隱藏讀取畫面來防止無回應的鑽石商城頁面卡在讀取中...12/16新增
        GameController.Instance.Popup.HideLightLoading();
    }


    public void OnRechargeRefresh()
    {
        SetBuyGemWait(false);
    }

    public void OnDestroy()
    {

        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent += OnRechargeRefresh;
        GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UIGem);
        GameController.Instance.ModelEventSystem.OnBuyGems -= ModelEventSystem_OnBuyGems;
        GameController.Instance.ModelEventSystem.OnPaymentInfo -= ModelEventSystem_OnPaymentInfo;
    }

    void ModelEventSystem_OnPaymentInfo()
    {
        GameController.Instance.ModelEventSystem.OnPaymentInfo -= ModelEventSystem_OnPaymentInfo;
        GameController.Instance.Popup.HideLightLoading();
        DataReady();
    }

    void ModelEventSystem_OnBuyGems(int num)
    {

        //GameController.Instance.ModelEventSystem.OnBuyGems -= ModelEventSystem_OnBuyGems;

        //購買成功  
        //鑽石的數量不直接刷新，而是在動畫中動態刷新
        //GemText.text = GameController.Instance.PlayerCtr.PlayerData.gem.ToString();

        if (num == -1)
        {
            //失敗 各種原因
            SetBuyGemWait(false);
            GameController.Instance.Popup.HideLightLoading();
        }
        else
        {
            //StartCoroutine(PlayEffect());
        }
    }

    public IEnumerator PlayEffect()
    {
        //yield return StartCoroutine(GemEffect());

        //GameController.Instance.EffectProxy.ScrollText(GemText, int.Parse(GemText.text), GameController.Instance.PlayerCtr.PlayerData.gem, .3f);
        yield return new WaitForSeconds(1f);

        cutItem.Reset();
        SetBuyGemWait(false);
    }

    //鑽石圖標飛上
    //public IEnumerator GemEffect()
    //{
    //    List<GameObject> flys = new List<GameObject>();
    //    UiGem uiGem = GameController.Instance.Popup.GetPopup(PopupID.UIShop).GetComponent<UiGem>();
    //    Vector3 to = GemImg.transform.position;
    //    float speed = 10f;
    //    Vector3 from = cutItem.GemImgPrefab.transform.position;
    //    float dis = Vector3.Distance(from, to);
    //    float time = dis / speed;
    //    int flyCount = 2 * cutItem.Index + 1;
    //    flyCount = flyCount > 9 ? 9 : flyCount;

    //    RectTransform gemImgT = null;
    //    //裡面放了鑽石飛翔的音效
    //    audioSource.Play();

    //    for (int i = 0; i < flyCount; i++)
    //    {
    //        gemImgT = GameObject.Instantiate(cutItem.GemImgPrefab.gameObject).transform as RectTransform;
    //        gemImgT.SetParent(uiGem.transform);
    //        gemImgT.gameObject.SetActive(true);
    //        gemImgT.localScale = cutItem.GemImgPrefab.transform.localScale;
    //        gemImgT.position = cutItem.GemImgPrefab.transform.position;

    //        LeanTween.move(gemImgT.gameObject, to, time).setEase(LeanTweenType.easeInQuad).setOnComplete(delegate ()
    //        {
    //            PlayGemAnimation();
    //        });

    //        flys.Add(gemImgT.gameObject);

    //        yield return new WaitForSeconds(.06f);
    //    }

    //    //gemImgT 也是最後一個圖標
    //    while (LeanTween.isTweening(gemImgT.gameObject))
    //    {
    //        yield return 0;
    //    }

    //    foreach (GameObject item in flys)
    //    {
    //        GameObject.Destroy(item);
    //    }

    //    flys = null;
    //}

    void BuyGem(UiGemListItem item)
    {
        cutItem = item;     //保存當前購買的item項
        SetBuyGemWait(true);
        GameController.Instance.PlayerCtr.BuyGem(item.data.Id);
        GameController.Instance.Popup.ShowLightLoading();
    }

    public void SetDatas(Data[] datas)
    {
        for (int i = 0; i < datas.Length; i++)
        {
            Data data = datas[i];
            UiGemListItem item = CreateItem();
            item.SetData(data);
            item.Index = i;
            item.OnBuyGem += Item_OnBuyGem;
            items.Add(item);
        }

        //專門為小米3的芯片做的兼容mask
        TegraMask tMask = GetComponentInParent<TegraMask>();

        if (tMask != null)
        {
            tMask.RegisteredGraphics(transform);
        }
    }

    private void Item_OnBuyGem(UiGemListItem obj)
    {
        BuyGem(obj);
    }

    public void SetItemButtonsEnable(bool val)
    {
        for (int i = 0; i < items.Count; i++)
        {
            UiGemListItem item = items[i];
            if (item != null)
            {
                item.button.interactable = val;
            }
        }
    }

    void DataReady()
    {
        int count = 0;
        foreach (KeyValuePair<string, PaymentSystemConfig> keyVal in GameController.Instance.Model.PaymentSystemConfigs)
        {
            PaymentSystemConfig config = keyVal.Value;
            if (config.SortId != -1)
            {
                count++;
            }
        }
        UiGemList.Data[] datas = new UiGemList.Data[count];

        //從配置中獲取數據。數據尚未排序
        foreach (KeyValuePair<string, PaymentSystemConfig> keyVal in GameController.Instance.Model.PaymentSystemConfigs)
        {
            PaymentSystemConfig config = keyVal.Value;
            if (config.SortId == -1)
            {
                continue;
            }
            UiGemList.Data data = new UiGemList.Data();
            data.GemNum = config.BuyNum;
            data.Icon = config.BuyGemIcon;
            data.Id = config.PaymentId;
            data.Msg = Utils.GetTextByStringID(config.PaymentStringId.ToString());
            data.Price = config.Rmb;
            data.Scale = config.BuyGemSale;
            datas[config.SortId - 1] = data;
        }

        SetDatas(datas);
    }
 
    protected void SetBuyGemWait(bool val)
    {
        SetItemButtonsEnable(!val);
    }

    UiGemListItem CreateItem()
    {
        RectTransform rt = GameController.Instance.QMaxAssetsFactory.CreatePrefab("Prefabs/Ui/UIShop/UIGemItem") as RectTransform;
        rt.SetParent(transform);
        rt.anchoredPosition3D = Vector3.zero;
        rt.localScale = new Vector3(1, 1, 1);

        return rt.GetComponent<UiGemListItem>();
    }

    public struct Data
    {
        public string Id;
        public string Icon;
        public string Msg;
        public int GemNum;
        public float Scale; //折扣
        public float Price; //軟妹幣數量
    }
}
