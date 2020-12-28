using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Net;
using Com4Love.Qmax.Tools;
using DoPlatform;
using PathologicalGames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;


/// <summary>
/// 全局唯一單例，控制層的總集
/// </summary>
public class GameController
{
    static protected GameController _instance;
    static public GameController Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameController();

            return _instance;
        }
    }


    /// <summary>
    /// 網路類型
    /// </summary>
    public NetType UsedNetType = NetType.Extranet;

    public Transform Root;
    public Queue<Action> InvokeQueue;

    public QmaxClient Client;
    public LoginCtr LoginCtr;
    public SceneCtr SceneCtr;
    public PlayerCtr PlayerCtr;
    public StageCtr StageCtr;
    public UnitCtr UnitCtr;
    public PlayingRuleCtr PlayingRuleCtr;
    public TreeFightCtr TreeFightCtr;
    public GoodsCtr GoodsCtr;
    public MailCtr MailCtr;
    public PropCtr PropCtr;
    public ButtonTipsCtr ButtonTipsCtr;

    /// <summary>
    /// 控制AssetBundle加載
    /// </summary>
    public AssetBundleManager AssetBundleMrg;

    public QmaxModel Model;

    /// <summary>
    /// View層的事件系统
    /// </summary>
    public ViewEventSystem ViewEventSystem;
    /// <summary>
    /// Model層的事件系统
    /// </summary>
    public ModelEventSystem ModelEventSystem;

    public GOPoolManager PoolManager;
    public ServerTime ServerTime;
    public Popup Popup;
    public QMaxAssetsFactory QMaxAssetsFactory;
    public AudioManager AudioManager;
    /// <summary>
    /// 所有程序的特效在這裡統一提供接口
    /// </summary>
    public EffectProxy EffectProxy;

    /// <summary>
    /// 控製圖集加載的管理器
    /// </summary>
    public AtlasManager AtlasManager;

    /// <summary>
    /// 是否已經啟動
    /// </summary>
    public bool IsLaunched = false;

    /// <summary>
    /// 測試戰鬥場景加載的Stopwatch id
    /// </summary>
    public int TestPrefID = 0;

    public MonoBehaviour MonoBeh = null;

    protected bool isClientDisConnected = false;

    /// <summary>
    /// 是否是弱連接
    /// </summary>
    public bool IsSoftLink = false;

    public AudioMixer AudioMixer;

    public IAPKit IapKit;

    public GameController() { }



    /// <summary>
    /// 因為各個Controller裡會引用GameBehaviour.Instrance
    /// 如果在構造函數里生成各個Controller，會造成循環引用。因此新建一個Launch()函数。
    /// </summary>
    public void Launch()
    {
        Debug.LogFormat("dataPath={0}\npersistentDataPath={1}\nstreamingAssetsPath={2}\ntemporaryCachePath={3}\nSystem.Environment.CurrentDirectory={4}",
            Application.dataPath,
            Application.persistentDataPath,
            Application.streamingAssetsPath,
            Application.temporaryCachePath,
            System.Environment.CurrentDirectory);

        if (IsLaunched)
            return;

        // Q.LogTagWhiteList = new List<LogTag>();
        //Q.LogTagWhiteList.Add(LogTag.Net);
        // Q.LogTagWhiteList.Add(LogTag.Test);
        //Q.LogTagWhiteList.Add(LogTag.Assert);
        //Q.LogTagBlackList = new List<LogTag>();
        //Q.LogTagBlackList.Add(LogTag.Battle);

        SkeletonAnimator.DefaultSkipRate = 2;

        ViewEventSystem = new ViewEventSystem();
        ModelEventSystem = new ModelEventSystem();

        Model = new QmaxModel(ModelEventSystem);


        //Client = UsedNetType == NetType.Outline ? new QmaxOutlineClient(Model) : new QmaxClientEx();
        if (IsSoftLink)
            Client = new QmaxClient2();
        else
            Client = new QmaxClientEx();


        IsLaunched = true;
        Application.targetFrameRate = 60;

        ServerTime = new ServerTime();
        Popup = new Popup();
        QMaxAssetsFactory = new QMaxAssetsFactory();
        AtlasManager = new AtlasManager("Textures/SpriteSheets");
        Model.SdkData = new QmaxModel.SdkLoginData();
        AudioMixer = Resources.Load<AudioMixer>("AudioMixer");
        Q.Assert(AudioMixer != null);

        LoginCtr = new LoginCtr();
        SceneCtr = new SceneCtr();
        PlayerCtr = new PlayerCtr();
        StageCtr = new StageCtr();
        UnitCtr = new UnitCtr();

        PropCtr = new PropCtr();
        ButtonTipsCtr = new ButtonTipsCtr();
        PlayingRuleCtr = new PlayingRuleCtr(this);
        TreeFightCtr = new TreeFightCtr();
        GoodsCtr = new GoodsCtr();
        MailCtr = new MailCtr();

        //在Scene裡創建一個GameController
        GameObject prefab = Resources.Load<GameObject>("Prefabs/GameController");
        Root = UnityEngine.Object.Instantiate<GameObject>(prefab).transform;
        MonoBeh = Root.GetComponent<EventDispatcher>();
        EventDispatcher ed = MonoBeh as EventDispatcher;
        PoolManager = new GOPoolManager(Root.GetComponent<SpawnPool>(), Root);
        AudioManager = Root.GetComponent<AudioManager>();

        Loom loom = Loom.Current;
        loom.gameObject.transform.SetParent(Root);
        CreateAssetBundleMgr();

        EffectProxy = new EffectProxy();

        ed.StartEvent += OnStartEvent;
        ed.UpdateEvent += OnUpdateEvent;
        ed.ApplicationQuitEvent += OnApplicationQuitEvent;
        ed.DestroyEvent += OnDestroyEvent;
        ed.AppLicationPauseEvent += OnApplicationPauseEvent;
        initLoadCloud();

        ModelEventSystem.BeforeSceneChangeEvent += BeforeSceneChangeEvent;

        ViewEventSystem.BackToLoginSceneEvent += OnBackToLoginSceneEvent;


        ViewEventSystem.JumpSceneShowCloudEvent += OnJumpSceneOpenCloudEvent;

        ViewEventSystem.JumpSceneHideCloudEvent += OnJumpSceneHideCloudEvent;

        Client.ErrorResponseEvent += OnClinetErrorResponseCallback;
        //Client.AddResponseCallback(Module.User, OnKickOffEvent);
        if (IsSoftLink)
            (Client as QmaxClient2).ResponseLockEvent += OnNetLoadingCallback;
        else
            (Client as QmaxClientEx).ResponseLockEvent += OnNetLoadingCallback;
    }

    private Transform loadCloud;
    private void initLoadCloud()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Ui/UILoadingCloud");
        loadCloud = UnityEngine.Object.Instantiate<GameObject>(prefab).transform;
        loadCloud.name = string.Format("{0}", "LoadCloud");
        loadCloud.SetParent(Root);
        loadCloud.gameObject.SetActive(false);
    }


    public void OnJumpSceneOpenCloudEvent(Action callBack = null)
    {
        if (loadCloud != null)
        {
            UILoadingCloudBehaviour loadBeh = loadCloud.GetComponent<UILoadingCloudBehaviour>();
            loadBeh.PlayShowCloud(callBack);
        }
    }

    public void OnJumpSceneHideCloudEvent(Action callBack = null, Action hideCallBack = null)
    {
        if (loadCloud != null && SceneCtr.CloudFlag)
        {
            UILoadingCloudBehaviour loadBeh = loadCloud.GetComponent<UILoadingCloudBehaviour>();
            loadBeh.PlayHideCloud(callBack, hideCallBack);
        }
    }


    private void OnApplicationPauseEvent(bool isPause)
    {
        if (!isPause)
        {
            //從桌面返回到遊戲中 序列化推送的信息。將推送信息序列化到model的PushData屬性中
            //model保持pushData的引用，在VIEW中自己通過自己的邏輯去執行pushdata裡面的信息
            ExecutePushData();
        }

        if (ViewEventSystem.ApplicationPauseEvent != null)
            ViewEventSystem.ApplicationPauseEvent(isPause);
    }

    public void InitDoSDKCallbacks()
    {
        DoSDK.Instance.setLoginCallback(DoSDKLoginCallBack);
        DoSDK.Instance.setSwitchAccountListener(DoSdkSwitchAccountCallBack);
        DoSDK.Instance.setExitCallback(DoSdkExit);
        DoSDK.Instance.setLogoutCallback(DoSdkLogoutCallBack);
        DoSDK.Instance.setPayCallback(DoSdkPayCallBack);
    }

    private void DoSdkPayCallBack(Callback callback, ResultStatus resultStatus)
    {
        if (Callback.PAY == callback)
        {
            if (resultStatus != ResultStatus.FINISHED)
            {
                if (ModelEventSystem.OnBuyGems != null)
                    ModelEventSystem.OnBuyGems(-1);

                PropCtr.TemSelectPropReset();

                switch (resultStatus)
                {
                    case ResultStatus.ERROR_PAY_ORDER_NOT_NULL:
                        //todo 支付訂單不能為空
                        Q.Log("支付訂單不能為空");
                        break;
                    case ResultStatus.ERROR_PAY_AMOUNT_NOT_NULL:
                        //todo 支付數量不能為空
                        Q.Log("支付數量不能為空");
                        break;
                    case ResultStatus.ERROR_PAY_PRODUCT_ID_NOT_NULL:
                        //todo 商品ID不能為空
                        Q.Log("商品ID不能為空");
                        break;
                    case ResultStatus.ERROR_PAY_PRODUCT_NAME_NOT_NULL:
                        //todo 商品名字不能為空
                        Q.Log("商品名字不能為空");
                        break;
                    case ResultStatus.ERROR_PAY_PAYDES_NOT_NULL:
                        //todo 商品描述不能為空
                        Q.Log("商品描述不能為空");
                        break;
                    case ResultStatus.ERROR_PAY_FAILURE:
                        //todo 支付失敗
                        Q.Log("支付失敗");
                        break;
                    default:

                        break;
                }
            }
            else
            {
                Client.RechargeRefresh();
            }
        }
    }

    /// <summary>
    /// 登錄回調
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="resultStatus"></param>
    private void DoSDKLoginCallBack(Callback callback, ResultStatus resultStatus)
    {
        string log = string.Format("call dosdk logininCallBack!!!!!!!!!!! Callback:{0} ResultStatus:{1} ", callback, resultStatus);
        Q.Log(log);

        LoginCtr.OnDoSdkLoginCallback(resultStatus);
    }

    /// <summary>
    /// 註銷回調
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="resultStatus"></param>
    private void DoSdkLogoutCallBack(Callback callback, ResultStatus resultStatus)
    {
        string log = string.Format("call dosdk logoutCallBack!!!!!!!!!!! Callback:{0} ResultStatus:{1} ", callback, resultStatus);
        Q.Log(log);
        if (ResultStatus.ERROR_LOGOUT_FAILURE != resultStatus)
        {
            if (Application.loadedLevelName != Scenes.LoginScene.ToString())
            {
                if (ViewEventSystem.BackToLoginSceneEvent != null)
                    ViewEventSystem.BackToLoginSceneEvent();
            }
        }

        //把用戶名設置為空
        if (PlayerPrefsTools.HasKey(OnOff.Account))
        {
            PlayerPrefsTools.SetStringValue(OnOff.Account, "");
        }
    }

    private void DoSdkExit(Callback callback, ResultStatus resultStatus)
    {
        Q.Log("Call dosdk exit functioin CallBack:" + callback + " ResultStatus:" + resultStatus);
        if (resultStatus == ResultStatus.FINISHED)
        {
            QuitGame();
        }
    }

    /// <summary>
    /// 切換帳號回調
    /// </summary>
    /// <param name="callBack"></param>
    /// <param name="resultStatus"></param>
    void DoSdkSwitchAccountCallBack(Callback callBack, ResultStatus resultStatus)
    {
        string log = string.Format("call dosdk switchAccountCallBack!!!!!!!!!!! Callback:{0} ResultStatus:{1} ",
            callBack, resultStatus);
        Q.Log(log);
        if (ResultStatus.ERROR_SWITCH_ACCOUNT_FAILURE != resultStatus)
        {
            if (Application.loadedLevelName != Scenes.LoginScene.ToString())
            {
                if (ViewEventSystem.BackToLoginSceneEvent != null)
                    ViewEventSystem.BackToLoginSceneEvent();
            }
        }
    }

    /// <summary>
    /// 清理GameController，回到Launch之後的狀態
    /// </summary>
    public void Clear()
    {
        if (!IsLaunched)
            return;

        if (InvokeQueue != null)
        {
            InvokeQueue.Clear();
        }

        Model.Clear(ModelEventSystem);
        Model.SdkData = new QmaxModel.SdkLoginData();
        Model.SdkData.appKey = PackageConfig.YOUAI_APP_KEY;

        if (Client != null)
        {
            Client.Close();
        }

        Popup.CloseAll();

        if (LoginCtr != null)
            LoginCtr.Clear();
        else
            LoginCtr = new LoginCtr();

        if (SceneCtr != null)
            SceneCtr.Clear();
        else
            SceneCtr = new SceneCtr();

        if (PlayerCtr == null)
            PlayerCtr = new Com4Love.Qmax.Ctr.PlayerCtr();
        else
            PlayerCtr.Clear();

        if (StageCtr == null)
            StageCtr = new Com4Love.Qmax.Ctr.StageCtr();
        else
            StageCtr.Clear();


        if (UnitCtr != null)
            UnitCtr.Clear();
        else
            UnitCtr = new Com4Love.Qmax.Ctr.UnitCtr();

        if (PlayingRuleCtr == null)
            PlayingRuleCtr = new PlayingRuleCtr(this);
        else
            PlayingRuleCtr.Clear();

        if (TreeFightCtr == null)
        {
            TreeFightCtr = new TreeFightCtr();
        }
        else
        {
            TreeFightCtr.Clear();
        }

        if (GoodsCtr == null)
        {
            GoodsCtr = new GoodsCtr();
        }
        else
        {
            GoodsCtr.Clear();
        }

        if (PropCtr == null)
        {
            PropCtr = new PropCtr();
        }
        else
        {
            PropCtr.Clear();
        }

        if (ButtonTipsCtr == null)
        {
            ButtonTipsCtr = new ButtonTipsCtr();
        }
        else
        {
            ButtonTipsCtr.Clear();
        }

        CreateAssetBundleMgr();
    }


    private void OnBackToLoginSceneEvent()
    {
        Clear();
        SceneCtr.LoadLevel(Scenes.LoginScene, null, false);
    }

    void BackToLoginSceneEvent()
    {
        throw new System.NotImplementedException();
    }

    private void OnNetLoadingCallback(bool isLock)
    {
        Q.Log(LogTag.Test, "OnNetLoadingCallback() 1 lock={0}", isLock);
        InvokeOnMainThread(delegate()
        {
            Q.Log(LogTag.Test, "OnNetLoadingCallback lock={0}", isLock);
            if (isLock)
            {
                Popup.ShowLightLoading();
            }
            else
            {
                Popup.HideLightLoading();
            }
        });
    }


    private void CreateAssetBundleMgr()
    {
        string urlPrefix = string.Format(
            "{0}/assetbundles",
            PackageConfig.UseTestCDN ? PackageConfig.DEV_CDN_URL : PackageConfig.CDN_URL
        );
        AssetBundleMrg = new AssetBundleManager(urlPrefix, PackageConfig.AssetBundlePlatform, MonoBeh);
    }
    public void AlertRespLogicException(byte module,
                                    byte cmd,
                                    int iResponseCode,
                                    bool handleFatalError = true,
                                    Action<byte, byte, int> clickOKAction = null)
    {
        AlertRespLogicException(module, cmd, (ResponseCode)iResponseCode, handleFatalError, clickOKAction);
    }

    /// <summary>
    /// 彈窗提示協議回包status!=0的對應信息
    /// </summary>
    /// <param name="module"></param>
    /// <param name="cmd"></param>
    /// <param name="failCode"></param>
    /// <param name="handleFatalError">是否處理FatalError。
    /// true，則FatalError直接返回登錄界面，非FatalError給clickOKAction處理；
    /// false，則全部status都給clickOKAction處理
    /// </param>
    /// <param name="clickOKAction">點擊Alert框確定按鈕後的處理Action。如果無需處理，則可以傳null</param>
    public void AlertRespLogicException(byte module,
                                        byte cmd,
                                        ResponseCode failCode,
                                        bool handleFatalError = true,
                                        Action<byte, byte, int> clickOKAction = null)
    {
        Q.Assert(failCode != 0, "GM:InfoRespLogicException Assert 1");
        if (failCode == ResponseCode.SUCCESS)
            return;

        //重大錯誤，
        List<ResponseCode> fatalErrorStatusCodes = new List<ResponseCode>();
        //網絡斷開發送失敗
        fatalErrorStatusCodes.Add(ResponseCode.CONNECT_FAIL);
        //發送超時
        fatalErrorStatusCodes.Add(ResponseCode.TIME_OUT);
        fatalErrorStatusCodes.Add(ResponseCode.SERVER_UNLOGIN);
        fatalErrorStatusCodes.Add(ResponseCode.SERVER_DOWN);
        fatalErrorStatusCodes.Add(ResponseCode.SERVER_INNER_ERROR);

        //包括未列出的狀態碼，認為是未知錯誤
        bool isFatalError = fatalErrorStatusCodes.Contains(failCode) ||
            !Model.LanguageConfigsByStatusCode.ContainsKey((int)failCode);

        string msg = "未知錯誤";
        if (Model.LanguageConfigsByStatusCode.ContainsKey((int)failCode))
        {
            msg = Model.LanguageConfigsByStatusCode[(int)failCode];
        }
        if (!Popup.IsPopup(PopupID.UIReconnect))
        {
            UIReconnectBehaviour uiReconnect = Popup.Open(PopupID.UIReconnect, null, true, false).GetComponent<UIReconnectBehaviour>();

            msg = string.Format("{0}\n module={1},cmd={2},status={3}", msg, module, cmd, failCode);
            uiReconnect.setWindowData(msg, 68, "NetDisconnect");
            uiReconnect.ButtonOK.onClick.AddListener(delegate()
            {
                uiReconnect.ButtonOK.onClick.RemoveAllListeners();

                if (isFatalError && handleFatalError)
                {
                    //Fatal Error, 回到登錄界面
                    ViewEventSystem.BackToLoginSceneEvent();
                }
                else
                {
                    //關閉彈窗，返回上層處理
                    Popup.Close(PopupID.UIReconnect, false);
                    if (clickOKAction != null)
                        clickOKAction(module, cmd, (int)failCode);
                }
                Client.UnRetry();
            });
            // 重試
            uiReconnect.ButtonRetry.onClick.AddListener(delegate()
            {
                uiReconnect.ButtonRetry.onClick.RemoveAllListeners();
                Popup.Close(PopupID.UIReconnect, false);
                Client.Retry();
            });
        }
    }

    /// <summary>
    ///協議層回包，Status!=0時的情況統一處理
    /// </summary>
    /// <param name="module"></param>
    /// <param name="cmd"></param>
    /// <param name="status"></param>
    /// <param name="value"></param>
    private void OnClinetErrorResponseCallback(byte module, byte cmd, short status, object value)
    {
        Q.Warning("ErrorResponse m={0}, c={1}, s={1}", module, cmd, status);
    }


    private void BeforeSceneChangeEvent(Scenes preScene, Scenes nextScene)
    {
        if (IsSoftLink)
            return;

        //退出登錄界面
        if (preScene == Scenes.LoginScene)
            Client.DisConnectEvent += OnClientDisConnect;

        //進入登錄界面
        if (nextScene == Scenes.LoginScene)
            Client.DisConnectEvent -= OnClientDisConnect;
    }

    private void OnClientDisConnect()
    {
        //isClientDisConnected = true;
    }

    protected void OnStartEvent()
    {
        //加載公用材質
        //AtlasManager.LoadAtlas(Atlas.Tile);
        AtlasManager.LoadAtlas(Atlas.Tile, AtlasName.Tile0);
        AtlasManager.LoadAtlas(Atlas.Tile, AtlasName.Tile1);
        AtlasManager.LoadAtlas(Atlas.UIComponent);
        //AtlasManager.LoadAtlas(Atlas.UIAlert);        //臨時刪除

        //執行推送信息序列化
        ExecutePushData();
    }

    public void QuitGame()
    {
        UnityEngine.Profiling.Profiler.enabled = false;
        Application.Quit();
    }


    protected void OnUpdateEvent()
    {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Application.loadedLevelName == Scenes.BattleScene.ToString())
            {
                //戰斗場景打開暫停界面
                if (ViewEventSystem.ClickEscapeEvent != null)
                    ViewEventSystem.ClickEscapeEvent();
            }
            else if (Application.loadedLevelName != Scenes.LoadingScene.ToString())
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                if (!Popup.IsPopup(PopupID.UIOut))
                {
                    Popup.Open(PopupID.UIOut, null, true, true);
                }

