using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.User;
using System;
using System.Collections.Generic;
using System.Timers;


namespace Com4Love.Qmax.Net
{
    /// <summary>
    /// 核心網絡層 + 業務邏輯接口的封裝
    /// </summary>
    public class QmaxClient2 : QmaxClient
    {
        protected enum State
        {

            Disconnected,

            /// <summary>
            /// 正在嘗試連接中
            /// </summary>
            Connecting,

            /// <summary>
            /// 已連接，但是未登錄的狀態，尚不可發送消息
            /// </summary>
            Connected,

            /// <summary>
            /// 連接且登錄的狀態，到了這個狀態才可以發送消息
            /// </summary>
            HasLogined,
        }

        /// <summary>
        /// 心跳間隔
        /// </summary>
        public const double HEART_BEAT_INTERVAL = 30000;


        /// <summary>
        /// 目前：
        /// 當某種需要Lock的消息*第一次*發出時，都會調用ResponseLockEvent(true)
        /// 當某種需要Lock的消息*所有*回包都收到後，都會調用ResponseLockEvent(false)
        /// 
        /// TODO 改成：
        /// 當*第一條*需要Lock的消息發出時，會調用ResponseLockEvent(true);
        /// 當*所有*需要Lock的消息收到回包時，會調用ResponseLockEvent(false);
        /// 
        /// 注意：這個事件可能並非在主線程裡調用，調用Unity的API注意要在主線程中調用
        /// </summary>
        public event Action<bool> ResponseLockEvent;


        /// <summary>
        /// 是否開始發心跳包
        /// </summary>
        public virtual bool HeartBeatEnabled
        {
            get { return heartBeatTimer != null && heartBeatTimer.Enabled; }

            set
            {
                if (heartBeatTimer != null)
                    heartBeatTimer.Enabled = value;
            }
        }


