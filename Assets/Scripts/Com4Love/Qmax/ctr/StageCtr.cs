using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net;
using Com4Love.Qmax.Net.Protocols.Stage;
using System;
using System.Collections.Generic;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Data.VO;
using UnityEngine;
using Com4Love.Qmax.Net.Protocols.counterpart;

namespace Com4Love.Qmax.Ctr
{
    /// <summary>
    /// 關卡控制
    /// </summary>
    public class StageCtr : IDisposable
    {
        private int unLockingStage = -1;    //正在解鎖的關卡， 前端緩存用

        private GameController gameCtr;

        private int loadStageIdx = 1;

        public int StarChangeLevel;

        protected PlayerCtr playerCtr
        {
            get
            {
                return gameCtr.PlayerCtr;
            }
        }

        public StageCtr()
        {
            gameCtr = GameController.Instance;
            gameCtr.Client.AddResponseCallback(Module.Stage, OnStageResponse);
            gameCtr.Client.AddResponseCallback(Module.Counterpart, OnCounterpartStageResponse);
        }


        public void Clear()
        {
            loadStageIdx = 1;
            gameCtr.Model.Stages.Clear();
            gameCtr.Model.ActiveStageIds.Clear();
            gameCtr.Model.allStageStar = 0;
        }

        public void Dispose()
        {
            Clear();
            gameCtr.Client.RemoveResponseCallback(Module.Stage, OnStageResponse);
        }

        /// <summary>
        /// 是否是活動場景
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        public bool IsActivityStage(StageConfig config)
        {
            //return config.ID == StageConfig.TREEACTIVITY_STAGEID;
            return config.TypeId == 0;
        }

        public bool IsActivityStage(int configId)
        {
            return IsActivityStage(GameController.Instance.Model.StageConfigs[configId]);
        }


 
        void OnCounterpartStageResponse(byte module, byte cmd, short status, object value)
        {
            if (cmd == (byte)CounterpartCmd.GET_STAGE_LIST)
            {
                Debug.Log(value);
            }

            Action OnMainThread = delegate ()
            {
                if (cmd == (byte)CounterpartCmd.BEGAIN_STAGE)
                {
                    if (status != 0)
                    {
                        if (status == 301)
                        {
                            ///進入商店///
                            GameController.Instance.Popup.Open(PopupID.UIPowerShop, null, true, true);
                            return;
                        }
                        else
                        {
                            gameCtr.AlertRespLogicException(module, cmd, status);
                            return;
                        }
                    }

                    GameController.Instance.TreeFightCtr.Init(CacheCounterpartStageId);

                    if (gameCtr.ModelEventSystem.OnBeginCounterpartStage != null)
                    {
                        gameCtr.ModelEventSystem.OnBeginCounterpartStage();
                    }

                    //SubmitCounterpartFight(testStageId, new List<Step>(), 1000, new Dictionary<byte, int>());           //test
                }
                else if (cmd == (byte)CounterpartCmd.GET_STAGE_LIST)
                {
                    if (status != 0)
                    {
                        gameCtr.AlertRespLogicException(module, cmd, status);
                        return;
                    }

                    CounterpartStageListResponse response = (CounterpartStageListResponse)value;
                    InitActiveStages(response.stages);

                    //List<int> testData = new List<int>();
                    //testData.Add(1301);
                    //testStageId = response.stages[0].stageId;                   //test
                    //BeginCounterpartStage(testStageId, testData);           //test

                    if (gameCtr.ModelEventSystem.OnCounterpartStageList != null)
                    {
                        gameCtr.ModelEventSystem.OnCounterpartStageList();
                    }
                }
                else if (cmd == (byte)CounterpartCmd.SUBMIT_COUNTERPART_FIGHT)
                {
                    if (status != 0)
                    {
                        gameCtr.AlertRespLogicException(module, cmd, status);
                        return;
                    }

                    SubmitCounterpartResponse response = (SubmitCounterpartResponse)value;
                    gameCtr.PlayerCtr.UpdateByResponse(response.valueResultResponse);

                    if (gameCtr.ModelEventSystem.OnSubmitCounterpart != null)
                    {
                        gameCtr.ModelEventSystem.OnSubmitCounterpart(response);
                    }
                }
            };

            gameCtr.InvokeOnMainThread(OnMainThread);
        }

        /// <summary>
        /// 獲取所有的活動關卡數據  (關卡副本)
        /// </summary>
        public void GetCounterpartStageList()
        {
            gameCtr.Client.CounterpartStageList();
        }

