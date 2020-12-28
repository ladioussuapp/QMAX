using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Net.Protocols.Stage;
using Com4Love.Qmax.TileBehaviour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Com4Love.Qmax.Ctr
{
    /// <summary>
    /// 核心玩法控制層
    /// </summary>
    public class PlayingRuleCtr : IDisposable
    {
        /// <summary>
        /// 夥伴配置
        /// </summary>
        public Dictionary<int, UnitConfig> UnitConfigs;

        /// <summary>
        /// 關卡配置
        /// </summary>
        public StageConfig stageConfig;

        /// <summary>
        /// 地形物配置，id -> ObjectConfig
        /// </summary>
        public Dictionary<int, TileObjectConfig> TileObjectConfigs;

        /// <summary>
        /// 技能配置
        /// </summary>
        public Dictionary<int, SkillConfig> SkillConfigs;

        /// <summary>
        /// Combo配置
        /// </summary>
        public Dictionary<int, ComboConfig> ComboConfigs;

        private BattleModelModifyAgent batModel;
        private ModelEventSystem modelEvtSys;
        private ViewEventSystem viewEvtSys;
        private BoardBehaviour boardBeh;
        private BoardModifyingRules modifyingRules;

        private ElementRuleCtr elementRuleCtr;


        /// <summary>
        /// 斜向掉落的標誌位，true的話優先判斷向左下方掉落，false的話優先判斷從右下方掉落
        /// </summary>
        private bool dropDireFlg = true;

        private int numRow = 0;
        private int numCol = 0;

        // 收集器觸發ID
        private int[,] collectorMap;

        public PlayingRuleCtr(ModelEventSystem modelEvtSys,
                              ViewEventSystem viewEvtSys)
        {
            this.modelEvtSys = modelEvtSys;
            this.viewEvtSys = viewEvtSys;
        }

        public PlayingRuleCtr(GameController gc)
            : this(gc.ModelEventSystem, gc.ViewEventSystem)
        {
            this.modelEvtSys.OnModelReady += OnModelReady;
        }

        public void SetConfigs(Dictionary<int, UnitConfig> UnitConfigs,
                               Dictionary<int, TileObjectConfig> TileObjectConfigs,
                               Dictionary<int, ComboConfig> ComboConfigs,
                               Dictionary<int, SkillConfig> SkillConfigs)
        {
            this.UnitConfigs = UnitConfigs;
            this.TileObjectConfigs = TileObjectConfigs;
            this.ComboConfigs = ComboConfigs;
            this.SkillConfigs = SkillConfigs;
        }


        public void Clear()
        {
            GameController gc = GameController.Instance;
            modelEvtSys = gc.ModelEventSystem;
            viewEvtSys = gc.ViewEventSystem;
        }



        public void Dispose()
        {
            if (modifyingRules != null)
                modifyingRules = null;
        }


        public void InitWithLevel(BoardBehaviour boardBeh,
                                  int level,
                                  List<int> units)
        {
            GameController gameCtrl = GameController.Instance;
            LevelConfig lvConf = gameCtrl.Model.GetBattleLevel(level);
            StageConfig stageConf = gameCtrl.Model.StageConfigs[level];
            Stage stageData = gameCtrl.StageCtr.GetStageData(level);
            InitWithLevel(boardBeh, gameCtrl.Model.BattleModel, lvConf, stageConf, stageData, units);

          
        }

        /// <summary>
        /// 根據關卡等級初始化陣型
        /// </summary>
        /// <param name="boardBeh"></param>
        /// <param name="battleModel"></param>
        /// <param name="levelConfig"></param>
        /// <param name="stageConfig"></param>
        /// <param name="stage"></param>
        /// <param name="units"></param>
        public void InitWithLevel(BoardBehaviour boardBeh,
                                  BattleModelModifyAgent battleModel,
                                  LevelConfig levelConfig,
                                  StageConfig stageConfig,
                                  Stage stage,
                                  List<int> units)
        {
            Q.Log("InitWithLevel(), StageID={0}, StageName={1}, StageNameStrID={2} units.Count={3}",
                stageConfig.ID,
                stageConfig.Name,
                stageConfig.NameStringID,
                units.Count);
            this.boardBeh = boardBeh;
            this.batModel = battleModel;

            //初始化夥伴數據
            battleModel.CrtUnitDict = new Dictionary<ColorType, Unit>();
            for (int i = 0, n = units.Count; i < n; i++)
            {
                Unit data = new Unit(UnitConfigs[units[i]]);
                data.Hp = data.Config.UnitHp;
                battleModel.CrtUnitDict.Add(data.Config.UnitColor, data);

                SkillConfig skillConfig = SkillConfigs[data.Config.UnitSkillId];
                battleModel.SkillConfDict.Add(skillConfig.SkillColor, skillConfig);
                battleModel.SkillCDDict.Add(skillConfig.SkillColor, 0);
            }


            battleModel.CrtLevelConfig = levelConfig;
            Q.Assert(battleModel.CrtLevelConfig != null);
            battleModel.CrtStageConfig = stageConfig;
            battleModel.StageLimit = stage.stageLimit;
            Q.Assert(battleModel.CrtLevelConfig != null);

            numRow = battleModel.CrtLevelConfig.NumRow;
            numCol = battleModel.CrtLevelConfig.NumCol;
            elementRuleCtr = new ElementRuleCtr(battleModel, TileObjectConfigs, numRow, numCol);

            if (modifyingRules != null)
            {
                modifyingRules.Dispose();
                modifyingRules = null;
            }
            //modifyingRules = new BoardModifyingRules(boardBeh, elementRuleCtr);
            modifyingRules = new BoardModifyingRules(
                batModel,
                boardBeh,
                elementRuleCtr,
                viewEvtSys,
                modelEvtSys,
                TileObjectConfigs,
                numRow,
                numCol);
            modifyingRules.playingRuleCtr = this;

            //初始化敵人數據
            battleModel.EnemiesData = new List<Unit>();
            GameController gc = GameController.Instance;
            for (int i = 0, n = battleModel.CrtStageConfig.MonsterUnitID.Length; i < n; i++)
            {
                UnitConfig cfg = UnitConfigs[battleModel.CrtStageConfig.MonsterUnitID[i]];
                if (cfg.UnitSkillId != -1)
                {
                    List<SkillConfig> skillCfgArr = new List<SkillConfig>();
                    for (int ii = 0; ii < cfg.UnitSkillIdArr.Count; ii++)
                    {
                        SkillConfig skillConfig = SkillConfigs[cfg.UnitSkillIdArr[ii]];
                        skillCfgArr.Add(skillConfig);
                    }
                    if (!battleModel.EneriesSkillConDict.ContainsKey(cfg.ID))
                    {
                        battleModel.EneriesSkillConDict.Add(cfg.ID, skillCfgArr);
                    }
                }

                //battleModel.AddEnemiesData(cfg);
                gc.PropCtr.SetEnemiesData(battleModel, cfg);
            }

            battleModel.Steps = new List<Step>();

            // 初始化剩餘步數 or 剩餘時間
            if (battleModel.CrtStageConfig.Mode == BattleMode.Normal)
            {
                //battleModel.SetRemainSteps(battleModel.StageLimit);
                gc.PropCtr.SetRemainSteps(battleModel);
            }
            else if (battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
            {
                battleModel.SetRemainTime(battleModel.StageLimit);
            }

            battleModel.UnitGoal = 0;
            battleModel.ObjectGoal = new Dictionary<int, int>();
            List<StageConfig.Goal>[] arr = { 
                                               BattleTools.ParseGoal(stage.targets[1]), 
                                               BattleTools.ParseGoal(stage.targets[2]), 
                                               BattleTools.ParseGoal(stage.targets[3]) 
                                           };
            for (int i = 0, n = arr.Length; i < n; i++)
            {
                List<StageConfig.Goal> goalList = arr[i];
                foreach (StageConfig.Goal g in goalList)
                {
                    if (g.Type == BattleGoal.Object && !battleModel.ObjectGoal.ContainsKey(g.RelativeID))
                        battleModel.ObjectGoal.Add(g.RelativeID, 0);
                }
            }

            //Stage stage = gc.StageCtr.GetStageData(level);
            //獲取此關卡已經打到的星星數
            battleModel.CurStage = stage;
            battleModel.PreStar = stage.star;

            battleModel.WinCount = stage.win;
            battleModel.FailCount = stage.lose;

            List<StageConfig.Goal> goals = BattleTools.ParseGoal(stage.targets[1]);

            int enemyNum = 0;


            //檢查是否是所有目標都是分數
            bool isScoreGoal = true;
            foreach (var item in goals)
            {
                if (item.Type != BattleGoal.Score)
                {
                    isScoreGoal = false;
                    break;
                }
            }


            if (!isScoreGoal)
            {
                //非分數目標使用一星配置
                enemyNum = gc.PropCtr.SetCurrentGoal(battleModel, goals);
            }
            else
            {
                int starnum = stage.star + 1;
                if (starnum > 3)
                    starnum = 3;

                //enemyNum = battleModel.SetCurrentGoal(BattleTools.ParseGoal(stage.targets[(byte)starnum]));
                enemyNum = gc.PropCtr.SetCurrentGoal(battleModel, BattleTools.ParseGoal(stage.targets[(byte)starnum]));
            }

            Q.Assert(enemyNum <= stageConfig.MonsterUnitID.Length, "--關卡怪物目標比配置關卡怪物多");

            gc.PropCtr.SetStarScore(battleModel);


            List<int[,]> srcConfigLayers = new List<int[,]>();
            srcConfigLayers.Add(battleModel.CrtLevelConfig.ElementLayer);
            srcConfigLayers.Add(battleModel.CrtLevelConfig.CollectLayer);
            srcConfigLayers.Add(battleModel.CrtLevelConfig.ObstacleLayer);
            srcConfigLayers.Add(battleModel.CrtLevelConfig.CoveringLayer);
            srcConfigLayers.Add(battleModel.CrtLevelConfig.SeperatorHLayer);
            srcConfigLayers.Add(battleModel.CrtLevelConfig.SeperatorVLayer);
            srcConfigLayers.Add(battleModel.CrtLevelConfig.BottomLayer);

            collectorMap = new int[numRow, numCol];
            battleModel.InitDatas(numRow, numCol);
            for (int i = 0, n = srcConfigLayers.Count; i < n; i++)
            {
                int[,] srcConf = srcConfigLayers[i];
                for (int r = 0; r < numRow; r++)
                {
                    for (int c = 0; c < numCol; c++)
                    {
                        TileObject tileObj;
                        //Q.Log("i={0}, r={1}, c={2}", i, r, c);
                        int objID = srcConf[r, c];
                        if (objID != 0)
                        {
                            Q.Assert(TileObjectConfigs.ContainsKey(objID), "Can not find the key {0}.", objID);
                            TileObjectConfig objConf = TileObjectConfigs[objID];
                            tileObj = new TileObject(r, c, objConf);
                            battleModel.SetDataAt(tileObj, r, c, objConf.ObjectType);

                            if (objConf.ObjectType == TileType.Collect)
                            {
                                collectorMap[r, c] = 266;
                            }
                        }
                    }
                }//for
            }//for

            battleModel.ChangeRandomWeight();

            if (modelEvtSys.OnBoardInit != null)
                modelEvtSys.OnBoardInit(battleModel.GenerateBoardInitEventArgment());

            //這個檢查是為了構造LinkableStack
            if (battleModel.LinkableStack == null)
                battleModel.LinkableStack = new Stack<TileObject>();
            else
                battleModel.LinkableStack.Clear();
            CheckElimatablePath(null, battleModel.LinkableStack);
        }//InitWithLevel


        private void OnModelReady()
        {
            Q.Log("PlayingRuleCtr:OnModelReady");
            this.modelEvtSys.OnModelReady -= OnModelReady;
            GameController gc = GameController.Instance;
            SetConfigs(gc.Model.UnitConfigs, gc.Model.TileObjectConfigs, gc.Model.ComboConfigs, gc.Model.SkillConfigs);
        }

        public void CalcEliminatePath(List<Position> linkPath,
                                      out List<Position> retElimPath,
                                      out List<int> retElimOrders)
        {
            elementRuleCtr.CalcElimPathByLinkPath(linkPath, out retElimPath, out retElimOrders);
        }


        /// <summary>
        /// 提供真正消除行為前的預覽，不改變數據層
        /// </summary>
        /// <param name="linkPath"></param>
        /// <param name="retOriDatas"></param>
        /// <param name="retNewDatas"></param>
        /// <param name="retOrders"></param>
        /// <param name="retElimRewards"></param>
        public void PreviewEliminate(List<Position> linkPath, 
                                     out List<TileObject> retOriDatas,
                                     out List<TileObject> retNewDatas,
                                     out List<int> retOrders,
                                     out List<ItemQtt[]> retElimRewards)
        {
            //地形物被消除之後對應的TileObject
            List<Position> extraAtkPos = null;
            List<ColorType> extraAtkColor = null;
            List<int> extraAtkOrders = null;
            elementRuleCtr.Eliminate(linkPath,
                null,
                false,
                out retOriDatas,
                out retNewDatas,
                out retOrders,
                out retElimRewards,
                out extraAtkPos,
                out extraAtkColor,
                out extraAtkOrders);
        }

        /// <summary>
        /// 根據提供的路徑消除元素
        /// </summary>
        /// <param name="linkPath"></param>
        /// <param name="initHurts">初始的傷害值</param>
        /// <returns></returns>
        public bool Eliminate(List<Position> linkPath, List<int> initHurts = null)
        {
            UnityEngine.Debug.LogFormat("PlayingRuleCtr:Eliminate1, {0}", linkPath.Count);

            if (linkPath.Count <= 0)
            {
                Q.Assert(false);
                return false;
            }

            Position firstPos = linkPath[0];
            TileObject firstElement = batModel.GetElementAt(firstPos.Row, firstPos.Col);
            ColorType mainColor = firstElement == null ? ColorType.None : firstElement.Config.ColorType;


            batModel.CommitModifying();

            //地形物被消除之後對應的TileObject
            List<TileObject> afterEliminatedData = null;
            List<TileObject> beforeEliminatedData = null;
            List<int> orders = null;
            List<ItemQtt[]> elimRewards = null;
            List<Position> extraAtkPos = null;
            List<ColorType> extraAtkColor = null;
            List<int> extraAtkOrders = null;
            elementRuleCtr.Eliminate(linkPath,
                initHurts,
                true,
                out beforeEliminatedData,
                out afterEliminatedData,
                out orders,
                out elimRewards,
                out extraAtkPos,
                out extraAtkColor,
                out extraAtkOrders);

            // UnityEngine.Debug.LogFormat("PlayingRuleCtr:Eliminate1, {0}.{1}.{2}",
            //     beforeEliminatedData.Count,
            //     afterEliminatedData.Count,
            //     orders.Count);

            //完全沒有可以消除的地形物
            if (beforeEliminatedData.Count == 0)
                return false;


            List<TileObject> beforeCollectedDatas = null;
            List<TileObject> afterCollectedDatas = null;
            //執行掉落
            List<List<ModelEventSystem.Move>> dropLit = Drop(out beforeCollectedDatas, out afterCollectedDatas);


            List<TileObject> allBeforeDatas = new List<TileObject>();
            allBeforeDatas.AddRange(beforeEliminatedData);
            allBeforeDatas.AddRange(beforeCollectedDatas);
            List<TileObject> allAfterDatas = new List<TileObject>();
            allAfterDatas.AddRange(afterEliminatedData);
            allAfterDatas.AddRange(afterCollectedDatas);

            //記錄消除數據
            RecordEliminate(allBeforeDatas);
            //清理可連接路徑提示的數據
            batModel.LinkableStack.Clear();
            //更新關卡目標
            //檢查該物品是否是勝利條件之一
            CheckBattleObjGoal(allBeforeDatas, allAfterDatas);


            //檢查技能CD，可能會拋出ThrowTile事件
            List<TileObject> throwTileList = ExecGuySkillCD(beforeEliminatedData);
            //發送消除事件
            DispatchEliminateEvent(mainColor,
                linkPath,
                beforeEliminatedData,
                afterEliminatedData,
                orders,
                elimRewards,
                dropLit,
                throwTileList,
                extraAtkPos,
                extraAtkColor,
                extraAtkOrders);

            batModel.ChangeRandomWeight();
            return true;
        }//Eliminate



        /// <summary>
        /// 保存消除記錄，是提交戰鬥結果時所需的記錄
        /// </summary>
        /// <param name="eliminatedObjs"></param>
        /// <returns></returns>
        private void RecordEliminate(List<TileObject> eliminatedObjs)
        {
            //普通元素List
            List<Remove> normalRemoveList = new List<Remove>();
            //除普通元素之外的其他地形物List
            List<Remove> otherRemoveList = new List<Remove>();
            Dictionary<int, Remove> dict = new Dictionary<int, Remove>();
            for (int i = 0, n = eliminatedObjs.Count; i < n; i++)
            {
                int confId = eliminatedObjs[i].ConfigID;
                if (dict.ContainsKey(confId))
                {
                    dict[confId].num++;
                }
                else
                {
                    Remove r = new Remove(confId, 1);
                    dict.Add(confId, r);

                    if (confId <= 5)
                        normalRemoveList.Add(r);
                    else
                        otherRemoveList.Add(r);
                }
            }

            byte useGoods = batModel.MinusRemainSteps() ? (byte)0 : (byte)1;
            Step step = new Step(normalRemoveList, otherRemoveList, useGoods);
            batModel.Steps.Add(step);
            //batModel.RemainSteps--;

        }


        /// <summary>
        /// 執行夥伴技能的CD時間
        /// </summary>
        /// <param name="eliminatedObjs"></param>
        private List<TileObject> ExecGuySkillCD(List<TileObject> eliminatedObjs)
        {
            for (int i = 0, n = eliminatedObjs.Count; i < n; i++)
            {
                TileObject tObj = eliminatedObjs[i];
                if (tObj.Config == null || tObj.Config.ObjectType != TileType.Element)
                    continue;

                if (batModel.SkillCDDict.ContainsKey(tObj.Config.ColorType))
                    batModel.SkillCDDict[tObj.Config.ColorType]++;
                else
                    batModel.SkillCDDict.Add(tObj.Config.ColorType, 1);
            }
            return batModel.CheckSkillCD();
        }


        /// <summary>
        /// 檢查關卡中的消除地形物目標，看是否是勝利條件之一
        /// 檢查該物品是否是勝利條件之一
        /// </summary>
        private void CheckBattleObjGoal(List<TileObject> oriDatas,
                                        List<TileObject> newDatas)
        {
            for (int i = 0, n = oriDatas.Count; i < n; i++)
            {
                TileObject nData = newDatas[i];
                TileObject oData = oriDatas[i];
                Q.Assert(oData != null && oData.Config != null);
                if (nData != null)
                    Q.Assert(nData.Config != null);

                //是否已經完全消除
                bool isTotalyEliminated = false;
                isTotalyEliminated = nData == null || nData.Config == null;
                //已經變成不可消除的地形物，也認為是完全消除了
                isTotalyEliminated =
                    isTotalyEliminated ||
                    (nData.OriID != nData.Config.ID && nData.Config.Level == -1);

                if (!isTotalyEliminated)
                    continue;

                //消除目標可能是某種顏色的消除物，需要判斷顏色
                if (oData.Config.ObjectType == TileType.Element && batModel.ObjectGoal.ContainsKey((int)oData.Config.ColorType))
                    batModel.ObjectGoal[(int)oData.Config.ColorType]++;
                //也可能是某一種地形物，這時需要取其OriID
                else if (batModel.ObjectGoal.ContainsKey(oData.OriID))
                    batModel.ObjectGoal[oData.OriID]++;
            }
        }

        /// <summary>
        /// 發送消除事件
        /// </summary>
        /// <param name="mainColor"></param>
        /// <param name="beforeEliminatedData"></param>
        /// <param name="afterEliminatedData"></param>
        /// <param name="dropLit"></param>
        /// <param name="throwTileList"></param>
        private void DispatchEliminateEvent(ColorType mainColor,
                                            List<Position> linkPath,
                                            List<TileObject> beforeEliminatedData,
                                            List<TileObject> afterEliminatedData,
                                            List<int> elimOrders,
                                            List<ItemQtt[]> elimRewards,
                                            List<List<ModelEventSystem.Move>> dropLit,
                                            List<TileObject> throwTileList,
                                            List<Position> extraAtkPos,
                                            List<ColorType> extraAtkColor,
                                            List<int> extraAtkOrders)
        {
            string msg = string.Format("beforeEliminatedData.Count:{0}, afterEliminatedData.Count:{1}", beforeEliminatedData.Count, afterEliminatedData.Count);
            Q.Assert(beforeEliminatedData.Count == afterEliminatedData.Count, msg);
            ModelEventSystem.EliminateEventArgs args = new ModelEventSystem.EliminateEventArgs();
            args.LinkPath = linkPath;
            args.OriTileDatas = beforeEliminatedData;
            args.NewTileDatas = afterEliminatedData;
            args.MainColor = mainColor;
            args.ElimOrders = elimOrders;
            args.ElimRewards = elimRewards;
            args.ExtraAtkPos = extraAtkPos;
            args.ExtraAtkColor = extraAtkColor;
            args.ExtraAtkOrders = extraAtkOrders;

            //執行掉落
            args.DropList = dropLit;

            //檢查技能CD，可能會拋出ThrowTile事件
            args.ThrowTileList = throwTileList;

            if (modelEvtSys.OnBoardEliminate != null)
                modelEvtSys.OnBoardEliminate(args);
        }



        int SortRule(TileObject one, TileObject two)
        {
            int oneColor = (int)one.Config.ColorType;
            int twoColor = (int)two.Config.ColorType;

            if (oneColor > twoColor)
            {
                return 1;
            }
            else if (oneColor < twoColor)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 將當前棋盤元素重新排列，得出一個可以連接的陣型
        /// </summary>
        public void Rearrange()
        {
            List<TileObject> elementToRearrange = new List<TileObject>();
            Q.Log("---- Rearrange {0},{1}", numRow, numCol);
            for (int r = 0; r < numRow; r++)
            {
                for (int c = 0; c < numCol; c++)
                {
                    TileObject ele = batModel.GetElementAt(r, c);
                    TileObject cover = batModel.GetDataAt(r, c, TileType.Cover);
                    if (ele == null)
                        continue;

                    //被覆蓋物覆蓋的消除物不參與重新排列
                    if (cover != null)
                        continue;

                    elementToRearrange.Add(ele);
                }//for
            }//for

            Rearrange(elementToRearrange, null);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementToRearrange"></param>
        /// <param name="NoRearrangeTile">不參與排序的位置</param>
        /// <param name="RearrangeByColor"></param>
        public void Rearrange(List<TileObject> elementToRearrange,
                              TileObject[,] NoRearrangeTile,
                              bool RearrangeByColor = false)
        {
            Q.Log("需要重新排列 {0}.{1}", elementToRearrange.Count, RearrangeByColor);
            bool success = false;
            TileObject[,] newBoard;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (RearrangeByColor)
            {
                elementToRearrange.Sort(SortRule);
            }

            sw.Reset();
            long duration1 = sw.ElapsedMilliseconds;
            sw.Start();

            int count = 0;
            while (true)
            {
                List<TileObject> list = new List<TileObject>(elementToRearrange);
                newBoard = new TileObject[batModel.CrtLevelConfig.NumRow, batModel.CrtLevelConfig.NumCol];
                for (int r = 0; r < numRow; r++)
                {
                    for (int c = 0; c < numCol; c++)
                    {
                        if (!batModel.ContainsDataAt(r, c, TileType.Element))
                            continue;

                        if (batModel.ContainsDataAt(r, c, TileType.Cover))
                            continue;

                        Q.Assert(list.Count > 0);

                        ///不需要排序的列表裡的物品要剔除///
                        if (NoRearrangeTile != null)
                        {
                            if (r < NoRearrangeTile.GetLength(0) && c < NoRearrangeTile.GetLength(1))
                            {
                                if (NoRearrangeTile[r, c] != null)
                                    continue;
                            }
                        }

                        int index = 0;
                        ///不是按顏色排布就隨機//
                        if (!RearrangeByColor)
                        {
                            //隨機取一個從原來的陣型中取一個元素，進行填充
                            index = UnityEngine.Random.Range(0, list.Count);
                        }
                        //Q.Log(LogTag.Test, "--------- randomIndex={0}, list.count={1}, r={2}, c={2}",
                        //    index, list.Count, r, c);
                        TileObject ele = list[index];
                        list.RemoveAt(index);
                        //這裡要重新生成一個元素
                        newBoard[r, c] = new TileObject(ele);
                    }//for
                }//for
                success = CheckElimatablePath(newBoard);
                if (success)
                    break;

                //死都隨機不出來可連接陣型
                if (++count >= BattleModel.REARRANGE_TRY_TIMES)
                {
                    Q.Assert(false, "count=={0}, 死都隨機不出來可連接陣型", count);
                    break;
                }
            }

            Q.Log("count={0}, 重排序完畢", count);

            if (!success)
            {
                //重排失敗，直接宣布關卡失敗
                batModel.SubmitFightRequest();
                return;
            }


            sw.Reset();
            long duration2 = sw.ElapsedMilliseconds;
            sw.Start();
            //Q.Log("BattleModel::Rearrange(), test count = {0}", count);

            Vector2[,] oriPositions = new Vector2[batModel.CrtLevelConfig.NumRow, batModel.CrtLevelConfig.NumCol];
            for (int r = 0, numRow = newBoard.GetLength(0); r < numRow; r++)
            {
                for (int c = 0, numCol = newBoard.GetLength(1); c < numCol; c++)
                {
                    TileObject ele = newBoard[r, c];
                    if (batModel.ContainsDataAt(r, c, TileType.Cover))
                        continue;
                    //更新Model
                    batModel.SetDataAt(ele, r, c, TileType.Element);
                    if (ele == null)
                        continue;

                    oriPositions[r, c] = new Vector2(ele.Row, ele.Col);
                    ele.Row = r;
                    ele.Col = c;
                }
            }

            sw.Reset();
            long duration3 = sw.ElapsedMilliseconds;
            sw.Stop();

            Q.Log("BattleModel::Rearrange(), duration1={0}, duration2={1}, duration3={2}",
                duration1, duration2, duration3);

            ModelEventSystem.RearrangeEventArgs args = new ModelEventSystem.RearrangeEventArgs();
            args.NewElementData = newBoard;
            args.OriPositions = oriPositions;
            if (modelEvtSys.OnBoardRearrange != null)
                modelEvtSys.OnBoardRearrange(args);
        }

        /// <summary>
        /// 按照顏色排序///
        /// </summary>
        public void RearrangeByColor()
        {
            List<TileObject> elementToRearrange = new List<TileObject>();

            TileObject[,] NoRearrange = new TileObject[numRow, numCol];

            for (int r = 0; r < numRow; r++)
            {
                for (int c = 0; c < numCol; c++)
                {
                    TileObject ele = batModel.GetElementAt(r, c);
                    TileObject cover = batModel.GetDataAt(r, c, TileType.Cover);

                    if (ele == null)
                        continue;

                    ///顏色為none的 例如全屏炸彈...不參與排序//
                    if (ele.Config.ColorType == ColorType.None || ele.Config.ColorType == ColorType.All)
                    {
                        NoRearrange[r, c] = ele;
                        continue;
                    }

                    //被覆蓋物覆蓋的消除物不參與重新排列
                    if (cover != null)
                        continue;
                    elementToRearrange.Add(ele);
                }//for
            }//for

            Rearrange(elementToRearrange, NoRearrange, true);
        }


        /// <summary>
        /// 檢查整個陣型是否有可消除的路徑
        /// </summary>
        /// <param name="data"></param>
        /// <param name="resultStack">如果找到則返回第一個匹配的路徑；找不到返回null.</param>
        /// <returns></returns>
        public bool CheckElimatablePath(TileObject[,] data = null, Stack<TileObject> resultStack = null)
        {
            if (data == null)
                data = batModel.GetElementMap();

            for (int r = 0, n = data.GetLength(0); r < n; r++)
            {
                for (int c = 0, nn = data.GetLength(1); c < nn; c++)
                {
                    if (resultStack != null)
                        resultStack.Clear();

                    if (CheckLinkableAt(data, r, c, ColorType.None, 3, resultStack))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// 檢查某個位置石佛有needLen個相同元素可連接
        /// </summary>
        /// <param name="elements">元素數據</param>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <param name="color">需要連接的ColorType。如果為ColorType.None，則默認為當前顏色</param>
        /// <param name="needLen">所需的長度</param>
        /// <param name="returnLinkStack">可以連接的元素會放入該List。默認為null</param>
        /// <param name="linkTable">已經遍歷過的位置標為true</param>
        /// <returns></returns>     
        private bool CheckLinkableAt(TileObject[,] elements,
            int r, int c,
            ColorType color = ColorType.None, int needLen = 3,
            Stack<TileObject> returnLinkStack = null,
            bool[,] linkTable = null)
        {
            if (r < 0 || r >= batModel.CrtLevelConfig.NumRow || c < 0 || c >= batModel.CrtLevelConfig.NumCol)
                return false;

            if (linkTable == null)
                linkTable = new bool[elements.GetLength(0), elements.GetLength(1)];
            else if (linkTable[r, c])//已經遍歷過該位置了，返回false
                return false;

            //表示該位置已遍歷過
            linkTable[r, c] = true;

            if (batModel.ContainsDataAt(r, c, TileType.Obstacle) || batModel.ContainsDataAt(r, c, TileType.Cover))
                return false;

            TileObject element = elements[r, c];
            if (element == null)
                return false;

            Q.Assert(element != null);

            //消除物可能是ColorType.None，比如全屏炸彈
            if (element.Config.ColorType == ColorType.None)
                return false;

            if (color == ColorType.None)
                color = element.Config.ColorType;

            if (color != element.Config.ColorType)
                return false;

            //該位置的條件全部檢測成功，放入棧中
            if (returnLinkStack != null)
                returnLinkStack.Push(element);

            //只需檢查一個元素，返回true
            if (needLen == 1)
                return true;

            //檢查後續的元素，八個方向都要檢查
            //先檢查間隔物間隔，再檢查可連接性
            //TODO 後續改進算法，可以考慮用A*來改進效率
            if (CheckLinkableBySeperator(r, c, r, c + 1) && CheckLinkableAt(elements, r, c + 1, color, needLen - 1, returnLinkStack, linkTable))
                return true;
            if (CheckLinkableBySeperator(r, c, r + 1, c + 1) && CheckLinkableAt(elements, r + 1, c + 1, color, needLen - 1, returnLinkStack, linkTable))
                return true;
            if (CheckLinkableBySeperator(r, c, r + 1, c) && CheckLinkableAt(elements, r + 1, c, color, needLen - 1, returnLinkStack, linkTable))
                return true;
            if (CheckLinkableBySeperator(r, c, r + 1, c - 1) && CheckLinkableAt(elements, r + 1, c - 1, color, needLen - 1, returnLinkStack, linkTable))
                return true;
            if (CheckLinkableBySeperator(r, c, r, c - 1) && CheckLinkableAt(elements, r, c - 1, color, needLen - 1, returnLinkStack, linkTable))
                return true;
            if (CheckLinkableBySeperator(r, c, r - 1, c - 1) && CheckLinkableAt(elements, r - 1, c - 1, color, needLen - 1, returnLinkStack, linkTable))
                return true;
            if (CheckLinkableBySeperator(r, c, r - 1, c) && CheckLinkableAt(elements, r - 1, c, color, needLen - 1, returnLinkStack, linkTable))
                return true;
            if (CheckLinkableBySeperator(r, c, r - 1, c + 1) && CheckLinkableAt(elements, r - 1, c + 1, color, needLen - 1, returnLinkStack, linkTable))
                return true;

            //因為其他元素檢查失敗，需要把已經push的元素彈出
            if (returnLinkStack != null)
                returnLinkStack.Pop();
            return false;
        }



        /// <summary>
        /// 根據周圍的間隔物，檢查兩個位置可否連接
        /// </summary>
        /// <returns></returns>
        public bool CheckLinkableBySeperator(int row1, int col1, int row2, int col2)
        {
            if (row2 < 0 || row2 >= batModel.CrtLevelConfig.NumRow || col2 < 0 || col2 >= batModel.CrtLevelConfig.NumCol)
                return false;

            int minCol = Mathf.Min(col1, col2);
            int minRow = Mathf.Min(row1, row2);
            int maxCol = Mathf.Max(col1, col2);
            int maxRow = Mathf.Max(row1, row2);
            //兩個位置之間的四個間隔物
            //如果兩個位置是相鄰的，那麼有可能h1==h2，或v1==v2
            bool h1Flg = batModel.ContainsDataAt(minRow, minCol, TileType.SeperatorH);
            bool h2Flg = batModel.ContainsDataAt(minRow, maxCol, TileType.SeperatorH);
            bool v1Flg = batModel.ContainsDataAt(minRow, minCol, TileType.SeperatorV);
            bool v2Flg = batModel.ContainsDataAt(maxRow, minCol, TileType.SeperatorV);

            //同行
            if (minRow == maxRow)
            {
                return !v1Flg;
            }

            //同列
            if (minCol == maxCol)
            {
                return !h1Flg;
            }

            //兩個位置之間最多有四個間隔物
            int count = 0;
            if (h1Flg)
                count++;
            if (h2Flg)
                count++;
            if (v1Flg)
                count++;
            if (v2Flg)
                count++;

            if (count <= 1)
                return true;

            if (count >= 3)
                return false;

            //有且只有兩個間隔物的狀態比較複雜
            if (h1Flg && h2Flg)
                return false;

            if (v1Flg && v2Flg)
                return false;


            //上左下右
            if ((row1 == minRow && col1 == minCol) || (row1 == maxRow && col1 == maxCol))
            {
                if (v1Flg && h1Flg)
                    return false;

                if (v2Flg && h2Flg)
                    return false;
            }
            else//上右下左
            {
                if (v1Flg && h2Flg)
                    return false;

                if (v2Flg && h1Flg)
                    return false;
            }
            return true;
        }

        public void CalcLeftStepAward()
        {
            if (batModel.CheckLevelGoalCon())
            {
                if (!batModel.HasLeftStepAward())
                {
                    return;
                }
                for (int ii = 0; ii < batModel.RemainSteps; ii++)
                {
                    TileObject tObj = batModel.GetStepAwardTile();
                    if (tObj == null)
                    {
                        continue;
                    }
                    StepAnimationItem item = new StepAnimationItem(0);
                    GameObject ga = boardBeh.eleViews[tObj.Row, tObj.Col];
                    ElementBehaviour bomb = ga.GetComponent<ElementBehaviour>();
                    item.mainTile = ga;

                    Dictionary<ColorType, int> elements = new Dictionary<ColorType, int>();
                    if (bomb != null)
                    {
                        //把炸彈範圍內的消除物都添加進來
                        List<Position> elimRange = null;
                        List<int> elimOrders = null;
                        elementRuleCtr.CalcElimRangeAt(
                            new Position(bomb.Row, bomb.Col),
                            0, bomb.Config,
                            out elimRange,
                            out elimOrders
                        );

                        for (int i = 0, n = elimRange.Count; i < n; i++)
                        {
                            Position p = elimRange[i];
                            if (boardBeh.eleViews[p.Row, p.Col] != null)
                            {
                                item.ElimRangeTiles.Add(boardBeh.eleViews[p.Row, p.Col]);
                                item.ElimOrders.Add(elimOrders[i]);
                            }
                        }

                    }

                    foreach (GameObject gaTemp in item.ElimRangeTiles)
                    {
                        if (gaTemp == null)
                            continue;

                        BaseTileBehaviour beh = gaTemp.GetComponent<BaseTileBehaviour>();
                        if (beh.Data != null && beh.Config.ObjectType == TileType.Element)
                        {
                            batModel.AddStepAward(beh.Config.deliverAward.type, beh.Config.deliverAward.Qtt);
                            batModel.SetDataAt(null, beh.Row, beh.Col, TileType.Element);

                            if (elements.ContainsKey(beh.Config.ColorType))
                                elements[beh.Config.ColorType]++;
                            else
                                elements.Add(beh.Config.ColorType, 1);
                        }
                    }

                    ColorType mainColor = bomb != null ? bomb.Config.ColorType : ColorType.None;

                    item.EliminateScore = batModel.CalcLeftRewardScore(null, mainColor, elements);

                    GameController.Instance.PropCtr.AddScore(batModel, item.EliminateScore);

                    batModel.StepAwardList.Add(item);
                }
            }
        }


        public List<List<ModelEventSystem.Move>> Drop()
        {
            List<TileObject> beforeDatas = null;
            List<TileObject> afterDatas = null;
            return Drop(out beforeDatas, out afterDatas);
        }


        /// <summary>
        /// 執行元素掉落邏輯
        /// </summary>
        public List<List<ModelEventSystem.Move>> Drop(out List<TileObject> beforeDatas, out List<TileObject> afterDatas)
        {
            int count = 0;
            List<List<ModelEventSystem.Move>> allDrops = new List<List<ModelEventSystem.Move>>();
            int numRow = batModel.CrtLevelConfig.NumRow;
            int numCol = batModel.CrtLevelConfig.NumCol;
            beforeDatas = new List<TileObject>();
            afterDatas = new List<TileObject>();

            // 掉落中途是否消除了元素
            bool isEliminate = false;

            Action<Vector2, Vector2, Direction, List<ModelEventSystem.Move>, List<TileObject>, List<TileObject>> func =
                delegate(Vector2 from, Vector2 to, Direction d, List<ModelEventSystem.Move> list, List<TileObject> bDatas, List<TileObject> aDatas)
                {
                    int fromR = (int)from.x;
                    int fromC = (int)from.y;
                    int toR = (int)to.x;
                    int toC = (int)to.y;
                    ModelEventSystem.Move m = new ModelEventSystem.Move(fromR, fromC, d);
                    TileObject target = null;
                    if (batModel.ContainsDataAt(fromR, fromC, TileType.Element))
                    {
                        target = batModel.GetElementAt(fromR, fromC);
                        batModel.SetDataAt(target, toR, toC, TileType.Element);
                        batModel.SetDataAt(null, fromR, fromC, TileType.Element);
                    }
                    else if (batModel.ContainsDataAt(fromR, fromC, TileType.Obstacle))
                    {
                        target = batModel.GetDataAt(fromR, fromC, TileType.Obstacle);
                        // 收集物碰到收集器
                        if (target.Config.ID == collectorMap[toR, toC])
                        {
                            isEliminate = true;
                            // 標記為被發生收集的位置
                            m.collectPosition.Add(toR + "$" + toC, target.Config.ID);
                            batModel.SetDataAt(null, toR, toC, TileType.Obstacle);
                            bDatas.Add(target);
                            aDatas.Add(null);
                        }
                        else
                        {
                            batModel.SetDataAt(target, toR, toC, TileType.Obstacle);
                        }
                        batModel.SetDataAt(null, fromR, fromC, TileType.Obstacle);
                    }

                    target.Row = toR;
                    target.Col = toC;
                    m.NewValue = target;
                    list.Add(m);
                };

            while (true)
            {
                //所有可以掉落的元素掉落一格，認為是One Drop
                List<ModelEventSystem.Move> oneDrop = new List<ModelEventSystem.Move>();
                // 每個One Drop重新標記掉落中途沒有元素被消除
                isEliminate = false;

                //從底往上遍歷
                //最頂行（r==0）單獨處理
                for (int r = numRow - 1; r >= 0; r--)
                {
                    //每一行需要遍歷3遍

                    //正下方掉落
                    for (int c = 0; c < numCol; c++)
                    {
                        if (IsDropable(r, c, r + 1, c))
                            func(new Vector2(r, c), new Vector2(r + 1, c), Direction.D, oneDrop, beforeDatas, afterDatas);
                    }

                    if (dropDireFlg)
                    {
                        //向左下方掉落
                        for (int c = 0; c < numCol; c++)
                        {
                            if (!IsDropable(r, c, r + 1, c - 1))
                                continue;

                            //目標位置將會被直連填充，因此斜向填充
                            if (WouldBeStraightFill(r + 1, c - 1))
                                continue;

                            func(new Vector2(r, c), new Vector2(r + 1, c - 1), Direction.DL, oneDrop, beforeDatas, afterDatas);
                        }
                        //向右下方掉落
                        for (int c = 0; c < numCol; c++)
                        {
                            if (!IsDropable(r, c, r + 1, c + 1))
                                continue;

                            //目標位置將會被直連填充，因此斜向填充
                            if (WouldBeStraightFill(r + 1, c + 1))
                                continue;

                            func(new Vector2(r, c), new Vector2(r + 1, c + 1), Direction.DR, oneDrop, beforeDatas, afterDatas);
                        }
                    }
                    else
                    {
                        //向右下方掉落
                        for (int c = 0; c < numCol; c++)
                        {
                            if (!IsDropable(r, c, r + 1, c + 1))
                                continue;

                            //目標位置將會被直連填充，因此斜向填充
                            if (WouldBeStraightFill(r + 1, c + 1))
                                continue;

                            func(new Vector2(r, c), new Vector2(r + 1, c + 1), Direction.DR, oneDrop, beforeDatas, afterDatas);
                        }
                        //向左下方掉落
                        for (int c = 0; c < numCol; c++)
                        {
                            if (!IsDropable(r, c, r + 1, c - 1))
                                continue;

                            //目標位置將會被直連填充，因此斜向填充
                            if (WouldBeStraightFill(r + 1, c - 1))
                                continue;

                            func(new Vector2(r, c), new Vector2(r + 1, c - 1), Direction.DL, oneDrop, beforeDatas, afterDatas);
                        }
                    }
                    dropDireFlg = !dropDireFlg;
                }

                //遍歷最上面一行，檢查是否需要掉落新元素
                for (int c = 0; c < numCol; c++)
                {
                    if (IsDropable(-1, c, 0, c))
                    {
                        TileObject newValue = RandomElement(0, c);
                        oneDrop.Add(new ModelEventSystem.Move(-1, c, Direction.D, newValue));
                        if (newValue.Config.ObjectType == TileType.Element)
                            batModel.SetDataAt(newValue, 0, c, TileType.Element);
                        else if (newValue.Config.ObjectType == TileType.Obstacle)
                            batModel.SetDataAt(newValue, 0, c, TileType.Obstacle);


                        int objectId = newValue.ConfigID;
                        int num = batModel.GetTileNum(newValue.ConfigID);

                        foreach (RandomSeedConfig seed in batModel.CrtStageConfig.dynamicRandomSeeds.Values)
                        {
                            if (seed.ObjectNum > 0 && objectId == seed.ObjectId && num >= seed.ObjectNum)
                            {
                                batModel.TotalRandomSeed[objectId] = 0;
                            }
                        }
                    }
                }

                //Q.Log("count={0}, oneDrop.Count={1}", count, oneDrop.Count);
                //沒有東西可以掉落了
                if (oneDrop.Count == 0)
                    break;

                allDrops.Add(oneDrop);
                if (isEliminate)
                {
                    // 有元素在掉落中途被消除了，需要重新計算權重
                    batModel.ChangeRandomWeight();
                }
                Q.Assert(++count <= 50, "掉落計算可能死循環");
            }//while

            //ModelEventSystem.MoveEventArgs args = new ModelEventSystem.MoveEventArgs();
            //args.Moves = allDrops;
            ////Q.Log(args.ToString());
            //modelEvtSys.DispatchMoveEvent(this, args);
            return allDrops;
        }


        /// <summary>
        /// 目標位置可否**被**掉落
        /// </summary>
        /// <param name="fromRow"></param>
        /// <param name="fromCol"></param>
        /// <param name="toRow"></param>
        /// <param name="toCol"></param>
        /// <returns></returns>
        private bool IsDropable(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (toRow < 0 || toRow >= batModel.CrtLevelConfig.NumRow || toCol < 0 || toCol >= batModel.CrtLevelConfig.NumCol)
                return false;

            //起始位置允許為-1，表示為新掉落消除物
            if (fromRow < -1 || fromRow >= batModel.CrtLevelConfig.NumRow || fromCol < 0 || fromCol >= batModel.CrtLevelConfig.NumCol)
                return false;

            //目標位置有消除物
            if (batModel.ContainsDataAt(toRow, toCol, TileType.Element))
                return false;

            //目標位置已有障礙物
            if (batModel.ContainsDataAt(toRow, toCol, TileType.Obstacle) || batModel.ContainsDataAt(toRow, toCol, TileType.Cover))
                return false;

            if (fromRow == -1)
                return true;

            //源位置被覆蓋物覆蓋，無法掉落
            if (batModel.ContainsDataAt(fromRow, fromCol, TileType.Cover))
                return false;

            //源位置，消除物層沒有東西掉落
            bool noSrcEle = !batModel.ContainsDataAt(fromRow, fromCol, TileType.Element);
            //源位置，障礙物層沒有東西掉落，或者無法掉落           
            bool noSrcObs =
                !batModel.ContainsDataAt(fromRow, fromCol, TileType.Obstacle) ||
                !batModel.GetDataAt(fromRow, fromCol, TileType.Obstacle).Config.Dropable;

            if (noSrcEle && noSrcObs)
                return false;

            //被間隔物阻擋，無法掉落
            if (!CheckLinkableBySeperator(fromRow, fromCol, toRow, toCol))
                return false;

            return true;
        }

        /// <summary>
        /// 是否會被直連填充
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private bool WouldBeStraightFill(int row, int col)
        {
            Q.Assert(row >= 0 && row < batModel.CrtLevelConfig.NumRow);
            Q.Assert(col >= 0 && col < batModel.CrtLevelConfig.NumCol);

            if (batModel.ContainsDataAt(row, col, TileType.Element) &&
                batModel.ContainsDataAt(row, col, TileType.Cover))
            {
                return true;
            }

            for (int r = row - 1; r >= 0; r--)
            {
                //被障礙物阻擋，不會填充
                if (batModel.ContainsDataAt(r, col, TileType.Obstacle))
                {
                    TileObject ob = batModel.GetDataAt(r, col, TileType.Obstacle);
                    if (!ob.Config.Dropable)
                        return false;
                }

                //被間隔物阻擋，不會填充
                if (batModel.ContainsDataAt(r, col, TileType.SeperatorH))
                    return false;

                if (batModel.ContainsDataAt(r, col, TileType.Cover))
                    return false;

                //直接上方有消除物，可填充             
                if (batModel.ContainsDataAt(r, col, TileType.Element))
                    return true;
            }

            //直連到頂部，會生成新元素填充
            return true;
        }


        private TileObject RandomElement(int row, int col)
        {
            ///計算總權重
            int objectId = batModel.CreateRandomElement();
            //const int MinElementTileID = 1;
            //const int MaxElementTileID = 5;
            //int id = UnityEngine.Random.Range(MinElementTileID, MaxElementTileID + 1);
            if (!TileObjectConfigs.ContainsKey(objectId))
            {
                const int MinElementTileID = 1;
                const int MaxElementTileID = 5;
                int id = UnityEngine.Random.Range(MinElementTileID, MaxElementTileID + 1);
                objectId = id;
            }
            TileObjectConfig conf = TileObjectConfigs[objectId];
            TileObject ret = new TileObject(row, col, conf);
            return ret;
        }




        /// <summary>
        /// 全面檢查數據層和表現層是否一致
        /// </summary>
        public void __CheckModelAndView()
        {
#if UNITY_EDITOR
            Action<TileObject, BaseTileBehaviour> CheckSync = delegate(TileObject data, BaseTileBehaviour beh)
            {
                if (beh == null || data == null)
                {
                    if (!Q.Assert(beh == null && data == null, "{0},{1}", data != null, beh != null))
                    {
                        if (beh != null)
                            Q.Log("beh name: " + beh.name);
                        else if (data != null)
                            Q.Log("data: r={0}, c={1}, id={2}", data.Row, data.Col, data.ConfigID);
                    }
                }
                else
                {
                    //Q.Log("Check {0}", new Position(data.Row, data.Col));
                    Q.Assert(beh.Data == data);
                    Q.Assert(beh.Row == data.Row);
                    Q.Assert(beh.Col == data.Col);
                    Q.Assert(beh.Col == data.Col);
                }
            };

            int numRow = batModel.CrtLevelConfig.NumRow;
            int numCol = batModel.CrtLevelConfig.NumCol;
            for (int r = 0; r < numRow; r++)
            {
                for (int c = 0; c < numCol; c++)
                {
                    TileType[] types = {
                                           TileType.Cover,
                                           TileType.Element,
                                           TileType.Obstacle,
                                           TileType.SeperatorH,
                                           TileType.SeperatorV,
                                           TileType.Bottom,
                                       };
                    for (int i = 0, n = types.Length; i < n; i++)
                    {
                        TileType t = types[i];
                        TileObject data = batModel.GetDataAt(r, c, t);
                        GameObject[,] list = boardBeh.GetTypeGameObjects(t);
                        BaseTileBehaviour beh = list[r, c] == null ? null : list[r, c].GetComponent<BaseTileBehaviour>();
                        CheckSync(data, beh);
                    }
                }
            }
#endif
        }
    }//PlayingRuleCtr
}
