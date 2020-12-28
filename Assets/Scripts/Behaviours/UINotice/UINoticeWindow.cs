using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using System.Collections.Generic;
using Com4Love.Qmax.Data.VO;
using UnityEngine.UI;

public class UINoticeWindow : MonoBehaviour {
    public UIButtonBehaviour CloseButton;
    public UINoticeList List;

    public void SetData(List<NoticeInfo> datas)
    {
        //排序
        datas.Sort(NoticeCompare);

        foreach (NoticeInfo notice in datas)
        {
            List.AddItem(notice);
        }

        //TegraMask可能在list身上
        TegraMask tMask = List.GetComponent<TegraMask>();

        if (tMask != null)
        {
            tMask.SetMaterialDirty();
        }
    }

    private int NoticeCompare(NoticeInfo a, NoticeInfo b)
    {
        int res = 0;

        if (a.Time > b.Time)
        {
            res = -1;
        }
        else if (a.Time < b.Time)
        {
            res = 1;
        }

        return res;
    }

    void Awake()
    {
        CloseButton.onClick += CloseButton_onClick;

        if (GameController.Instance.AtlasManager != null)
        {
            GameController.Instance.AtlasManager.AddAtlas(Atlas.UINotice);
        }
    }
 
    public void OnDestroy()
    {
        CloseButton.onClick -= CloseButton_onClick;

        if (GameController.Instance.AtlasManager != null)
        {
            GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UINotice);
        }
    }
    
    void CloseButton_onClick(UIButtonBehaviour button)
    {
        CloseButton.onClick -= CloseButton_onClick;

        GameController.Instance.Popup.Close(PopupID.UINoticeWindow);
    }

 
}