#elif UNITY_ANDROID
                Q.Log("Call dosdk exit functioin Enter ");
                DoSDK.Instance.exit();
#endif
            }
        }
#endif

        if (ServerTime != null)
            ServerTime.OnUpdate();

        if (InvokeQueue != null)
        {
            while (InvokeQueue.Count > 0)
            {
                InvokeQueue.Dequeue()();
            }
        }
    }

    protected void OnApplicationQuitEvent()
    {
        Q.Log("GameBehaviour::OnApplicationQuit()");
    }

    protected void OnDestroyEvent()
    {
        Q.Log("GameBehaviour::OnDestroy()");
        if (Client != null)
        {
            if (!IsSoftLink)
                Client.DisConnectEvent -= OnClientDisConnect;

            Client.Close();
        }
        Root = null;
        this.AudioManager = null;

        EventDispatcher ed = MonoBeh as EventDispatcher;
        ed.StartEvent -= OnStartEvent;
        ed.UpdateEvent -= OnUpdateEvent;
        ed.ApplicationQuitEvent -= OnApplicationQuitEvent;
        ed.AppLicationPauseEvent -= OnApplicationPauseEvent;
        ed.DestroyEvent -= OnDestroyEvent;
        MonoBeh = null;
    }

#if (!UNITY_EDITOR) && UNITY_ANDROID
    private static string pushfile = "/push.dat";
    private static string pushswitch = "/pushswitch.dat";
