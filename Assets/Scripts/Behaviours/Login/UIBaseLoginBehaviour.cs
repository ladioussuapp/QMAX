
using Com4Love.Qmax;
using Com4Love.Qmax.Net;
using System;
using UnityEngine;
public class UIBaseLoginBehaviour : MonoBehaviour
{
    public virtual void InitLogin() { }

    public virtual void OnClickSwitchAcount()
    {

    }


    public virtual void OnStartClick()
    {

    }
    protected void DisplayErrorMsgWithAlert(int titleImg_, string bodySource_, string msg)
    {
        GameController gc = GameController.Instance;
        if (!gc.Popup.IsPopup(PopupID.UIReconnect))
        {
            UIReconnectBehaviour uiReconnect = gc.Popup.Open(PopupID.UIReconnect).GetComponent<UIReconnectBehaviour>();
            //不顯示重試按鈕
            uiReconnect.HideRetryButtom();
            uiReconnect.setWindowData(msg, titleImg_, bodySource_);

            uiReconnect.ButtonOK.onClick.AddListener(() =>
            {
                uiReconnect.ButtonOK.onClick.RemoveAllListeners();
                gc.Popup.Close(PopupID.UIReconnect, false);
            });

            uiReconnect.ButtonRetry.onClick.AddListener(() =>
            {
                uiReconnect.ButtonRetry.onClick.RemoveAllListeners();
                gc.Popup.Close(PopupID.UIReconnect, false);
            });
        }
    }



    protected int[] netDisconnect = { 1, 2, 4, 8, 9, 10, 11, 12, 13, 14, 15 };
    protected int[] serverMaintain = { 7, 120 };
    protected int[] accountInvalid = { 5, 100, 101, 102, 103, 104, 105, 106, 108, 109, 111, 112, 119 };


    protected void DisplayErrorMsgWithAlert(ResponseCode code)
    {
        //每個或多個status對應一個提示窗口的標題圖片名，內容部分的動畫prefab地址，提示文字的語言表ID。
        //目前這些數據沒有配表，寫死在代碼裡
        Q.Log("登錄失敗：status = " + code);
        //string titleImg = "UIWIFI_0010";
        string bodySource = "NetDisconnect";
        int title = 68;

        string msg = Utils.GetTextByStatusCode(code);
        //沒有找到語言表項目
        if (msg == "")
            msg = string.Format("登錄失敗：statusCode={0}", code);

        if (Array.IndexOf(netDisconnect, code) > -1)
        {
            title = 68;
            bodySource = "NetDisconnect";
        }
        else if (Array.IndexOf(serverMaintain, code) > -1)
        {
            title = 70;
            bodySource = "ServerMaintain";
        }
        else if (Array.IndexOf(accountInvalid, code) > -1)
        {
            title = 69;
            bodySource = "AccountInvalid";
        }

        DisplayErrorMsgWithAlert(title, bodySource, msg);
    }

}
