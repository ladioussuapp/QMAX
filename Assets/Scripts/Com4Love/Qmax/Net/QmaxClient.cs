using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.achievement;
using Com4Love.Qmax.Net.Protocols.activity;
using Com4Love.Qmax.Net.Protocols.actorgame;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.Base;
using Com4Love.Qmax.Net.Protocols.counterpart;
using Com4Love.Qmax.Net.Protocols.Energy;
using Com4Love.Qmax.Net.Protocols.getchance;
using Com4Love.Qmax.Net.Protocols.goods;
using Com4Love.Qmax.Net.Protocols.mail;
using Com4Love.Qmax.Net.Protocols.recharge;
using Com4Love.Qmax.Net.Protocols.sign;
using Com4Love.Qmax.Net.Protocols.stage;
using Com4Love.Qmax.Net.Protocols.Stage;
using Com4Love.Qmax.Net.Protocols.tree;
using Com4Love.Qmax.Net.Protocols.Unit;
using Com4Love.Qmax.Net.Protocols.User;
using DoPlatform;
using GameXP.Framewrok;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;


namespace Com4Love.Qmax.Net
{
    /// <summary>
    /// 核心網絡層 + 業務邏輯接口的封裝
    /// </summary>
    public class QmaxClient : BaseClient
    {
        public enum ProtocolType
        {
            /// <summary>
            /// 有請求，有回包
            /// </summary>
            NeedResp,

            /// <summary>
            /// 有請求，無回包
            /// </summary>
            NoResp,

            /// <summary>
            /// 後台主動推送的包
            /// </summary>
            ServerPush,

            Http,
        }


        static public bool NoServer = false;
        static public bool SkipHttpServer = true || NoServer;

        /// <summary>
        /// 登錄超時時間
        /// </summary>
        static public int LoginTimeout = 5000;

        /// <summary>
        /// 需要loading 的消息計數 (考慮移到子類實現)
        /// key,count
        /// </summary>
        protected Dictionary<int, int> responseLock;


        /// <summary>
        /// 協議類型的字典
        /// 根據是否有回包，是否是後台直接推送的來區分協議
        /// </summary>
        protected Dictionary<byte, Dictionary<byte, ProtocolType>> protocolDict;


