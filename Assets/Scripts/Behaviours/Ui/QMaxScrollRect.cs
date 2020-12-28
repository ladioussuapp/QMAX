using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Com4Love.Qmax;


//控制ScrollRect行為 待重寫
public class QMaxScrollRect : MonoBehaviour, IBeginDragHandler, IInitializePotentialDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary> 
    /// 參數：上一個選中，當前選中
    /// </summary>
    public event Action<ScrollRectItem, ScrollRectItem> OnSelectChange;
    //public event Action<Vector2> OnScroll;
    //public event Action<Vector2> OnDragEvent;
    public event Action<PointerEventData> OnDragBeginEvent;
    public List<ScrollRectItem> items;
    public RectTransform content;
    public bool horizontal = false;
    public bool vertical = false;
    //拖動的
    public float sensitivity = 1f;
    //切換項時需要拖動的距離比例
    public float switchSensitivity = .3f;
    public float tweenSensitivity = 0.1f;
    public bool autoSelect = true;
    [NonSerialized]
    [HideInInspector]
    public bool DragAble = true;        //不要改動以上變量的命名（大小寫），否則鏈接的資源會丟

    [HideInInspector]
    public ScrollRectItem itemFirst;

    ScrollRectItem selectItem;
    bool isDragging = false;
    bool isScrolling = false;   //當contentAnchoredPosition修改時 設置為true，表示需要滾動，當滾動到contentAnchoredPosition位置時設置成false
    //默認的內容位置 此位置時第一項正好選中狀態
    Vector2 defaultContentAnchoredPosition;
    //目標位置
    Vector2 contentAnchoredPosition;
    List<ScrollRectItemData> datas;

    /// <summary>
    /// 目標位置
    /// </summary>
    public Vector2 ContentAnchoredPosition
    {
        get
        {
            return contentAnchoredPosition;
        }
    }

    public bool IsScrolling
    {
        get
        {
            return isScrolling;
        }
    }

    public bool IsDragging
    {
        get
        {
            return isDragging;
        }
    }

    public int SelectIndex
    {
        get
        {
            return items.IndexOf(SelectItem);
        }
        set
        {
            Q.Assert(value <= items.Count - 1, "選中項索引超限");

            if (itemFirst == null)
            {
                defaultSelectIndex = value;
            }
            else
            {
                ScrollRectItem item = items[value];
                SelectItem = item;
            }
        }
    }

    /// <summary>
    /// 選中項
    /// </summary>
    public ScrollRectItem SelectItem
    {
        get
        {
            return selectItem;
        }
        set
        {
            if (selectItem == value)
            {
                return;
            }

            if (itemFirst == null)
            {
                //還沒有初始化 沒執行start
                defaultSelectIndex = items.IndexOf(value);
                return;
            }

            ScrollRectItem itemSelectOld = selectItem;

            if (itemSelectOld != null)
            {
                itemSelectOld.IsSelect = false;
            }

            selectItem = value;
            selectItem.IsSelect = true;

            //設置內容的緩動坐標
            //沒有縮放 itemSelect.ViewRect.anchoredPosition的值與itemFirst.ViewRect.anchoredPosition一樣時正好在目標位置
            contentAnchoredPosition = defaultContentAnchoredPosition - (selectItem.ViewRect.anchoredPosition - itemFirst.ViewRect.anchoredPosition);

            if (OnSelectChange != null)
            {
                OnSelectChange(itemSelectOld, selectItem);
            }
        }
    }


    //內容直接移動到位置
    public void ContentToPosition()
    {
        content.anchoredPosition = contentAnchoredPosition;
    }


    public List<ScrollRectItemData> GetDatas()
    {
        return datas;
    }

    public void SetDatas(List<ScrollRectItemData> itemDatas)
    {
        Q.Assert(items[0] != null, "QMaxScrollRect::SetDatas 需要有一個item當做拷貝的prefab");

        datas = itemDatas;
        ScrollRectItem item = items[0];   
        ScrollRectItemData data;
        RectTransform itemT;

        int itemCount = items.Count;

        for (int i = 0; i < datas.Count; i++)
        {
            data = datas[i];

            if (i < itemCount)
            {
                item = items[i];
            }
            else
            {
                itemT = GameObject.Instantiate<GameObject>(item.gameObject).GetComponent<RectTransform>();
                itemT.gameObject.name = string.Format("ScrollRectItem{0}" , i);
                itemT.SetParent(content);
                itemT.localScale = new Vector3(1, 1, 1);
                item = itemT.GetComponent<ScrollRectItem>();
                items.Add(item);
            }

            item.Data = data;
        }
    }

    public int defaultSelectIndex = 0;

    void Awake()
    {
        defaultContentAnchoredPosition = content.anchoredPosition;
        contentAnchoredPosition = content.anchoredPosition;
        //content.localPosition = new Vector3(content.localPosition.x, content.localPosition.y,0);
    }

    void Start()
    {
        ScrollRectItem item;
        itemFirst = items[0];

        for (int i = 0; i < items.Count; i++)
        {
            item = items[i];
            item.index = i;

            if (i == defaultSelectIndex)
            {
                ContentSizeFitter sizeFitter = content.GetComponent<ContentSizeFitter>();

                if (sizeFitter == null)
                {
                    SetSelectDefault(item);
                }
                else
                {
                    StartCoroutine(DelaySelectDefault(item));
                }
            }
        }
    }

    void SetSelectDefault(ScrollRectItem item)
    {
        if (selectItem == null)
        {
            SelectItem = item;
            ContentToPosition();
        }
    }

    //start之後，直接設置選中，會獲取到錯誤的坐標。 
    IEnumerator DelaySelectDefault(ScrollRectItem item)
    {
        yield return new WaitForEndOfFrame();

        SetSelectDefault(item);
    }
 
    public List<ScrollRectItem> GetItems()
    {
        return items;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!DragAble)
        {
            return;
        }

        isDragging = false;
        CheckItemSelect();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!DragAble)
        {
            return;
        }

        isDragging = true;
        float anchoredPosX = horizontal ? content.anchoredPosition.x + eventData.delta.x * sensitivity : content.anchoredPosition.x;
        float anchoredPosY = vertical ? content.anchoredPosition.y + eventData.delta.y * sensitivity : content.anchoredPosition.y;
        //Vector2 fromPosition = content.anchoredPosition;
        content.anchoredPosition = new Vector2(anchoredPosX, anchoredPosY);

        //if (OnDragEvent != null)
        //{
        //    OnDragEvent(content.anchoredPosition - fromPosition);
        //}

        //if (OnScroll != null)
        //{
        //    OnScroll(content.anchoredPosition - fromPosition);
        //}
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!DragAble)
        {
            return;
        }

        isDragging = true;

        if (OnDragBeginEvent != null)
        {
            OnDragBeginEvent(eventData);
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        //Debug.Log("OnInitializePotentialDrag:" + eventData);

    }

    //拖動結束後檢查當前選中是否更改
    void CheckItemSelect()
    {
        if (!autoSelect || items.Count == 0)
        {
            return;
        }

        ScrollRectItem item;
        float nearest = itemFirst.Width * switchSensitivity;
        Vector2 selectOffset = contentAnchoredPosition - content.anchoredPosition;
        int selectIndex = items.IndexOf(selectItem);
        int selectIndexNew = selectIndex;
        float offset = 0;

        if (horizontal)
        {
            offset = selectOffset.x;
        }
        else if (vertical)
        {
            offset = selectOffset.y;
        }

        if (Mathf.Abs(offset) > nearest)
        {
            //切换
            if (offset > 0f)
            {
                selectIndexNew = selectIndexNew == items.Count - 1 ? selectIndexNew : selectIndexNew + 1;
            }
            else
            {
                selectIndexNew = selectIndexNew == 0 ? 0 : selectIndexNew - 1;
            }
        }

        item = items[selectIndexNew];

        if (item != selectItem && item.SelectAble)
        {
            SelectItem = item;
        }
    }

    void LateUpdate()
    {
        //if (!isScrolling)
        //{
        //    return;
        //}

        if (isDragging)
        {
            isScrolling = false;
            return;
        }

        if (Vector3.Distance(content.anchoredPosition, contentAnchoredPosition) <= 1f)
        {
            isScrolling = false;
            content.anchoredPosition = contentAnchoredPosition;
            return;
        }

        isScrolling = true;
        Vector2 fromPosition = content.anchoredPosition;
        content.anchoredPosition = Vector2.Lerp(fromPosition, contentAnchoredPosition, tweenSensitivity);
 
        //if (OnScroll != null)
        //{
        //    OnScroll(content.anchoredPosition - fromPosition);
        //}
    }

}
