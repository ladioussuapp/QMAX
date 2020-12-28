using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.VO;
using System;

public class UIUnlockListItem : MonoBehaviour {
    public Text MsgText;
    public UIButtonBehaviour EnterButton;
    public Image ArrowImg;
    public event Action<StageLockInfo> OnEnterClick;
    private StageLockInfo data;

	// Use this for initialization
	void Start () {
        EnterButton.onClick += EnterButton_onClick;    
	}
    void OnDestroy()
    {
        EnterButton.onClick -= EnterButton_onClick;    
    }

    void EnterButton_onClick(UIButtonBehaviour button)
    {
        if (OnEnterClick != null)
        {
            OnEnterClick(data);
        }
    }

	
    public void SetData(StageLockInfo data)
    {
        this.data = data;

        MsgText.text = data.Msg;
 
        EnterButton.gameObject.SetActive(!data.Res);
        ArrowImg.gameObject.SetActive(data.Res);
    }
}
