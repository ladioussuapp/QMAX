using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.achievement;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.Stage;
using Com4Love.Qmax.Net.Protocols.tree;
using Com4Love.Qmax.Net.Protocols.activity;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Com4Love.Qmax.Net.Protocols.goods;
using Com4Love.Qmax.Net.Protocols.getchance;
using Com4Love.Qmax.Net.Protocols.counterpart;
using Com4Love.Qmax.Net;

namespace Com4Love.Qmax
{
    /// <summary>
    /// 
    /// </summary>
    public class ModelEventSystem
    {
        public class Move
        {
            public TileObject NewValue;
            /// <summary>
            /// 原來的行號
            /// </summary>
            public int OriRow;
            /// <summary>
            /// 原來的列號
            /// </summary>
            public int OriCol;
            public Direction Direction;
            /// <summary>
            /// 收集位置
            /// </summary>
            public Dictionary<string, int> collectPosition = new Dictionary<string, int>();

            public Move(int oriRow = 0, int oriCol = 0, Direction direction = Direction.None, TileObject newValue = null)
            {
                this.OriRow = oriRow;
                this.OriCol = oriCol;
                this.Direction = direction;
                this.NewValue = newValue;
            }

            public override String ToString()
            {
                return String.Format("Move:r={0},c={1},dire={2},new={3}.", OriRow, OriCol, Direction, NewValue.Config.ColorType);
            }
        }

        #region EventArgs
        public class InitEventArgs : EventArgs
        {
            public TileObject[,] Elements;
            public TileObject[,] Collects;
            public TileObject[,] Obstacles;
            public TileObject[,] Covers;
            public TileObject[,] SeperatorH;
            public TileObject[,] SeperatorV;
            public TileObject[,] Bottom;
            public Dictionary<ColorType, Unit> Units;
        }
        public class MoveEventArgs : EventArgs
        {
            public List<List<ModelEventSystem.Move>> Moves;

            public override string ToString()
            {
                StringBuilder b = new StringBuilder("[MoveEventArgs], count=");
                b.Append(Moves.Count);
                b.Append("\n");
                for (int i = 0, n = Moves.Count; i < n; i++)
                {
                    List<ModelEventSystem.Move> subList = Moves[i];
                    for (int ii = 0, nn = subList.Count; ii < nn; ii++)
                    {
                        ModelEventSystem.Move m = subList[ii];
                        b.Append(m.ToString());
                        b.Append("\n");
                    }
                    b.Append("-------------\n");
                }
                return b.ToString();
            }
        }

        /// <summary>
        /// 步數用完了事件
        /// </summary>
        public Action<StepEmptyEventArgs> OnStepEmpty;

        public class StepEmptyEventArgs : EventArgs { }

        /// <summary>
        /// 時間用完了事件
        /// </summary>
        public Action<TimeEmptyEventArgs> OnTimeEmpty;

        public class TimeEmptyEventArgs : EventArgs { }

        public class EliminateEventArgs : EventArgs
        {
            public ColorType MainColor;

            /// <summary>
            /// 連接路徑
            /// </summary>
            public List<Position> LinkPath;

            /// <summary>
            /// 被消除之前的消除物數據
            /// 這裡不包括掉落過程中造成的數據改變
            /// </summary>
            public List<TileObject> OriTileDatas;

            /// <summary>
            /// 被消除之後的消除物數據
            /// 這裡不包括掉落過程中造成的數據改變
            /// </summary>
            public List<TileObject> NewTileDatas;

            /// <summary>
            /// 消除順序
            /// </summary>
            public List<int> ElimOrders;

            /// <summary>
            /// 消除獎勵
            /// </summary>
            public List<ItemQtt[]> ElimRewards;

            public List<List<ModelEventSystem.Move>> DropList;

            /// <summary>
            /// 夥伴扔出的技能消除物
            /// </summary>
            public List<TileObject> ThrowTileList;

            /// <summary>
            /// 怪物扔出的障礙物、覆蓋物
            /// </summary>
            public TileObject MonsterThrowTile;

            /// <summary>
            /// 額外攻擊的位置
            /// </summary>
            public List<Position> ExtraAtkPos;

