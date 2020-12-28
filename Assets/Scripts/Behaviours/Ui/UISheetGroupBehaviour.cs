using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Com4Love.Qmax;

public class UISheetGroupBehaviour : MonoBehaviour
{
    public delegate void SheetChangeDelegate(UISheetBehaviour sheet);
    public event SheetChangeDelegate OnChange;
    [SerializeField]
    private List<UISheetBehaviour> items;
    private UISheetBehaviour selected;

    //void Awake()
    //{
        //items由unity初始化
        //items = new List<UISheetBehaviour>();
    //}

    public void Add(UISheetBehaviour sheet)
    {
        if (items == null)
            return;

        items.Add(sheet);
        sheet.transform.SetParent(transform);
        sheet.transform.localScale = new Vector3(1, 1, 1);
    }

    public void SetSelected(int index)
    {
        if (items == null || items.Count == 0)
            return;

        Q.Assert(items.Count > index, "UISheetGroupBehaviour::SetSelected() 索引超限");

        UISheetBehaviour sheet = items[index];
        sheet.IsSelect = true;
        //SetSelected(sheet);
    }

    public void SetSelected(UISheetBehaviour sheet)
    {
        if (items == null || items.Count == 0)
            return;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == sheet)
            {
                selected = sheet;
                continue;
            }
            items[i].IsSelect = false;
        }
        if (OnChange != null)
            OnChange(sheet);
    }

    public UISheetBehaviour GetSelected()
    {
        return selected;
    }

    public int GetSelectIndex()
    {
        return items == null ? -1 : items.IndexOf(selected);
    }
}
