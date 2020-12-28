using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Net.Protocols.Stage;
using System;
using System.Collections.Generic;

namespace Com4Love.Qmax.Ctr
{
    public class TreeFightCtr : IDisposable
    {
        public TreeFightData TreeFightData;
        public bool IsBegin = false;    //戰鬥是否開始標識
        public bool IsPause = false;

        //緩存當前對應的一連3個配置
        public TimeLimitedHPConfig[] NextHPConfigs;
        public int CutHpConfigIndex;     //當前hpConfig相對於NextHPConfigs裡的索引， view中很多地方需要

        /// <summary>
        /// 當前3個配置組合的總血量 忽略大樹等級，如果大樹滿級，則此血量是最後三個配置的血量
        /// </summary>
        public int CutGroupHpTotal;

        TimeLimitedHPConfig cutHPConfig;

        /// <summary>
        /// 當前戰斗大樹HP獎勵的配置 AktDamage方法中根據傷害值進入下一階段
        /// </summary>
        public TimeLimitedHPConfig CutHPConfig
        {
            get
            {
                return cutHPConfig;
            }
            set
            {
                cutHPConfig = value;

                if (NextHPConfigs[0] == null)
                {
                    //第一次緩存
                    UpdateNextHPConfigs();
                }
                else if (NextHPConfigs[2] != null && NextHPConfigs[2].ID < cutHPConfig.ID)
                {
                    UpdateNextHPConfigs();
                }
            }
        }

        /// <summary>
        /// view層需要根據當前3個獎勵配置顯示進度信息 所以在當前配置更新後，需要判斷是否需要刷新接下來的3個配置信息
        /// </summary>
        void UpdateNextHPConfigs()
        {
            //刷新3個hp獎勵配置 第一項永遠是當前的 cutHPConfig
            NextHPConfigs[0] = cutHPConfig;
            CutHpConfigIndex = 0;
            TimeLimitedHPConfig hpConfig;
            CutGroupHpTotal = cutHPConfig.Hp;
            TreeFightData.LoseHpInGroupPreView = 0;
            TreeFightData.LoseHpInGroup = 0;

            for (int i = 1; i < 3; i++)
            {
                if (GameController.Instance.Model.TimeLimitedHPIdConfigs.ContainsKey(cutHPConfig.ID + i))
                {
                    hpConfig = GameController.Instance.Model.TimeLimitedHPIdConfigs[cutHPConfig.ID + i];
                    NextHPConfigs[i] = hpConfig;
                    CutGroupHpTotal += hpConfig.Hp;
                }
                else
                {
                    //所有配置已過完
                    NextHPConfigs[i] = null;
                }
            }

            if (GameController.Instance.ModelEventSystem.OnTreeFightHPConfigChange != null)
            {
                GameController.Instance.ModelEventSystem.OnTreeFightHPConfigChange();
            }
        }

        /// <summary>
        /// 在 CounterpartCmd.BEGAIN_STAGE 返回後初始化。   調用start後開始戰鬥
        /// </summary>
        /// <param name="stageId"></param>
        public void Init(int stageId)
        {
            TreeFightData = new TreeFightData();
            //int actTime, short actId, 
            //TreeFightData.ActiivtyId = actId;
            //TreeFightData.ActivityTime = actTime;
            TreeFightData.cutStageId = stageId;
            TreeFightData.MaxLevel = 40;            //後期看看大樹等級從哪裡獲取 TODO
            TreeFightData.MaxHp = GameController.Instance.Model.TimeLimitedHPLvlConfigs[TreeFightData.MaxLevel].hpTotal;  //大树的血量指的是当前等级的大树的 HP奖励血量
            TreeFightData.AwardData = new TreeFightAwardData();
            int step = GameController.Instance.StageCtr.GetStageLimitStep(stageId);
            TreeFightData.TimeLeft = step;
            NextHPConfigs = new TimeLimitedHPConfig[3];
        }

        /// <summary>
        /// 暫停
        /// </summary>
        public void Pause()
        {
            if (!IsBegin || IsPause)
            {
                return;
            }

            IsPause = true;

            if (GameController.Instance.ModelEventSystem.OnTreeFightPause != null)
            {
                GameController.Instance.ModelEventSystem.OnTreeFightPause();
            }
        }

        public void GoOn()
        {
            if (!IsBegin || !IsPause)
            {
                return;
            }

            IsPause = false;
        }