        public QmaxClient()
        {
            responseLock = new Dictionary<int, int>();
            protocolDict = new Dictionary<byte, Dictionary<byte, ProtocolType>>();

            //雖然Http登錄、註冊是走Http，但是在這裡也是走同樣機制
            //http請求全部認為是不需要回包
            RegisterResponseType((byte)Module.Http, (byte)HttpCmd.LOGIN, typeof(LoginResponse), ProtocolType.Http, false);
            RegisterResponseType((byte)Module.Http, (byte)HttpCmd.REGISTER, typeof(string), ProtocolType.Http, false);
            RegisterResponseType((byte)Module.Http, (byte)HttpCmd.GET_ADDRESS, typeof(GetAddressResponse), ProtocolType.Http, false);
            RegisterResponseType((byte)Module.Http, (byte)HttpCmd.DIS_CONNECT, null, ProtocolType.Http, false);
            RegisterResponseType((byte)Module.Http, (byte)HttpCmd.GET_NOTICE, typeof(string), ProtocolType.Http, false);
            RegisterResponseType((byte)Module.Http, (byte)HttpCmd.RECHARGE_INFO, typeof(string), ProtocolType.Http, false);
            RegisterResponseType((byte)Module.Http, (byte)HttpCmd.PUSH_RECHARGE_RESULT, null, ProtocolType.Http, false);

            //所有的回包協議要在這裡做註冊
            //User
            RegisterResponseType((byte)Module.User, (byte)UserCmd.HEART_BEAT, typeof(int), ProtocolType.NeedResp, false);
            RegisterResponseType((byte)Module.User, (byte)UserCmd.USER_LOGIN, typeof(UserLoginResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.User, (byte)UserCmd.GET_ACTOR, typeof(GetActorResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.User, (byte)UserCmd.CREATE_ACTOR, typeof(CreateActorResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.User, (byte)UserCmd.ACTOR_LOGIN, typeof(ActorLoginResponse), ProtocolType.NeedResp, true);
            //RegisterResponseType((byte)Module.User, (byte)UserCmd.PUSH_ACTOR_ATTRIBUTE, null, ProtocolType.NoResp, false);
            //RegisterResponseType((byte)Module.User, (byte)UserCmd.SAVE_GUIDES_STEP, null, ProtocolType.NoResp, false);
            //RegisterResponseType((byte)Module.User, (byte)UserCmd.SAVE_MONTION, null, ProtocolType.NoResp, false);
            RegisterResponseType((byte)Module.User, (byte)UserCmd.KICK_OFF, typeof(KickOffResponse), ProtocolType.ServerPush);
            RegisterResponseType((byte)Module.User, (byte)UserCmd.USER_RECONNECTION, typeof(UserLoginResponse), ProtocolType.NeedResp, true);
            //RegisterResponseType((byte)Module.User, (byte)UserCmd.SAVE_PUSH_KEY, null, ProtocolType.NoResp, false);


            //unit
            RegisterResponseType((byte)Module.Unit, (byte)UnitCmd.UPGRAD_UNIT, typeof(UpgradeUnitResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Unit, (byte)UnitCmd.BUY_UPGRADE, typeof(ValueResultListResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Unit, (byte)UnitCmd.FAST_UPGRAD_UNIT, typeof(FastUpgradeUnitResponse), ProtocolType.NeedResp, true);

            //ActorGame
            RegisterResponseType((byte)Module.ActorGame, (byte)ActorGameCmd.GET_DATA, typeof(ActorGameResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.ActorGame, (byte)ActorGameCmd.SAVE_GUIDE, null, ProtocolType.NoResp, false);
            RegisterResponseType((byte)Module.ActorGame, (byte)ActorGameCmd.PUSH_RECHARGE_RESULT, typeof(RechargeResponse), ProtocolType.ServerPush);
            RegisterResponseType((byte)Module.ActorGame, (byte)ActorGameCmd.RECHARGE_REFRESH, typeof(ActorGameAndGoodsResponse), ProtocolType.NeedResp, true);

            //Energy
            RegisterResponseType((byte)Module.Energy, (byte)EnergyCmd.REFRESH_ENERGY, typeof(RefreshEnergyResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Energy, (byte)EnergyCmd.BUY_ENERGY, typeof(ValueResultListResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Energy, (byte)EnergyCmd.INCRE_MAXENERGY, typeof(ValueResultListResponse), ProtocolType.NeedResp, true);

            //Stage
            RegisterResponseType((byte)Module.Stage, (byte)StageCmd.GET_STAGEINFO, typeof(StageInfoResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Stage, (byte)StageCmd.STAGE_BEGIN, typeof(BeginStageResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Stage, (byte)StageCmd.LIST_STAGEDATA, typeof(ListStageResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Stage, (byte)StageCmd.STAGE_DATA, typeof(Stage), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Stage, (byte)StageCmd.SUBMIT_STAGEFIGHT, typeof(SubmitStageFightResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Stage, (byte)StageCmd.SUBMIT_STAGEFIGHT_TEST, typeof(SubmitStageFightResponse), ProtocolType.NeedResp, true);
            //RegisterResponseType((byte)Module.Stage, (byte)StageCmd.PASS_STAGE, typeof(SubmitStageFightResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Stage, (byte)StageCmd.GEM_BUY_STEP, typeof(BuyStepResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Stage, (byte)StageCmd.GEM_BUY_TIME, typeof(BuyTimeResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Stage, (byte)StageCmd.GEM_UNLOCK_STAGE, typeof(UnlockStageResponse), ProtocolType.NeedResp, true);

            //GetChance
            RegisterResponseType((byte)Module.GetChance, (byte)GetChanceCmd.OPEN_BOX, typeof(GetChanceResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.GetChance, (byte)GetChanceCmd.BUY_KEYS, typeof(ValueResultListResponse), ProtocolType.NeedResp, true);

            //Activity
            //RegisterResponseType((byte)Module.Activity, (byte)ActivityCmd.EXCHANGE_CODE, typeof(ValueResultListResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Activity, (byte)ActivityCmd.EXCHANGE_CODE, typeof(ExchangeCodeResponse), ProtocolType.NeedResp, true);

            //Sign
            RegisterResponseType((byte)Module.SIGN, (byte)SignCmd.SIGN, typeof(SignResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.SIGN, (byte)SignCmd.INFO, typeof(SignInfoResponse), ProtocolType.NeedResp, true);

            //RegisterResponseType((byte)Module.Unit, (byte)UnitCmd.GET_DATA, typeof(UnitListResponse));

            //tree
            RegisterResponseType((byte)Module.Tree, (byte)TreeCmd.GET_INFO, typeof(TreeInfoResponse), ProtocolType.NeedResp, false);
            RegisterResponseType((byte)Module.Tree, (byte)TreeCmd.ENTER, typeof(TreeStageResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Tree, (byte)TreeCmd.SUBMIT_FIGHT, typeof(SubmitTreeFightResponse), ProtocolType.NeedResp, true);

            RegisterResponseType(100, 2, typeof(TestRequest), ProtocolType.NeedResp, true);

            // achieve
            RegisterResponseType((byte)Module.Achievement, (byte)AchievementCmd.GET_ACHIEVEMENT_LIST, typeof(AchievementListResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Achievement, (byte)AchievementCmd.GET_ACHIEVEMENT_REWARD, typeof(AchievementRewardResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Achievement, (byte)AchievementCmd.REFRESH_ACHIEVEMENT_LIST, typeof(AchievementListResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Achievement, (byte)AchievementCmd.ROLL_POLLING_ACHIEVE, typeof(AchievementPollingResponse), ProtocolType.NeedResp, false);

            // goods
            RegisterResponseType((byte)Module.Goods, (byte)GoodsCmd.GET_ALL_GOODS, typeof(GoodsListResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Goods, (byte)GoodsCmd.USE_GOODS, typeof(UseGoodsResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Goods, (byte)GoodsCmd.BUY_GOODS, typeof(BuyGoodsResponse), ProtocolType.NeedResp, true);

            //mail
            RegisterResponseType((byte)Module.Mail, (byte)MailCmd.FEEDBACK, typeof(int), ProtocolType.NeedResp, true);

            //Counterpart
            RegisterResponseType((byte)Module.Counterpart, (byte)CounterpartCmd.BEGAIN_STAGE, typeof(int), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Counterpart, (byte)CounterpartCmd.GET_STAGE_LIST, typeof(CounterpartStageListResponse), ProtocolType.NeedResp, true);
            RegisterResponseType((byte)Module.Counterpart, (byte)CounterpartCmd.SUBMIT_COUNTERPART_FIGHT, typeof(SubmitCounterpartResponse), ProtocolType.NeedResp, true);

            //購買驗證協議
            RegisterResponseType((byte)Module.Purchase, (byte)PurchaseCmd.VALIDATE, typeof(RechargeResponse), ProtocolType.NeedResp, true);
        }

        public virtual void Retry()
        {

        }

        public virtual void UnRetry()
        {

        }

        public override void Dispose()
        {
            responseLock.Clear();
            base.Dispose();
        }

        /// <summary>
        /// 註冊回包解析協議
        /// </summary>
        /// <param name="module"></param>
        /// <param name="cmd"></param>
        /// <param name="respType"></param>
        /// <param name="pType"></param>
        /// <param name="waitResponse"></param>
        public void RegisterResponseType(byte module, byte cmd, Type respType,
            ProtocolType pType, bool waitResponse = false)
        {
            //協議類型
            if (!protocolDict.ContainsKey(module))
                protocolDict.Add(module, new Dictionary<byte, ProtocolType>());
            protocolDict[module].Add(cmd, pType);

            //是否需要等包
            if (waitResponse)
            {
                //需要鎖定的消息，進入responseLock
                int lockKey = module * 1000 + cmd;
                if (!responseLock.ContainsKey(lockKey))
                {
                    responseLock.Add(lockKey, 0);
                }
            }

            if (respType != null)
            {
                base.RegisterResponseType(module, cmd, respType);
            }
        }


        #region Protocol
        /// <summary>
        /// 測試協議
        /// </summary>
        public virtual void SendTestRequest()
        {
            List<int> list = new List<int>();
            list.Add(1);

            Dictionary<byte, string> dict = new Dictionary<byte, string>();
            dict[0] = "0 byte";
            dict[1] = "1 byte";
            TestRequest value = new TestRequest(
                1,
                2,
                3,
                4,
                5.6f,
                7.8,
                "string",
                list, dict);
            SendMsg(100, 2, value);
        }


        /// <summary>
        /// 獲取大樹信息，主要是活動相關的信息，後期可以考慮把大樹等級信息加上
        /// </summary>
        public virtual void GetTreeInfo()
        {
            SendMsg((byte)Module.Tree, (byte)TreeCmd.GET_INFO, null);
        }

        /// <summary>
        /// 進入大樹活動戰斗場景
        /// </summary>
        public virtual void EnterTreeActivity()
        {
            SendMsg((byte)Module.Tree, (byte)TreeCmd.ENTER, null);
        }

        /// <summary>
        /// 大樹戰鬥提交
        /// </summary>
        /// <param name="activityTime"></param>
        /// <param name="activityId"></param>
        /// <param name="steps"></param>
        /// <param name="damage"></param>
        /// <param name="rewards"></param>
        public virtual void SubmitTreeFightRequest(int activityTime, short activityId, List<Step> steps, int damage, Dictionary<byte, int> rewards)
        {
            SubmitTreeFightRequest request = new SubmitTreeFightRequest(activityTime, activityId, steps, damage, rewards);
            SendMsg((byte)Module.Tree, (byte)TreeCmd.SUBMIT_FIGHT, request);
        }

        /// <summary>
        /// 升級夥伴
        /// </summary>
        /// <param name="unitId"></param>
        public virtual void UpgradUnit(int unitId)
        {
            UpgradeUnitRequest req = new UpgradeUnitRequest();
            req.unitId = unitId;
            SendMsg((byte)Module.Unit, (byte)UnitCmd.UPGRAD_UNIT, req);
        }

        /// <summary>
        /// 自動升級夥伴
        /// </summary>
        /// <param name="unitId"></param>
        public virtual void AutoUpgradUnit(int unitId)
        {
            FastUpgradeUnitRequest req = new FastUpgradeUnitRequest();
            req.unitId = unitId;
            SendMsg((byte)Module.Unit, (byte)UnitCmd.FAST_UPGRAD_UNIT, req);
        }

        /// <summary>
        /// 購買步數
        /// </summary>
        public virtual void BuyStep()
        {
            SendMsg((byte)Module.Stage, (byte)StageCmd.GEM_BUY_STEP, null);
        }

        /// <summary>
        /// 購買時間
        /// </summary>
        public virtual void BuyTime()
        {
            SendMsg((byte)Module.Stage, (byte)StageCmd.GEM_BUY_TIME, null);
        }

        /// <summary>
        /// 抽獎
        /// </summary>
        public virtual void OpenBox()
        {
            SendMsg((byte)Module.GetChance, (byte)GetChanceCmd.OPEN_BOX, null);
        }

        public virtual void BuyKey()
        {
            SendMsg((byte)Module.GetChance, (byte)GetChanceCmd.BUY_KEYS, null);
        }

        public virtual void StageData(int stageId)
        {
            StageDataRequest req = new StageDataRequest();
            req.stageId = stageId;

            SendMsg((byte)Module.Stage, (byte)StageCmd.STAGE_DATA, req);
        }

        public virtual void GemUnlockStage(int stageId)
        {
            UnlockStageRequest req = new UnlockStageRequest();
            req.stageId = stageId;

            SendMsg((byte)Module.Stage, (byte)StageCmd.GEM_UNLOCK_STAGE, req);
        }

        public virtual void SendListStageData(int startIdx, int endIdx)
        {
            ListStageDataRequest stageList = new ListStageDataRequest();
            stageList.startId = startIdx;
            stageList.endId = endIdx;
            SendMsg((byte)Module.Stage, (byte)StageCmd.LIST_STAGEDATA, stageList);
        }

        /// <summary>
        /// 獲取關卡最高通關記錄
        /// </summary>
        public virtual void GetStageInfo()
        {
            SendMsg((byte)Module.Stage, (byte)StageCmd.GET_STAGEINFO, null);
        }

        /// <summary>
        /// 購買體力
        /// </summary>
        /// <param name="type"></param>
        public virtual void BuyEnergy(byte type)
        {
            BuyEnergyRequest req = new BuyEnergyRequest();
            req.type = type;
            SendMsg((byte)Module.Energy, (byte)EnergyCmd.BUY_ENERGY, req);
        }

        /// <summary>
        /// 提升體力上限
        /// </summary>
        /// <param name="type"></param>
        public virtual void BuyMaxEnergy(byte type)
        {
            BuyMaxEnergyRequest req = new BuyMaxEnergyRequest();
            req.type = type;
            SendMsg((byte)Module.Energy, (byte)EnergyCmd.INCRE_MAXENERGY, req);
        }

        /// <summary>
        /// 獲取最新體力
        /// </summary>
        public virtual void RefreshEnergy()
        {
            SendMsg((byte)Module.Energy, (byte)EnergyCmd.REFRESH_ENERGY, null);
        }

        /// <summary>
        /// 獲取玩家信息
        /// </summary>
        public virtual void ActorGame()
        {
            SendMsg((byte)Module.ActorGame, (byte)ActorGameCmd.GET_DATA, null);
        }
        /// <summary>
        /// 已經改成遊戲累計登陸獎勵的領取接口////
        /// 獲取獎勵信息見SignInfo////
        /// </summary>
        public virtual void Sign()
        {
            SendMsg((byte)Module.SIGN, (byte)SignCmd.SIGN, null);
        }
        /// <summary>
        ///獲取登陸獎勵的信息，得到登陸天數和改登陸天數的獎勵是否已經領取///
        /// </summary>
        public virtual void SignInfo()
        {
            SendMsg((byte)Module.SIGN, (byte)SignCmd.INFO, null);
        }

        /// <summary>
        /// 玩家登錄
        /// </summary>
        /// <param name="token"></param>
        /// <param name="protocolVersion"></param>
        /// <param name="platformId"></param>
        /// <param name="sdkVersion"></param>
        public virtual void UserLogin(string token,
                                      string protocolVersion,
                                      string platformId,
                                      string sdkVersion = "")
        {
            Q.Log("登錄遊服 token={0}, ver={1}, platformId={2}, sdk={3}", token, protocolVersion, platformId, sdkVersion);
            UserLoginRequest req = new UserLoginRequest();
            req.token = token;
            req.version = protocolVersion;
            req.platformId = platformId;
            req.sdkVersion = sdkVersion;
            SendMsg((byte)Module.User, (byte)UserCmd.USER_LOGIN, req);

            if (NoServer)
            {
                // no server play
                UserLoginResponse res = new UserLoginResponse();
                res.uid = "uid";
                res.reconnectId = token;
                ReceiveMsg((byte)Module.User, (byte)UserCmd.USER_LOGIN, res, (short)ResponseCode.SUCCESS);
            }
        }

        /// <summary>
        /// 獲取角色列表
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="channelId"></param>
        public virtual void GetActor(int serverId, string channelId)
        {
            GetActorRequest req = new GetActorRequest();
            req.serverId = serverId;
            req.channelId = channelId;
            SendMsg((byte)Module.User, (byte)UserCmd.GET_ACTOR, req);

            if (NoServer)
            {
                // local play
                GetActorResponse res = new GetActorResponse();
                res.list.Add(new ActorInfo(1, "actorName", 123456));
                ReceiveMsg((byte)Module.User, (byte)UserCmd.GET_ACTOR, res, (short)ResponseCode.SUCCESS);
            }
        }


        /// <summary>
        /// 創建角色
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="actorName"></param>
        /// <param name="channelId"></param>
        /// <param name="sim"></param>
        /// <param name="mac"></param>
        /// <param name="imei"></param>
        public virtual void CreateActor(int serverId, string actorName, string channelId,
                                string osversion = "", string phonetype = "",
                                string sim = "", string mac = "", string imei = "",
                                string phoneos = "", string language = "")
        {
            CreateActorRequest req = new CreateActorRequest();
            req.serverId = serverId;
            req.actorName = actorName;
            req.channelId = channelId;
            req.sim = sim;
            req.mac = mac;
            req.imei = imei;
            req.osversion = osversion;
            req.phonetype = phonetype;
            req.phoneos = phoneos;
            req.language = language;
            SendMsg((byte)Module.User, (byte)UserCmd.CREATE_ACTOR, req);
        }

        /// <summary>
        /// 角色登錄
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="actorId"></param>
        /// <param name="sim"></param>
        /// <param name="mac"></param>
        /// <param name="imei"></param>
        public virtual void ActorLogin(int serverId, long actorId,
                               string sim = "", string mac = "", string imei = "")
        {
            ActorLoginRequest req = new ActorLoginRequest();
            req.serverId = serverId;
            req.actorId = actorId;
            req.sim = sim;
            req.mac = mac;
            req.imei = imei;
            SendMsg((byte)Module.User, (byte)UserCmd.ACTOR_LOGIN, req);

            if (NoServer)
            {
                ActorLoginResponse res = new ActorLoginResponse(1, "actorName", "channelId", "platformId", 1);
                ReceiveMsg((byte)Module.User, (byte)UserCmd.ACTOR_LOGIN, res, (short)ResponseCode.SUCCESS);
            }
            
        }

        /// <summary>
        /// 發送斷線重連
        /// </summary>
        /// <param name="platformId">平台ID</param>
        /// <param name="connectionId">連接ID</param>
        /// <param name="version">版本號</param>
        public virtual void Reconnect(string platformId, string connectionId, long actorId, string version)
        {
            UserReconnectRequest req = new UserReconnectRequest();
            req.platformId = platformId;
            req.connectionId = connectionId;
            req.version = version;
            req.actorId = actorId;

            Q.Log(LogTag.Test, "QmaxClient:Reconnect actorID={0}", actorId);

            SendMsg((byte)Module.User, (byte)UserCmd.USER_RECONNECTION, req);
        }


        /// <summary>
        /// 進入關卡請求
        /// </summary>
        /// <param name="stageId"></param>
        /// <param name="units"></param>
        public virtual void BeginStage(int stageId, List<int> units, Dictionary<int, int> useGoods)
        {
            BeginStageRequest req = new BeginStageRequest();
            req.stageId = stageId;
            req.unitList = units;
            req.useGoods = useGoods;
            SendMsg((byte)Module.Stage, (byte)StageCmd.STAGE_BEGIN, req);
        }

        /// <summary>
        /// 提交戰鬥結果數據
        /// </summary>
        /// <param name="stageId"></param>
        /// <param name="steps"></param>
        public virtual void SubmitStageFightRequest(int stageId, int star, int score, List<Step> steps, Dictionary<byte, int> rewards)
        {
            SubmitStageFightRequest req = new SubmitStageFightRequest();
            req.stageId = stageId;
            req.steps = steps;
            req.star = star;
            req.rewards = rewards;
            req.score = score;
            Q.Log(LogTag.Test, req.ToString());
            //正式協議
            //SendMsg((byte)Module.Stage, (byte)StageCmd.SUBMIT_STAGEFIGHT, req);
            //測試協議
            SendMsg((byte)Module.Stage, (byte)StageCmd.SUBMIT_STAGEFIGHT_TEST, req);
        }


        /// <summary>
        /// 主動發送心跳包
        /// </summary>
        public virtual void HeartBeat()
        {
            SendMsg((byte)Module.User, (byte)UserCmd.HEART_BEAT, null);
        }


        /// <summary>
        /// 獲取購買鑽石配置
        /// </summary>
        /// <param name="httpRoot"></param>
        public virtual void GetRechargeInfo(string httpRoot)
        {
            string url = httpRoot + "/payment/PaymentSystemConfig.xml";
            object data = new { time = UnityEngine.Time.time };

            HttpUtils.Get(url, data, delegate(HttpWebResponse resp)
            {
                if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                {
                    string res = HttpUtils.ReadHttpResponse(resp);
                    ReceiveMsg((byte)Module.Http, (byte)HttpCmd.RECHARGE_INFO, res, 0);
                }
                else
                {
                    HandleWebException((byte)Module.Http, (byte)HttpCmd.RECHARGE_INFO, resp.StatusCode);
                }
            },
            delegate(Exception e)
            {
                HandleWebException((byte)Module.Http, (byte)HttpCmd.RECHARGE_INFO, HttpStatusCode.NotFound, e as WebException);
            });
        }

        public virtual void Recharge(string url,
                                     string platformId,
                                     int serverId,
                                     string uid,
                                     long actorId,
                                     string rechargeId)
        {
            //object data = new { time = UnityEngine.Time.time, uid = uid, platformId = platformId, serverId = serverId, actorId = actorId, rechargeId = rechargeId };
            object data = new { serverId = serverId, actorId = actorId, rechargeId = rechargeId };

            //string urlF = string.Format("{0}actorId={1}&serverId={2}&cfgId={3}", url, actorId, serverId, rechargeId);

            HttpUtils.Post(url, data, delegate(HttpWebResponse resp)
            {
                if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                {
                    //string res = HttpUtils.ReadHttpResponse(resp);
                    //JSONInStream json = new JSONInStream(res);
                    int code = 0;
                    //json.Content("status", out code);
                    ReceiveMsg((byte)Module.Http, (byte)HttpCmd.PUSH_RECHARGE_RESULT, null, (short)code);
                }
                else
                {
                    HandleWebException((byte)Module.Http, (byte)HttpCmd.PUSH_RECHARGE_RESULT, resp.StatusCode);
                }
            },
            delegate(Exception e)
            {
                HandleWebException((byte)Module.Http, (byte)HttpCmd.PUSH_RECHARGE_RESULT, HttpStatusCode.NotFound, e as WebException);
            });
        }


        /// <summary>
        /// HTTP登錄
        /// </summary>
        /// <param name="url">http請求的url</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="callback"></param>
        /// <param name="failCallback"></param>
        public virtual void HttpLogin(string url,
                                      string username,
                                      string password,
                                      Action<LoginResponse> callback,
                                      Action<ResponseCode> failCallback)
        {
            Q.Log(LogTag.Net, "QmaxClient::HttpLogin, url={0}, username={1}, password={2}", url, username, password);

            if (SkipHttpServer)
            {
                //回調要在主線程里處理
                GameController.Instance.InvokeOnMainThread(() =>
                {
                    LoginResponse lp = new LoginResponse();
                    lp.UID = username;
                    lp.Token = password;
                    lp.StatusCode = ResponseCode.SUCCESS;
                    callback(lp);
                });
            }
            else
            {
                password = Utils.GetMd5Hash(password);

                object data = new { username = username, password = password };
                HttpUtils.Post(url, data, delegate (HttpWebResponse resp)
                {
                    if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                    {
                        string res = HttpUtils.ReadHttpResponse(resp);
                        Q.Log("登錄返回字符串：" + res);
                        LoginResponse lp = new LoginResponse();
                        JSONInStream json = new JSONInStream(res);
                        int status = 0;
                        int uid = 0;
                        json.Content("statusCode", out status)
                            .Content("uid", out uid)
                            .Content("token", out lp.Token);
                        lp.StatusCode = (ResponseCode)status;
                        lp.UID = uid.ToString();


                        //回調要在主線程里處理
                        GameController.Instance.InvokeOnMainThread(() =>
                        {
                            if (lp.StatusCode == 0 && callback != null)
                                callback(lp);
                            else if (failCallback != null)
                                failCallback(lp.StatusCode);
                        });

                        ReceiveMsg((byte)Module.Http, (byte)HttpCmd.LOGIN, lp, (short)lp.StatusCode);
                    }
                    else
                    {
                        if (failCallback != null)
                        {
                            GameController.Instance.InvokeOnMainThread(() =>
                            {
                                failCallback(ResponseCode.HTTP_FAIL);
                            });
                        }

                        HandleWebException((byte)Module.Http, (byte)HttpCmd.LOGIN, resp.StatusCode);
                    }
                },
                delegate (Exception e)
                {
                    if (failCallback != null)
                    {
                        GameController.Instance.InvokeOnMainThread(() =>
                        {
                            failCallback(ResponseCode.TIME_OUT);
                        });
                    }

                    HandleWebException((byte)Module.Http, (byte)HttpCmd.LOGIN, HttpStatusCode.NotFound, e as WebException);
                });
            }

        }//Login


        /// <summary>
        /// Http註冊
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public virtual void HttpRegister(string url,
                                         string username,
                                         string password,
                                         Action<RegisterResponse> callback,
                                         Action<ResponseCode> failCallback)
        {
            Debug.LogFormat("註冊: url={0}, username={1}, psw={2}", url, username, password);
            password = Utils.GetMd5Hash(password);
            object data = new { username = username, password = password };

            if (SkipHttpServer)
            {
                //回調要在主線程里處理
                GameController.Instance.InvokeOnMainThread(() =>
                {
                    RegisterResponse res = new RegisterResponse();
                    res.StatusCode = 0;
                    res.Token = password;
                    res.Uid = username;
                    callback(res);
                });
            }
            else
            {

                HttpUtils.Post(url, data, delegate (HttpWebResponse resp)
                {
                    if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                    {
                        //{"statusCode":1002,"token":"","uid":0}
                        string resStr = HttpUtils.ReadHttpResponse(resp);
                        Debug.Log("註冊返回:" + resStr);
                        RegisterResponse res = new RegisterResponse();
                        JSONInStream json = new JSONInStream(resStr);
                        int status = 0;
                        int uid = 0;
                        json.Content("statusCode", out status)
                            .Content("uid", out uid)
                            .Content("token", out res.Token);
                        res.StatusCode = (ResponseCode)status;
                        res.Uid = uid.ToString();

                        //回調要在主線程里處理
                        GameController.Instance.InvokeOnMainThread(() =>
                        {
                            if (res.StatusCode == 0 && callback != null)
                                callback(res);
                            else if (failCallback != null)
                                failCallback(res.StatusCode);
                        });

                        ReceiveMsg((byte)Module.Http, (byte)HttpCmd.REGISTER, res, (short)res.StatusCode);
                    }
                    else
                    {
                        if (failCallback != null)
                        {
                            GameController.Instance.InvokeOnMainThread(() => { failCallback(ResponseCode.HTTP_FAIL); });
                        }

                        HandleWebException((byte)Module.Http, (byte)HttpCmd.REGISTER, resp.StatusCode);
                    }
                },
                delegate (Exception e)
                {
                    if (failCallback != null)
                    {
                        GameController.Instance.InvokeOnMainThread(() => { failCallback(ResponseCode.TIME_OUT); });
                    }
                    HandleWebException((byte)Module.Http, (byte)HttpCmd.REGISTER, HttpStatusCode.NotFound, e as WebException);
                });
            }
        }

        public virtual void SaveGuideIndex(int guideIndex, int guideid)
        {
            ActorSaveGuideRequest req = new ActorSaveGuideRequest();
            req.currentGuideStep = guideIndex;
            req.guideId = guideid;

            SendMsg((byte)Module.ActorGame, (byte)ActorGameCmd.SAVE_GUIDE, req);
        }

        public virtual void RechargeRefresh()
        {
#if UNITY_ANDROID || UNITY_IOS
            (GameController.Instance.Client as QmaxClient2).KeepConnection = true;
#endif
            SendMsg((byte)Module.ActorGame, (byte)ActorGameCmd.RECHARGE_REFRESH, null);
        }

        public virtual void GetNotice(string url,
                                      Action<string> callback,
                                      Action<ResponseCode> failCallback)
        {
            object data = new { time = UnityEngine.Time.time };
            HttpUtils.Get(url, data, delegate(HttpWebResponse resp)
            {
                if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                {
                    string res = HttpUtils.ReadHttpResponse(resp);
                    res = res.Replace("\\r\\n", "\r\n");

                    if (callback != null)
                    {
                        GameController.Instance.InvokeOnMainThread(() => { callback(res); });
                    }

                    ReceiveMsg((byte)Module.Http, (byte)HttpCmd.GET_NOTICE, res, 0);
                }
                else
                {
                    if (failCallback != null)
                    {
                        GameController.Instance.InvokeOnMainThread(() => { failCallback(ResponseCode.REQ_NOTICE_FAIL); });
                    }
                    HandleWebException((byte)Module.Http, (byte)HttpCmd.GET_NOTICE, resp.StatusCode);
                }
            },
            delegate(Exception e)
            {
                if (failCallback != null)
                {
                    GameController.Instance.InvokeOnMainThread(() => { failCallback(ResponseCode.REQ_NOTICE_FAIL); });
                }
                HandleWebException((byte)Module.Http, (byte)HttpCmd.GET_NOTICE, HttpStatusCode.NotFound, e as WebException);
            });
        }

        public virtual void GetAddress(string httpRoot,
                                       string platformID,
                                       string channelID,
                                       string token,
                                       string sdkVersion,
                                       string uid,
                                       Action<GetAddressResponse> callback,
                                       Action<ResponseCode> failCallback)
        {
            string url = httpRoot + "getAddress";
            Q.Log(LogTag.Login, "QmaxClient:GetAddress 1 url={0}, platformId={1}, channelId={2}", url, platformID, channelID);
            object data = new { time = UnityEngine.Time.time, platformId = platformID, channelId = channelID, sdkVersion = sdkVersion, token = token };

            if (SkipHttpServer)
            {
                GetAddressResponse ar = new GetAddressResponse();
                string msg = "success";
                int status = (int)ResponseCode.SUCCESS;

                switch (GameController.Instance.UsedNetType)
                {
                    case NetType.Xiaolu:
                        ar.Host = "35.194.219.198";
                        ar.Port = 20001;
                        break;
                    case NetType.Localhost:
                    default:
                        ar.Host = "127.0.0.1";
                        ar.Port = 10123;
                        break;
                }
                ar.serverId = 1;

                Q.Log(LogTag.Login, "QmaxClient:GetAddress response code={0}, message{1}, host={2}, serverId={3}, port={4}", status, msg, ar.Host, ar.serverId, ar.Port);

                callback(ar);
            }
            else
            {
                HttpUtils.Post(url, data, delegate (HttpWebResponse resp)
                {
                    if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                    {
                        string res = HttpUtils.ReadHttpResponse(resp);
                        Q.Log(LogTag.Login, "QmaxClient:GetAddress response data={0}", res);

                        GetAddressResponse ar = new GetAddressResponse();
                        JSONInStream json = new JSONInStream(res);
                        string msg = null;
                        int status = 0;
                        json.Content("code", out status)
                            .Content("message", out msg)
                            .Content("host", out ar.Host)
                            .Content("serverId", out ar.serverId)
                            .Content("port", out ar.Port);
                        ResponseCode respCode = (ResponseCode)status;
                        Q.Log(LogTag.Login, "QmaxClient:GetAddress response code={0}, message{1}, host={2}, serverId={3}, port={4}", status, msg, ar.Host, ar.serverId, ar.Port);

                        GameController.Instance.InvokeOnMainThread(() =>
                        {
                            if (respCode == ResponseCode.SUCCESS && callback != null)
                            {
                                callback(ar);
                            }
                            else if (failCallback != null)
                            {
                                failCallback(respCode);
                            }
                        });


                        ReceiveMsg((byte)Module.Http, (byte)HttpCmd.GET_ADDRESS, ar, (short)status);
                    }
                    else
                    {
                        if (failCallback != null)
                            GameController.Instance.InvokeOnMainThread(() => { failCallback(ResponseCode.HTTP_FAIL); });

                        HandleWebException((byte)Module.Http, (byte)HttpCmd.GET_ADDRESS, resp.StatusCode);
                    }
                },
                delegate (Exception e)
                {
                    if (failCallback != null)
                        GameController.Instance.InvokeOnMainThread(() => { failCallback(ResponseCode.TIME_OUT); });
                    HandleWebException((byte)Module.Http, (byte)HttpCmd.GET_ADDRESS, HttpStatusCode.NotFound, e as WebException);
                });
            }
        }

        //UpgradeA:1   UpgradeB:2
        public virtual void BuyUpgrade(Dictionary<byte, int> buyArgs)
        {
            BuyUpgradeRequest req = new BuyUpgradeRequest();
            req.buyArgs = buyArgs;

            SendMsg((byte)Module.Unit, (byte)UnitCmd.BUY_UPGRADE, req);
        }

        /// <summary>
        /// 兌換碼
        /// </summary>
        /// <param name="code"></param>
        public virtual void ExchangeCode(string code)
        {
            ExchangeCodeRequest exchangeCodeRequest = new ExchangeCodeRequest(code);
            SendMsg((byte)Module.Activity, (byte)ActivityCmd.EXCHANGE_CODE, exchangeCodeRequest);
        }
        #endregion Protocol


        /// <summary>
        /// 統一處理Http請求的WebException
        /// </summary>
        /// <param name="module"></param>
        /// <param name="cmd"></param>
        /// <param name="e"></param>
        protected short HandleWebException(byte module,
                                           byte cmd,
                                           HttpStatusCode statusCode,
                                           WebException e = null)
        {
            ResponseCode status = ResponseCode.SUCCESS;
            if (e != null)
            {
                Q.Warning("QmaxClient:WebException module={0}, cmd={1}, status={2}, msg={3}", module, cmd, e.Status, e.Message);
                switch (e.Status)
                {
                    case WebExceptionStatus.Timeout:
                        status = ResponseCode.TIME_OUT;
                        break;
                    default:
                        status = ResponseCode.CONNECT_FAIL;
                        break;
                }
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                status = ResponseCode.HTTP_FAIL;
            }
            ReceiveMsg(module, cmd, null, (short)status);
            return (short)status;
        }


        /// <summary>
        /// 發送一個沒有返回的消息///
        /// </summary>
        public void SendMessageNoRe(string Url, object data)
        {
            //             HttpLib.Request.Post(Url, data, delegate(string res)
            //             {
            //                 //TODO
            //             }, delegate(WebException ex)
            //             {
            //                 //TODO
            //             }, delegate()
            //             { 
            //                 //TODO
            //             });
        }

        /// <summary>
        /// 發送打開成就界面的請求
        /// </summary>
        public void OpenAchieveData(byte type = 99)
        {
            string channelId = GameController.Instance.Model.SdkData.channalID;
            AchievementListRequest achieveRequest = new AchievementListRequest(channelId, type);
            SendMsg((byte)Module.Achievement, (byte)AchievementCmd.GET_ACHIEVEMENT_LIST, achieveRequest);
        }

        /// <summary>
        /// 領取成就
        /// </summary>
        public void RewardAchieve(int achievementId)
        {
            AchievementReceiveRequest achieveReceiveRequest = new AchievementReceiveRequest(achievementId);
            SendMsg((byte)Module.Achievement, (byte)AchievementCmd.GET_ACHIEVEMENT_REWARD, achieveReceiveRequest);
        }

        /// <summary>
        /// 刷新成就
        /// </summary>
        public void RefreshAchieve()
        {
            string channelId = GameController.Instance.Model.SdkData.channalID;
            AchievementRefreshRequest refreshRequest = new AchievementRefreshRequest(channelId);
            SendMsg((byte)Module.Achievement, (byte)AchievementCmd.REFRESH_ACHIEVEMENT_LIST, refreshRequest);
        }

        /// <summary>
        /// 獲取已達成的成就數量
        /// </summary>
        public void GetReachAchieveCount()
        {
            string channelId = GameController.Instance.Model.SdkData.channalID;
            AchievementPollingRequest pollingRequest = new AchievementPollingRequest(channelId);
            SendMsg((byte)Module.Achievement, (byte)AchievementCmd.ROLL_POLLING_ACHIEVE, pollingRequest);
        }

        #region 背包協議
        /// <summary>
        /// 刷新背包物品
        /// </summary>
        public void GetAllGoodsList()
        {
            SendMsg((byte)Module.Goods, (byte)GoodsCmd.GET_ALL_GOODS, null);
        }

        /// <summary>
        /// 使用背包物品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        public void UseGoods(int id, int num)
        {
            GoodsItem item = new GoodsItem(id, num);
            SendMsg((byte)Module.Goods, (byte)GoodsCmd.USE_GOODS, item);
        }

        public void BuyGoods(int cfgId)
        {
            List<int> ids = new List<int>();
            ids.Add(cfgId);
            BuyGoodsRequest buyGoods = new BuyGoodsRequest(ids);
            SendMsg((byte)Module.Goods, (byte)GoodsCmd.BUY_GOODS, buyGoods);
        }

        public void BuyGoodsList(List<int> cfgIds)
        {
            BuyGoodsRequest buyGoods = new BuyGoodsRequest(cfgIds);
            SendMsg((byte)Module.Goods, (byte)GoodsCmd.BUY_GOODS, buyGoods);
        }

        #endregion

        #region 郵箱
        public void FeedBack(long actorId, string qq, string mail, string content)
        {
            FeedBackRequest req = new FeedBackRequest();
            req.actorId = actorId;
            req.qq = qq;
            req.mail = mail;
            req.content = content;
            req.feedbackType = 5;
            SendMsg((byte)Module.Mail, (byte)MailCmd.FEEDBACK, req);
        }
        #endregion

        #region 活動關卡 關卡副本

        /// <summary>
        /// 開始進入活動關卡
        /// </summary>
        /// <param name="stageId"></param>
        /// <param name="unitList"></param>
        public void CounterpartBeginStage(int stageId, List<int> unitList)
        {
            CounterpartBeginStageRequest req = new CounterpartBeginStageRequest();
            req.stageId = stageId;
            req.unitList = unitList;
            SendMsg((byte)Module.Counterpart, (byte)CounterpartCmd.BEGAIN_STAGE, req);
        }

        public void CounterpartStageList()
        {
            SendMsg((byte)Module.Counterpart, (byte)CounterpartCmd.GET_STAGE_LIST, null);
        }

        public void SubmitCounterpart(int stageId, List<Step> steps, int damage, Dictionary<byte, int> rewards)
        {
            SubmitCounterpartRequest req = new SubmitCounterpartRequest();
            req.stageId = stageId;
            req.damage = damage;
            req.rewards = rewards;
            req.steps = steps;

            SendMsg((byte)Module.Counterpart, (byte)CounterpartCmd.SUBMIT_COUNTERPART_FIGHT, req);
        }

        #endregion


        /// <summary>
        /// IAP驗證收據
        /// </summary>
        /// <param name="receipt"></param>
        public void IapValidateReceipt(string receipt)
        {
            var req = new PurchaseRequest();
            req.receipt = receipt;
            SendMsg((byte)Module.Purchase, (byte)PurchaseCmd.VALIDATE, req);
        }
    }
}
