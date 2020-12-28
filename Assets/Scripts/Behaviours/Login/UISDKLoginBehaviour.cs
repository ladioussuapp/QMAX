using Com4Love.Qmax;
using Com4Love.Qmax.Net;
using DoPlatform;

public class UISDKLoginBehaviour : UIBaseLoginBehaviour
{
    private GameController gCtr;

    public override void InitLogin()
    {
        if (!DoSDK.Instance.isLogin())
        {
            GameController.Instance.LoginCtr.DoSdkLogin();
        }

        gCtr = GameController.Instance;
        gCtr.ModelEventSystem.OnLoginProgress += OnLoginProgress;
    }



    public override void OnClickSwitchAcount()
    {
        gCtr.LoginCtr.SwitchAccount();
    }


    public override void OnStartClick()
    {
        gCtr.LoginCtr.ConnectGameServer(GameController.Instance.Model.SdkData.GetAddressResponse);
    }


    private void OnLoginProgress(ResponseCode code, LoginState state)
    {
        if(code != ResponseCode.SUCCESS)
        {
            DisplayErrorMsgWithAlert(code);
        }
    }
}
