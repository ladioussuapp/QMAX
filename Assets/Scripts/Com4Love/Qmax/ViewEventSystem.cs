using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.TileBehaviour;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax
{
    /// <summary>
    /// View的事件系统
    /// </summary>
    public class ViewEventSystem
    {
        public enum TileStatusChangeMode
        {
            ToLink, //變為連接狀態
            ToUnlink, //變為非連接狀態
            ChangeData, //更改數據
        }

        /// <summary>
        /// 連接事件
        /// </summary>
        /// <param name="mode">1 新連接一個元素；2 取消連接；3 回退一個元素</param>
        /// <param name="mainColor"></param>
        /// <param name="linkPath"></param>
        /// <param name="dragBackTile">當mode==3時，該參數為回退的參數。可以為null</param>
        public Action<int, ColorType, List<Position>, BaseTileBehaviour> BoardLinkEvent;


        /// <summary>
        /// 
        /// </summary>
        public Action<TileObject,
                      TileStatusChangeMode,
                      List<Position>> TileStatusChangeEvent;


        /// <summary>
        /// 點擊了地圖按鈕事件
        /// </summary>
        public Action<int> ClickMapBtnEvent;


        /// <summary>
        /// 返回登錄場景事件
        /// </summary>
        public Action BackToLoginSceneEvent;

        public Action<Action> JumpSceneShowCloudEvent;

        /// <summary>
        /// 跳轉場景播放關閉雲層動畫事件
        /// 第一個參數播放動畫進入Over狀態回調函數
        /// 第二個參數播放動畫退出Over狀態回調函數
        /// </summary>
        public Action<Action, Action> JumpSceneHideCloudEvent;

        /// <summary>
        /// 戰鬥關卡，進入戰鬥前會有一段動畫表現。這是動畫完成的事件
        /// </summary>
        public Action<Action> BattleInitAnimCompleteEvent;

        /// <summary>
        /// 戰斗場景中第一次鏡頭移動完成
        /// 目前用在大樹活動中
        /// </summary>
        public Action BattleFirstMoveCompleteEvent;

        /// <summary>
        /// 返回鍵被按
        /// </summary>
        public Action ClickEscapeEvent;


        /// <summary>
        /// 控制顯示技能範圍提示
        /// </summary>
        /// <param name="int">1 顯示，2全部隱藏</param>
        /// <param name="List<Vector2>">坐標</param>
        public Action<int, List<Position>> ControlSkillRangeEvent;

        /// <summary>
        /// Application暫停事件
        /// </summary>
        public Action<bool> ApplicationPauseEvent;

        ///登陸領取成功事件///
        public Action LoginSuccessAwardEvent;

        /// <summary>
        /// 打開大樹活動
        /// </summary>
        public Action TryToStartTreeAct;

        /// <summary>
        /// 炸彈的影響狀態改變
        /// </summary>
        public Action<BaseTileBehaviour, bool> AffectedChannge;

        /// <summary>
        /// 準備就緒
        /// </summary>
        public Action ReadyGo;

        ///增加五步動畫事件
        public Action AddSteps;

        /// <summary>
        /// 戰鬥時，攝像機移動
        /// param: 0 開始移動 1移動中？ 2移動完成，結束
        /// </summary>
        public Action<int> OnBattleCameraMove;

        /// <summary>
        /// 怪物位置改變 有血條顯示的話會依賴它
        /// </summary>
        public Action<Vector3> OnBattleEnemyPosChange;

        /// <summary>
        /// 有怪沒怪
        /// </summary>
        public Action<bool> OnBattleEnemyDisplayChange;

        /// <summary>
        /// 滾動到某一個關卡
        /// </summary>
        public Action<int> MoveToMapLvl;

        /// <summary>
        /// 滾動到下一個關卡完成
        /// </summary>
        public Action OnMoveToNextLvlComplete;

        /// <summary>
        /// 清理所有註冊事件
        /// </summary>
        public void Clear()
        {
            BoardLinkEvent = null;
            TileStatusChangeEvent = null;
            ClickMapBtnEvent = null;
            BackToLoginSceneEvent = null;
            ClickEscapeEvent = null;
            ControlSkillRangeEvent = null;
            ApplicationPauseEvent = null;
            AffectedChannge = null;
        }
    }
}
