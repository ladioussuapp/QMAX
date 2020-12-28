using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.Unit;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//簡單的unitlist 第一個界面。繼承自ScrollRect之後發現沒辦法在編輯器中看到自己的屬性值
public class UnitList : ScrollRect
{
    protected UnitListItem itemFirst;
    protected UnitListItem itemSelect;
    protected List<UnitListItem> items = new List<UnitListItem>();
    protected float Width;
    protected float Height;
    protected RectTransform rectTransform;
    protected float MaxPos;
    protected float MinPos;
    protected float contentPaddingTop;
    protected float contentPaddingBottom;
    protected VerticalLayoutGroup vLayoutGroup;
    protected bool isDragging = false;
    protected Vector2 contentAnchoredPosition;
    protected List<Unit> lockUnits;
    public GameObject ArrowUp;
    public GameObject ArrowDown;

    public ColorType Color = 0;
    public Image LockImg;
    public Button TipButton;

    public Action<UnitListItem> OnSelected;

    public UnitConfig GetSelectUnit()
    {
        if (itemSelect == null)
        {
            return null;
        }

        return itemSelect.Data;
    }

    protected override void Awake()
    {
        TipButton = GetComponent<Button>();
        TipButton.onClick.AddListener(OnTipClick);
    }

    protected override void OnDestroy()
    {
        TipButton.onClick.RemoveAllListeners();
    }

    protected void OnTipClick()
    {
        ShowSkillTip();
    }

    protected override void Start()
    {
        //關閉編輯器的時候執行Start()????
        //if (!GameBehaviour.IsPlaying)
        //{
        //    return;
        //}

        base.Start();
        contentAnchoredPosition = content.anchoredPosition;
        rectTransform = this.transform as RectTransform;
        Width = rectTransform.rect.width;
        Height = rectTransform.rect.height;
        vLayoutGroup = content.GetComponent<VerticalLayoutGroup>();
        contentPaddingTop = vLayoutGroup.padding.top;
        contentPaddingBottom = vLayoutGroup.padding.bottom;
 
    }
 
    protected override void OnEnable()
    {
        base.OnEnable();
        //測試 每次打開都刷新 需改為 有夥伴更改時再刷新
        dataChanged = true;
    }

    //添加到了等級就會解鎖的伙伴
    protected List<UnitConfig> GetLockUnit(int passStage)
    {
        List<UnitConfig> lockUnits = GameController.Instance.UnitCtr.GetLockUnits(passStage, Color);

        return lockUnits;
    }

    protected void UpdateUnit()
    {
        if (Color == 0)
        {
            return;
        }

        List<Unit> units = GameController.Instance.PlayerCtr.PlayerData.list;
        UnitConfig config = null;

        if (itemSelect != null)
        {
            itemSelect.IsSelect = false;
            itemSelect = null;
        }

        while (content.childCount > 0)
        {
            GameController.Instance.PoolManager.Despawn(content.GetChild(0));
        }

        bool selectFlag = false;

        //添加已有的伙伴
        foreach (Unit item in units)
        {
            config = GameController.Instance.Model.UnitConfigs[item.unitId];

            if (config.UnitColor == Color)
            {
                UnitListItem listItem = UpdateItem(config, false);

                if (CheckUnitToSelect(item.unitId) && !selectFlag)
                {
                    selectFlag = true;
                    SelectItem(listItem);
                }
            }
        }

        if (items.Count > 0)
        {
            int passStage = GameController.Instance.PlayerCtr.PlayerData == null ? 0 : GameController.Instance.PlayerCtr.PlayerData.passStageId;
            List<UnitConfig> lockUnits = GetLockUnit(passStage);

            //添加帶解鎖的伙伴
            foreach (UnitConfig uConfig in lockUnits)
            {
                UpdateItem(uConfig, true);
            }

            itemFirst = items[0];

            //如果沒有上次出戰的伙伴，則默認選中第一隻
            if (!selectFlag)
            {
                SelectItem(itemFirst);
            }

            LockImg.gameObject.SetActive(false);
        }
        else
        {
            LockImg.gameObject.SetActive(true);
        }

        //SelectItem會調用 但是SelectItem調用的時候可能正在初始化items裡的length不準確，所以在初始化末尾調用一次
        UpdateArrow();

        //小米3手機兼容方案
        TegraMask tMask = GetComponentInParent<TegraMask>();

        if (tMask != null)
        {
            tMask.SetMaterialDirty();
        }
    }