            /// <summary>
            /// 額外攻擊的顏色
            /// </summary>
            public List<ColorType> ExtraAtkColor;

            /// <summary>
            /// 額外攻擊的延遲
            /// </summary>
            public List<int> ExtraAtkOrders;
        }

        public class MonstorThrowTileEventArgs : EventArgs
        {
            public TileObject tileObj;
        }

        public class StepConvertAwardEventArgs : EventArgs { }

        public class ThrowTileEventArgs : EventArgs
        {
            public TileObject tileObj;
        }

        public class BattleResultEventArgs : EventArgs
        {
            public bool Result;
            public int Key;
            public int UpgradeA;
            public int UpgradeB;
            public int Gem;
            public int Star;
            public int Coin;
        }

        public class RearrangeEventArgs : EventArgs
        {
            /// <summary>
            /// 新的陣型數據
            /// </summary>
            public TileObject[,] NewElementData;

            /// <summary>
            /// 對應的舊位置
            /// </summary>
            public Vector2[,] OriPositions;
        }

        #endregion EventArgs

        #region Events

        /// <summary>
        /// 場景改變前觸發 (應該是view事件)
        /// </summary>
        /// <param name="preScene">更改前的場景</param>
        /// <param name="nextScene">更改後的場景</param>
        public Action<Scenes, Scenes> BeforeSceneChangeEvent;

        /// <summary>
        /// 場景改變後觸發
        /// </summary>
        /// <param name="preScene">更改前的場景</param>
        /// <param name="nextScene">更改後的場景</param>
        public Action<Scenes, Scenes> AfterSceneChangeEvent;


        /// <summary>
        /// 棋盤初始化事件
        /// </summary>
        public Action<InitEventArgs> OnBoardInit;


        /// <summary>
        /// 元素移動事件
        /// </summary>
        public Action<MoveEventArgs> OnBoardMove;

        /// <summary>
        /// 棋盤被消除確認的事件
        /// </summary>
        public Action<EliminateEventArgs> OnBoardEliminate;

        /// <summary>
        /// 扔出一個地形物的事件。
        /// 可能包括：夥伴扔出技能消除物，敵人扔出障礙物、覆蓋物等事件
        /// </summary>
        /// <param name="Vector2">位置</param>
        /// <param name="TileObjectConfig">對應地形物</param>
        public Action<ThrowTileEventArgs> OnThrowTile;

        public Action<MonstorThrowTileEventArgs> OnMonstorThorwTile;

        /// <summary>
        /// 棋盤重新排列的事件
        /// </summary>
        public Action<RearrangeEventArgs> OnBoardRearrange;

        /// <summary>
        /// 戰鬥結果事件
        /// </summary>
        public Action<BattleResultEventArgs> OnBattleResult;

        /// <summary>
        /// 達到通關條件
        /// </summary>
        public Action<ReachGoalEventArgs> onReachGoalEvent;
        public class ReachGoalEventArgs : EventArgs { }


        /// <summary>
        /// 登錄結果
        /// </summary>
        /// <param>ResponseCode</param>
        /// <param>string, 提示消息</param>
        public Action<ResponseCode, LoginState> OnLoginProgress;

        /// <summary>
        /// 關卡信息刷新 一般為某關的星數更改
        /// </summary>
        /// <param name="stage"></param>
        public Action<Stage> OnStageInfoRef;

        /// <summary>
        /// 通關事件
        /// </summary>
        public Action OnStagePassed;

        /// <summary>
        /// 勝利事件，與通關事件不同，勝利事件先於通關事件
        /// </summary>
        public Action OnStageWin;

        /// <summary>
        /// 失敗事件
        /// </summary>
        public Action OnStageLose;

        /// <summary>
        /// 通關後獲得獎勵
        /// </summary>
        public Action<bool, SubmitStageFightResponse> OnStageRewardRespone;

        /// <summary>
        /// 玩家信息改變
        /// </summary>
        public Action<List<RewardType>, ActorGameResponse> OnPlayerInfoRef;

        /// <summary>
        /// 人物信息初始化
        /// </summary>
        public Action<ActorGameResponse> OnPlayerInfoInit;

