using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols.goods;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class GoodsWinBehaviour : MonoBehaviour
{

    public enum GoodsMode : int
    {
        Shop,
        Fight,
    }

    public GameObject BoxesContent;
    public Button CloseButton;
    public Button ShopButton;
    public RectTransform ItemInfoLayer;

    public GameObject ClearInfo_Canvas;
    public GameObject ClearInfo_ScroolRect;

    private List<GameObject> itemBoxes = new List<GameObject>();
    private List<GoodsItem> goodsItems = new List<GoodsItem>();
    
    private Dictionary<int, GoodsItem> goodsItemMap = new Dictionary<int, GoodsItem>();

    private GameObject itemBox;
    private GridLayoutGroup itemGrid;
    private GameObject itemInfo;

    private int boxCount = 0;
    void Awake()
    {
        itemBox = Resources.Load<GameObject>("Prefabs/Ui/UIGoods/GoodsItemBox");
        itemGrid = BoxesContent.GetComponentInChildren<GridLayoutGroup>();

        //TODO:Should optimize with perfab pool 
        itemInfo = Resources.Load<GameObject>("Prefabs/Ui/UIGoods/GoodsItemInfo");

        CloseButton.onClick.AddListener(this.OnCloseButtonClick);
        ShopButton.onClick.AddListener(this.OnShopButtonClick);

        GameController.Instance.ModelEventSystem.OnGetGoodsList += this.OnGetGoodsList;
        GameController.Instance.ModelEventSystem.OnBuyGoods += this.OnBuyGoods;
        GameController.Instance.ModelEventSystem.OnUseGoodsItem += this.OnGoodsItemUse;
        GameController.Instance.ModelEventSystem.OnGoodsWinRefresh += this.OnRefresh;

        EventTriggerListener.Get(ClearInfo_Canvas).onClick += ClearInfo;
        EventTriggerListener.Get(ClearInfo_ScroolRect).onClick += ClearInfo;
    }

    // Use this for initialization
    void Start()
    {
        this.RefreshItemBoxes();
    }

    void ClearInfo(GameObject go)
    {
        GameController.Instance.ModelEventSystem.OnGoodsItemInfoClear();
    }

    // <summary>
    /// 匹配容器大小
    /// </summary>
    private void FitBoxCanvas(int num)
    {
        int itemCount = num;
        if (itemCount <= 16)
        {
            boxCount = 20;
        }
        else
        {
            boxCount = itemCount - itemCount % 4 + 8;
        }

        if (itemBoxes.Count < boxCount)
        {
            for (int i = 0; i < boxCount; i++)
            {
                AddItemBox(itemBoxes.Count + i);
            }
        }
        else
        {
            for (int i = 0; i < itemBoxes.Count - boxCount; i++)
            {
                RemoveItemBox(boxCount + i);
            }
        }

    }

    private GoodsMode _mode;
    /// <summary>
    /// 道具窗口功能模式
    /// </summary>
    public GoodsMode Mode
    {
        get
        {
            return _mode;
        }
        set
        {
            _mode = value;
        }
    }

    private void AddItemBox(int index)
    {
        GameObject item = Instantiate(itemBox);
        GoodsItemBehaviour beh = item.GetComponent<GoodsItemBehaviour>();
        beh.Index = index;
        beh.ItemInfoIns = itemInfo;
        beh.ItemInfoLayer = ItemInfoLayer;
        item.transform.SetParent(itemGrid.transform);
        item.transform.localScale = new Vector3(1, 1, 1);
        item.transform.localRotation = new Quaternion(0, 0, 0, 0);
        item.transform.localPosition = new Vector3(0, 0, 0);
        itemBoxes.Add(item);
    }
    private void RemoveItemBox(int index)
    {
        if (index <= itemBoxes.Count - 1)
        {
            GameObject tmp = itemBoxes[index];
            itemBoxes.RemoveAt(index);
            Destroy(tmp);
        }
    }

    /// <summary>
    /// 刷新背包
    /// </summary>
    private void RefreshItemBoxes()
    {
        goodsItems.Clear();
        //拷貝一份(因為元素是類)
        foreach (var item in GameController.Instance.GoodsCtr.GoodsItems)
        {
            goodsItems.Add(new GoodsItem(item.id, item.num));
        }
        //
        Dictionary<int, GoodsConfig> config = GameController.Instance.Model.GoodsConfigs;
        //幹掉SortId為-1的鬼
        for (int i = goodsItems.Count - 1; i > 0; --i)
        {
            if (config[goodsItems[i].id].SortId == -1)
            {
                goodsItems.RemoveAt(i);
            }
        }
        //排序一下
        goodsItems.Sort(delegate (GoodsItem a, GoodsItem b)
        {
            return config[a.id].SortId.CompareTo(config[b.id].SortId);
        });
        //多於99的分組
        for (int i = 0; i < goodsItems.Count; i++)
        {
            if (goodsItems[i].num > 99)
            {
                int group = goodsItems[i].num / 99;
                GoodsItem[] items = new GoodsItem[group];
                for (int j = 0; j < items.Length; j++)
                {
                    items[j] = new GoodsItem(goodsItems[i].id, 99);
                    goodsItems.Insert(i+j,items[j]);
                }
                goodsItems[i+ group].num = goodsItems[i + group].num % 99;
            }

        }
        //適配
        this.FitBoxCanvas(goodsItems.Count);
        //
        goodsItemMap.Clear();

        foreach (var item in itemBoxes)
        {
            item.GetComponent<GoodsItemBehaviour>().ItemData = null;
        }
        if (itemBoxes.Count >= goodsItems.Count)
        {
            if (goodsItems != null)
            {
                for (int i = 0; i < goodsItems.Count; i++)
                {

                    itemBoxes[i].GetComponent<GoodsItemBehaviour>().ItemData = goodsItems[i];
                    itemBoxes[i].GetComponent<GoodsItemBehaviour>().Index = i;
                    goodsItemMap[goodsItems[i].id] = goodsItems[i];
                }
            }
        }

    }

    void OnDestroy()
    {
        GameController.Instance.ModelEventSystem.OnGetGoodsList -= this.OnGetGoodsList;
        GameController.Instance.ModelEventSystem.OnUseGoodsItem -= this.OnGoodsItemUse;
        GameController.Instance.ModelEventSystem.OnBuyGoods -= this.OnBuyGoods;
        GameController.Instance.ModelEventSystem.OnGoodsWinRefresh -= this.OnRefresh;
    }

    private void OnCloseButtonClick()
    {
        if (GameController.Instance.Popup.IsPopup(PopupID.UIGoodsWin))
        {
            GameController.Instance.Popup.Close(PopupID.UIGoodsWin, true);
        }
    }

    private void OnShopButtonClick()
    {
        GameController.Instance.ModelEventSystem.OnGoodsItemInfoClear();
        //         if (GameController.Instance.Popup.IsPopup(PopupID.UIGoodsWin))
        //         {
        //             GameController.Instance.Popup.Close(PopupID.UIGoodsWin, true);
        //         }

        //
        /*
        if (!GameController.Instance.Popup.IsPopup(PopupID.UILeanShop))
        {
            GameController.Instance.Popup.Open(PopupID.UILeanShop, null, true, true);
        }
        */
        UIShop.Open(UIShop.TapIndex.GOODS_INDEX);
    }

    /// <summary>
    /// 刷新背包
    /// </summary>
    /// <param name="res"></param>
    private void OnGetGoodsList(GoodsListResponse res)
    {

    }
    /// <summary>
    /// 道具被使用
    /// </summary>
    /// <param name="res"></param>
    private void OnGoodsItemUse(UseGoodsResponse res)
    {
        RefreshItemBoxes();
    }

    private void OnBuyGoods(int oldNum, GoodsItem res)
    {
        RefreshItemBoxes();
    }

    private void OnRefresh()
    {
        RefreshItemBoxes();
    }

    // Update is called once per frame
    void Update()
    {

    }

}