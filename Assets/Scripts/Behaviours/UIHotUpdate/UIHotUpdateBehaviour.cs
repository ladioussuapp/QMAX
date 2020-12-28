using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using System;
using UnityEngine.UI;

public class UIHotUpdateBehaviour : MonoBehaviour {

    public UIButtonBehaviour SubmitBtn;

    public UIButtonBehaviour CancelBtn;
    public Text UpgradeTip;
    private float updateTotalSize;

    private Action<int> m_CallBack;

    public void Awake()
    {
        GameController.Instance.AtlasManager.AddAtlas(Atlas.UIHotUpdate);
    }

    public void SetData(float value, Action<int> callBack)
    {
        updateTotalSize = value;
        m_CallBack = callBack;

        //UpgradeTip.text = string.Format("發現新版本，需下载{0}MB資源，\n是否更新？", updateTotalSize / 1024);
    }

    void Start () {
        SubmitBtn.onClick += OnSubmitClick;
        CancelBtn.onClick += OnCancelClick;


        UpgradeTip.text = string.Format("適配發現新版本，需下載{0}MB資源，\n是否更新?", (updateTotalSize / 1024).ToString("0.00"));
	}

    private void OnCancelClick(UIButtonBehaviour button)
    {
        if (m_CallBack != null)
        {
            m_CallBack(0);
            GameController.Instance.Popup.Close(PopupID.UIHotUpdate);
        }
    }

    private void OnSubmitClick(UIButtonBehaviour button)
    {
        if (m_CallBack != null)
        {
            m_CallBack(1);
            GameController.Instance.Popup.Close(PopupID.UIHotUpdate);
        }
    } 

    public void OnDestory()
    {
        SubmitBtn.onClick -= OnSubmitClick;
        CancelBtn.onClick -= OnCancelClick;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