        /// <summary>
        /// 地圖數據初始化完成
        /// </summary>
        public Action OnStageListDataInit;

        /// <summary>
        /// 戰斗場景完成一個目標
        /// </summary>
        /// <param name="star">當前達到的星級</param>
        /// <param name="nextGoal">下一級的目標</param>
        public Action<int, List<StageConfig.Goal>> OnBattleGoalComplete;

        /// <summary>
        /// 夥伴升級事件
        /// </summary>
        public Action OnUnitUpgrad;

        /// <summary>
        /// 抽寶箱
        /// </summary>
        public Action<GetChanceResponse> OnOpenBox;

        ///// <summary>
        ///// 購買剪刀 鑰匙
        ///// </summary>
        public Action<ValueResultListResponse> OnBuyKeys;

        /// <summary>
        /// 兌換碼
        /// </summary>
        public Action<ExchangeCodeResponse> OnExchangeCode;
        public Action<short> OnExchangeCodeFail;
        public Action<Language> OnLanguageChange;

        /// <summary>
        /// 購買步數
        /// </summary>
        public Action<int> OnBuyMoves;

        /// <summary>
        /// 購買時間
        /// </summary>
        public Action<int> OnBuyTime;

        /// <summary>
        /// 購買鑽石
        /// </summary>
        public Action<int> OnBuyGems;

        /// <summary>
        /// 戰鬥剩餘步數
        /// </summary>
        public Action<int> OnLastStep;

        /// <summary>
        /// 戰鬥剩餘時間
        /// </summary>
        public Action<int> OnLastTime;

        /// <summary>
        /// 購買配置信息返回
        /// </summary>
        public Action OnPaymentInfo;

        /// <summary>
        /// 解鎖關卡返回
        /// </summary>
        public Action<int> OnStageUnlocked;

        /// <summary>
        /// 購買升級材料返回
        /// </summary>
        public Action OnBuyUpgrade;


        /// <summary>
        /// 充值後刷新信息
        /// </summary>
        public Action OnRechargeRefreshEvent;


        public Action<bool> OnBuyEnergyEvent;

        /// <summary>
        /// 體力自動增長事件  時間刷新
        /// </summary>
        public Action OnEnergyTimeGrowEvent;

        /// <summary>
        /// 5 5消息發送成功
        /// </summary>
        public Action OnStageBeginEvent;

        /// <summary>
        /// 失敗事件//
        /// </summary>
        public Action OnStageBeginErrorEvent;

        /// <summary>
        /// 簽到
        /// </summary>
        public Action<Com4Love.Qmax.Net.Protocols.sign.SignResponse> OnSignEvent;

        //簽到解鎖按鈕事件
        public Action OnSignUnlockEvent;

        ///簽到信息///
        public Action<int, bool> OnSignInfoEvent;

        ///// <summary>
        ///// 大樹信息返回    暫時棄用
        ///// </summary>
        //public Action OnTreeActInfoUpdate;

        ///// <summary>
        ///// 申請進入大樹戰鬥返回  param:狀態 0允許進入  1 未到時間 2 活動結束 3 之前已打過活動
        ///// (保留 通過另外一種方式可以進大樹活動或者大樹戰鬥)
        ///// </summary>
        ////public Action<int> OnEnterTreeActivity;
        public Action OnTreeFightBegin;
        public Action<BattleResultEventArgs> OnTreeFightComplete;
        /// <summary>
        /// 連線過程中的獎勵 參數 橘子桃子
        /// </summary>
        public Action<int, int> OnTreeFightLineAward;
        /// <summary>
        ///大樹HP獎勵 參數 橘子桃子鑽石
        /// </summary>
        public Action<TimeLimitedHPConfig> OnTreeFightHPAward;

        /// <summary>
        /// 大樹的最終傷害獎勵
        /// </summary>
        public Action<int, int, int> OnTreeFightDamageAward;

        /// <summary>
        /// 大樹被打 參數1為當前次攻擊造成的傷害。大樹的具體傷害數值和剩餘血量從TreeFightData中獲取 參數2 是否是預扣血
        /// </summary>
        public Action<int, bool> OnTreeFightDamaged;
        /// <summary>
        /// 倒計時
        /// </summary>
        public Action OnTreeFightTimeTick;