    //上回合有使用的伙伴則選中之前的伙伴，否則選中第一個
    protected bool CheckUnitToSelect(int id)
    {
        List<int> lastUnits = GameController.Instance.PlayerCtr.PlayerData.lastFightUnits;

        if (lastUnits != null && lastUnits.Count > 0)
        {
            if (lastUnits.IndexOf(id) > -1)
            {
                return true;
            }
        }

        return false;
    }

    public UnitListItem UpdateItem(UnitConfig config, bool locked)
    {
        RectTransform renderer = GameController.Instance.PoolManager.PrefabSpawn(ITEM_RENDERER) as RectTransform;
        renderer.SetParent(content.transform);
        renderer.anchoredPosition3D = Vector3.zero;
        renderer.localScale = new Vector3(1f, 1f, 1f);
        UnitListItem item = renderer.GetComponent<UnitListItem>();
        item.Data = config;
        item.Locked = locked;
        items.Add(item);
        renderer.name = string.Format("UnitListItem{0}", config.ID);

        return item;
    }

    public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        isDragging = false;

        if (itemSelect == null)
        {
            return;
        }

        CheckNearestItem();
        ShowSkillTip();
    }

    public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnDrag(eventData);
        isDragging = true;
    }

    protected void CheckNearestItem()
    {
        UnitListItem item;
        float nearest = itemFirst.rectTransform.anchoredPosition.y - itemSelect.rectTransform.anchoredPosition.y;
        nearest = Mathf.Abs(nearest - content.anchoredPosition.y);

        for (int i = 0; i < items.Count; i++)
        {
            item = items[i];
            float dis = itemFirst.rectTransform.anchoredPosition.y - item.rectTransform.anchoredPosition.y;
            dis = Mathf.Abs(dis - content.anchoredPosition.y);

            if (!item.Locked && dis < nearest)
            {
                SelectItem(item);
                return;
            }
        }
    }

    public void SelectItem(UnitListItem item)
    {
        if (itemSelect == item)
        {
            return;
        }

        if (itemSelect != null)
        {
            itemSelect.IsSelect = false;
        }

        itemSelect = item;
        item.IsSelect = true;
        selectChanged = true;
        UpdateArrow();
        if (OnSelected!=null)
            OnSelected(item);
    }

    protected float msgDisTime = 0f;

    protected void ShowSkillTip()
    {
        if (itemSelect == null)
        {

            return;
        }

        UIUnitSelectWindowBehaviour window = GameController.Instance.Popup.GetPopup(PopupID.UISelectHero).GetComponent<UIUnitSelectWindowBehaviour>();
        window.ShowSkillTip(itemSelect.Data, itemSelect.SkillIcon.sprite);
    }

    protected void UpdateArrow()
    {
        ArrowUp.SetActive(false);
        ArrowDown.SetActive(false);

        if (itemSelect != null)
        {
            int index = items.IndexOf(itemSelect);

            if (index > 0)
            {
                ArrowUp.SetActive(true);
            }

            if (index < items.Count - 1)
            {
                UnitListItem nextItem = items[index + 1];

                if (nextItem.Locked == false)
                {
                    ArrowDown.SetActive(true);
                }
            }
        }
    }

    protected void Update()
    {
        if (selectChanged)
        {
            selectChanged = false;
            contentAnchoredPosition = new Vector2(content.anchoredPosition.x, itemFirst.rectTransform.anchoredPosition.y - itemSelect.rectTransform.anchoredPosition.y);

            if (!selectInit)
            {
                selectInit = true;
                content.anchoredPosition = contentAnchoredPosition;
            }
        }

        if (dataChanged)
        {
            dataChanged = false;
            //添加夥伴進去後的第一幀時 位置還未刷新，下一幀再初始化選中
            UpdateUnit();
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (isDragging || content.anchoredPosition.y == contentAnchoredPosition.y)
        {
            return;
        }

        float cutY = content.anchoredPosition.y;
        float toY = contentAnchoredPosition.y;

        float y = iTween.FloatUpdate(cutY, toY, 8f);

        if (Mathf.Abs(y - toY) < 1f)
        {
            y = toY;
        }

        content.anchoredPosition = new Vector2(contentAnchoredPosition.x, y);
    }

    protected bool MsgChanged = true;
    protected bool selectChanged = false;
    protected bool selectInit = false;
    protected bool dataChanged = true;
    protected const string ITEM_RENDERER = "Prefabs/Ui/UnitListItemRender";
}
