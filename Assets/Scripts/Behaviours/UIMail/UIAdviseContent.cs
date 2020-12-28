using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;

public class UIAdviseContent : MonoBehaviour
{
    public Text UserIdText;
    public Text QQText;
    public Text EMailText;
    public Text MsgText;
    public Text MsgPlaceholderText;
    public Text InfoText;
    public UIButtonBehaviour EnterButton;

    // Use this for initialization
    void Start()
    {
        EnterButton.onClick += EnterButton_onClick;

        if (GameController.Instance.Model.SdkData.userId != "")
        {
            if (GameController.Instance.Model.LoginData != null)
            {
                UserIdText.text = GameController.Instance.Model.LoginData.actorId.ToString();
            }
        }

        InfoText.text = Utils.GetTextByID(1679);
        GameController.Instance.ModelEventSystem.OnMailFeedBack += OnMailFeedBack;
    }

    public void OnDestroy()
    {
        EnterButton.onClick -= EnterButton_onClick;
        GameController.Instance.ModelEventSystem.OnMailFeedBack -= OnMailFeedBack;
    }

    void OnMailFeedBack(int stateCode)
    {
        GameController.Instance.ModelEventSystem.OnMailFeedBack -= OnMailFeedBack;
 
        if (stateCode == 0)
        {
            SendComplete();
        }
        else
        {
            SendLimit();
        }
    }

    private void EnterButton_onClick(UIButtonBehaviour button)
    {
        if (CheckInput())
        {
            //提交
            GameController.Instance.MailCtr.FeedBack(GameController.Instance.Model.LoginData.actorId, QQText.text, EMailText.text, MsgText.text);
        }
    }

    void SendLimit()
    {
        UIAlertBehaviour uiAlert = UIAlertBehaviour.Alert(Utils.GetText("反饋次數過多，請明天再試"), "", "", 2, 0, 0, (byte)UIAlertBehaviour.ButtonStates.ButtonOk);
        uiAlert.OnClickOKButton += UiAlert_OnClickOKButton;
        uiAlert.OnClickCacelButton += UiAlert_OnClickOKButton;
    }

    void SendComplete()
    {
        UIAlertBehaviour uiAlert = UIAlertBehaviour.Alert(Utils.GetText("此條信息發送成功"),"" ,"" , 2, 0, 0, (byte)UIAlertBehaviour.ButtonStates.ButtonOk);
        uiAlert.OnClickOKButton += UiAlert_OnClickOKButton;
        uiAlert.OnClickCacelButton += UiAlert_OnClickOKButton;
    }

    private void UiAlert_OnClickOKButton(UIButtonBehaviour obj)
    {
        GameController.Instance.Popup.Close(PopupID.UIMail);
    }

    bool CheckInput()
    {
        //界面已經做了限制 代碼限制不讓傳到後台
        if (QQText.text.Length > 128 || EMailText.text.Length > 128 || MsgText.text.Length > 300)
        {
            return false;
        }

        MsgText.text = MsgText.text.Trim();

        if (MsgText.text.Length == 0)
        {
            //必填項
            //MsgPlaceholderText.text = Utils.GetTextByID(1686);
            MsgPlaceholderText.color = Color.red;

            return false;
        }

        return true;
    }


}