#endif

    public void SetPushSwitch(bool isOpen)
    {
#if (!UNITY_EDITOR) && UNITY_ANDROID && QMAX_ACTIVITY
        AndroidJavaClass jcls = new AndroidJavaClass("com.loves.qmax.QMaxActivity");
        string qmaxRoot = jcls.CallStatic<string>("getQMaxFilesPath");
        Q.Log("QMaxFilesPath : ", qmaxRoot);

        FileInfo file = new FileInfo(qmaxRoot + pushswitch);
        // 打開推送
        if (isOpen && file.Exists)
        {
            file.Delete();
        }
        // 關閉推送
        else if (!isOpen && !file.Exists)
        {
            FileStream fileStream = new FileStream(qmaxRoot + pushswitch, FileMode.OpenOrCreate);
            fileStream.WriteByte(0);
            fileStream.Close();
        }
#endif
    }

    public void ExecutePushData()
    {
#if (!UNITY_EDITOR) && UNITY_ANDROID && QMAX_ACTIVITY
        AndroidJavaClass jcls = new AndroidJavaClass("com.loves.qmax.QMaxActivity");
        string qmaxRoot = jcls.CallStatic<string>("getQMaxFilesPath");

        FileInfo file = new FileInfo(qmaxRoot + pushfile);
        if (!file.Exists)
        {
            return;
        }
        FileStream fileStream = new FileStream(qmaxRoot + pushfile, FileMode.Open);
        byte[] bytes = new byte[fileStream.Length];
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        fileStream.Close();

        string ret = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

        if (!string.IsNullOrEmpty(ret))
        {
            JSONInStream json = new JSONInStream(ret);
            Model.PushData = json;
        }
        else
        {
            Model.PushData = null;
        }

        if (ModelEventSystem.OnPushDataSerialized != null)
        {
            ModelEventSystem.OnPushDataSerialized();
        } 
#endif
    }

    public void ClearPushData()
    {
#if (!UNITY_EDITOR) && UNITY_ANDROID && QMAX_ACTIVITY
        AndroidJavaClass jcls = new AndroidJavaClass("com.loves.qmax.QMaxActivity");
        string qmaxRoot = jcls.CallStatic<string>("getQMaxFilesPath");

        FileInfo file = new FileInfo(qmaxRoot + pushfile);
        if (!file.Exists)
        {
            return;
        }
        file.Delete();

        Model.PushData = null;
#endif
    }


    /// <summary>
    /// 在主線程中執行Action
    /// </summary>
    /// <param name="act"></param>
    public void InvokeOnMainThread(Action act)
    {
        if (InvokeQueue == null)
            InvokeQueue = new Queue<Action>();

        InvokeQueue.Enqueue(act);
    }


    /// <summary>
    /// 測試回調DoSdk切換賬號
    /// </summary>
    public void __DoSdkSwitchAccountCallBack()
    {

        DoSdkSwitchAccountCallBack(Callback.SWITCH_ACCOUNT, ResultStatus.FINISHED);
    }


    /// <summary>
    /// 測試回調Dosdk登出
    /// </summary>
    public void __DoSdkLogout()
    {
        DoSdkLogoutCallBack(Callback.LOGOUT, ResultStatus.FINISHED);
    }



    public void InitIAPKit()
    {
        Debug.Log("GameCtr:InitIAPKit");
        const string gaName = "IAPKit";
        GameObject ga = new GameObject();
        ga.name = gaName;
        IapKit = ga.gameObject.AddComponent<IAPKit>();
        ga.transform.SetParent(Root);
        IapKit.OnPaymentFail += OnIAPPaymentFail;
        IapKit.OnPaymentComplete += OnIAPPaymentComplete;
        IapKit.InitKit(gaName);
    }


    private void DeinitIAPKit()
    {
        IapKit.OnPaymentFail -= OnIAPPaymentFail;
        IapKit.OnPaymentComplete -= OnIAPPaymentComplete;
    }

    private void OnIAPPaymentComplete(string transactionID, string username, string receipt)
    {
        //Debug.LogFormat("GameCtr:OnIAPPaymentComplete tID={0}, user={1}", transactionID, username);
        if (Model.LoginData == null || username != Model.LoginData.actorId.ToString())
        {
            //這個訂單不是當前用戶完成的，忽略
            return;
        }

        Action OnValidateComplete = null;
        OnValidateComplete = () =>
        {
            Debug.LogFormat("GameCtr:OnIAPPaymentComplete OnValidateComplete 購買物品成功");
            ModelEventSystem.OnRechargeRefreshEvent -= OnValidateComplete;
            IapKit.FinishTransactionByID(transactionID);
            var str = Utils.GetText("購買物品成功");
            Popup.ShowTextFloat(str, LayerCtrlBehaviour.ActiveLayer.FloatLayer as RectTransform);
        };
        ModelEventSystem.OnRechargeRefreshEvent += OnValidateComplete;
        Client.IapValidateReceipt(receipt);
    }

    private void OnIAPPaymentFail(string transactionID, string username)
    {
        Debug.LogFormat("GameCtr:OnIAPPaymentFail tID={0}, user={1}", transactionID, username);
        if (Model.LoginData == null || username != Model.LoginData.actorId.ToString())
        {
            //這個訂單不是當前用戶完成的，忽略
            return;
        }

        var str = Utils.GetText("玩家取消購買");
        Popup.ShowTextFloat(str, LayerCtrlBehaviour.ActiveLayer.FloatLayer as RectTransform);
        IapKit.FinishTransactionByID(transactionID);
        if (ModelEventSystem.OnBuyGems != null)
            ModelEventSystem.OnBuyGems(-1);

        PropCtr.TemSelectPropReset();
    }
}
