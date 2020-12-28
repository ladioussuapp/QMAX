using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Net;
using Com4Love.Qmax.Net.Protocols.Base;
using Com4Love.Qmax.Net.Protocols.User;
using DoPlatform;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Com4Love.Qmax.Ctr
{
    /// <summary>
    /// 登錄測試案例
    /// . 無SDK，無登錄記錄，註冊賬號，登錄成功
    /// . 無SDK，有登錄記錄，註冊賬號，登錄成功
    /// . 無SDK，無登錄記錄，登錄已有賬號成功
    /// . 無SDK，有登錄記錄，登錄已有賬號成功
    /// . 無SDK，有登錄記錄，修改賬號登錄成功
    /// . 無SDK，保存上次登錄的賬號功能
    /// . 無SDK，公告拉取失敗，可以正常登錄
    /// . 無SDK，請求登錄服失敗，無法登錄，無需嘗試重連
    /// . 無SDK，建立連接失敗，無法登錄，無需嘗試重連
    /// . 無SDK，建立連接成功，登錄協議[1,1]返回失敗，無需嘗試重連
    /// . SDK母包，無登錄記錄，註冊賬號，登錄成功
    /// . SDK母包，無登錄記錄，登錄已有賬號，成功
    /// . SDK母包，有登錄記錄，自動登錄，成功
    /// . SDK母包，SDK初始化失敗，無法登錄
    /// . SDK母包，拉取公告失敗，可以登錄
    /// . SDK母包，請求登錄服失敗，無法登錄
    /// </summary>
    public class LoginCtr : IDisposable
    {
        /// <summary>
        /// http根目錄
        /// </summary>
        public string HttpRoot;

        /// <summary>
        /// 儲值
        /// </summary>
        public string RechargeRoot;

        /// <summary>
        /// cdn根目錄
        /// </summary>
        public string CdnRoot;

        private string username;

        private string password;

        private QmaxClient qmaxClient;
        private GameController gameCtr;

        protected LoginState state = LoginState.None;

        public LoginState State { get; set; }

        public bool IsLogin
        {
            get
            {
                return State != LoginState.None;
            }
        }

        public LoginCtr()
        {
            qmaxClient = GameController.Instance.Client;
            gameCtr = GameController.Instance;
            qmaxClient.AddResponseCallback(Module.User, OnUserModuleResponse);
        }

        public void Dispose()
        {
            qmaxClient.RemoveResponseCallback(Module.User, OnUserModuleResponse);
        }


        public void Clear()
        {
            username = null;
            password = null;

            qmaxClient = GameController.Instance.Client;
            gameCtr = GameController.Instance;
            state = LoginState.None;
        }



        /// <summary>
        /// 沒有接入DoSdk時走的登錄
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void NoSdkLogin(string username, string password)
        {
            Q.Log("LoginCtr:NoSdkLogin");
            this.username = username;
            this.password = password;
            gameCtr.Model.SdkData.UserName = username;

            gameCtr.Popup.ShowLightLoading();

            qmaxClient.HttpLogin(PackageConfig.HTTP_LOGIN_URL, username, password,
                (LoginResponse resp) =>
                {
                    Debug.Log("NoSDK登錄成功");
                    gameCtr.Model.SdkData.token = resp.Token;
                    gameCtr.Model.SdkData.userId = resp.UID;
                    //保存帳號
                    SaveAccountLocal();
                    //登錄成功，請求遊服地址
                    ReqGameServerAddress();
                    gameCtr.Popup.HideLightLoading();
                },
                (ResponseCode failCode) =>
                {
                    Debug.LogFormat("NoSDK登錄失敗, codde={0}", failCode);
                    if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                        gameCtr.ModelEventSystem.OnLoginProgress(failCode, LoginState.Login);
                    gameCtr.Popup.HideLightLoading();
                });
        }


        /// <summary>
        /// 走註冊流程，註冊成功後會自動走登錄流程
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void NoSdkRegister(string username, string password)
        {
            this.username = username;
            this.password = password;
            qmaxClient.HttpRegister(PackageConfig.HTTP_REGISTER_URL, username, password,
                (RegisterResponse resp) =>
                {
                    //保存帳號
                    gameCtr.Model.SdkData.token = resp.Token;
                    gameCtr.Model.SdkData.userId = resp.Uid;
                    SaveAccountLocal();
                    //註冊成功，請求遊服地址
                    ReqGameServerAddress();
                    gameCtr.Popup.HideLightLoading();
                },
                (ResponseCode failCode) =>
                {
                    gameCtr.Popup.HideLightLoading();
                    if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                        gameCtr.ModelEventSystem.OnLoginProgress(failCode, LoginState.Register);
                });
        }


        /// <summary>
        /// 獲取遊服地址
        /// </summary>
        public void ReqGameServerAddress()
        {
            string channelID = gameCtr.Model.SdkData.channalID;
            string versionID = gameCtr.Model.SdkData.sdkVersion;
            string token = gameCtr.Model.SdkData.token;

            Q.Log("LoginCtr:ReqGameServerAddress, token={0}", token);

            gameCtr.Popup.ShowLightLoading();
            qmaxClient.GetAddress(HttpRoot, channelID, channelID, token, versionID, null,
                (GetAddressResponse resp) =>
                {
                    Q.Log("請求遊服信息成功");
                    gameCtr.Popup.HideLightLoading();

                    gameCtr.Model.SdkData.GetAddressResponse = resp;
                    if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                        gameCtr.ModelEventSystem.OnLoginProgress(ResponseCode.SUCCESS, LoginState.ReqGameServerInfo);
                },
                (ResponseCode failCode) =>
                {
                    Q.Log("請求遊服信息失敗");
                    gameCtr.Popup.HideLightLoading();

                    if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                        gameCtr.ModelEventSystem.OnLoginProgress(failCode, LoginState.ReqGameServerInfo);
                });
        }


        /// <summary>
        /// sdk登录
        /// </summary>
        public void DoSdkLogin()
        {
            Q.Log("LoginCtr:DoSdkLogin");
            gameCtr.Popup.ShowLightLoading();
            DoSDK.Instance.login();
        }

        public void OnDoSdkLoginCallback(ResultStatus resultStatus)
        {
            if (resultStatus != ResultStatus.ERROR_LOGIN_FAILURE)
            {
                gameCtr.Popup.HideLightLoading();
                //sdk登錄成功，不需要隱藏Loading，因為還有下一步操作
                GameController.Instance.Model.SdkData.token = DoSDK.Instance.token();
                ReqGameServerAddress();
            }
            else
            {
                //sdk登錄失敗，隱藏loading
                //TODO 這裡需要提示
                gameCtr.Popup.HideLightLoading();
            }
            //這個方法是在SDK調用時候判定的，沒有SDK登錄調用判定不用這個方法///
            PlayerPrefsTools.SetStringValue(OnOff.Account, "SDK");
        }

        /// <summary>
        /// 切換賬號
        /// </summary>
        public void SwitchAccount()
        {
            Q.Log(LogTag.Login, GetType().Name + ":SwitchAccount");
            DoSDK.Instance.switchAccount();
        }


        /// <summary>
        /// 註銷
        /// </summary>
        public void Logout()
        {
            DoSDK.Instance.logout();
        }



        /// <summary>
        /// 請求公告
        /// </summary>
        /// <param>callback, 第一個參數是獲取公告是否成功，第二個參數是公告內容</param>
        public void ReqNotice(Action<bool, List<NoticeInfo>> callback)
        {
            string channelID = gameCtr.Model.SdkData.channalID;
            //公告分渠道
            string url = string.Format("{0}/notice/notice_{1}.json", CdnRoot, channelID);
            //錯誤的鏈接，用於測試拉取公告失敗
            //string url = string.Format("{0}/notice/notice_{1}111.json", CdnRoot, DoSDK.Instance.channel());
            Q.Log(LogTag.Login, "notice url={0}", url);
            qmaxClient.GetNotice(url,
                (string json) =>
                {
                    Debug.Log(json);
                    Debug.Assert(!string.IsNullOrEmpty(json));
                    var notices = HandleNoticeContent(json);
                    if (callback != null)
                        callback(true, notices);
                },
                (ResponseCode failCode) =>
                {
                    if (callback != null)
                        callback(false, null);
                }
            );
        }


        /// <summary>
        /// socket登錄
        /// </summary>
        /// <param name="res"></param>
        public void ConnectGameServer(GetAddressResponse res)
        {
            Debug.Assert(res != null);
            Debug.Log("LoginCtr:ConnectGameServer");

            if (PackageConfig.DOSDK && !DoSDK.Instance.isLogin())
            {
                //SDK尚未登錄，先走SDK登錄
                gameCtr.Popup.HideLightLoading();
                DoSdkLogin();
                return;
            }

            if (res == null)
            {
                //返回數據有問題
                gameCtr.Popup.HideLightLoading();
                //登錄服獲取數據失敗
                if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                {
                    gameCtr.ModelEventSystem.OnLoginProgress(
                        ResponseCode.CONNECT_LOGIN_SERVER_FAIL,
                        LoginState.ConnectGameServer);
                }
                return;
            }

            string channelID = gameCtr.Model.SdkData.channalID;
            string sdkVersion = gameCtr.Model.SdkData.sdkVersion;
            string token = gameCtr.Model.SdkData.token;

            GetAddressResponse config = GameController.Instance.Model.SdkData.GetAddressResponse;
            if (gameCtr.IsSoftLink)
            {
                Q.Log("弱连接登录");
                (qmaxClient as QmaxClient2).SetConfig(channelID, sdkVersion, PackageConfig.PROTOCOL_VER);
                (qmaxClient as QmaxClient2).SetLoginData(config.Host, config.Port, token);
                //整個登錄過程保證QmaxClient是連接狀態
                (qmaxClient as QmaxClient2).KeepConnection = true;
                qmaxClient.UserLogin(token, PackageConfig.PROTOCOL_VER, channelID, sdkVersion);
            }
            else
            {
                Action<bool> connectResult = null;
                connectResult = delegate(bool ret)
                {
                    qmaxClient.ConnectResultEvnet -= connectResult;
                    //在主線程裡調用處理
                    gameCtr.InvokeOnMainThread(delegate()
                        {
                            if (!ret)
                            {
                                Q.Log("連接遊服失敗");
                                if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                                {
                                    gameCtr.ModelEventSystem.OnLoginProgress(
                                        ResponseCode.CONNECT_GAME_SERVER_FAIL,
                                        LoginState.ConnectGameServer);
                                }
                            }
                            else
                            {

                                Q.Log("連接遊服成功");
                                if (gameCtr.IsSoftLink)
                                    ((QmaxClient2)qmaxClient).HeartBeatEnabled = true;
                                else
                                    ((QmaxClientEx)qmaxClient).HeartBeatEnabled = true;

                                qmaxClient.UserLogin(token, PackageConfig.PROTOCOL_VER, channelID, sdkVersion);
                            }
                        });
                };

                Q.Log("非弱連接登錄 {0}:{1}", res.Host, res.Port);
                qmaxClient.ConnectResultEvnet += connectResult;
                qmaxClient.Connect(res.Host, res.Port);
            }
        }

        /// <summary>
        /// 请求可购买物品的信息
        /// </summary>
        public void ReqPurchaseInfo(Action onComplete)
        {
            Debug.Log("請求商品信息");
            //先要初始化
            gameCtr.InitIAPKit();

            if (!gameCtr.IapKit.IsProductsAvailable())
            {
                DeleteAllProductsInfo();
                if (onComplete != null)
                    onComplete();
                return;
            }

            List<string> idList = new List<string>();
            foreach (var pair in gameCtr.Model.PaymentSystemConfigs)
            {
                idList.Add(pair.Value.PaymentId);
            }
            var configs = gameCtr.Model.PaymentSystemConfigs;
            Action<String> OnProductsReqResponse = null;

            OnProductsReqResponse = (string jsonInfo) =>
            {
                gameCtr.IapKit.OnProductsReqResponse -= OnProductsReqResponse;
                JSONInStream json = new JSONInStream(jsonInfo);
                Debug.LogFormat("請求購買信息返回 jsonCount={0}", json.Count);
                for (int i = 0, n = json.Count; i < n; i++)
                {
                    string pID = null;
                    string price = null;
                    string title = null;
                    string desc = null;
                    json.Start(i)
                            .Content("productIdentifier", out pID)
                            .Content("localizedTitle", out title)
                            .Content("localizedDescription", out desc)
                            .Content("price", out price)
                        .End();

                    if (configs.ContainsKey(pID))
                    {
                        //根據返回的價格修正
                        //因為有可能配置表與iTunes Connect配的價格不一致，以iTunes Connect為準
                        configs[pID].Rmb = Convert.ToInt32(price);
                        Debug.LogFormat("返回物品={0}, 價格={1}", pID, price);
                    }
                }

                if (onComplete != null)
                    onComplete();

                gameCtr.Popup.HideLightLoading();
                gameCtr.ModelEventSystem.OnLoginProgress(ResponseCode.SUCCESS, LoginState.ReqPurchaseInfo);
            };
            gameCtr.IapKit.OnProductsReqResponse += OnProductsReqResponse;
            gameCtr.Popup.ShowLightLoading();
            gameCtr.IapKit.StartProductsRequest(idList);
        }

        //不支持購買行為的處理
        private void DeleteAllProductsInfo()
        {
            //因為不支持購買，所以這裡要把所有
            List<string> list = new List<string>();
            foreach (var pair in gameCtr.Model.PaymentSystemConfigs)
            {
                list.Add(pair.Key);
            }
            for (int i = 0, n = list.Count; i < n; i++)
            {
                gameCtr.Model.PaymentSystemConfigs.Remove(list[i]);
            }
        }


        private List<NoticeInfo> HandleNoticeContent(string notice)
        {
            JSONInStream jsonInStream = new JSONInStream(notice);
            NoticeInfo noticeInfo;
            List<NoticeInfo> notices = new List<NoticeInfo>();

            int id;
            string title;
            string info;
            string sourceImg;
            long time;
            for (int i = 0; i < jsonInStream.node.fields_.Count; i++)
            {
                JSONObjectFieldValue fVal = jsonInStream.node.GetField(i) as JSONObjectFieldValue;
                JSONNode cNode = (JSONNode)fVal.value;
                JSONInStream cJsonInStream = new JSONInStream(cNode);
                cJsonInStream.Content("id", out id)
                .Content("title", out title)
                .Content("sourceImg", out sourceImg)
                .Content("time", out time)
                .Content("info", out info);

                noticeInfo = new NoticeInfo();
                noticeInfo.Id = id;
                noticeInfo.Title = title;
                noticeInfo.Info = info;
                noticeInfo.Time = time;
                noticeInfo.SourceImg = sourceImg;
                notices.Add(noticeInfo);
            }

            return notices;
        }

        private void OnUserModuleResponse(byte module, byte cmd, short status, object value)
        {
            Action OnMainThread = delegate()
            {
                //Q.Log(LogTag.Test, "LoginCtr:OnUserModuleResponse: m={0}, c={1}", module, cmd);
                if (status != 0)
                {
                    Q.Log(LogTag.Net, "登錄出錯：cmd" + cmd + " status:" + status);

                    //if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                    //    gameCtr.ModelEventSystem.OnLoginProgress(status);
                    return;
                }

                if (module == (byte)Module.User)
                {
                    string msg;
                    var channelID = GameController.Instance.Model.SdkData.channalID;
                    switch ((UserCmd)cmd)
                    {
                        case UserCmd.HEART_BEAT:
                            GameController.Instance.ServerTime.UnixTime = (int)value;
                            break;
                        case UserCmd.USER_LOGIN:
                            //連接遊服成功
                            Q.Log("登錄遊服成功");
                            if (gameCtr.IsSoftLink)
                                ((QmaxClient2)qmaxClient).ReconnectId = (value as UserLoginResponse).reconnectId;
                            else
                                ((QmaxClientEx)qmaxClient).reConnectId = (value as UserLoginResponse).reconnectId;
                            qmaxClient.CryptKey = (value as UserLoginResponse).cryptKey;
                            GameController.Instance.Model.SdkData.userId = (value as UserLoginResponse).uid;

                            qmaxClient.GetActor(GameController.Instance.Model.SdkData.GetAddressResponse.serverId, channelID);
                            //qmaxClient.SendTestRequest();
                            break;
                        case UserCmd.GET_ACTOR:
                            GetActorResponse getActorResp = (GetActorResponse)value;
                            if (getActorResp.list.Count == 0)
                            {
                                msg = "沒有角色，需要創建角色";
                                //if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                                //    gameCtr.ModelEventSystem.OnLoginProgress(0, msg, LoginState.CreateActor, State);
                                State = LoginState.CreateActor;
                                Q.Log(LogTag.Login, msg);
#if UNITY_IOS
                                string phoneos = "iOS";
#elif UNITY_ANDROID
                                string phoneos = "Android";
#elif UNITY_STANDALONE_WIN
                                string phoneos = "Standlone Windows";
#elif UNITY_STANDALONE_OSX
                                string phoneos = "Standlone OSX";
#endif
                                qmaxClient.CreateActor(
                                    GameController.Instance.Model.SdkData.GetAddressResponse.serverId,
                                    RandomActorName(),
                                    channelID,
                                    SystemInfo.operatingSystem, //osversion
                                    SystemInfo.deviceModel, //phonetype
                                    "", //sim
                                    "", //mac
                                    SystemInfo.deviceUniqueIdentifier, //imei
                                    phoneos, //phoneos
                                    Application.systemLanguage.ToString() //language
                                );
                            }
                            else
                            {
                                msg = "已創建角色，使用第一個角色登錄";

                                if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                                    gameCtr.ModelEventSystem.OnLoginProgress(ResponseCode.SUCCESS, LoginState.CreateActor);


                                State = LoginState.ActorLogin;
                                Q.Log(LogTag.Login, msg);
                                ActorInfo actorInfo = getActorResp.list[0];
                                GameController.Instance.Model.SdkData.actorId = actorInfo.actorId;
                                qmaxClient.ActorLogin(GameController.Instance.Model.SdkData.GetAddressResponse.serverId, actorInfo.actorId);
                            }
                            break;
                        case UserCmd.CREATE_ACTOR:
                            msg = "創建角色成功。";
                            if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                                gameCtr.ModelEventSystem.OnLoginProgress(ResponseCode.SUCCESS, LoginState.CreateActor);

                            Q.Log(LogTag.Login, msg);
                            State = LoginState.ActorLogin;
                            CreateActorResponse createActorResp = (CreateActorResponse)value;
                            gameCtr.Model.SdkData.actorId = createActorResp.actorId;
                            qmaxClient.ActorLogin(GameController.Instance.Model.SdkData.GetAddressResponse.serverId, createActorResp.actorId);
                            break;
                        case UserCmd.ACTOR_LOGIN:
                            msg = "角色登錄成功。";
                            //這裡發第一次心跳，是為了獲取服務器時間
                            GameController.Instance.Client.HeartBeat();
                            if (gameCtr.ModelEventSystem.OnLoginProgress != null)
                                gameCtr.ModelEventSystem.OnLoginProgress(ResponseCode.SUCCESS, LoginState.ActorLogin);
                            ActorLoginResponse actorLoginResp = (ActorLoginResponse)value;
                            GameController.Instance.Model.LoginData = actorLoginResp;

                            //註冊推送功能
                            RegisterPushNotification();

                            if (gameCtr.IsSoftLink)
                            {
                                (qmaxClient as QmaxClient2).KeepConnection = false;
                            }

                            State = LoginState.Login;
                            JSONNode node = new JSONNode();
                            JSONStringFieldValue viplvl = new JSONStringFieldValue("1");
                            JSONStringFieldValue roleLevel = new JSONStringFieldValue("1");
                            JSONStringFieldValue roleName = new JSONStringFieldValue(actorLoginResp.actorName);
                            JSONStringFieldValue roleId = new JSONStringFieldValue("" + actorLoginResp.actorId);
                            JSONStringFieldValue zoneName = new JSONStringFieldValue("1");
                            JSONStringFieldValue zoneId = new JSONStringFieldValue("" + actorLoginResp.serverId);
                            node.AddField("viplvl", viplvl);
                            node.AddField("roleLevel", roleLevel);
                            node.AddField("roleName", roleName);
                            node.AddField("roleId", roleId);
                            node.AddField("zoneName", zoneName);
                            node.AddField("zoneId", zoneId);
                            if (PackageConfig.BUGLY)
                            {
                                BuglyAgent.ConfigDefault(
                                    channelID,
                                    PackageConfig.Version,
                                    roleId + "|" + roleName,
                                    0);
                            }
                            string jsonStr = node.Serialize();
                            Q.Log(LogTag.Login, "extends jsonStr:\n{0}", jsonStr);
                            DoSDK.Instance.extendData(jsonStr);
                            //跳轉到地圖場景
                            Q.Log(LogTag.Login, "{0}\n{1}", msg, actorLoginResp);
                            break;
                    }
                }
            };

            gameCtr.InvokeOnMainThread(OnMainThread);
        }


        /// <summary>
        /// 保存登錄的帳號密碼
        /// </summary>
        private void SaveAccountLocal()
        {
            ///存儲玩家賬號密碼//
            PlayerPrefsTools.SetStringValue(OnOff.Account, username);
            PlayerPrefsTools.SetStringValue(OnOff.AccountPass, password);
        }


        /// <summary>
        /// 添加推送功能        
        /// </summary>
        private void RegisterPushNotification()
        {
#if QMAX_XGPUSH && (!UNITY_EDITOR)
#if UNITY_ANDROID
            //AndroidJavaClass jcls = new AndroidJavaClass("com.loves.qmax.QMaxActivity");
            //jcls.CallStatic("registerPush", actorLoginResp.actorId);

#elif UNITY_IOS
            //TODO
#endif
#endif

        }

        private string GetTokenByUnityEditor()
        {
            string newToken = gameCtr.Model.SdkData.UserName + GameController.Instance.Model.SdkData.appKey;
            newToken = Utils.GetMd5Hash(newToken);
            newToken = gameCtr.Model.SdkData.UserName + "#" + newToken;

            return newToken;
        }


        /// <summary>
        /// 生成隨機角色名
        /// </summary>
        /// <returns></returns>
        private string RandomActorName()
        {
            return username + "1";
        }
    }
}
