using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using System.Collections.Generic;
using UnityEngine;
using DoPlatform;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Net;

public class Bootstrap : MonoBehaviour
{
    /// <summary>
    /// 使用網絡的類型
    /// </summary>
    public NetType UsedNetType;

    /// <summary>
    /// 是否自動登錄
    /// </summary>
    public bool AutoLogin = false;
    public string Username;
    public string Password;

#if AUTO_FIGHT
    public bool isOpenGuide = false;
#else
    public bool isOpenGuide = true;
#endif

    public GuideVersion guideVersion = GuideVersion.Version_1;

    private GameController gameCtr;

    void Awake()
    {
        gameCtr = GameController.Instance;

        if (!gameCtr.IsLaunched)
        {
            GuideManager.getInstance().isOpenGuide = isOpenGuide;
            GuideManager.getInstance().version = guideVersion;
            Q.Log("正在使用的網絡類型: {0}", UsedNetType);

            gameCtr.UsedNetType = UsedNetType;
            gameCtr.Launch();
            InitHttpRoot(); //初始化網絡地址
            // PlayerPrefs.DeleteAll();   //每次開機時  清空所有所有玩家帳戶資料

            if (PackageConfig.DOSDK)
            {
                DoSDK.Instance.setAppkeys(PackageConfig.YOUAI_APP_KEY, PackageConfig.YOUAI_APP_ID);
                DoSDK.Instance.setInitCallback(InitSDKCallback);
                DoSDK.Instance.initDoSDK();
                gameCtr.InitDoSDKCallbacks();
            }
            else
            {
                Q.Log("PackageConfig.DOSDK == false");
                InitSDKCallback(Callback.INIT, ResultStatus.FINISHED);
            }

            gameCtr.Model.SdkData.appKey = PackageConfig.YOUAI_APP_KEY;

            gameCtr.Model.LoadInitCfg(null);

            if (PackageConfig.BUGLY)
            {
                BuglyAgent.ConfigDebugMode(true);
                BuglyAgent.RegisterLogCallback(CallbackDelegate.Instance.OnApplicationLogCallbackHandler);
                BuglyAgent.InitWithAppId(PackageConfig.BUGLY_APP_ID);
                BuglyAgent.ConfigAutoReportLogLevel(LogSeverity.LogWarning);
                BuglyAgent.EnableExceptionHandler();
            }
        }

        if (gameCtr.LoginCtr != null && gameCtr.LoginCtr.IsLogin)
        {
            return;
        }

        if (Application.isEditor)
        {
            if (AutoLogin)
            {
                Q.Log("AutoLogin");
                gameCtr.LoginCtr.NoSdkLogin(Username, Password);
                GameController.Instance.ModelEventSystem.OnLoginProgress += OnLoginProgress;
            }
        }
        else
        {
            isOpenGuide = true;
        }
        //標註-------------------------------------------------------------------------------------12/24
        gameCtr.Model.Language = Language.ChineseTraditional;
    }

    //windows版本的網絡默認去讀本地目錄獲取
    void InitHttpRoot()
    {
        Dictionary<NetType, string> dict = new Dictionary<NetType, string>();
        dict.Add(NetType.Extranet, PackageConfig.HTTP_ROOT_EXTRA);
        dict.Add(NetType.Intranet, PackageConfig.HTTP_ROOT_INTRA);
        dict.Add(NetType.Localhost, PackageConfig.HTTP_LOCALHOST);
        dict.Add(NetType.Xiaolu, PackageConfig.HTTP_ROOT_LU);
        dict.Add(NetType.Ligang, PackageConfig.HTTP_ROOT_LIGANG);
        dict.Add(NetType.Beijing, PackageConfig.HTTP_BEIJING);
        dict.Add(NetType.Outline, "");

#if UNITY_STANDALONE
        string url = PathKit.ReadUTFString("url.txt");

        if (url != null && url != "")
        {
            gameCtr.LoginCtr.HttpRoot = url;
        }
        else
        {
            gameCtr.LoginCtr.HttpRoot = string.Format("{0}{1}", dict[UsedNetType], PackageConfig.LOGIN_PORT);

#if !UNITY_EDITOR
            PathKit.WriteUTFString("url.txt", gameCtr.LoginCtr.HttpRoot);
#endif
        }
#else
        gameCtr.LoginCtr.HttpRoot = string.Format("{0}{1}", dict[UsedNetType], PackageConfig.LOGIN_PORT);
        gameCtr.LoginCtr.RechargeRoot = string.Format("{0}{1}", dict[UsedNetType], PackageConfig.RECHARGE_PORT);

#endif

        gameCtr.LoginCtr.CdnRoot = PackageConfig.CDN_URL;   //暫時指定正式目錄
    }

    private void InitSDKCallback(Callback callback, ResultStatus resultStatus)
    {
        string log = string.Format("call dosdk initSDKCallback!!!!!!!!!!! Callback:{0} ResultStatus:{1} ", callback, resultStatus);
        Q.Log(log);
        if (resultStatus != ResultStatus.ERROR_INIT_FAILURE)
        {
            if (PackageConfig.DOSDK)
            {
                DoSDK.Instance.setDebug(true);
                gameCtr.Model.SdkData.channalID = DoSDK.Instance.channel();
                gameCtr.Model.SdkData.sdkVersion = DoSDK.Instance.version();
                gameCtr.Model.SdkData.platformID = DoSDK.Instance.channel();
            }
            else
            {
                gameCtr.Model.SdkData.channalID = "pc_channel";
                gameCtr.Model.SdkData.sdkVersion = "pc_version";
                gameCtr.Model.SdkData.platformID = "base2";
            }


            GameObject bsbGO = GameObject.Find("SceneBehaviour") as GameObject;
            if (bsbGO != null)
            {
                BootstrapSceneBehaviour bsb = bsbGO.GetComponent<BootstrapSceneBehaviour>();
                bsb.Show();
            }
        }
        else
        {
            GameObject bsbGO = GameObject.Find("SceneBehaviour") as GameObject;
            if (bsbGO != null)
            {
                BootstrapSceneBehaviour bsb = bsbGO.GetComponent<BootstrapSceneBehaviour>();
                bsb.initSDKFailShow();
            }
        }

        //Destroy(gameObject);
    }


    private void OnLoginProgress(ResponseCode code, LoginState loginState)
    {
        if (loginState != LoginState.ReqGameServerInfo)
            return;

        GameController gc = GameController.Instance;
        gc.ModelEventSystem.OnLoginProgress -= OnLoginProgress;
        if (code == ResponseCode.SUCCESS)
        {
            gc.LoginCtr.ConnectGameServer(gc.Model.SdkData.GetAddressResponse);
        }
        else
        {
            Q.Log("自動登錄失敗：" + code);
        }
    }
}
