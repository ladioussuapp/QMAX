using Com4Love.Qmax;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Net;
using System;
using UnityEngine;

/// <summary>
/// 當走自建賬號系統時，會按照這個流程走
/// </summary>
public class UIEditorLoginBehaviour : UIBaseLoginBehaviour
{
    /// <summary>
    /// 登錄窗口
    /// </summary>
    private UILoginWindowBehaviour loginWindow;

    private GameController gameCtr;

    public override void InitLogin()
    {
        gameCtr = GameController.Instance;
        CheckAccountCache();
        gameCtr.ModelEventSystem.OnLoginProgress += OnLoginProgress;
    }


    /// <summary>
    /// 點擊切換賬號按鈕
    /// </summary>
    public override void OnClickSwitchAcount()
    {
        var defAccount = "";
        var defPas = "";
        if (PlayerPrefsTools.HasKey(OnOff.Account) && PlayerPrefsTools.HasKey(OnOff.AccountPass))
        {
            defAccount = PlayerPrefsTools.GetStringValue(OnOff.Account);
            defPas = PlayerPrefsTools.GetStringValue(OnOff.AccountPass);
        }
        OpenLoginWindow();
        loginWindow.SetAccount(defAccount, defPas);
    }

    /// <summary>
    /// 點擊開始按鈕
    /// </summary>
    public override void OnStartClick()
    {
        if (gameCtr.Model.SdkData.GetAddressResponse != null)
        {
            gameCtr.LoginCtr.ConnectGameServer(gameCtr.Model.SdkData.GetAddressResponse);
        }
        else if (PlayerPrefsTools.HasKey(OnOff.Account) && PlayerPrefsTools.HasKey(OnOff.AccountPass))
        {
            var defAccount = PlayerPrefsTools.GetStringValue(OnOff.Account);
            var defPas = PlayerPrefsTools.GetStringValue(OnOff.AccountPass);
            gameCtr.LoginCtr.NoSdkLogin(defAccount, defPas);
        }
    }


    private void OnClickLoginWinRegister(string username, string password)
    {
        gameCtr.LoginCtr.NoSdkRegister(username, password);
    }

    private void OnClickLoginWinLogin(string username, string password)
    {
        gameCtr.LoginCtr.NoSdkLogin(username, password);
    }



    private void CheckAccountCache()
    {
        //如果有默認賬戶，則直接登錄
        if (PlayerPrefsTools.HasKey(OnOff.Account) && PlayerPrefsTools.HasKey(OnOff.AccountPass))
        {
            var defAccount = PlayerPrefsTools.GetStringValue(OnOff.Account);
            var defPas = PlayerPrefsTools.GetStringValue(OnOff.AccountPass);
            gameCtr.LoginCtr.NoSdkLogin(defAccount, defPas);
        }
        else
        {
            //沒有默認賬號，彈出窗口讓填賬號密碼
            OpenLoginWindow();
            loginWindow.SetAccount("", "");
        }
    }


    /// <summary>
    /// 打開登陸界面
    /// </summary>
    private void OpenLoginWindow()
    {
        if (gameCtr.Popup.IsPopup(PopupID.UILoginWindow))
        {
            loginWindow = gameCtr.Popup.GetPopup(PopupID.UILoginWindow).GetComponent<UILoginWindowBehaviour>();
        }
        else
        {
            loginWindow = gameCtr.Popup.Open(PopupID.UILoginWindow, null, false).GetComponent<UILoginWindowBehaviour>();
            loginWindow.OnClickLogin += OnClickLoginWinLogin;
            loginWindow.OnClickRegister += OnClickLoginWinRegister;
        }
    }


    private void OnLoginProgress(ResponseCode code, LoginState loginState)
    {
        switch (loginState)
        {
            case LoginState.Login:
            case LoginState.Register:
                {
                    if (code != ResponseCode.SUCCESS)
                        DisplayErrMsgOnLoginWin(code);
                    break;
                }
            case LoginState.ReqGameServerInfo:
                if (code == ResponseCode.SUCCESS)
                {
                    //請求服務器信息成功，關閉登錄界面
                    //等待點擊開始按鈕
                    if (gameCtr.Popup.IsPopup(PopupID.UILoginWindow))
                        gameCtr.Popup.Close(PopupID.UILoginWindow, false);
                }
                else
                {
                    DisplayErrorMsgWithAlert(code);
                }
                break;
            case LoginState.ConnectGameServer:
                if (code != ResponseCode.SUCCESS)
                {
                    DisplayErrMsgOnLoginWin(code);
                    //連接遊服失敗，重新打開登錄界面
                    OpenLoginWindow();
                }
                break;
            case LoginState.CreateActor:
                if (code != ResponseCode.SUCCESS)
                {
                    DisplayErrorMsgWithAlert(code);
                }
                break;
            case LoginState.ActorLogin:
                if (code == ResponseCode.SUCCESS)
                {
                    //沒有使用DOSDK且是在iOS平台上，需要請求購買信息
                    if (!PackageConfig.DOSDK && 
                        Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        //角色登錄成功之後，請求可購買物品信息
                        gameCtr.LoginCtr.ReqPurchaseInfo(() => { });
                    }
                    else
                    {
                        //使用了DOSDK，或者不是在iOS平台，不需要走這個配置
                        gameCtr.ModelEventSystem.OnLoginProgress(ResponseCode.SUCCESS, LoginState.ReqPurchaseInfo);
                    }
                }
                else
                {
                    DisplayErrorMsgWithAlert(code);
                }
                break;
            case LoginState.ReqPurchaseInfo:
                //do nothing
                break;
        }
    }


    /// <summary>
    /// 錯誤信息顯示分兩種：一種是顯示在登錄框上，一種是以彈框形式顯示
    /// </summary>
    /// <param name="failCode"></param>
    private void DisplayErrMsgOnLoginWin(ResponseCode failCode)
    {
        OpenLoginWindow();
        loginWindow.SetMsg(Utils.GetTextByStatusCode(failCode));
    }

    protected void OnDestroy()
    {
        if (loginWindow != null)
        {
            loginWindow.OnClickRegister -= OnClickLoginWinLogin;
            loginWindow.OnClickLogin -= OnClickLoginWinLogin;
        }
    }
}