        int CacheCounterpartStageId = -1;       //BeginCounterpartStage 消息返回後 ，服務器不返回stageid，緩存起來

        /// <summary>
        /// 進入一個活動關卡(大樹)
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="unitList"></param>
        public void BeginCounterpartStage(int stageId, List<int> unitList)
        {
            //Debug.Log("測試 直接進入活動關卡");
            //GameController.Instance.ModelEventSystem.OnBeginCounterpartStage();
            //GameController.Instance.TreeFightCtr.Init(stageId);
            //return;

            CacheCounterpartStageId = stageId;
            gameCtr.Client.CounterpartBeginStage(stageId, unitList);
        }

        /// <summary>
        /// 提交活動（大樹）戰鬥結果
        /// </summary>
        /// <param name="stageId"></param>
        /// <param name="steps"></param>
        /// <param name="damage"></param>
        /// <param name="rewards"></param>
        public void SubmitCounterpartFight(int stageId, List<Step> steps, int damage, Dictionary<byte, int> rewards)
        {
            gameCtr.Client.SubmitCounterpart(stageId, steps, damage, rewards);
        }

        private void OnStageResponse(byte module, byte cmd, short status, object value)
        {
            Action OnMainThread = delegate ()
            {
                if (cmd == (byte)StageCmd.GET_STAGEINFO)
                {
                    if (status != 0)
                    {
                        gameCtr.AlertRespLogicException(module, cmd, status);
                        return;
                    }

                    //獲取最高通關記錄。
                    //目前在上線時通過基本信息返回中已經包含最高通關記錄。 (此消息目前無用)
                    StageInfoResponse res = (StageInfoResponse)value;
                    playerCtr.PlayerData.passStageId = res.passStageId;
                }
                else if ((byte)StageCmd.LIST_STAGEDATA == cmd)
                {
                    if (status != 0)
                    {
                        gameCtr.AlertRespLogicException(module, cmd, status, false,
                            delegate (byte m, byte c, int s)
                            {
                                //任何錯誤都會導致直接返回登錄界面
                                gameCtr.ViewEventSystem.BackToLoginSceneEvent();
                            });
                        return;
                    }
                    loadStageIdx++;
                    ListStageResponse res = (ListStageResponse)value;
                    InitStages(res.stages);
                    if (loadStageIdx <= Convert.ToInt32(gameCtr.PlayerCtr.PlayerData.passStageId / 30) + 1)
                    {
                        LoadStageListData();
                    }
                    else
                    {
                        if (GameController.Instance.ModelEventSystem.OnStageListDataInit != null)
                        {
                            GameController.Instance.ModelEventSystem.OnStageListDataInit();
                        }
                    }
                }
                else if (cmd == (byte)StageCmd.SUBMIT_STAGEFIGHT_TEST)
                {
                    if (status != 0)
                    {
                        gameCtr.AlertRespLogicException(module, cmd, status, false,
                            delegate (byte m, byte c, int s)
                            {
                                //任何錯誤都會導致直接返回登錄界面
                                gameCtr.ViewEventSystem.BackToLoginSceneEvent();
                            });
                        return;
                    }

                    //通關返回。
                    //後台只返回獲得的獎勵與成功失敗   需要手動刷新最高通關的信息TODO
                    SubmitStageFightResponse res = (SubmitStageFightResponse)value;
                    UpdateBySubmitStageFightResponse(res);
                    gameCtr.PlayerCtr.RefreshEnergy();       //通關之後刷新體力
                }
                else if (cmd == (byte)StageCmd.STAGE_BEGIN)
                {
                    //體力不足//
                    if (status == 301)
                    {
                        ///進入商店///
                        GameController.Instance.Popup.Open(PopupID.UIPowerShop, null, true, true);
                        return;
                    }
                    ///物品不足//
                    else if (status == 1302)
                    {
                    }
                    else if (status != 0)
                    {
                        gameCtr.AlertRespLogicException(module, cmd, status, true,
                            delegate (byte m, byte c, int s)
                            {
                                if (gameCtr.ModelEventSystem.OnStageBeginErrorEvent != null)
                                    gameCtr.ModelEventSystem.OnStageBeginErrorEvent();
                            }
                        );
                        return;
                    }

                    //ValueResultListResponse res = (ValueResultListResponse)value;
                    BeginStageResponse res = (BeginStageResponse)value;
                    ValueResultListResponse valueResult = res.valueResultListResponse;
                    Stage stage = res.stage;
                    updateFightCount(stage);
                    //gameCtr.ModelEventSystem.DispatchStageFightCountRefEvent(stage);
                    gameCtr.PlayerCtr.UpdateByResponse(valueResult);    //直接使用結果數據刷新
                    if (gameCtr.ModelEventSystem.OnStageBeginEvent != null)
                        gameCtr.ModelEventSystem.OnStageBeginEvent();
                }
                else if (cmd == (byte)StageCmd.STAGE_DATA)
                {
                    //此接口棄用
                    Q.Assert(false, "StageCmd.STAGE_DATA 接口弃用");

                    //Stage res = (Stage)value;
                    ////獲取某關卡的數據，緩存起來
                    //UpdateStage(res);
                }
                else if (cmd == (byte)StageCmd.GEM_UNLOCK_STAGE)
                {
                    if (status != 0)
                    {
                        gameCtr.AlertRespLogicException(module, cmd, status);
                        return;
                    }
                    UnlockStageResponse res = (UnlockStageResponse)value;
                    //解鎖關卡返回
                    ValueResultListResponse valueRlist = res.valueResultListResponse;

                    Stage nextStage = res.unlockStage;

                    UpdateStage(nextStage);
                    updateFightCount(nextStage);

                    playerCtr.UpdateByResponse(valueRlist);
                    playerCtr.PlayerData.gemUnlockStageId.Add(unLockingStage);

                    if (gameCtr.ModelEventSystem.OnStageUnlocked != null)
                        gameCtr.ModelEventSystem.OnStageUnlocked(unLockingStage);

                    unLockingStage = -1;
                }
            };

            gameCtr.InvokeOnMainThread(OnMainThread);
        }

