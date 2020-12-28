using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.Base;
using Com4Love.Qmax.Net.Protocols.Energy;
using Com4Love.Qmax.Net.Protocols.getchance;
using Com4Love.Qmax.Net.Protocols.Stage;
using Com4Love.Qmax.Net.Protocols.Unit;
using Com4Love.Qmax.Net.Protocols.User;
using System;
using System.Collections.Generic;

namespace Com4Love.Qmax.Net
{
    /// <summary>
    /// 离线层，与QmaxClient平行，模拟网络层回包
    /// TODO 后续会扩展，将真实数据保存在本地，完善离线玩法
    /// </summary>
    public class QmaxOutlineClient : QmaxClientEx
    {
        public override bool HeartBeatEnabled { set; get; }
        public override bool Connected { get { return true; } }

        protected QmaxModel model;


        public QmaxOutlineClient(QmaxModel model)
        {
            this.model = model;
        }


        public override void Connect(string host, int port)
        {
            //base.Connect(host, port);
            //Do nonthing.
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        public override void Close() { }

        /// <summary>
        /// 获取玩家信息
        /// </summary>
        public override void ActorGame()
        {
            ActorGameResponse res = model.PlayerData;
            if (res == null)
            {
                res = new ActorGameResponse();
                TestData td = model.TestData;
                res.energy = (short)td.CrtEnergy;
                res.energyMax = (short)td.MaxEnergy;
                res.gem = td.Gem;
                res.key = 10;
                res.upgradeA = td.UpgradeA;
                res.upgradeB = td.UpgradeB;
                //假设最近一次涨体力是5分钟前
                res.fixEnergyTime = (int)Utils.DateTimeToUnixTime(DateTime.UtcNow.AddMinutes(-5));
                res.list = new List<Protocols.Unit.Unit>();
                for (int i = 0, n = td.OwnUnits.Count; i < n; i++)
                {

                    res.list.Add(new Protocols.Unit.Unit(td.OwnUnits[i]));
                }

                res.lastFightUnits = new List<int>(td.LastFightUnits);
                res.passStageId = td.PassStageId;
            }
            AddResponse(Module.ActorGame, (byte)ActorGameCmd.GET_DATA, 0, res);
        }

        public override void ActorLogin(int serverId, long actorId, string sim = "", string mac = "", string imei = "")
        {
            //base.ActorLogin(serverId, actorId, sim, mac, imei);
            ActorLoginResponse res = new ActorLoginResponse();
            res.actorId = 1;
            res.actorName = "name";
            res.channelId = "channelID";
            AddResponse(Module.User, (byte)UserCmd.ACTOR_LOGIN, 0, res);
        }

        public override void BeginStage(int stageId, List<int> units, Dictionary<int, int> usegoods)
        {
            //base.BeginStage(stageId, units);
            ValueResultListResponse res = new ValueResultListResponse();
            res.list = new List<ValueResult>();
            StageConfig config = null;
            short code = 0;
            if (!model.StageConfigs.ContainsKey(stageId))
                code = 501;//关卡不存在
            else if (stageId > model.PlayerData.passStageId + 1)
                code = 502;//当前关卡尚未开启
            else
            {
                config = model.StageConfigs[stageId];
                if (config.CostEnergy > model.PlayerData.energy)
                    code = 301;//体力不足
                else if (config.CostGem.Qtt > model.PlayerData.gem)
                    code = 301;//钻石不足
                //TODO 暂时不做其他条件判断
            }

            if (code == 0)
            {
                //扣除体力
                ValueResult v = new ValueResult(
                    (int)RewardType.Energy,
                    model.PlayerData.energy - config.CostEnergy, 0,
                    config.CostEnergy);
                res.list.Add(v);
                //扣除钻石
                v = new ValueResult(
                    (int)RewardType.Gem,
                    model.PlayerData.gem - config.CostGem.Qtt, 0,
                    config.CostGem.Qtt);
                res.list.Add(v);
            }

            AddResponse(Module.Stage, (byte)StageCmd.STAGE_BEGIN, code, res);
        }


        /// <summary>
        /// 购买体力
        /// </summary>
        /// <param name="type"></param>
        public override void BuyEnergy(byte type)
        {
            //base.BuyEnergy(byte);
            //离线状态下无法进行购买行为
            ValueResultListResponse res = new ValueResultListResponse();
            res.list = new List<ValueResult>();

            AddResponse(Module.Energy, (byte)EnergyCmd.BUY_ENERGY, -1, res);
        }

        public override void BuyMaxEnergy(byte type)
        {
            //base.BuyMaxEnergy(type);
            //离线状态下无法进行购买行为
            ValueResultListResponse res = new ValueResultListResponse();
            res.list = new List<ValueResult>();
            AddResponse(Module.Energy, (byte)EnergyCmd.INCRE_MAXENERGY, -1, res);
        }


        public override void CreateActor(int serverId, string actorName, string channelId,
                                string osversion = "", string phonetype = "",
                                string sim = "", string mac = "", string imei = "",
                                string phoneos = "", string language = "")
        {
            //base.CreateActor(serverId, actorName, channelId, sim, mac, imei);
            CreateActorResponse res = new CreateActorResponse();
            res.actorId = 123456;
            AddResponse(Module.User, (byte)UserCmd.CREATE_ACTOR, 0, res);
        }


        public override void GetActor(int serverId, string channelId)
        {
            //base.GetActor(serverId, channelId);
            GetActorResponse res = new GetActorResponse();
            res.list = new List<ActorInfo>();
            ActorInfo info = new ActorInfo();
            info.actorId = 123456;
            info.actorName = "";
            DateTime d = DateTime.Now.AddDays(-1);
            info.lastLogoutTime = (int)Utils.DateTimeToUnixTime(d);
            res.list.Add(info);
            AddResponse(Module.User, (byte)UserCmd.GET_ACTOR, 0, res);
        }

        public override void GetStageInfo()
        {
            //base.GetStageInfo();
            StageInfoResponse res = new StageInfoResponse();
            if (model.PlayerData == null || model.PlayerData.passStageId == 0)
                res.passStageId = 20;
            else
                res.passStageId = model.PlayerData.passStageId;
            AddResponse(Module.Stage, (byte)StageCmd.GET_STAGEINFO, 0, res);
        }

        public override void HeartBeat()
        {
            //base.HeartBeat();
            //Do nothing.
        }


        public override void HttpLogin(string url,
                                       string username,
                                       string password,
                                       Action<LoginResponse> callback,
                                       Action<ResponseCode> failCallback)
        {
            //base.Login(url, username, password, hash);
            LoginResponse resp = new LoginResponse();
            resp.UID = "123465";
            resp.Token = "";

            if (callback != null)
                callback(resp);

            AddResponse(Module.Http, (byte)HttpCmd.LOGIN, 0, resp);
        }

        public override void OpenBox()
        {
            //base.OpenBox();
            ValueResult vr = new ValueResult();
            vr.changeType = 1;

            int[] ChanceTest = { 106, 203, 303, 405, 106, 203, 303 };
            RewardType[] valueTypes = { RewardType.UpgradeA, RewardType.UpgradeB, RewardType.Gem, RewardType.Unit, RewardType.UpgradeA, RewardType.UpgradeB, RewardType.Gem };
            int[] Counts = { 70, 10, 50, 5101, 70, 10, 50 };
            int[] Units = { 2101, 3101, 4101, 5101 };

            int testIndex = UnityEngine.Random.Range(0, ChanceTest.Length);
            //随机送出
            int chanceId = ChanceTest[testIndex];
            vr.valuesType = (int)valueTypes[testIndex];
            vr.changeValue = Counts[testIndex];

            switch (vr.valuesType)
            {
                case (int)RewardType.UpgradeA:
                    vr.current = model.PlayerData.upgradeA + vr.changeValue;
                    break;
                case (int)RewardType.UpgradeB:
                    vr.current = model.PlayerData.upgradeB + vr.changeValue;
                    break;
                case (int)RewardType.Gem:
                    vr.current = model.PlayerData.gem + vr.changeValue;
                    break;
                case (int)RewardType.Unit:
                    //判断玩家有没有此伙伴
                    int unitIndex = UnityEngine.Random.Range(0, Units.Length);
                    vr.current = vr.changeValue = Units[unitIndex];
                    break;
                case (int)RewardType.Energy:
                    vr.current = model.PlayerData.energy + vr.changeValue;
                    break;
            }

            GetChanceResponse res = new GetChanceResponse();
            res.id = chanceId;
            res.valueResultListResponse = new ValueResultListResponse();
            res.valueResultListResponse.list = new List<ValueResult>();
            res.valueResultListResponse.list.Add(vr);

            vr = new ValueResult();
            vr.changeType = 0;
            vr.changeValue = 1;
            vr.current = GameController.Instance.PlayerCtr.PlayerData.key - 1;
            vr.valuesType = (int)RewardType.Key;
            res.valueResultListResponse.list.Add(vr);

            AddResponse(Module.GetChance, (byte)GetChanceCmd.OPEN_BOX, 0, res);
        }


        public override void RefreshEnergy()
        {
            //base.RefreshEnergy();
            RefreshEnergyResponse res = new RefreshEnergyResponse();
            if (model.PlayerData == null)
            {
                res.energy = 30;
                //上次恢复体力，十分钟之前
                DateTime d = DateTime.Now.AddMinutes(-10);
                res.fixEnergyTime = (int)Utils.DateTimeToUnixTime(d);
            }
            else
            {
                //BUG TODO  时间太大
                int seconds = (int)Utils.DateTimeToUnixTime(DateTime.Now) - model.PlayerData.fixEnergyTime;
                //每60秒回复1点体力   
                res.energy = (short)(model.PlayerData.energy + seconds / 60);

                DateTime d = DateTime.Now.AddSeconds(seconds % 60);
                res.fixEnergyTime = (int)Utils.DateTimeToUnixTime(d);

                if (res.energy > 50)
                {
                    res.energy = 50;
                }
            }
            AddResponse(Module.Energy, (byte)EnergyCmd.REFRESH_ENERGY, 0, res);
        }

        public override void HttpRegister(string url,
                                          string username,
                                          string password,
                                          Action<RegisterResponse> callback,
                                          Action<ResponseCode> failCallback)
        {
            //base.Register(url, username, password, hash);
            RegisterResponse res = new RegisterResponse();
            res.Uid = "123456";
            AddResponse(Module.Http, (byte)HttpCmd.REGISTER, 0, res);
        }

        public override void StageData(int stageId)
        {
            //base.StageData(stageId);
            Stage res = null;
            if (model.Stages.ContainsKey(stageId))
                res = model.Stages[stageId];
            else
            {
                res = new Stage();
                res.stageId = stageId;
                res.star = 0;
            }
            AddResponse(Module.Stage, (byte)StageCmd.STAGE_DATA, 0, res);
        }

        //第一次打某关胜利时给予firstStageGift 奖励  重复刷某官胜利时给予normalStageGift 。 所有关卡失败时给予failedStageGift奖励（如果有的话）
        //之前打 1星  再次打  2星 并不能拿到 firstStageGift 奖励       目前此逻辑是错的    这3个配置已不打包给前端
        public override void SubmitStageFightRequest(int stageId, int star, int score, List<Step> steps, Dictionary<byte, int> rewards)
        {
            //base.SubmitStageFightRequest(stageId, star, steps);
            //SubmitStageFightResponse res = new SubmitStageFightResponse();
            //res.stage.star = (byte)star;
            //res.valueResultResponse = new ValueResultListResponse();
            //res.valueResultResponse.list = new List<ValueResult>();
            //Q.Assert(model.StageConfigs.ContainsKey(stageId));
            //ItemQtt[] giftResult;       //给予的奖励， 胜利和失败可能都会有，第一次胜利与重复刷胜利时

            //if (star == 3 || star > model.Stages[stageId].star)
            //{
            //    //关卡奖励，每次通关都会给予
            //    StageConfig config = model.StageConfigs[stageId];
            //    for (int i = 0, n = config.FirstStageGift.Length; i < n; i++)
            //    {
            //        ItemQtt itemQtt = config.FirstStageGift[i];
            //        ValueResult vr = new ValueResult();
            //        vr.changeType = 1;//奖励
            //        vr.changeValue = itemQtt.Qtt;
            //        vr.valuesType = itemQtt.Id;
            //        switch (vr.valuesType)
            //        {
            //            case (int)PlayerValueType.Key:
            //                vr.current = model.PlayerData.key + itemQtt.Qtt;
            //                break;
            //            case (int)PlayerValueType.UpgradeA:
            //                vr.current = model.PlayerData.upgradeA + itemQtt.Qtt;
            //                break;
            //            case (int)PlayerValueType.UpgradeB:
            //                vr.current = model.PlayerData.upgradeB + itemQtt.Qtt;
            //                break;
            //            case (int)PlayerValueType.Gem:
            //                vr.current = model.PlayerData.gem + itemQtt.Qtt;
            //                break;
            //        }
            //        res.valueResultResponse.list.Add(vr);
            //    }


            //    //res.passStageId = Math.Max(model.PlayerData.passStageId, stageId);
            //}
            //else
            //{
            //    //通关失败
            //    //res.passStageId = model.PlayerData.passStageId;
            //}

            //AddResponse(Module.Stage, (byte)StageCmd.SUBMIT_STAGEFIGHT, 0, res);
        }


        public override void UpgradUnit(int unitId)
        {
            //正常逻辑下，离线玩法无法升级
            //base.UpgradUnit(unitId);
            UpgradeUnitResponse res = new UpgradeUnitResponse();
            res.beforeUnitId = unitId;
            res.valueResultList = new ValueResultListResponse();
            res.valueResultList.list = new List<ValueResult>();
            Q.Assert(model.UnitConfigs.ContainsKey(unitId));
            UnitConfig config = model.UnitConfigs[unitId];
            short code = 0;
            if (config.UnitUpgradeA > model.PlayerData.upgradeA ||
               config.UnitUpgradeB > model.PlayerData.upgradeB)
            {
                //道具不足
                code = 301;
                res.afterUnitId = unitId;
            }
            else
            {
                ValueResult vr = new ValueResult(
                    (int)RewardType.UpgradeA,
                    model.PlayerData.upgradeA - config.UnitUpgradeA,
                    0, config.UnitUpgradeA
                    );
                res.valueResultList.list.Add(vr);

                vr = new ValueResult(
                    (int)RewardType.UpgradeB,
                    model.PlayerData.upgradeB - config.UnitUpgradeB,
                    0, config.UnitUpgradeB);
                res.valueResultList.list.Add(vr);

                res.afterUnitId = config.UnitUpgrade;
            }

            AddResponse(Module.Unit, (byte)UnitCmd.UPGRAD_UNIT, code, res);
        }


        public override void UserLogin(string token, string version, string platformId, string sdkVersion = "")
        {
            //base.UserLogin(token, version, platformId);
            UserLoginResponse res = new UserLoginResponse();
            res.uid = "123456";
            res.parameters = new Dictionary<string, string>();
            res.reconnectId = "456123";
            AddResponse(Module.User, (byte)UserCmd.USER_LOGIN, 0, res);
        }


        public override void SendTestRequest()
        {
            //base.SendTestRequest();
        }


        protected void AddResponse(Module module, byte cmd, short status, object value)
        {
            Response r = new Response();
            r.Module = (byte)module;
            r.Cmd = cmd;
            r.Status = status;
            r.Value = value;
            responseCallbackDict[module]((byte)module, cmd, status, value);
            //responseQueue.Enqueue(r);
        }

        public override void ExchangeCode(string code)
        {
            //base.ExchangeCode(code);

        }
    }
}