        /// <summary>
        /// 對大樹造成傷害 ctr計算出對敵人的傷害後，或者view 中某個動作執行完成之後調用
        /// </summary>
        /// <param name="val"></param>
        /// <param name="preCareResult">是否是提前計算傷害值 用做view中玩家連線後的血條預扣血。同樣需要計算扣血</param>
        public void AtkDamage(int val, bool preCareResult = false)
        {
            if (preCareResult)
            {
                //用來測試預扣血的情況 預扣血中 傳過來的val值 就是接下來要攻擊的值。
                TreeFightData.LoseHpInGroupPreView = TreeFightData.LoseHpInGroup + val;     //LoseHpInGroup為真實扣血情況
            }
            else
            {
                TreeFightData.DamageTotal += val;
                TreeFightData.LoseHpInLevel += val;
                TreeFightData.LoseHpInGroup += val;

                if (CutHPConfig.Level < TreeFightData.MaxLevel)
                {
                    //當前等級的HP獎勵已經達到
                    //計算HP獎勵
                    while (TreeFightData.LoseHpInLevel > CutHPConfig.Hp)
                    {
                        TreeFightData.LoseHpInLevel -= CutHPConfig.Hp;
                        HpAward(CutHPConfig);
                        int nextId = CutHPConfig.ID + 1;

                        if (GameController.Instance.Model.TimeLimitedHPIdConfigs.ContainsKey(nextId) && CutHPConfig.Level < TreeFightData.MaxLevel)
                        {
                            CutHPConfig = GameController.Instance.Model.TimeLimitedHPIdConfigs[nextId];     //到下一个阶段  需要发消息出去TO DO
                            CutHpConfigIndex = Array.IndexOf(NextHPConfigs, CutHPConfig);
                        }
                        else
                        {
                            //已經到最大等級
                            TreeFightData.LoseHpInLevel = 0;
                            break;
                        }
                    }
                }
            }

            //UnityEngine.Debug.Log(string.Format("攻擊大樹     LoseHpInLevel:{0}   當前HP獎勵等級：{1}    當前等級總血量：{2}", TreeFightData.LoseHpInLevel, CutHPConfig.Level, CutHPConfig.Hp));
            if (GameController.Instance.ModelEventSystem.OnTreeFightDamaged != null)
            {
                GameController.Instance.ModelEventSystem.OnTreeFightDamaged(val, preCareResult);
            }
        }

        /// <summary>
        /// 消除消除物
        ///連線的個數 和 最終消除的總數量
        /// </summary>
        public List<ItemQtt> Eliminate(int lineCount, int eliminatCount)
        {
            return LineAward(lineCount, eliminatCount);
        }

        /// <summary>
        /// 提交大樹戰鬥結果    自己在大樹的uibattle裡面調用
        /// </summary>
        public void SubmitFightRequest()
        {
            Dictionary<byte, int> awards = new Dictionary<byte, int>();
            awards.Add((byte)RewardType.UpgradeA, TreeFightData.AwardData.UpgradeA);
            awards.Add((byte)RewardType.UpgradeB, TreeFightData.AwardData.UpgradeB);
            awards.Add((byte)RewardType.Gem, TreeFightData.AwardData.Gem);
            GameController.Instance.StageCtr.SubmitCounterpartFight(TreeFightData.cutStageId, GameController.Instance.Model.BattleModel.Steps, TreeFightData.DamageTotal, awards);
            //UnityEngine.Debug.Log("提交戰鬥獎勵：" + TreeFightData.AwardData.UpgradeA + "|" + TreeFightData.AwardData.UpgradeB + "|" + TreeFightData.AwardData.Gem);
        }

        /// <summary>
        /// 給HP獎勵
        /// </summary>
        /// <param name="hpConfig"></param>
        void HpAward(TimeLimitedHPConfig hpConfig)
        {
            TreeFightData.AwardData.UpgradeA += hpConfig.UpgradeA;
            TreeFightData.AwardData.UpgradeB += hpConfig.UpgradeB;
            TreeFightData.AwardData.Gem += hpConfig.Gem;

            //通知外部獲取hp獎勵
            if (GameController.Instance.ModelEventSystem.OnTreeFightHPAward != null)
            {
                GameController.Instance.ModelEventSystem.OnTreeFightHPAward(hpConfig);
            }
        }

