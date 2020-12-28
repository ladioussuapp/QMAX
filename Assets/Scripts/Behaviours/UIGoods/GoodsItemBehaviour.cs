using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Net.Protocols.goods;
using UnityEngine.UI;
using System;
using Com4Love.Qmax;

public class GoodsItemBehaviour : MonoBehaviour
{
    [HideInInspector]
    public int Index;
    [HideInInspector]
    public GameObject ItemInfoIns;
    [HideInInspector]
    public RectTransform ItemInfoLayer;

    public GameObject ClickButton;
    public GameObject ItemImage;
    public GameObject IconNum;

    private GoodsItem goodsItem;
    private GameObject itemInfo;
    private GoodsConfig goodsConfig;

    private GameController gameCtr;

    void Awake()
    {
        GameController.Instance.ModelEventSystem.OnGoodsItemInfoClear += OnInfoClear;
        gameCtr = GameController.Instance;
    }

    // Use this for initialization
    void Start()
    {
        ClickButton.GetComponent<Button>().onClick.AddListener(this.OnClick);
    }

    void OnDestroy()
    {
        GameController.Instance.ModelEventSystem.OnGoodsItemInfoClear -= OnInfoClear;
    }

    public GoodsItem ItemData
    {
        set
        {
            goodsItem = value;
            //TODO:有就顯示東西，無就空
            if (goodsItem == null)
            {
                ItemImage.SetActive(false);
                IconNum.SetActive(false);
                ClickButton.SetActive(false);

                goodsConfig = null;
            }
            else
            {

                ItemImage.SetActive(true);
                IconNum.SetActive(true);
                ClickButton.SetActive(true);

                goodsConfig = gameCtr.Model.GoodsConfigs[goodsItem.id];

                Image im = ItemImage.GetComponent<Image>();
                im.sprite = gameCtr.AtlasManager.GetSprite(Atlas.UIComponent, goodsConfig.GoodsIcon);

                IconNum.SetActive(true);
                IconNum.transform.Find("Text").gameObject.GetComponent<Text>().text = String.Format("{0}", goodsItem.num);

            }
        }
        get
        {
            return goodsItem;
        }
    }

    public void Clear()
    {
        if (itemInfo != null)
        {
            Destroy(itemInfo);
            itemInfo = null;
        }
    }

    private void OnInfoClear()
    {
        this.Clear();
    }

    private void OnClick()
    {
        GameController.Instance.ModelEventSystem.OnGoodsItemInfoClear();
        if (ItemInfoIns != null && goodsItem != null && itemInfo == null)
        {
            itemInfo = Instantiate(ItemInfoIns);

            if (ItemInfoLayer != null)
            {
                itemInfo.transform.SetParent(this.ItemInfoLayer);
                itemInfo.transform.localScale = new Vector3(1, 1, 1);

                Vector3 screenPoint = Camera.main.WorldToScreenPoint(this.transform.position);
                Vector2 outPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(ItemInfoLayer, screenPoint, Camera.main, out outPos);
                Vector3 targetLocalPos = new Vector3(outPos.x, outPos.y);
                itemInfo.transform.localPosition = targetLocalPos;
            }

            GoodsItemInfoBehaviour beh = itemInfo.GetComponentInChildren<GoodsItemInfoBehaviour>();
            beh.GoodsConfig = GameController.Instance.Model.GoodsConfigs[goodsItem.id];
            beh.Num = goodsItem.num;
            beh.Type = this.GetType(Index);
        }
    }

    private GoodsItemInfoBehaviour.EType GetType(int index)
    {
        int poit = index % 4;
        GoodsItemInfoBehaviour.EType type;
        if (poit == 0)
        {
            type = GoodsItemInfoBehaviour.EType.Left;
        }
        else if (poit > 0 && poit <= 2)
        {
            type = GoodsItemInfoBehaviour.EType.Center;
        }
        else
        {
            type = GoodsItemInfoBehaviour.EType.Right;
        }
        return type;
    }

    public bool IsWithData
    {
        get { return goodsItem != null; }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
