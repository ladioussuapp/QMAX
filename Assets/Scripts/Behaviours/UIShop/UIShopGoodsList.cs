using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com4Love.Qmax.Data.Config;

public class UIShopGoodsList : MonoBehaviour
{
    [HideInInspector]
    public List<UIShopGoodsListItem> items;
    public CycleListBehaviour CycleList;
    List<UIShopGoodsListItem.Data> datas;

    /// <summary>
    /// 普通道具
    /// </summary>
    public int GoodsTap = 2;

    public void Awake()
    {
        datas = new List<UIShopGoodsListItem.Data>();
        List<ShopConfig> datasAll = GameController.Instance.Model.ShopConfigList;

        for (int i = 0; i < datasAll.Count; i++)
        {
            ShopConfig config = datasAll[i];

            if (config.Tab != GoodsTap || config.SortId < 0)
            {
                //小於0時表示不顯示在商城中 可能是單個的物品數據
                continue;
            }

            UIShopGoodsListItem.Data itemData = new UIShopGoodsListItem.Data();
            itemData.Config = config;
            datas.Add(itemData);
        }

        CycleList.MaxItemCount = datas.Count;
    }

    // Use this for initialization
    void Start()
    {
        CycleList.OnItemReset += CycleList_OnItemReset;
        int defaultMaxCount = datas.Count > 7 ? 7 : datas.Count;

        for (int i = 0; i < defaultMaxCount; i++)
        {
            UIShopGoodsListItem.Data itemData = datas[i];
            UIShopGoodsListItem item = CreateItem();
            item.ItemData = itemData;
            items.Add(item);
        }

        TegraMask TMask = GetComponentInParent<TegraMask>();

        if (TMask != null)
        {
            TMask.RegisteredGraphics(transform);
        }
    }

    public void OnDestroy()
    {
        CycleList.OnItemReset -= CycleList_OnItemReset;
    }

    private void CycleList_OnItemReset(Transform transform, int index)
    {
        //動態列表替換
        if (index < datas.Count)
        {
            UIShopGoodsListItem.Data itemData = datas[index];
            UIShopGoodsListItem item = transform.GetComponent<UIShopGoodsListItem>();
            item.ItemData = itemData;
            item.UpdateData();
        }
    }

    UIShopGoodsListItem CreateItem()
    {
        RectTransform rt = GameController.Instance.QMaxAssetsFactory.CreatePrefab("Prefabs/Ui/UIShop/UIShopGoodsItem") as RectTransform;
        rt.gameObject.SetActive(true);
        rt.SetParent(transform);
        rt.anchoredPosition3D = Vector3.zero;
        rt.localScale = new Vector3(1, 1, 1);

        return rt.GetComponent<UIShopGoodsListItem>();
    }

}