        /// <summary>
        /// 連線與消除獎勵
        /// </summary>
        List<ItemQtt> LineAward(int lineCount, int eliminatCount)
        {
            List<ItemQtt> awardQtts = new List<ItemQtt>();

            //基本的連線獎勵，只要連線成功就給
            int baseLineAward = GameController.Instance.Model.TimeLimitedLineConfigs[1].Number;
            //連線疊加獎勵 每增加多少個連線的消除物，就增加1個橘子。
            int LineAwardAddCount = GameController.Instance.Model.TimeLimitedLineConfigs[2].Number;
            //達到以下消除數量時追加一個桃子
            int awardUpgradeA = 0;  //最終獲得的橘子
            int awardUpgradeB = 0;  //最總獲得的桃子

            if (lineCount < 3)
            {
                //3連以下沒有獎勵
                return awardQtts;
            }

            awardUpgradeA += baseLineAward;     //連線就有的基本獎勵
            lineCount -= 3;
            awardUpgradeA += (int)(lineCount / LineAwardAddCount);

            int configIdTmp = 3;
            int EliminatAwardAddCount;
            int maxTLLineConfigId = 7;      //最大消除獎勵配置id

            while (configIdTmp <= maxTLLineConfigId)
            {
                if (GameController.Instance.Model.TimeLimitedLineConfigs.ContainsKey(configIdTmp))
                {
                    //多餘 EliminatAwardAddCount 個消除增加一個桃子
                    EliminatAwardAddCount = GameController.Instance.Model.TimeLimitedLineConfigs[configIdTmp].Number;
                    configIdTmp++;

                    if (eliminatCount >= EliminatAwardAddCount)
                    {
                        //達到了獎勵的需求 加一個桃子
                        awardUpgradeB++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    //所有獎勵的配置已經計算過
                    break;
                }
            }

            TreeFightData.AwardData.UpgradeA += awardUpgradeA;
            TreeFightData.AwardData.UpgradeB += awardUpgradeB;

            if (GameController.Instance.ModelEventSystem.OnTreeFightLineAward != null)
            {
                GameController.Instance.ModelEventSystem.OnTreeFightLineAward(awardUpgradeA, awardUpgradeB);
            }


            if (awardUpgradeA != 0)
            {
                ItemQtt upgradeAIQ = new ItemQtt();
                upgradeAIQ.type = RewardType.UpgradeA;
                upgradeAIQ.Qtt = awardUpgradeA;
                awardQtts.Add(upgradeAIQ);
            }

            if (awardUpgradeB != 0)
            {
                ItemQtt upgradeBIQ = new ItemQtt();
                upgradeBIQ.type = RewardType.UpgradeB;
                upgradeBIQ.Qtt = awardUpgradeB;
                awardQtts.Add(upgradeBIQ);
            }

            return awardQtts;
        }

        //總的傷害獎勵 倒計時完成時計算
        void DamageTotalAward(int damageTotal)
        {
            int awardUpgradeA = 0;
            int awardUpgradeB = 0;
            int awardGem = 0;

            TimeLimitedSummaryConfig damageConfig = null;

            for (int i = 0; i < GameController.Instance.Model.TimeLimitedSummaryConfigs.Count; i++)
            {
                TimeLimitedSummaryConfig damageConfigTmp = GameController.Instance.Model.TimeLimitedSummaryConfigs[i];

                if (damageTotal >= damageConfigTmp.DamageTotal)
                {
                    damageConfig = damageConfigTmp;
                }
            }

            //damageConfig為null 則說明玩家傷害值太低，沒到可以獎勵的地步
            if (damageConfig != null)
            {
                TreeFightData.AwardData.UpgradeA += damageConfig.UpgradeA;
                TreeFightData.AwardData.UpgradeB += damageConfig.UpgradeB;

                awardUpgradeA = damageConfig.UpgradeA;
                awardUpgradeB = damageConfig.UpgradeB;
            }
 
            if (GameController.Instance.ModelEventSystem.OnTreeFightDamageAward != null)
            {
                GameController.Instance.ModelEventSystem.OnTreeFightDamageAward(awardUpgradeA, awardUpgradeB, awardGem);
            }
        }

        /// <summary>
        /// 關卡完成
        /// </summary>
        void StageComplete()
        {
            IsBegin = false;
            DamageTotalAward(TreeFightData.DamageTotal);

            if (GameController.Instance.ModelEventSystem.OnTreeFightComplete != null)
            {
                ModelEventSystem.BattleResultEventArgs args = new ModelEventSystem.BattleResultEventArgs();
                args.Result = true;
                args.Key = 0;
                args.Star = 3;
                args.UpgradeA = TreeFightData.AwardData.UpgradeA;
                args.UpgradeB = TreeFightData.AwardData.UpgradeB;
                args.Gem = TreeFightData.AwardData.Gem;
                GameController.Instance.ModelEventSystem.OnTreeFightComplete(args);
            }
        }

        /// <summary>
        /// 遊戲開始，倒計時開始
        /// 最好是通過view去調用
        /// </summary>
        public void StageBegin()
        {
            if (IsBegin)
            {
                return;
            }

            IsBegin = true;
            GameController.Instance.ServerTime.AddTimeTick(OnTimeTick);
            //賦值後會拋出事件
            CutHPConfig = GameController.Instance.Model.TimeLimitedHPIdConfigs[1];      //永遠都從第一級的打起 

            if (GameController.Instance.ModelEventSystem.OnTreeFightBegin != null)
            {
                GameController.Instance.ModelEventSystem.OnTreeFightBegin();
            }
        }

        //倒計時
        private void OnTimeTick(double duration)
        {
            //可以被暫停
            if (!IsBegin || IsPause)
            {
                return;
            }

            TreeFightData.TimeLeft -= duration;
            if (TreeFightData.TimeLeft >= 0)
            {
                if (GameController.Instance.ModelEventSystem.OnTreeFightTimeTick != null)
                {
                    GameController.Instance.ModelEventSystem.OnTreeFightTimeTick();
                }
            }
            else
            {
                //時間到
                StageComplete();
            }
        }

        //遊戲中斷後，比如點擊返回鍵 需要調用
        public void Dispose()
        {
            Clear();
        }

        //需要關卡放棄的時候也調用下
        public void Clear()
        {
            ///提交數據異常返回登錄界面清除數據時TreeFightData可能為null
            if (TreeFightData != null)
                GameController.Instance.StageCtr.UpdateActiveStage(TreeFightData.cutStageId);
            //移除監聽
            GameController.Instance.ServerTime.RemoveTimeTick(OnTimeTick);
            TreeFightData = null;
        }
    }
}