        /// <summary>
        /// 是否曾經通過某關
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        private bool IsStagePassed(int stageId)
        {
            if (gameCtr.Model.Stages.ContainsKey(stageId))
            {
                Stage stage = gameCtr.Model.Stages[stageId];

                //如果此關卡比passStageId小 則表示打過，而不管有沒有關卡數據
                if (stage.star != 0 || stage.stageId <= gameCtr.PlayerCtr.PlayerData.passStageId)
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateBySubmitStageFightResponse(SubmitStageFightResponse res)
        {
            Stage stage = res.stage;
            //人物基本信息刷新

            if (!IsStagePassed(stage.stageId) && stage.star != 0)
            {
                //戰鬥了一個新關卡
                gameCtr.Model.PrePassStageId = gameCtr.PlayerCtr.PlayerData.passStageId;
                gameCtr.Model.IsStagePassedInLastFight = true;
                gameCtr.PlayerCtr.PlayerData.passStageId = stage.stageId;

                //關掉自己動彈出開
                if (!GuideManager.getInstance().IsGuideOver() &&
                    GuideManager.getInstance().version == GuideVersion.Version_1 && GuideManager.getInstance().CurrentGuideID() == 1)
                {
                    if (GameController.Instance.PlayerCtr.PlayerData.passStageId == 1)
                    {
                        GameController.Instance.Model.IsStagePassedInLastFight = false;
                    }
                }
            }
            else
            {
                gameCtr.Model.IsStagePassedInLastFight = false;
            }

            Stage stageOld = GetStageData(stage.stageId);

            StarChangeLevel = 0;

            if (stageOld.star < stage.star)
            {
                //玩家星星數改變
                UpdateStage(stage);
            }
            updateFightCount(stage);

            Stage nextStage = res.nextStage;
            if (nextStage != null)
            {
                UpdateStage(nextStage);
                updateFightCount(nextStage);
            }

            //刷新獲得的物品
            gameCtr.PlayerCtr.UpdateByResponse(res.valueResultResponse);
            if (gameCtr.ModelEventSystem.OnStagePassed != null)
                gameCtr.ModelEventSystem.OnStagePassed();
            if (gameCtr.ModelEventSystem.OnStageRewardRespone != null)
                gameCtr.ModelEventSystem.OnStageRewardRespone(stage.star != 0, res);
        }


        public void LoadStageListData()
        {
            int endIdx = loadStageIdx * 30;

            if (loadStageIdx * 30 >= playerCtr.PlayerData.passStageId)
            {
                endIdx = playerCtr.PlayerData.passStageId + 1;
            }
            gameCtr.Client.SendListStageData((loadStageIdx - 1) * 30 + 1, endIdx);
        }

        /// <summary>
        /// 初始化所有關卡數據信息(主要是星數)
        /// </summary>
        /// <param name="stages"></param>
        public void InitStages(List<Stage> stageList)
        {
            Dictionary<int, Stage> stages = gameCtr.Model.Stages;
            Stage stage;

            for (int i = 0; i < stageList.Count; i++)
            {
                stage = stageList[i];

                if (stage.stageId > gameCtr.PlayerCtr.PlayerData.passStageId)
                {
                    //後台邏輯為 只要打過的關卡都會被記錄，與玩家當前的最高通關關卡無關。
                    //在調試過程中，經常會改變最高通關關卡值， 前端就默認為 高於最高通關關卡都為沒打過的關卡
                    //continue;
                }

                if (stages.ContainsKey(stage.stageId))
                {
                    stages[stage.stageId] = stage;
                }
                else
                {
                    stages.Add(stage.stageId, stage);
                }

                gameCtr.Model.allStageStar += stage.star;
            }
        }

        /// <summary>
        /// 初始化可以打的活動關卡
        /// </summary>
        /// <param name="stageList"></param>
        void InitActiveStages(List<Stage> stageList)
        {
            Dictionary<int, Stage> stages = gameCtr.Model.Stages;

            for (int i = 0; i < stageList.Count; i++)
            {
                Stage stage = stageList[i];

                if (!stages.ContainsKey(stage.stageId))
                {
                    stages.Add(stage.stageId, stage);
                    gameCtr.Model.ActiveStageIds.Add(stage.stageId);
                }
            }

            gameCtr.Model.ActiveStageIds.Sort(IntComparison);
        }

        int IntComparison(int a , int b)
        {
            if (a > b)
            {
                return 1;
            }
            else if (a < b)
            {
                return -1;
            }
            else
                return 0;
        }

        /// <summary>
        /// 活動關卡 打完後直接移除
        /// </summary>
        /// <param name="stage"></param>
        public void UpdateActiveStage(int activeStageId)
        {
            if (gameCtr.Model.Stages.ContainsKey(activeStageId))
            {
                gameCtr.Model.Stages.Remove(activeStageId);
                gameCtr.Model.ActiveStageIds.Remove(activeStageId);
            }
        }

        public void updateFightCount(Stage stage)
        {
            if ( stage != null )
            {
                Dictionary<int, Stage> stages = gameCtr.Model.Stages;
                if (stages.ContainsKey(stage.stageId))
                {
                    stages[stage.stageId].win = stage.win;
                    stages[stage.stageId].lose = stage.lose;
                }
            }
            else
            {
                Q.Log(LogTag.Error, "updateFightCount(). stage == null");
            }
        }

        protected void UpdateStage(Stage stage)
        {
            Dictionary<int, Stage> stages = gameCtr.Model.Stages;
            int newStar = stage.star;
            int oldStar = 0;

            if (stages.ContainsKey(stage.stageId))
            {

                oldStar = stages[stage.stageId].star;
                stages[stage.stageId] = stage;
            }
            else
            {
                stages.Add(stage.stageId, stage);
            }

            int starUp = newStar - oldStar;

            if (starUp > 0)
            {
                //星數增加
                gameCtr.Model.allStageStar += starUp;
                StarChangeLevel = stage.stageId;
            }
        }

        /// <summary>
        /// 獲取當前通關信息(最高通關關卡) （棄用，最高通關會在一開始後台傳下）
        /// </summary>
        //public void GetStageInfo()
        //{
        //    gameCtr.Client.GetStageInfo();
        //}

        /// <summary>
        /// 關卡所需要的解鎖信息
        /// </summary>
        /// <param name="sConfig"></param>
        /// <returns></returns>
        public List<StageLockInfo> GetStageLockInfo(StageConfig sConfig)
        {
            List<StageLockInfo> list = new List<StageLockInfo>();
            int unitCountTmp = 0;

            for (int i = 0; i < sConfig.Unlocks.Length; i++)
            {
                StageConfig.StageUnlock unlock = sConfig.Unlocks[i];
                StageLockInfo info = new StageLockInfo();
                info.Type = unlock.Type;
                info.Res = true;

                switch (unlock.Type)
                {
                    //通關限制不顯示。。。
                    //case 1:
                    //    info.Msg = "通關限制";

                    //    if (playerCtr.PlayerData.passStageId < sConfig.StagePassedUnlock)
                    //    {
                    //        info.Res = false;
                    //    }
                    //    break;
                    case 2:
                        //VIP等级
                        info.Msg = Utils.GetTextByID(2354, unlock.param);
                        break;
                    case 3:
                        //特殊條件
                        info.Msg = "特殊條件測試用文字";
                        break;
                    case 4:
                        //夥伴數量
                        unitCountTmp = unlock.param;
                        info.Msg = Utils.GetTextByID(2352, unlock.param);

                        if (playerCtr.PlayerData.list.Count < unlock.param)
                        {
                            info.Res = false;
                        }
                        break;
                    case 5:
                        //夥伴等級  依賴於夥伴數量
                        info.Msg = Utils.GetTextByID(2353, unitCountTmp, unlock.param); //第一个参数为要求的伙伴数量
                        UnitConfig uConfig;
                        int matchCount = 0;

                        for (int j = 0; j < playerCtr.PlayerData.list.Count; j++)
                        {
                            uConfig = gameCtr.Model.UnitConfigs[playerCtr.PlayerData.list[j].unitId];

                            if (uConfig.Level >= unlock.param)
                            {
                                matchCount++;
                            }
                        }

                        if (matchCount < unitCountTmp)
                        {
                            info.Res = false;
                        }
                        break;
                    case 6:
                        info.Msg = String.Format("通關星數 {0}/{1}", gameCtr.Model.allStageStar, unlock.param);

                        if (gameCtr.Model.allStageStar < unlock.param)
                        {
                            info.Res = false;
                            break;
                        }
                        break;
                    default:
                        break;
                }

                if (info.Type != 1)
                {
                    //通關限制暫時去掉
                    list.Add(info);
                }
            }

            return list;
        }

        /// <summary>
        /// 某關卡的解鎖信息    0 完全已解鎖  1 條件滿足，未解鎖  2 條件不滿足，未解鎖，需要花費鑽石
        /// </summary>
        /// <param name="stageId">關卡ID</param>
        /// <returns></returns>
        public int GetStageLockState(int stageId)
        {
            //1.過完前一關
            //2.VIP等級
            //3.特殊條件
            //4.夥伴數量
            //5.夥伴等級
            //6.星星數量
            if (playerCtr.PlayerData.gemUnlockStageId.IndexOf(stageId) > -1)
            {
                //已經解鎖
                return 0;
            }

            StageConfig sConfig = gameCtr.Model.StageConfigs[stageId];
            List<StageLockInfo> lockInfos = GetStageLockInfo(sConfig);

            if (lockInfos.Count == 0)
            {
                return 0;
            }

            StageLockInfo info;
            for (int i = 0; i < lockInfos.Count; i++)
            {
                info = lockInfos[i];

                if (!info.Res)
                {
                    return 2;
                }
            }

            return 1;
        }

        ///// <summary>
        ///// 某關卡是否存在。按鈕模型已經做好，但是配置沒有跟上
        ///// </summary>
        ///// <param name="stageId"></param>
        ///// <returns></returns>
        //public bool CheckStageExist(int stageId)
        //{
        //    return gameCtr.Model.StageConfigs.ContainsKey(stageId);
        //}

        /// <summary>
        /// 獲取關卡的信息  
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        public Stage GetStageData(int stageId)
        {
#if TREEACTIVITY_DEBUG
            Debug.Log("大树战斗获取临时关卡数据");
            Stage stage = new Stage();
            stage.stageId = StageConfig.TREEACTIVITY_STAGEID;
            stage.stageLimit = 60;
            stage.star = 0;
            stage.targets = new Dictionary<byte, string>();
            stage.targets.Add(1, "");
            stage.targets.Add(2, "");
            stage.targets.Add(3, "");
            return stage;
#endif

            Dictionary<int, Stage> stages = gameCtr.Model.Stages;

            if (!stages.ContainsKey(stageId))
            {
                return null;
            }
            else
            {
                return stages[stageId];
            }
        }

        public int GetStageLimitStep(int stageId)
        {
            int limitStep = 0;
            Stage stage = GetStageData(stageId);

            Q.Assert(stage != null, "Stage is null");

            if (stage != null)
            {
                limitStep = stage.stageLimit;
            }

            return limitStep;
        }

        /// <summary>
        /// 解鎖關卡
        /// </summary>
        /// <param name="stageId"></param>
        public void GemUnLockStage(int stageId)
        {
            unLockingStage = stageId;
            gameCtr.Client.GemUnlockStage(stageId);
        }

        /// <summary>
        /// 是否足夠體力打某關卡
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        public bool CheckEnergyEnough(int stageId)
        {
            PlayerCtr player = gameCtr.PlayerCtr;
            StageConfig stage = gameCtr.Model.StageConfigs[stageId];

            if (player.PlayerData.energy == -1 && player.PlayerData.energyMax == -1)
            {
                return true;
            }
            if (player.PlayerData.energy < stage.CostEnergy)
            {
                return false;
            }

            return true;
        }

        //5 5消息。   開始某關卡
        public void BeginStage(int stageId, List<int> units, Dictionary<int, int> usegoods)
        {
            StarChangeLevel = 0;
            gameCtr.Client.BeginStage(stageId, units, usegoods);
        }

        public StageConfig GetStage(int stageId)
        {
            StageConfig sConfig = null;
            gameCtr.Model.StageConfigs.TryGetValue(stageId, out sConfig);
            return sConfig;
        }

        /// <summary>
        /// 獲取活動場景 暫時只有一個活動 (舊邏輯)
        /// </summary>
        /// <returns></returns>
        public StageConfig GetActiveStage()
        {

            throw new Exception("大樹不在是一個活動關卡");
        }

        /// <summary>
        /// 通過主線關卡ID獲取其對應的活動關卡 ， 有可能沒有
        /// </summary>
        /// <param name="mainStageId">活動關卡所依賴</param>
        /// <returns></returns>
        public StageConfig GetActiveStage(int mainStageId)
        {
            List<int> activeStages = gameCtr.Model.ActiveStageIds;
            StageConfig activeStageConfig;
            int targetMainStageId;

            Q.Log("StageCtr.GetActiveStage() StageConfigs.Count={0}, activeStages.Count={1}",
                gameCtr.Model.StageConfigs.Count,
                activeStages.Count);

            int StageCount = gameCtr.Model.StageConfigs.Count;
            for (int i = 0; i < activeStages.Count; i++)
            {

                if (activeStages[i] < 0 || activeStages[i] >= StageCount)
                {
                    Q.Warning("Warning: activeStages[{0}] value: {1}", i, activeStages[i]);
                    continue;
                }

                activeStageConfig = gameCtr.Model.StageConfigs[activeStages[i]];

                Q.Assert(activeStageConfig.Unlocks.Length == 1, "活動關卡解鎖信息超過1條 需要重新修改此處邏輯");

                StageConfig.StageUnlock unlock = activeStageConfig.Unlocks[0];
                targetMainStageId = unlock.param;

                if (targetMainStageId == mainStageId)
                {
                    return activeStageConfig;
                }
            }

            return null;
        }

        /// <summary>
        /// 下一關卡 當前需要打的關卡。目前是在已通關的關卡上加一，適用於沒有分支的情況下。
        /// </summary>
        /// <returns></returns>
        public StageConfig GetNextStage()
        {
            StageConfig sConfig = null;
            int nextStageId = gameCtr.PlayerCtr.PlayerData.passStageId + 1;
            gameCtr.Model.StageConfigs.TryGetValue(nextStageId, out sConfig);

            return sConfig;
        }


        /// <summary>
        /// 是否全部都是分數目標
        /// </summary>
        /// <param name="goals"></param>
        /// <returns></returns>
        public bool IsAllScoreGoal(List<StageConfig.Goal> goals)
        {
            foreach (var goal in goals)
            {
                if (goal.Type != BattleGoal.Score)
                    return false;
            }
            return true;
        }


        /// <summary>
        /// 獲取最小的已解鎖活動關卡，為null則表示沒有活動關卡可以打。 （解鎖並且未打過）
        /// </summary>
        /// <returns></returns>
        public StageConfig GetMinimumUnlockActiveStage()
        {
            //ActiveStageIds   為從小到大排列
            for (int i = 0; i < GameController.Instance.Model.ActiveStageIds.Count; i++)
            {
                int stageId = GameController.Instance.Model.ActiveStageIds[i];
                StageConfig stageConfig = GetStage(stageId);

                Q.Assert(stageConfig != null, "本地缺少活動關卡配置： lvl{0}", stageId);

                if (stageConfig == null)
                {
                    continue;
                }

                StageConfig.StageUnlock unlock = stageConfig.Unlocks[0];

                //開啟的主線關卡條件
                if (unlock.param <= gameCtr.PlayerCtr.PlayerData.passStageId)
                {
                    return stageConfig;
                }
            }

            return null;
        }
    }
}
