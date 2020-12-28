using UnityEngine;
using System.Collections;

public class ScrollRectItem : MonoBehaviour {
    [HideInInspector]
    public int index;
 
    public float Width
    {
        get
        {
            return ViewRect.sizeDelta.x;
        }
        set
        {
            ViewRect.sizeDelta = new Vector2(value, ViewRect.sizeDelta.y);
        }
    }

    public float Height
    {
        get
        {
            return ViewRect.sizeDelta.y;
        }
    }

    public RectTransform ViewRect
    {
        get
        {
            return transform as RectTransform;
        }
    }

    virtual public bool SelectAble
    {
        get
        {
            return true;
        }
    }

    protected bool isSelect = false;

    public bool IsSelect
    {
        get
        {
            
            return isSelect;
        }
        set
        {
            if (isSelect == value)
            {
                return;
            }

            isSelect = value;
            OnSelectChange();
        }
    }

    protected ScrollRectItemData data;

    public ScrollRectItemData Data
    {
        get
        {
            return data;
        }
        set
        {
            if (data == value)
            {
                return;
            }

            data = value;
            OnDataChange();
        }
    }

    public void DataChange()
    {
        //直接修改了引用的data後 可以調用此接口刷新
        OnDataChange();
    }

    virtual protected void OnDataChange()
    {

    }

    virtual protected void OnSelectChange()
    {
        
    }
 
}

public class ScrollRectItemData
{
 
    public ScrollRectItemData()
    {
        
    }

}
