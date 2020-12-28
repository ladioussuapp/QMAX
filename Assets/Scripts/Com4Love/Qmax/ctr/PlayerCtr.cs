using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.achievement;
using Com4Love.Qmax.Net.Protocols.activity;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.Energy;
using Com4Love.Qmax.Net.Protocols.getchance;
using Com4Love.Qmax.Net.Protocols.goods;
using Com4Love.Qmax.Net.Protocols.sign;
using Com4Love.Qmax.Net.Protocols.stage;
using Com4Love.Qmax.Net.Protocols.Unit;
using Com4Love.Qmax.Net.Protocols.User;
using DoPlatform;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Com4Love.Qmax.Ctr
{
    public class PlayerCtr : IDisposable
    {
        /// <summary>
        /// 玩家數據
        /// TODO 後續這裡遷移到QmaxModel層
        /// </summary>
        public ActorGameResponse PlayerData
        {
            get
            {
                return Model == null ? null : Model.PlayerData;
            }
        }
        public DateTime NextEnergyTime;
        public TimeSpan NextEnergyTimeSpan;
        public QmaxModel Model;

        public double EnergyLeftTime = 0;

        public int FreeLeftTime = 0;

        public bool isRequestData = false;


        /// <summary>
        /// 基本信息是否初始化
        /// </summary>
        public bool IsDataReady
        {
            get
            {
                return PlayerData != null;
            }
        }


        private GameController gameCtr;

        public PlayerCtr()
        {
            NextEnergyTime = DateTime.Now;
            isRequestData = false;
            gameCtr = GameController.Instance;
            gameCtr.ModelEventSystem.OnLoginProgress += OnLoginProgress;
            gameCtr.Client.AddResponseCallback(Module.ActorGame, OnNetResponse);
            gameCtr.Client.AddResponseCallback(Module.Energy, OnNetResponse);
            gameCtr.Client.AddResponseCallback(Module.GetChance, OnNetResponse);
            gameCtr.Client.AddResponseCallback(Module.Activity, OnNetResponse);
            gameCtr.Client.AddResponseCallback(Module.Stage, OnNetResponse);
            gameCtr.Client.AddResponseCallback(Module.SIGN, OnNetResponse);
            gameCtr.Client.AddResponseCallback(Module.Http, OnNetResponse);
            gameCtr.Client.AddResponseCallback(Module.Achievement, OnNetResponse);
            gameCtr.Client.AddResponseCallback(Module.Purchase, OnNetResponse);

            GameController.Instance.ServerTime.AddTimeTick(OnFreeTimeTick);
        }

        private void OnNetResponse(byte module, byte cmd, short status, object value)
        {
            //Q.Log(LogTag.Test, "PlayerCtr:OnNetResponse m={0}, c={1}", module, cmd);
            Action OnMainThread = delegate()
            {
                //Q.Log(LogTag.Test, "PlayerCtr:OnNetResponse2");
                Dictionary<Module, BaseClient.OnResponse> dict = new Dictionary<Module, BaseClient.OnResponse>();
                dict.Add(Module.ActorGame, OnActorGameResponse);
                dict.Add(Module.Energy, OnEnergy);
                dict.Add(Module.GetChance, OnOpenBox);
                dict.Add(Module.Activity, OnActivity);
                dict.Add(Module.Stage, OnBuyMoves);
                dict.Add(Module.SIGN, OnSign);
                dict.Add(Module.Http, OnHttpCmd);
                dict.Add(Module.Achievement, OnAchieveCmd);
                dict.Add(Module.Purchase, OnPurchaseCmd);

                dict[(Module)module](module, cmd, status, value);
            };

            gameCtr.InvokeOnMainThread(OnMainThread);
        }

        public void Clear()
        {
            NextEnergyTime = DateTime.Now;
            NextEnergyTimeSpan = TimeSpan.Zero;
            Model = null;
            EnergyLeftTime = 0;
            isRequestData = false;
        }


        public void Dispose()
        {
            Clear();
            gameCtr.ServerTime.RemoveTimeTick(EnergyTimeTick);
            gameCtr.ModelEventSystem.OnLoginProgress -= OnLoginProgress;
            gameCtr.Client.RemoveResponseCallback(Module.ActorGame, OnNetResponse);
            gameCtr.Client.RemoveResponseCallback(Module.Energy, OnNetResponse);
            gameCtr.Client.RemoveResponseCallback(Module.GetChance, OnNetResponse);
            gameCtr.Client.RemoveResponseCallback(Module.Activity, OnNetResponse);
            gameCtr.Client.RemoveResponseCallback(Module.SIGN, OnNetResponse);
            gameCtr.Client.RemoveResponseCallback(Module.Http, OnNetResponse);
            gameCtr.Client.RemoveResponseCallback(Module.Achievement, OnNetResponse);
            gameCtr.Client.RemoveResponseCallback(Module.Purchase, OnNetResponse);

            GameController.Instance.ServerTime.RemoveTimeTick(OnFreeTimeTick);
        }

        private void OnLoginProgress(ResponseCode code, LoginState newState)
        {
            Debug.LogFormat("PlayerCtr:OnLoginProgress {0}, {1}", code, newState);
            if (newState == LoginState.AllDone && code == ResponseCode.SUCCESS)
            {
                //在進入地圖場景時需要大樹的數據。| 之前的獲取大樹信息，現在是獲取活動關卡信息
                //獲取到玩家數據後會跳場景，所以要保證在接受到玩家消息之前就獲得大樹信息
                gameCtr.StageCtr.GetCounterpartStageList();
                gameCtr.Client.ActorGame();
            }
        }


        private void OnEnergy(byte module, byte cmd, short status, object value)
        {
            if (cmd == (byte)EnergyCmd.REFRESH_ENERGY)
            {
                //刷新體力失敗，直接忽略，不彈窗
                //因為刷新體力並非玩家手動觸發的邏輯，突然彈出彈窗會有不好的體驗
                if (status != 0)
                {
                    //gameCtr.InfoRespLogicException(module, cmd, status);
                    return;
                }

                RefreshEnergyResponse res = (RefreshEnergyResponse)value;

                UpdateByResponse(res);

                if (gameCtr.ModelEventSystem.OnEnergyTimeGrowEvent != null)
                    gameCtr.ModelEventSystem.OnEnergyTimeGrowEvent();
            }
            else if (cmd == (byte)EnergyCmd.BUY_ENERGY)
            {
                //如果購買體力失敗，彈出失敗提示界面
                if (status != 0)
                {
                    gameCtr.AlertRespLogicException(module, cmd, (ResponseCode)status);
                    return;
                }

                ValueResultListResponse vRes = (ValueResultListResponse)value;
                UpdateByResponse(vRes);
                if (gameCtr.ModelEventSystem.OnBuyEnergyEvent != null)
                    gameCtr.ModelEventSystem.OnBuyEnergyEvent(false);
            }
            else if (cmd == (byte)EnergyCmd.INCRE_MAXENERGY)
            {
                //如果購買體力上限失敗，彈出失敗提示界面
                if (status != 0)
                {
                    gameCtr.AlertRespLogicException(module, cmd, (ResponseCode)status);
                    return;
                }

                ValueResultListResponse vRes = (ValueResultListResponse)value;
                UpdateByResponse(vRes);
                if (gameCtr.ModelEventSystem.OnBuyEnergyEvent != null)
                    gameCtr.ModelEventSystem.OnBuyEnergyEvent(true);
            }
        }

        private void OnFreeTimeTick(double duration)
        {
            if (FreeLeftTime > 0)
                FreeLeftTime -= 1;
        }

        private void OnActorGameResponse(byte module, byte cmd, short status, object value)
        {
            Q.Log("PlayerCtr:OnActorGameResponse [{0}, {1}] status={2}", module, cmd, status);
            //所有基本信息返回
            if (cmd == (byte)ActorGameCmd.GET_DATA)
            {
                if (status != 0)
                {
                    //這裡可能會有危險，因為獲取玩家信息失敗的本來就在登錄界面
                    gameCtr.AlertRespLogicException(module, cmd, (ResponseCode)status);
                    return;
                }

                //基本信息，存起来
                ActorGameResponse res = (ActorGameResponse)value;
                GuideManager.getInstance().guideIndex = res.guideIndex;
                GuideManager.getInstance().GuideOverIDList = res.guideIds;
                InitPlayerData(res);
                gameCtr.StageCtr.LoadStageListData();
                //GuideManager.getInstance().guideIndex = 1;
                string msg = string.Format("玩家基本信息：{0}", res);
                Q.Log(msg);
            }
            else if (cmd == (byte)ActorGameCmd.RECHARGE_REFRESH)
            {
                if (status != 0)
                {
                    return;
                }
                //基本信息，存起來
                ActorGameAndGoodsResponse actorAndGoods = (ActorGameAndGoodsResponse)value;
                ActorGameResponse ActorRes = (ActorGameResponse)actorAndGoods.actorGameResponse;
                GuideManager.getInstance().guideIndex = ActorRes.guideIndex;
                GuideManager.getInstance().GuideOverIDList = ActorRes.guideIds;
                InitPlayerData(ActorRes);
                if (gameCtr.ModelEventSystem.OnGoodsRefreshList != null)
                {
                    GoodsListResponse goods = (GoodsListResponse)actorAndGoods.goodsListResponse;
                    gameCtr.ModelEventSystem.OnGoodsRefreshList(goods);
                }

                if(PackageConfig.BASE_LOGIN && Application.platform != RuntimePlatform.IPhonePlayer)
                {
                    if (GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent != null)
                    {
                        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent();
                    }
                }
                else if(PackageConfig.DOSDK)
                {
                    //DOSDK走這裡
                    GameController.Instance.Popup.ShowLightLoading();
                }
            }
            else if ((byte)ActorGameCmd.PUSH_RECHARGE_RESULT == cmd)
            {
                if (status != 0)
                {
                    return;
                }

                //沒有接入DOSDK，且iOS充值走[16, 3]協議

                if (PackageConfig.DOSDK)
                {
                    RechargeResponse rechargeRes = (RechargeResponse)value;
                    ValueResultListResponse vRes = rechargeRes.valueResultListResponse;
                    UpdateByResponse(vRes);
                    GoodsItem goodItem = rechargeRes.goodsItem;
                    if (goodItem.id != 0)
                    {
                        GameController.Instance.GoodsCtr.AddGoodsItem(goodItem);
                    }
                    (GameController.Instance.Client as QmaxClient2).KeepConnection = false;
                    GameController.Instance.Popup.HideLightLoading();

                    if (GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent != null)
                    {
                        GameController.Instance.ModelEventSystem.OnRechargeRefreshEvent();
                    }
                }
            }
        }

        //初始化玩家數據
        protected void InitPlayerData(ActorGameResponse data)
        {
            Q.Assert(data != null);
            Model = gameCtr.Model;
            Model.PlayerData = data;
            //初始化所有關卡星數，計算總星數
            //gameCtr.StageCtr.InitStages(data.allStage);

            FreeLeftTime = Model.PlayerData.freeTime;

            //夥伴按照ID從小到大排列
            Model.PlayerData.list.Sort(UnitListSortCompare);
            UpdateEnergyInfo(PlayerData.fixEnergyTime);

            //初始化 並且刷新
            if (gameCtr.ModelEventSystem.OnPlayerInfoInit != null)
                gameCtr.ModelEventSystem.OnPlayerInfoInit(PlayerData);
            if (gameCtr.ModelEventSystem.OnPlayerInfoRef != null)
                gameCtr.ModelEventSystem.OnPlayerInfoRef(null, PlayerData);

            //每秒刷新恢復體力倒計時
            gameCtr.ServerTime.AddTimeTick(EnergyTimeTick);
        }

        private int UnitListSortCompare(Unit a, Unit b)
        {
            int res = 0;

            if (a.unitId > b.unitId)
            {
                res = 1;
            }
            else if (a.unitId < b.unitId)
            {
                res = -1;
            }

            return res;
        }

        /// <summary>
        /// 刷新體力信息
        /// </summary>
        /// <param name="fixEnergyTime"></param>
        protected void UpdateEnergyInfo(int fixEnergyTime)
        {
            int recoverTime = 603;
            if (Model.GameSystemConfig != null)
            {
                recoverTime = 3 + Model.GameSystemConfig.recoverEnergyPerSec;
            }

            //int utcNow = (int)Utils.DateTimeToUnixTime(DateTime.UtcNow);
            int utcNow = (int)Utils.DateTimeToUnixTime(gameCtr.ServerTime.UniversalTime);
            //DateTime fixTime = Utils.UnixTimeToDateTime(fixEnergyTime);
            EnergyLeftTime = recoverTime - (utcNow - fixEnergyTime);
            NextEnergyTime = Utils.UnixTimeToLocalTime(fixEnergyTime);
            UpdateEnergyInfo(NextEnergyTime);
        }

        protected void UpdateEnergyInfo(DateTime energyTime)
        {
            NextEnergyTime = energyTime.AddMinutes(10);
            NextEnergyTimeSpan = NextEnergyTime - gameCtr.ServerTime.LocalTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        protected void EnergyTimeTick(double duration)
        {
            if (PlayerData != null && PlayerData.energy >= PlayerData.energyMax)
            {
                //滿體力的時候停止Timer?
                //不用了，就讓他跑吧
                isRequestData = false;
                return;
            }

            if (EnergyLeftTime <= 0 && !isRequestData)
            {
                RecoveryEnergy();
            }


            if (!isRequestData)
            {
                EnergyLeftTime -= duration;
            }
            //NextEnergyTimeSpan = NextEnergyTime - gameCtr.ServerTime.LocalTime;
            //NextEnergyTimeSpan = NextEnergyTime - DateTime.Now;
            //if (NextEnergyTimeSpan.TotalSeconds <= 0)
            //{
            //    //主動漲體力 同時發消息
            //    RecoveryEnergy();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Boolean CheckGetChanceAble()
        {
            //通關數為5時，進入抽獎界面，新手引導抽一次獎勵
            switch (GuideManager.getInstance().version)
            {
                case GuideVersion.Version_1:
                    return PlayerData.passStageId >= 2;
                default:
                    break;
            }
            return true;
        }


        /// <summary>
        /// 体力刷新
        /// </summary>
        /// <param name="res"></param>
        public void UpdateByResponse(RefreshEnergyResponse res)
        {
            //Debug.Log("接受體力刷新事件");

            PlayerData.energy = res.energy;
            PlayerData.energyMax = res.maxEnergy;
            PlayerData.fixEnergyTime = res.fixEnergyTime;
            List<RewardType> types = new List<RewardType>();
            types.Add(RewardType.Energy);
            types.Add(RewardType.MaxEnergy);
            isRequestData = false;
            UpdateEnergyInfo(PlayerData.fixEnergyTime);
            if (gameCtr.ModelEventSystem.OnPlayerInfoRef != null)
                gameCtr.ModelEventSystem.OnPlayerInfoRef(types, PlayerData);
        }

        /// <summary>
        /// 刷新玩家各種物品
        /// </summary>
        /// <param name="res"></param>
        public void UpdateByResponse(ValueResultListResponse res)
        {
            List<RewardType> types = new List<RewardType>();
            RewardType type;

            Q.Assert(res.list != null);
            if (res.list == null)
                return;

            foreach (ValueResult item in res.list)
            {
                type = (RewardType)item.valuesType;
                types.Add(type);
                UpdateByResponse(item);
            }

            if (gameCtr.ModelEventSystem.OnPlayerInfoRef != null)
                gameCtr.ModelEventSystem.OnPlayerInfoRef(types, PlayerData);
        }

        public void UpdateByResponse(List<ValueResult> res)
        {
            foreach (ValueResult item in res)
            {
                UpdateByResponse(item);
            }

            if (gameCtr.ModelEventSystem.OnPlayerInfoRef != null)
                gameCtr.ModelEventSystem.OnPlayerInfoRef(null, PlayerData);
        }

        void UpdateByResponse(ValueResult res)
        {
            Debug.Assert(PlayerData != null);
            if (PlayerData == null)
                return;

            //1 鑰 2 黃毛球 3 藍毛球 4 鑽石 5 體力上限 6 體力 7 伙伴
            switch (res.valuesType)
            {
                case 1:
                    PlayerData.key = (short)res.current;
                    break;
                case 2:
                    PlayerData.upgradeA = res.current;
                    break;
                case 3:
                    PlayerData.upgradeB = res.current;
                    break;
                case 4:
                    PlayerData.gem = res.current;
                    break;
                case 5:
                    PlayerData.energyMax = (short)res.current;
                    break;
                case 6:
                    PlayerData.energy = (short)res.current;
                    break;
                case 7:
                    //伙伴刷新?
                    UpdateUnitByResponse(res);
                    break;
                case 11:
                    //金幣
                    PlayerData.coin = res.current;
                    break;
                default:
                    break;
            }
        }

        //通過ValueResult刷新夥伴 比如：開寶箱獲得夥伴
        private void UpdateUnitByResponse(ValueResult res)
        {
            Q.Assert(res.changeType == 1, "夥伴數量減少？");

            Unit unit = GetUnit(res.changeValue);
            //Q.Assert(unit == null, "已擁有的伙伴屬性更新?");

            unit = new Unit();
            unit.unitId = res.changeValue;
            PlayerData.list.Add(unit);
            //夥伴按照ID從小到大順序
            PlayerData.list.Sort(UnitListSortCompare);
        }

        protected Unit GetUnit(int unitId)
        {
            foreach (Unit item in PlayerData.list)
            {
                if (item.unitId == unitId)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 請求體力信息
        /// </summary>
        public void RefreshEnergy()
        {
            gameCtr.Client.RefreshEnergy();
        }

        /// <summary>
        /// 恢復體力
        /// </summary>
        protected void RecoveryEnergy()
        {
            //Debug.Log("請求體力刷新");
            //前端主動恢復
            if (!isRequestData)
            {
                isRequestData = true;
                if (PlayerData == null)
                    return;

                //PlayerData.energy = (short)Mathf.Min(++PlayerData.energy, PlayerData.energyMax);

                UpdateEnergyInfo(NextEnergyTime);
                RefreshEnergy();
            }

        }

        /// <summary>
        /// 購買步數
        /// </summary>
        public void BuyMoves()
        {
            gameCtr.Client.BuyStep();
        }

        /// <summary>
        /// 購買時間
        /// </summary>
        public void BuyTime()
        {
            gameCtr.Client.BuyTime();
        }

        /// <summary>
        /// 夠不夠錢買步數
        /// </summary>
        /// <returns></returns>
        public bool CheckBuyMovesAble()
        {
            int price = gameCtr.Model.GameSystemConfig.buyMoves;
            return PlayerData.gem >= price;
        }

        /// <summary>
        /// 夠不夠錢買時間
        /// </summary>
        /// <returns></returns>
        public bool CheckBuyTimeAble()
        {
            int price = gameCtr.Model.GameSystemConfig.buyMoves;
            return PlayerData.gem >= price;
        }

        public void GetRechargeInfo()
        {
            gameCtr.Client.GetRechargeInfo(gameCtr.LoginCtr.CdnRoot);
        }

        /// <summary>
        /// 購買鑽石
        /// </summary>
        public void BuyGem(string id)
        {
            Debug.LogFormat("購買鑽石 id={0}", id);

            ActorLoginResponse res = gameCtr.Model.LoginData;

            if (Application.isEditor)
            {
                string chargeUrl = string.Format("{0}GMMaintain/testRecharge.do?", gameCtr.LoginCtr.RechargeRoot);
                gameCtr.Client.Recharge(chargeUrl, res.platformId, res.serverId, gameCtr.Model.SdkData.userId, res.actorId, id);
            }
            else if (PackageConfig.DOSDK)
            {
                PaymentSystemConfig item = gameCtr.Model.PaymentSystemConfigs[id];
                double timeKey = Utils.LocalTimeToUnixTime(gameCtr.ServerTime.UniversalTime);
                string key = string.Format("{0}#{1}#{2}#{3}", res.actorId, res.serverId, timeKey, item.PaymentId);
                string orderKey = string.Format("{0}#{1}", key, gameCtr.Model.SdkData.appKey);
                string orderMd5 = Utils.GetMd5Hash(orderKey);
                string orderId = string.Format("{0}@{1}", key, orderMd5);

                string productId = string.Format("{0}", item.PaymentId);
                string productName = string.Format("{0}", item.Rmb);
                string amount = string.Format("{0}", item.Rmb * 100);
                string paydes = string.Format("{0}", "extra");
                DoSDK.Instance.pay(orderId, productId, productName, amount, paydes);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string actorID = res.actorId.ToString();
                GameController.Instance.IapKit.StartPayment(id, 1, actorID);
            }
            else
            {
                Debug.Assert(false, "PlayerCtr:BuyGem()");
            }
        }

        public void BuyKey()
        {
            gameCtr.Client.BuyKey();
        }

        /// <summary>
        /// 購買體力回复
        /// </summary>
        /// <param name="type"></param>
        public void BuyEnergy(byte type)
        {
            gameCtr.Client.BuyEnergy(type);
        }

        /// <summary>
        /// 購買體力上限
        /// </summary>
        /// <param name="type"></param>
        public void BuyMaxEnergy(byte type)
        {
            gameCtr.Client.BuyMaxEnergy(type);
        }

        /// <summary>
        /// 開箱
        /// </summary>
        public void OpenBox()
        {
            if (PlayerData.key == 0)
            {
                return;
            }

            gameCtr.Client.OpenBox();
        }


        /// <summary>
        /// 購買移動次數返回
        /// </summary>
        /// <param name="module"></param>
        /// <param name="cmd"></param>
        /// <param name="status"></param>
        /// <param name="value"></param>
        private void OnBuyMoves(byte module, byte cmd, short status, object value)
        {
            //購買步數
            if (cmd == (byte)StageCmd.GEM_BUY_STEP)
            {
                if (status != 0)
                {
                    //購買失敗
                    gameCtr.AlertRespLogicException(module, cmd, (ResponseCode)status);
                    return;
                }

                BuyStepResponse res = (BuyStepResponse)value;
                UpdateByResponse(res.valueResults);

                gameCtr.Model.BattleModel.RemainSteps += res.addStepNum;

                if (gameCtr.ModelEventSystem.OnBuyMoves != null)
                    gameCtr.ModelEventSystem.OnBuyMoves(res.addStepNum);
            }
            //購買時間
            else if (cmd == (byte)StageCmd.GEM_BUY_TIME)
            {
                if (status != 0)
                {
                    //購買失敗
                    gameCtr.AlertRespLogicException(module, cmd, (ResponseCode)status);
                    return;
                }

                BuyTimeResponse res = (BuyTimeResponse)value;
                UpdateByResponse(res.valueResults);

                gameCtr.Model.BattleModel.AddRemainTime(res.addTimeNum);

                if (gameCtr.ModelEventSystem.OnBuyTime != null)
                    gameCtr.ModelEventSystem.OnBuyTime(res.addTimeNum);
            }
        }

        //開箱子返回
        private void OnOpenBox(byte module, byte cmd, short status, object value)
        {
            if (cmd == (byte)GetChanceCmd.OPEN_BOX)
            {
                if (status != 0)
                {
                    Q.Warning("開寶箱失敗");
                    gameCtr.AlertRespLogicException(module, cmd, (ResponseCode)status);
                    return;
                }

                GetChanceResponse res = (GetChanceResponse)value;
                UpdateByResponse(res.valueResultListResponse);

                if (res.valueGoodsItems != null && res.valueGoodsItems.Count > 0)
                {
                    //添加抽的道具到背包
                    GameController.Instance.GoodsCtr.AddGoodsItem(res.valueGoodsItems);
                }

                if (gameCtr.ModelEventSystem.OnOpenBox != null)
                    gameCtr.ModelEventSystem.OnOpenBox(res);

            }
            else if (cmd == (byte)GetChanceCmd.BUY_KEYS)
            {
                if (status != 0)
                {
                    Q.Warning("購買鑰匙失敗");
                    gameCtr.AlertRespLogicException(module, cmd, (ResponseCode)status);
                    return;
                }

                ValueResultListResponse res = (ValueResultListResponse)value;
                UpdateByResponse(res);
                if (gameCtr.ModelEventSystem.OnBuyKeys != null)
                    gameCtr.ModelEventSystem.OnBuyKeys(res);
            }
        }

        //活動返回
        private void OnActivity(byte module, byte cmd, short status, object value)
        {
            if (cmd == (byte)ActivityCmd.EXCHANGE_CODE)
            {
                ExchangeCodeResponse res = (ExchangeCodeResponse)value;
                if (status == 1001)
                {
                    if (gameCtr.ModelEventSystem.OnExchangeCodeFail != null)
                        gameCtr.ModelEventSystem.OnExchangeCodeFail(status);
                    Q.Warning("兌換碼已兌換");
                }
                else if (status == 1002)
                {
                    if (gameCtr.ModelEventSystem.OnExchangeCodeFail != null)
                        gameCtr.ModelEventSystem.OnExchangeCodeFail(status);
                    Q.Warning("兌換碼不存在");
                }
                else if (status == 0)
                {
                    UpdateByResponse(res.valueResultListResponse);
                    if (gameCtr.ModelEventSystem.OnExchangeCode != null)
                        gameCtr.ModelEventSystem.OnExchangeCode(res);
                }
                else
                {
                    Q.Warning("兌換碼兌換失敗");
                    gameCtr.AlertRespLogicException(module, cmd, (ResponseCode)status);
                }
            }
        }
        /// <summary>
        /// 已經改成遊戲累計登陸獎勵的領取接口////
        /// 獲取獎勵信息見SignInfo////
        /// </summary>
        public void Sign()
        {
            gameCtr.Client.Sign();
        }

        /// <summary>
        /// 獲取登陸獎勵的信息，得到登陸天數和改登陸天數的獎勵是否已經領取///
        /// </summary>
        public void SignInfo()
        {
            gameCtr.Client.SignInfo();
        }

        /// <summary>
        /// 獲取獎勵成功回調
        /// </summary>
        /// <param name="module"></param>
        /// <param name="cmd"></param>
        /// <param name="status"></param>
        /// <param name="value"></param>
        private void OnSign(byte module, byte cmd, short status, object value)
        {
            if (cmd == (byte)SignCmd.SIGN)
            {
                //不管成功失敗都要給按鈕解鎖
                if (GameController.Instance.ModelEventSystem.OnSignUnlockEvent != null)
                    GameController.Instance.ModelEventSystem.OnSignUnlockEvent();

                if (status != 0)
                {
                    Q.Warning("簽到失敗");
                    gameCtr.AlertRespLogicException(module, cmd, (ResponseCode)status);
                    return;
                }

                SignResponse res = (SignResponse)value;
                if (res == null)
                {
                    return;
                }
                //會通過判斷已有的伙伴顯示不同的素材 所以刷新數據放在下方
                if (gameCtr.ModelEventSystem.OnSignEvent != null)
                    gameCtr.ModelEventSystem.OnSignEvent(res);

            }
            else if (cmd == (byte)SignCmd.INFO)
            {
                if (status != 0)
                {
                    Q.Warning("獲取簽到信息失敗");
                    //不需要彈窗提示
                    //gameCtr.AlertRespLogicException(module, cmd, status);
                    if (gameCtr.ModelEventSystem.OnSignInfoEvent != null)
                        gameCtr.ModelEventSystem.OnSignInfoEvent(0, false);
                    return;
                }
                else
                {
                    SignInfoResponse res = (SignInfoResponse)value;
                    if (gameCtr.ModelEventSystem.OnSignInfoEvent != null)
                        gameCtr.ModelEventSystem.OnSignInfoEvent(res.signDay, (int)res.isReceived == 0);
                }
            }
        }


        private void OnHttpCmd(byte module, byte cmd, short status, object value)
        {
            if (status != 0)
            {
                //TODO
                return;
            }

            if (cmd == (byte)HttpCmd.PUSH_RECHARGE_RESULT)
            {
                gameCtr.Client.RechargeRefresh();
            }
            else if (cmd == (byte)HttpCmd.RECHARGE_INFO)
            {
                gameCtr.Model.GenPaymentSystemConfig((string)value);
                //購買金幣的配置信息獲取消息
                if (gameCtr.ModelEventSystem.OnPaymentInfo != null)
                    gameCtr.ModelEventSystem.OnPaymentInfo();
            }
        }
        //OnHttpCmd

        /// <summary>
        /// 判斷是否是無限體力模式時
        /// </summary>
        /// <returns>true 是無限體力</returns>
        public bool JudgeNoLimitEnergy()
        {
            return PlayerData.energyMax == -1 && -1 == PlayerData.energy;
        }


        private void OnAchieveCmd(byte module, byte cmd, short status, object value)
        {
            if (cmd == (byte)AchievementCmd.GET_ACHIEVEMENT_LIST)
            {
                AchievementListResponse res = (AchievementListResponse)value;
                if (res.achievements.Count > 1)
                {
                    // 打開成就列表
                    GameController.Instance.Popup.Open(PopupID.UIAchievement, null, true, true);
                }
                // 通知消息事件
                if (gameCtr.ModelEventSystem.OnAchieveOpen != null)
                    gameCtr.ModelEventSystem.OnAchieveOpen(res);
            }
            else if (cmd == (byte)AchievementCmd.GET_ACHIEVEMENT_REWARD)
            {
                // 成就領取獎勵
                AchievementRewardResponse res = (AchievementRewardResponse)value;
                if (gameCtr.ModelEventSystem.OnAchieveReward != null)
                    gameCtr.ModelEventSystem.OnAchieveReward(res);
            }
            else if (cmd == (byte)AchievementCmd.REFRESH_ACHIEVEMENT_LIST)
            {
                // 刷新成就列表
                AchievementListResponse res = (AchievementListResponse)value;
                if (gameCtr.ModelEventSystem.OnAchieveRefresh != null)
                    gameCtr.ModelEventSystem.OnAchieveRefresh(res);
            }
            else if (cmd == (byte)AchievementCmd.ROLL_POLLING_ACHIEVE)
            {
                // 獲取已達成的成就數量
                AchievementPollingResponse res = (AchievementPollingResponse)value;
                if (gameCtr.ModelEventSystem.OnReachAchieveCountUpdate != null)
                    gameCtr.ModelEventSystem.OnReachAchieveCountUpdate(res.achieveNum);
            }
        }
        //OnAchieveCmd



        private void OnPurchaseCmd(byte module, byte cmd, short status, object value)
        {
            ResponseCode respCode = (ResponseCode)status;
            Debug.LogFormat("PlayerCtr:OnPurchaseCmd [{0}, {1}], code={2}", module, cmd, respCode);
            switch ((PurchaseCmd)cmd)
            {
                case PurchaseCmd.VALIDATE:
                    if (respCode == ResponseCode.SUCCESS)
                    {
                        RechargeResponse rechargeRes = (RechargeResponse)value;
                        ValueResultListResponse vRes = rechargeRes.valueResultListResponse;

                        UpdateByResponse(vRes);
                        GoodsItem goodItem = rechargeRes.goodsItem;
                        if (goodItem.id != 0)
                        {
                            gameCtr.GoodsCtr.AddGoodsItem(goodItem);
                        }
                        (gameCtr.Client as QmaxClient2).KeepConnection = false;
                        gameCtr.Popup.HideLightLoading();

                        if (gameCtr.ModelEventSystem.OnRechargeRefreshEvent != null)
                        {
                            gameCtr.ModelEventSystem.OnRechargeRefreshEvent();
                        }
                    }
                    else
                    {
                        gameCtr.Popup.HideLightLoading();
                        var errMsg = string.Format("購買物品失敗 respCode={0}", respCode);
                        gameCtr.Popup.ShowTextFloat(errMsg, LayerCtrlBehaviour.ActiveLayer.FloatLayer as RectTransform);
                    }
                    break;
                default:
                    break;
            }
        }

        public int ValueResultSort(ValueResult one, ValueResult two)
        {
            if ((int)one.valuesType > (int)two.valuesType)
            {
                return 1;
            }
            else if ((int)one.valuesType < (int)two.valuesType)
            {
                return -1;
            }

            return 0;
        }
    }
}
