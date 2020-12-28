using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.TileBehaviour;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Ctr
{
    /// <summary>
    /// 處理棋盤的轉換規則
    /// TODO 把控制BombRange代碼放到外面去
    /// </summary>
    public class BoardModifyingRules : IDisposable
    {
        /// <summary>
        /// 單元測試模式
        /// </summary>
        public static bool UnitTestMode = false;

        public enum LinkType
        {
            Link,
            Unlink
        }

        private BattleModelModifyAgent batModel;

        private BoardBehaviour boardView;

        private Dictionary<int, TileObjectConfig> tileObjectConfigs;
        private ViewEventSystem viewEvtSys;
        private ModelEventSystem modelEvtSys;
        private ElementRuleCtr elementRuleCtr;

        public PlayingRuleCtr playingRuleCtr;

        private int numRow = 0;
        private int numCol = 0;


        public BoardModifyingRules(BoardBehaviour boardView,
                                   ElementRuleCtr elementRuleCtr)
        {
            GameController gc = GameController.Instance;
            batModel = gc.Model.BattleModel;
            tileObjectConfigs = gc.Model.TileObjectConfigs;
            viewEvtSys = gc.ViewEventSystem;
            modelEvtSys = gc.ModelEventSystem;
            numRow = batModel.CrtLevelConfig.NumRow;
            numCol = batModel.CrtLevelConfig.NumCol;
            this.elementRuleCtr = elementRuleCtr;
            this.boardView = boardView;
            RegisterEvent();
        }

        public BoardModifyingRules(BattleModelModifyAgent batModel,
                                   BoardBehaviour boardView,
                                   ElementRuleCtr elementRuleCtr,
                                   ViewEventSystem viewEvtSys,
                                   ModelEventSystem modelEvtSys,
                                   Dictionary<int, TileObjectConfig> tileObjectConfigs,
                                   int numRow,
                                   int numCol)
        {
            this.batModel = batModel;
            this.boardView = boardView;
            this.elementRuleCtr = elementRuleCtr;
            this.viewEvtSys = viewEvtSys;
            this.modelEvtSys = modelEvtSys;
            this.tileObjectConfigs = tileObjectConfigs;
            this.numRow = numRow;
            this.numCol = numCol;

            RegisterEvent();
        }


        public void Dispose()
        {
            UnregisterEvent();
        }

        /// <summary>
        /// 炸彈移動規則
        /// </summary>
        /// <param name="linkPath"></param>
        /// <param name="linkType"></param>
        public void BombMovingRule(Position oprPos, List<Position> linkPath, LinkType linkType)
        {
            if (linkPath == null)
            {
                Q.Assert(false, "BoardModifyingRules:BombMovingRule assert 1");
                return;
            }

            TileObject oprTileData = GetElementAt(oprPos);
            Q.Assert(oprTileData != null, "BombMovingRule: r={0},c={1}", oprPos.Row, oprPos.Col);
            bool isBomb = oprTileData.Config.ElementType == ElementType.Bomb;

            if (linkType == LinkType.Link)
            {
                //最後一個炸彈，如果為null，表示之前沒有連過炸彈
                TileObjectConfig lastPreBombConf = null;
                if (linkPath.Count > 1)
                {
                    Position p = linkPath[linkPath.Count - 2];
                    TileObject t = GetElementAt(p);
                    if (t != null && t.Config.ElementType == ElementType.Bomb)
                    {
                        lastPreBombConf = t.Config;
                    }
                }

                bool hasPreBomb = lastPreBombConf != null;
                List<TileObject> modifying = new List<TileObject>();
                if (hasPreBomb && isBomb)
                {
                    //把倒數第二個變成普通消除物
                    Position prePos = linkPath[linkPath.Count - 2];
                    TileObject preTile = GetElementAt(prePos);
                    //Q.Assert(preTile.Config.Script == ElementType.Bomb);
                    TileObjectConfig normalEleConf = tileObjectConfigs[(int)preTile.Config.ColorType];
                    TileObject changedPreTile = new TileObject(preTile);
                    changedPreTile.Config = normalEleConf;
                    modifying.Add(changedPreTile);
                }
                else if (hasPreBomb && !isBomb)
                {
                    //把倒數第二個變成普通消除物
                    Position prePos = linkPath[linkPath.Count - 2];
                    TileObject preTile = GetElementAt(prePos);
                    //Q.Assert(preTile.Config.Script == ElementType.Bomb);
                    TileObjectConfig normalEleConf = tileObjectConfigs[(int)lastPreBombConf.ColorType];
                    TileObject changedPreTile = new TileObject(preTile);
                    changedPreTile.Config = normalEleConf;
                    //把倒數第一個變成炸彈
                    TileObject crtTile = GetElementAt(oprPos);
                    TileObject changedCrtTile = new TileObject(crtTile);
                    changedCrtTile.Config = lastPreBombConf;

                    modifying.Add(changedPreTile);
                    modifying.Add(changedCrtTile);
                }
                else if (!hasPreBomb && isBomb)
                {
                    //do nothing
                }
                else
                {
                    //do nothing
                }

                //無論是否modifying.Count是否大於0，,都要提交
                //因為在unlink的時候，每unlink一個位置，都對應一次RevertModelAndSyncView()
                NewModifyingAndSyncView(modifying);
            }
            else if (linkType == LinkType.Unlink)
            {
                RevertModelAndSyncView();
            }
        }


        /// <summary>
        /// 轉換石規則
        /// </summary>
        public void ConverterRule(Position oprPos, List<Position> linkPath, LinkType linkType)
        {
            TileObject oprData = GetDataAt(oprPos, TileType.Element);
            Q.Assert(oprData != null);

            if (oprData.Config.ElementType != ElementType.ConvertBlock)
                return;

            if (linkType == LinkType.Link)
            {
                int targetID = (int)oprData.Config.ColorType;
                List<Position> range =
                    ElementRuleCtr.CalcElementRange(
                        oprData.Config,
                        oprPos,
                        true,
                        numRow,
                        numCol);

                if (range == null || range.Count == 0)
                    return;

                List<TileObject> changedElements = CovertNormalElement(range, targetID);

                //先把數據提交到數據層
                NewModifyingAndSyncView(changedElements);
            }
            else if (linkType == LinkType.Unlink)
            {
                //會退了一個轉換石，那麼就回退一層修改
                RevertModelAndSyncView();
            }
        }


        /// <summary>
        /// 炸彈級聯規則1：炸彈級聯到炸彈的轉換規則
        /// </summary>
        public void BombCascadeRule1(Position oprPos, List<Position> linkPath, LinkType linkType)
        {
            TileObject data = GetElementAt(oprPos);


            if (linkType == LinkType.Link)
            {
                List<TileObject> modifyingList = new List<TileObject>();
                if(data != null && data.Config.ElementType == ElementType.Bomb)
                {
                    //記錄哪些已經改變過了
                    bool[,] changedMap = new bool[numRow, numCol];
                    RecursiveExecBombCascadeRule1(oprPos, changedMap, data.Config.RangeMode, modifyingList);
                }
                
                NewModifyingAndSyncView(modifyingList);
            }
            else
            {
                RevertModelAndSyncView();
            }
        }




        /// <summary>
        /// 炸彈級聯規則2：炸彈級聯到轉換石的轉換規則
        /// </summary>
        public void BombCascadeRule2(Position oprPos, List<Position> linkPath, LinkType linkType)
        {
            TileObject data = GetElementAt(oprPos);

            if (linkType == LinkType.Link)
            {

                List<TileObject> modifyingList = new List<TileObject>();
                if (data != null && data.Config.ElementType == ElementType.Bomb)
                {
                    //當兩種不同顏色的轉換石，範圍重疊時，重疊部分用這個id的Tile填充
                    int overlapId = Convert.ToInt32(data.Config.ColorType);
                    List<Position> bombRange = elementRuleCtr.CalcElimRangeWithCascade(oprPos);

                    int[,] changedMap = new int[numRow, numCol];
                    for (int i = 0, n = bombRange.Count; i < n; i++)
                    {
                        Position p = bombRange[i];
                        TileObject tile = GetElementAt(p);
                        if (tile == null || tile.Config.ElementType != ElementType.ConvertBlock)
                            continue;
                        List<Position> convertRange = ElementRuleCtr.CalcElementRange(tile.Config, p, true, numRow, numCol);
                        int targetId = Convert.ToInt32(tile.Config.ColorType);
                        for (int j = 0, m = convertRange.Count; j < m; j++)
                        {
                            Position p2 = convertRange[j];
                            TileObject oriData = GetElementAt(p2);

                            //只轉換普通消除物
                            if (oriData == null || oriData.ConfigID > 5)
                                continue;

                            if (changedMap[p2.Row, p2.Col] == 0)
                            {
                                changedMap[p2.Row, p2.Col] = targetId;
                            }
                            else if (changedMap[p2.Row, p2.Col] != targetId)
                            {
                                changedMap[p2.Row, p2.Col] = overlapId;
                            }
                        }
                    }

                    for (int r = 0; r < numRow; r++)
                    {
                        for (int c = 0; c < numCol; c++)
                        {

                            if (changedMap[r, c] != 0)
                            {
                                TileObject newData = new TileObject(r, c, tileObjectConfigs[changedMap[r, c]]);
                                modifyingList.Add(newData);
                            }

                        }
                    }
                }

                NewModifyingAndSyncView(modifyingList);
            }
            else
            {
                RevertModelAndSyncView();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="changedMarkMap"></param>
        /// <param name="targetRangeMode">要轉成</param>
        /// <param name="modifyingList"></param>
        private void RecursiveExecBombCascadeRule1(Position startPos,
                                                   bool[,] changedMarkMap,
                                                   ElimRangeMode targetRangeMode,
                                                   List<TileObject> modifyingList)
        {
            if (!CheckElementMatchCascadeRule1(startPos, changedMarkMap))
                return;

            TileObject data = GetElementAt(startPos);
            TileObjectConfig conf = data.Config;
            TileObjectConfig changedConf = null;
            //是否是線性炸彈            
            bool isLinearBomb = conf.RangeMode == ElimRangeMode.Horizontal ||
                                conf.RangeMode == ElimRangeMode.Vertical;

            if (isLinearBomb && conf.RangeMode != targetRangeMode)
            {
                //橫向變縱向，縱向變橫向
                changedConf = tileObjectConfigs[Convert.ToInt32(conf.Arg3)];
                TileObject changedData = new TileObject(data);
                changedData.Config = changedConf;
                modifyingList.Add(changedData);
            }
            else
            {
                changedConf = conf;
            }
            changedMarkMap[startPos.Row, startPos.Col] = true;

            List<Position> range = ElementRuleCtr.CalcElementRange(changedConf, startPos, true, numRow, numCol);
            if (isLinearBomb)
            {
                ElimRangeMode nextRangeMode = targetRangeMode == ElimRangeMode.Vertical ? ElimRangeMode.Horizontal : ElimRangeMode.Vertical;
                for (int i = 0, n = range.Count; i < n; i++)
                {
                    Position p = range[i];
                    RecursiveExecBombCascadeRule1(p, changedMarkMap, nextRangeMode, modifyingList);
                }
            }
            else
            {
                //遇到了非線性炸彈，雖然不需要改方向，但是需要讓其次級的炸彈繼續變向行為
                for (int i = 0, n = range.Count; i < n; i++)
                {
                    Position p = range[i];
                    if (!CheckElementMatchCascadeRule1(p, changedMarkMap))
                        continue;

                    TileObject tile = GetElementAt(p);
                    ElimRangeMode nextRangeMode = targetRangeMode;

                    if (tile.Config.RangeMode == ElimRangeMode.Vertical)
                        nextRangeMode = ElimRangeMode.Horizontal;
                    else if (tile.Config.RangeMode == ElimRangeMode.Horizontal)
                        nextRangeMode = ElimRangeMode.Vertical;

                    RecursiveExecBombCascadeRule1(p, changedMarkMap, nextRangeMode, modifyingList);
                }
            }
        }


        /// <summary>
        /// 檢查某個位置的元素是否需要執行BombCascadeRule1
        /// </summary>
        /// <param name="p"></param>
        /// <param name="changedMarkMap"></param>
        /// <returns></returns>
        private bool CheckElementMatchCascadeRule1(Position p, bool[,] changedMarkMap)
        {
            TileObject data = GetElementAt(p);
            //不需要處理的條件
            //這個位置已經處理過
            bool notProcess = changedMarkMap[p.Row, p.Col];
            //該位置沒有消除物
            notProcess = notProcess || data == null;
            //不是炸彈，不需要處理
            notProcess = notProcess || data.Config.ElementType != ElementType.Bomb;

            return !notProcess;
        }



        private TileObject GetDataAt(Position pos, TileType type)
        {
            return batModel.GetDataAt(pos.Row, pos.Col, type);
        }

        private TileObject GetElementAt(Position p)
        {
            return batModel.GetElementAt(p.Row, p.Col);
        }


        protected BaseTileBehaviour GetViewAt(Position pos, TileType type)
        {
            GameObject ga = null;
            switch (type)
            {
                case TileType.Element:
                    ga = boardView.eleViews[pos.Row, pos.Col];
                    break;
                case TileType.Obstacle:
                    ga = boardView.obstacleViews[pos.Row, pos.Col];
                    break;
                case TileType.Cover:
                    ga = boardView.obstacleViews[pos.Row, pos.Col];
                    break;
                case TileType.SeperatorH:
                    ga = boardView.seperatorHViews[pos.Row, pos.Col];
                    break;
                case TileType.SeperatorV:
                    ga = boardView.seperatorVViews[pos.Row, pos.Col];
                    break;
                case TileType.Bottom:
                    ga = boardView.bottomViews[pos.Row, pos.Col];
                    break;
            }

            if (ga != null)
                return ga.GetComponent<BaseTileBehaviour>();
            else
                return null;
        }



        /// <summary>
        /// 把範圍內的普通消除物，轉換為目標消除物
        /// </summary>
        /// <param name="range"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        private List<TileObject> CovertNormalElement(List<Position> range, int targetID)
        {
            List<TileObject> ret = new List<TileObject>();
            TileObjectConfig targetConfig = tileObjectConfigs[targetID];
            for (int i = 0, n = range.Count; i < n; i++)
            {
                Position pos = range[i];

                //有覆蓋物的位置，轉換石不起作用
                if (batModel.ContainsDataAt(pos.Row, pos.Col, TileType.Cover))
                    continue;

                TileObject oriTile = GetElementAt(pos);

                if (oriTile == null ||
                    oriTile.Config.ID > 5 || //只轉換普通消除物
                    oriTile.Config.ID == targetID)//id相同
                {
                    continue;
                }

                ret.Add(new TileObject(pos.Row, pos.Col, targetConfig));
            }
            return ret;
        }


        private void NewModifyingAndSyncView(List<TileObject> modifying)
        {
            batModel.NewModifying(modifying);
            for (int i = 0, n = modifying.Count; i < n; i++)
            {
                TileObject data = modifying[i];
                ChangeElementView(data.Row, data.Col, data);

            }
        }


        private List<Position> RevertModelAndSyncView()
        {
            List<Position> ret = batModel.RevertOneLayerModifying();
            for (int i = 0, n = ret.Count; i < n; i++)
            {
                Position pos = ret[i];
                TileObject data = GetElementAt(pos);
                ChangeElementView(pos.Row, pos.Col, data);
            }
            return ret;
        }

        private void ChangeElementView(int r, int c, TileObject data)
        {
            if (boardView != null)
            {
                boardView.eleViews[r, c].GetComponent<ElementBehaviour>().Data = data;
            }
            else
            {
                Q.Log("ChangeElemetnView:r={0},c={1},id={2}", r, c, data.ConfigID);
            }
        }

        private void SetViewColorProminent(ColorType color)
        {
            if (boardView != null)
            {
                boardView.SetProminentElement(color);
            }
            else
            {
                Q.Log("SetViewColorProminent:c={0}", color);
            }
        }

        /// <summary>
        /// 顯示某個炸彈的範圍
        /// </summary>
        /// <param name="bomb"></param>
        private void DisplayBombRange(TileObject bomb)
        {
            Position targetPos = new Position(bomb.Row, bomb.Col);
            List<Position> linkPath = new List<Position>();
            linkPath.Add(new Position(bomb.Row, bomb.Col));
            List<Position> retElimPath = elementRuleCtr.CalcElimRangeWithCascade(targetPos);
            if (viewEvtSys != null && viewEvtSys.ControlSkillRangeEvent != null)
                viewEvtSys.ControlSkillRangeEvent(1, retElimPath);
        }


        /// <summary>
        /// 隱藏所有炸彈範圍
        /// </summary>
        private void HideAllBombRange()
        {
            if (viewEvtSys != null && viewEvtSys.ControlSkillRangeEvent != null)
                viewEvtSys.ControlSkillRangeEvent(2, null);
        }


        private void RegisterEvent()
        {
            if (viewEvtSys != null)
                viewEvtSys.TileStatusChangeEvent += OnTileStatusChange;

            if (modelEvtSys != null)
                modelEvtSys.OnBoardEliminate += OnModelEliminate;
        }


        private void UnregisterEvent()
        {
            if (viewEvtSys != null)
                viewEvtSys.TileStatusChangeEvent -= OnTileStatusChange;

            if (modelEvtSys != null)
                modelEvtSys.OnBoardEliminate -= OnModelEliminate;
        }

        private void OnTileStatusChange(TileObject tile,
                                        ViewEventSystem.TileStatusChangeMode mode,
                                        List<Position> linkPath)
        {
            if (tile == null || tile.Config.ObjectType != TileType.Element)
                return;

            if (mode == ViewEventSystem.TileStatusChangeMode.ChangeData)
                return;

            LinkType lt = LinkType.Link;
            Position p = new Position(tile.Row, tile.Col);

            HideAllBombRange();
            if (mode == ViewEventSystem.TileStatusChangeMode.ToLink)
            {
                lt = LinkType.Link;
                //Q.Log("----- OnTileStatusChange: p={0}, l={1}", p, lt);
                //注意順序
                BombMovingRule(p, linkPath, lt);
                playingRuleCtr.__CheckModelAndView();

                ConverterRule(p, linkPath, lt);
                playingRuleCtr.__CheckModelAndView();

                BombCascadeRule1(p, linkPath, lt);
                playingRuleCtr.__CheckModelAndView();

                //if (linkPath.Count >= 3)
                //{
                //    //只有連接隊列大於3時，才會考慮這條規則
                //    BombCascadeRule2(p, linkPath, lt);
                //    playingRuleCtr.__CheckModelAndView();
                //}
            }
            else if (mode == ViewEventSystem.TileStatusChangeMode.ToUnlink)
            {
                lt = LinkType.Unlink;
                //Q.Log("----- OnTileStatusChange: p={0}, l={1}", p, lt);
                //link的順序和unlink的處理時相反的

                //if (linkPath.Count >= 2)
                //{
                //    //linkPath.Count==2，表示原來的隊列是3
                //    //只有連接隊列大於3時，才會考慮這條規則
                //    BombCascadeRule2(p, linkPath, lt);
                //    playingRuleCtr.__CheckModelAndView();
                //}

                BombCascadeRule1(p, linkPath, lt);
                playingRuleCtr.__CheckModelAndView();

                ConverterRule(p, linkPath, lt);
                playingRuleCtr.__CheckModelAndView();

                BombMovingRule(p, linkPath, lt);
                playingRuleCtr.__CheckModelAndView();
            }

            
            if (linkPath.Count > 0)
            {
                //當前機制決定，炸彈只會是linkPath的最後一個
                Position lastPos = linkPath[linkPath.Count - 1];
                TileObject lastTile = GetElementAt(lastPos);
                if (lastTile != null & lastTile.Config.ElementType == ElementType.Bomb)
                {
                    DisplayBombRange(lastTile);
                }

                // main color由第一個元素決定
                Position firstPos = linkPath[0];
                SetViewColorProminent(GetElementAt(firstPos).Config.ColorType);
            }
        }


        private void OnModelEliminate(ModelEventSystem.EliminateEventArgs e)
        {
            //在消除生效的時候，隱藏炸彈範圍特效
            HideAllBombRange();
        }
    }
}