        private bool _keepConnection;
        /// <summary>
        /// 為true時，會保持連接；為false時，會在所有消息已發出、所有期待的回包已收到的情況下斷開連接
        /// </summary>
        public bool KeepConnection
        {
            get { return _keepConnection; }
            set
            {
                _keepConnection = value;
                if (state != State.HasLogined)
                    return;

                if (sendQueue.Count == 0 && waitRespQueue.Count == 0)
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// 連接的IP
        /// </summary>
        public string Host;

        /// <summary>
        /// 連接的端口
        /// </summary>
        public int Port;

        public string Token = null;

        /// <summary>
        /// 平台id
        /// </summary>
        public string PlatformID = null;

        /// <summary>
        /// 協議版本
        /// </summary>
        public string ProtocolVersion = null;

        public string SDKVersion = null;


        /// <summary>
        /// 重連ID
        /// </summary>
        public string ReconnectId = null;


        protected Timer heartBeatTimer;


        /// <summary>
        /// 發送隊列
        /// </summary>
        protected Queue<MsgStruct> sendQueue;

        /// <summary>
        /// 發出等待回包的消息隊列
        /// </summary>
        protected Queue<MsgStruct> waitRespQueue;

        /// <summary>
        /// 重試發送隊列
        /// </summary>
        private Queue<MsgStruct> retryQueue;

        private HashSet<ResponseCode> retryStatus;

        protected State state = State.Disconnected;


        public QmaxClient2()
            : base()
        {
            sendQueue = new Queue<MsgStruct>();
            waitRespQueue = new Queue<MsgStruct>();

            retryQueue = new Queue<MsgStruct>();
            retryStatus = new HashSet<ResponseCode>();
            //heartBeatTimer = new Timer(HEART_BEAT_INTERVAL);
            //heartBeatTimer.Elapsed += OnHeartBeatTimer;
            //heartBeatTimer.Enabled = false;

            ConnectResultEvnet += OnConnectResultEvnet;
            DisConnectEvent += OnDisConnectEvent;
            RegisterRetryStatus(ResponseCode.TIME_OUT);
            RegisterRetryStatus(ResponseCode.CONNECT_FAIL);
            RegisterRetryStatus(ResponseCode.CONNECT_GAME_SERVER_FAIL);
            RegisterRetryStatus(ResponseCode.CONNECT_LOGIN_SERVER_FAIL);
        }



        public void SetConfig(string platformID, string sdkVer, string protocolVer)
        {
            PlatformID = platformID;
            SDKVersion = sdkVer;
            ProtocolVersion = protocolVer;
        }


        /// <summary>
        /// 設置各種參數
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="token"></param>
        /// <param name="platformID"></param>
        /// <param name="sdkVer"></param>
        /// <param name="protocolVer"></param>
        public void SetLoginData(string host, int port, string token)
        {
            Host = host;
            Port = port;
            Token = token;
        }

        public override void Dispose()
        {
            responseLock.Clear();
            heartBeatTimer.Stop();
            ResponseLockEvent = null;
            ConnectResultEvnet -= OnConnectResultEvnet;
            DisConnectEvent -= OnDisConnectEvent;
            base.Dispose();
        }

        public override void Close()
        {
            Q.Assert(sendQueue.Count == 0, "QmaxClient2:Close Assert 1");
            Q.Assert(waitRespQueue.Count == 0, "QmaxClient2:Close Assert 2");

            if (sendQueue.Count > 0)
                sendQueue.Clear();

            if (waitRespQueue.Count > 0)
                waitRespQueue.Clear();

            base.Close();
        }



        public override void SendMsg(byte moduleID, byte cmd, IProtocol value, bool crypt = false)
        {
            Q.Log(LogTag.Net, "QmaxClient2:SendMsg m={0}, c={1}", moduleID, cmd);
            MsgStruct msg = new MsgStruct();
            msg.module = moduleID;
            msg.cmd = cmd;
            msg.protocol = value;
            msg.crypt = crypt;
            SendMsg(msg);
        }


        public void SendMsg(MsgStruct msg)
        {
            sendQueue.Enqueue(msg);

            switch (state)
            {
                case State.HasLogined:
                    //如果此時已經連接上，則全部發送出去
                    while (sendQueue.Count > 0)
                    {
                        RealSendMsg(sendQueue.Dequeue());
                    }
                    break;
                case State.Disconnected:
                    ConnectAndSend();
                    break;
                case State.Connected:
                //已連接，未登錄
                case State.Connecting:
                    //正在連接中，則等待連接回應
                    break;
            }
        }


        private void RealSendMsg(MsgStruct msg)
        {
            //Q.Log(LogTag.Net, "QmaxClient2:RealSendMsg m={0}, c={1}", msg.module, msg.cmd);
            byte moduleID = msg.module;
            byte cmd = msg.cmd;

            //保存必須等回包的消息
            int lockKey = moduleID * 1000 + cmd;
            if (responseLock.ContainsKey(lockKey))
            {
                msg.time = Utils.LocalTimeToUnixTime(DateTime.Now);
                //需要鎖定，直到對應的回包
                //如果計數器小於0，說明曾經在沒有請求的情況下被推送過一條同樣的消息
                if (responseLock[lockKey] == 0)
                {
                    //UnityEngine.Debug.Log("消息鎖定 key:" + lockKey);
                    //發送加鎖事件
                    if (ResponseLockEvent != null)
                    {
                        Q.Log("RealSendMsg() [{0}, {1}] lock=true", moduleID, cmd);
                        ResponseLockEvent(true);
                    }
                }
                responseLock[lockKey]++;
            }

            //需要回包的協議，則放進waitRespQueue中
            if (protocolDict[msg.module][msg.cmd] == ProtocolType.NeedResp)
                waitRespQueue.Enqueue(msg);

            base.SendMsg(moduleID, cmd, msg.protocol, msg.crypt);
        }


        public override void Connect(string host, int port)
        {
            state = State.Connecting;
            base.Connect(host, port);
        }


        public override void Retry()
        {
            while (retryQueue.Count > 0)
            {
                MsgStruct msg = retryQueue.Dequeue();
                if ((msg.module != (byte)Module.User && msg.cmd != (byte)UserCmd.USER_RECONNECTION) &&
                    (msg.module != (byte)Module.User && msg.cmd != (byte)UserCmd.USER_LOGIN))
                {
                    SendMsg(msg);
                }
            }
        }

        public override void UnRetry()
        {
            retryQueue.Clear();
        }


        /// <summary>
        /// 連接並發送消息
        /// </summary>
        protected void ConnectAndSend()
        {
            //Q.Log(LogTag.Net, "QmaxClient2:ConnectAndSend");
            //重連協議回包
            OnResponse OnProtocolResp = null;
            OnProtocolResp = delegate (byte module, byte cmd, short status, object value)
            {
                //Q.Log(LogTag.Net, "QamxClient2:OnProtocolResp 1 m={0}, c={1}", module, cmd);
                if (cmd != (byte)UserCmd.USER_RECONNECTION && cmd != (byte)UserCmd.USER_LOGIN)
                    return;

                RemoveResponseCallback(Module.User, OnProtocolResp);
                if (status != 0)
                {
                    state = State.Connected;
                    while (sendQueue.Count > 0)
                    {
                        MsgStruct m = sendQueue.Dequeue();
                        //由於直接從sendQueue發出，沒有經過RealSend()
                        //所以這裡需要手動添加到waitRespQueue
                        if (protocolDict[m.module][m.cmd] == ProtocolType.NeedResp)
                        {
                            waitRespQueue.Enqueue(m);
                            ReceiveMsg(m.module, m.cmd, null, CONNECT_FAIL_CODE);
                        }
                    }
                    return;
                }

                //重連協議，或UserLogin協議，回包都是相同類型
                UserLoginResponse resp = (UserLoginResponse)value;
                CryptKey = resp.cryptKey;
                ReconnectId = resp.reconnectId;
                state = State.HasLogined;

                while (sendQueue.Count > 0)
                {
                    RealSendMsg(sendQueue.Dequeue());
                }
            };

            //連接回調
            Action<bool> OnConnectCallback = null;
            OnConnectCallback = delegate (bool value)
            {
                //Q.Log(LogTag.Net, "QamxClient2:OnConnectCallback 0");
                ConnectResultEvnet -= OnConnectCallback;
                if (!value)
                {
                    //連接不成功，所有消息都返回不成功
                    while (sendQueue.Count > 0)
                    {
                        MsgStruct m = sendQueue.Dequeue();
                        //由於直接從sendQueue發出，沒有經過RealSend()
                        //所以這裡需要手動添加到waitRespQueue
                        if (protocolDict[m.module][m.cmd] == ProtocolType.NeedResp)
                        {
                            waitRespQueue.Enqueue(m);
                            ReceiveMsg(m.module, m.cmd, null, CONNECT_FAIL_CODE);
                        }
                    }
                    return;
                }

                //連接成功
                //沒有重連ID，所以需要走登錄
                //有重連id，走重連再發消息
                AddResponseCallback(Module.User, OnProtocolResp);
                long actorId = GameController.Instance.Model.SdkData.actorId;
                if (!String.IsNullOrEmpty(ReconnectId) && actorId != 0)
                {
                    Q.Log(LogTag.Net, "QamxClient2:OnConnectCallback 1");
                    UserReconnectRequest req = new UserReconnectRequest();
                    req.platformId = PlatformID;
                    req.connectionId = ReconnectId;
                    req.version = ProtocolVersion;
                    req.actorId = actorId;

                    MsgStruct m = new MsgStruct();
                    m.module = (byte)Module.User;
                    m.cmd = (byte)UserCmd.USER_RECONNECTION;
                    m.crypt = false;
                    m.protocol = req;

                    Q.Assert(sendQueue.Count > 0, "QamxClient2:OnConnectCallback Assert 2");

                    //這裡的邏輯是，如果最後一條剛好是UserLogin或者Reconnect協議
                    //那麼直接在這裡就可以返回了，無需再重新發送一次
                    if (sendQueue.Count > 0)
                    {
                        MsgStruct lastMsg = sendQueue.Peek();
                        if (m.module == lastMsg.module && m.cmd == lastMsg.cmd)
                            sendQueue.Dequeue();
                    }

                    RealSendMsg(m);
                }
                else
                {
                    Q.Log(LogTag.Net, "QamxClient2:OnConnectCallback 2");
                    UserLoginRequest req = new UserLoginRequest();
                    req.token = Token;
                    req.version = ProtocolVersion;
                    req.platformId = PlatformID;
                    req.sdkVersion = SDKVersion;
                    MsgStruct m = new MsgStruct();
                    m.module = (byte)Module.User;
                    m.cmd = (byte)UserCmd.USER_LOGIN;
                    m.crypt = false;
                    m.protocol = req;

                    Q.Assert(sendQueue.Count > 0, "QamxClient2:OnConnectCallback Assert 1");
                    //這裡的邏輯是，如果最後一條剛好是UserLogin或者Reconnect協議
                    //那麼直接在這裡就可以返回了，無需再重新發送一次
                    if (sendQueue.Count > 0)
                    {
                        MsgStruct lastMsg = sendQueue.Peek();
                        if (m.module == lastMsg.module && m.cmd == lastMsg.cmd)
                            sendQueue.Dequeue();
                    }

                    RealSendMsg(m);
                }
            };

            ConnectResultEvnet += OnConnectCallback;
            Connect(Host, Port);
        }

        public void RegisterRetryStatus(ResponseCode status)
        {
            retryStatus.Add(status);
        }

        protected override void ReceiveMsg(byte moduleId, byte cmd, object value, short status)
        {
            Q.Log(LogTag.Net, "QmaxClient2:ReceiveMsg 0 m={0}, c={1}, s={2}, waitRespQueue.Count={3}", moduleId, cmd, status, waitRespQueue.Count);

            ProtocolType pType = protocolDict[moduleId][cmd];
            switch (pType)
            {
                case ProtocolType.NeedResp:
                    Q.Assert(waitRespQueue.Count > 0, "QmaxClient:ReceiveMsg Assert 1");
                    MsgStruct m = waitRespQueue.Peek();
                    Q.Assert(m.module == moduleId && m.cmd == cmd, "QmaxClient:ReceiveMsg expect({0},{1}), receive({2})", m.module, m.cmd, moduleId + "," + cmd);

                    if (retryStatus.Contains((ResponseCode)status))
                    {
                        retryQueue.Enqueue(waitRespQueue.Dequeue());
                    }
                    else
                    {
                        waitRespQueue.Dequeue();
                    }
                    break;
                case ProtocolType.NoResp:
                    Q.Assert(false, "QmaxClient:ReceiveMsg Assert 1");
                    break;
                case ProtocolType.Http:
                    break;
                case ProtocolType.ServerPush:
                    break;
            }

            //可以用於模擬測試各種協議的返回碼
            //byte testM = (byte)Module.Stage; byte testC = (byte)StageCmd.GET_STAGEINFO;
            //byte testM = (byte)Module.Stage; byte testC = (byte)StageCmd.LIST_STAGEDATA;
            //byte testM = (byte)Module.Stage; byte testC = (byte)StageCmd.STAGE_DATA;
            //byte testM = (byte)Module.Stage; byte testC = (byte)StageCmd.STAGE_BEGIN;
            //byte testM = (byte)Module.Stage; byte testC = (byte)StageCmd.SUBMIT_STAGEFIGHT;
            //byte testM = (byte)Module.Stage; byte testC = (byte)StageCmd.SUBMIT_STAGEFIGHT_TEST;
            //byte testM = (byte)Module.Stage; byte testC = (byte)StageCmd.GEM_BUY_STEP;
            //byte testM = (byte)Module.Stage; byte testC = (byte)StageCmd.GEM_UNLOCK_STAGE;

            //byte testM = (byte)Module.Energy; byte testC = (byte)EnergyCmd.BUY_ENERGY;
            //byte testM = (byte)Module.Energy; byte testC = (byte)EnergyCmd.INCRE_MAXENERGY;
            //byte testM = (byte)Module.Energy; byte testC = (byte)EnergyCmd.REFRESH_ENERGY;

            //byte testM = (byte)Module.SIGN; byte testC = (byte)SignCmd.INFO;
            //byte testM = (byte)Module.SIGN; byte testC = (byte)SignCmd.SIGN;

            //byte testM = (byte)Module.Unit; byte testC = (byte)UnitCmd.BUY_UPGRADE;
            //byte testM = (byte)Module.Unit; byte testC = (byte)UnitCmd.FAST_UPGRAD_UNIT;
            //byte testM = (byte)Module.Unit; byte testC = (byte)UnitCmd.GET_DATA;
            //byte testM = (byte)Module.Unit; byte testC = (byte)UnitCmd.UPGRAD_UNIT;

            //byte testM = (byte)Module.GetChance; byte testC = (byte)GetChanceCmd.OPEN_BOX;
            //byte testM = (byte)Module.GetChance; byte testC = (byte)GetChanceCmd.BUY_KEYS;
            //if (moduleId == testM && cmd == testC)
            //{
            //    status = (int)ResponseCode.CONNECT_FAIL;
            //}

            base.ReceiveMsg(moduleId, cmd, value, status);

            Q.Log(LogTag.Net, "QmaxClient2:ReceiveMsg 1");
            int lockKey = moduleId * 1000 + cmd;
            //有可能註冊只有回包的消息
            if (responseLock.ContainsKey(lockKey))
            {
                responseLock[lockKey]--;
                //解鎖的包
                if (responseLock[lockKey] == 0)
                {
                    //發送解鎖事件
                    if (ResponseLockEvent != null)
                    {
                        Q.Log("ReceiveMsg() [{0}, {1}] lock=false", moduleId, cmd);
                        ResponseLockEvent(false);
                    }
                }
            }

            //全部回包都已經收到，斷開連接
            if (!KeepConnection && waitRespQueue.Count == 0 && sendQueue.Count == 0)
            {
                Close();
            }
        }


        private void OnDisConnectEvent()
        {
            state = State.Disconnected;
        }

        private void OnConnectResultEvnet(bool value)
        {
            Q.Log(LogTag.Net, "QmaxClient2:OnConnectResultEvnet {0}", value);
            state = value ? State.Connected : State.Disconnected;
        }

        private void OnHeartBeatTimer(object source, ElapsedEventArgs args)
        {
            if (Connected)
                HeartBeat();
        }
    }
}