        /// <summary>
        /// 3個HP獎勵配置文件更換    
        /// </summary>
        public Action OnTreeFightHPConfigChange;

        ///// <summary>
        ///// 提交戰鬥返回
        ///// </summary>
        //public Action<SubmitTreeFightResponse> OnTreeFightSubmitRequest;

        public Action OnTreeFightPause;

        /// <summary>
        /// 執行了推送信息的序列化，此回調中可以嘗試去獲取推送的信息
        /// </summary>
        public Action OnPushDataSerialized;

        /// <summary>
        /// 打開成就界面返回
        /// </summary>
        public Action<AchievementListResponse> OnAchieveOpen;

        /// <summary>
        /// 領取成就獎勵返回
        /// </summary>
        public Action<AchievementRewardResponse> OnAchieveReward;

        /// <summary>
        /// 刷新成就界面返回
        /// </summary>
        public Action<AchievementListResponse> OnAchieveRefresh;

        /// <summary>
        /// 獲最道具列表
        /// </summary>
        public Action<GoodsListResponse> OnGetGoodsList;

        /// <summary>
        /// 刷新道具列表方法
        /// </summary>
        public Action<object> OnGoodsRefreshList;

        /// <summary>
        /// 使用的背包道具返回
        /// </summary>
        public Action<UseGoodsResponse> OnUseGoodsItem;

        /// <summary>
        /// 購買道具返回
        /// </summary>
        public Action<int, GoodsItem> OnBuyGoods;

        public Action<BuyGoodsResponse> OnBuyGoodsList;

        /// <summary>
        /// 已達成的成就數量更新事件
        /// </summary>
        public Action<int> OnReachAchieveCountUpdate;

        /// <summary>
        /// 清除道具信息事件
        /// </summary>
        public Action OnGoodsItemInfoClear;

        /// <summary>
        /// 刷新背包界面
        /// </summary>
        public Action OnGoodsWinRefresh;


        /// <summary>
        /// 反饋
        /// </summary>
        public Action<int> OnMailFeedBack;

        /// <summary>
        /// 戰鬥提示
        /// </summary>
        public Action<bool, UIBattleTipsBehaviour.AnimType> OnBattleTipsCheck;

        /// <summary>
        /// 關卡按鈕加星顯示完成事件
        /// </summary>
        public Action OnStarEffectOver;

        /// <summary>
        /// 雲打開事件
        /// </summary>
        public Action OnCloudeShow;

        /// <summary>
        /// 雲關閉事件
        /// </summary>
        public Action OnCloudeHide;

        /// <summary>
        /// 戰鬥對話出現事件
        /// </summary>
        public Action OnDialogShow;

        /// <summary>
        /// 戰鬥對話關閉事件
        /// </summary>
        public Action OnDialogHide;


        public enum BattleInfoType
        {
            //步數，時間，敵人，關卡目標 改變
            Step, Time , EnemyHp , Goal
        }

        /// <summary>
        /// 戰鬥部分信息刷新 如果view需要等待某個步奏完成之後再刷新數據，則需要在viewEvent裡再定義一個事件
        /// </summary>
        public Action<BattleInfoType> OnBattleInfoUpdate;

        //關卡副本
        public Action OnCounterpartStageList;
        public Action OnBeginCounterpartStage;
        public Action<SubmitCounterpartResponse> OnSubmitCounterpart;

        /// <summary>
        /// QmaxModel準備完畢事件
        /// </summary>
        public Action OnModelReady;

        #endregion Events

        public ModelEventSystem()
        {

        }


        /// <summary>
        /// 清理所有註冊事件
        /// </summary>
        public void Clear()
        {

        }


        /// <summary>
        /// 玩家信息初始化事件
        /// </summary>
        /// <param name="data"></param>
        public void DispatchPlayerInitEvent(ActorGameResponse data)
        {
            if (OnPlayerInfoInit != null)
            {
                OnPlayerInfoInit(data);
            }
            //初始化 並且刷新
            if (OnPlayerInfoRef != null)
            {
                OnPlayerInfoRef(null, data);
            }
        }
    }
}