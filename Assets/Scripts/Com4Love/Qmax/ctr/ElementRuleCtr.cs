using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Ctr
{
    /// <summary>
    /// 消除物玩法類。
    /// </summary>
    /// 
    /// 概念
    /// 消除（Eliminate）
    /// • 針對目標是“位置”，而不是具體的地形物
    /// • 手指連接了消除物，引爆了消除物，則會在消除物消除的範圍內引發“消除”
    /// • 間隔物對“消除”行為不敏感，只對“影響”行為敏感
    /// • “消除”傳遞的優先級：覆蓋物，障礙物，消除物，底層物
    /// • 特殊規則：當消除物被“消除”時，對應的底層物也會收到1傷害。
    /// 消除範圍（EliminateRange）
    /// • 消除物的“消除範圍”，其內容是一連串位置信息
    /// • 普通消除物的消除範圍是當前位置，炸彈類是一個區域（多個位置）
    /// 影響（Affect）
    /// • 上下左右的相鄰消除物被“消除”，則當前位置會收到“影響”
    /// • 當且僅當消除物被破壞，才會觸發”影響“。
    /// • 影響行為的傷害值始終為1。
    /// • “影響”是有傳播方向的，而間隔物會格擋“影響”
    /// • 需要區分原先版本和當前版本此概念的變化：原來版本炸彈的範圍是歸入“影響範圍”，現在並不是，炸彈的範圍歸為“消除範圍”
    /// • 對“影響”敏感的地形物：
    /// ○ 間隔物
    /// ○ 障礙物
    /// ○ 覆蓋物
    /// • 對“影響”不敏感的地形物：
    /// ○ 消除物
    /// ○ 底層物
    /// ○ 爪子
    /// 傷害值（HurtValue）
    /// • “消除”、“影響”、“底層影響”都有伴隨的傷害值
    /// • 傷害值會按照以下優先級，遞減傳遞：覆蓋物，間隔物，障礙物，消除物，底層物
    /// 連接路徑（LinkPath）：玩家用手指經過的連線
    /// 消除路徑（EliminatePath）：
    /// • = 連接路徑 + 消除範圍 + 去重操作
    /// • 對比“消除路徑”和原來的EliminateQueue、AffectQueue：“消除路徑”的元素是位置(x,y)，而原來的隊列是地形物
    /// • UI層還需要消除順序
    /// 消除順序（EliminateOrder）
    /// 影響順序（AffectOrder）
    /// 轉換範圍（CovertRange）：針對轉換石的一個概念
    public class ElementRuleCtr
    {
        /// <summary>
        /// 單元測試模式
        /// </summary>
        public static bool UnitTestMode = false;

        private BattleModel batModel;

        /// <summary>
        /// TileObject的配置表
        /// </summary>
        private Dictionary<int, TileObjectConfig> tileObjConfDict;

        private int numRow = 0;
        private int numCol = 0;



        /// <summary>
        /// 獲取一個消除物的範圍（消除範圍/轉換範圍）
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="pos">起始位置</param>
        /// <param name="checkBound">是否做邊界檢測</param>
        /// <param name="numRow"></param>
        /// <param name="numCol"></param>
        /// <returns></returns>
        public static List<Position> CalcElementRange(TileObjectConfig conf,
                                                      Position pos,
                                                      bool checkBound = false,
                                                      int numRow = 7,
                                                      int numCol = 7)
        {
            List<Position> ret = new List<Position>();

            if (conf == null || conf.ObjectType != TileType.Element)
            {
                Q.Assert(false, "ElementRuleCtr:CalcElementRange assert 1");
                return null;
            }


            //做邊界檢測，並把通過的加入結果中
            Func<int, int, bool> checkBounds = delegate (int r, int c)
            {
                if (checkBound)
                    return r >= 0 && c >= 0 && r < numRow && c < numCol;
                else
                    return true;
            };

            int row = pos.Row;
            int col = pos.Col;

            ret.Add(pos);

            //非炸彈，“消除範圍”為當前位置
            if (conf.ElementType == ElementType.Normal)
                return ret;

            ElimRangeMode rangeMode = conf.RangeMode;
            int radius = Convert.ToInt32(conf.Arg1);
            switch (rangeMode)
            {
                case ElimRangeMode.Horizontal:
                    for (int i = 1; i <= radius; i++)
                    {
                        //左
                        if (checkBounds(row, col - i))
                        {
                            ret.Add(new Position(row, col - i));
                        }
                        //右
                        if (checkBounds(row, col + i))
                        {
                            ret.Add(new Position(row, col + i));
                        }
                    }
                    break;
                case ElimRangeMode.Vertical:
                    for (int i = 1; i <= radius; i++)
                    {
                        //上
                        if (checkBounds(row - i, col))
                        {
                            ret.Add(new Position(row - i, col));
                        }
                        //下
                        if (checkBounds(row + i, col))
                        {
                            ret.Add(new Position(row + i, col));
                        }
                    }
                    break;
                case ElimRangeMode.Rect:
                case ElimRangeMode.Diamond:
                    for (int offsetR = -radius; offsetR <= radius; offsetR++)
                    {
                        for (int offsetC = -radius; offsetC <= radius; offsetC++)
                        {
                            //忽略當前位置
                            if (offsetR == 0 && offsetC == 0)
                                continue;

                            //菱形區域炸與方形區域不同的地方
                            if (rangeMode == ElimRangeMode.Diamond &&
                                Math.Abs(offsetR) + Math.Abs(offsetC) > radius)
                            {
                                continue;
                            }

                            if (checkBounds(row + offsetR, col + offsetC))
                            {
                                ret.Add(new Position(row + offsetR, col + offsetC));
                            }
                        }
                    }
                    break;
            }//switch
            return ret;
        }


        public ElementRuleCtr(BattleModelModifyAgent batModel,
                              Dictionary<int, TileObjectConfig> tileObjConfDict,
                              int numRow,
                              int numCol)
        {
            this.tileObjConfDict = tileObjConfDict;
            this.batModel = batModel;
            this.numRow = numRow;
            this.numCol = numCol;
        }

        /// <summary>
        /// 提交連接路徑，返回所有被修改的地形物數據，包括修改前的狀態和修改後的狀態
        /// </summary>
        /// <param name="linkPath"></param>
        /// <param name="retOriDatas"></param>
        /// <param name="retNewDatas"></param>
        /// <param name="retOrders"></param>
        /// <param name="retElimRewards">消除獎勵</param>
        /// <param name="retElimRewards"></param>`
        /// <param name="extraAtkPos"></param>
        /// <param name="extraAtkColor"></param>
        /// <param name="extraAtkOrders"></param>
        public void Eliminate(List<Position> linkPath,
                              out List<TileObject> retOriDatas,
                              out List<TileObject> retNewDatas,
                              out List<int> retOrders,
                              out List<ItemQtt[]> retElimRewards)
        {            
            List<Position> extraAtkPos = null;
            List<ColorType> extraAtkColor = null;
            List<int> extraAtkOrders = null;
            Eliminate(linkPath,
                null, 
                true,
                out retOriDatas,
                out retNewDatas,
                out retOrders,
                out retElimRewards,
                out extraAtkPos, 
                out extraAtkColor, 
                out extraAtkOrders
            );
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="linkPath"></param>
        /// <param name="initHurts"></param>
        /// <param name="changeModel">是否要修改數據層</param>
        /// <param name="retOriDatas"></param>
        /// <param name="retNewDatas"></param>
        /// <param name="retOrders"></param>
        /// <param name="retElimRewards"></param>
        /// <param name="extraAtkPos"></param>
        /// <param name="extraAtkColor"></param>
        /// <param name="extraAtkOrders"></param>
        public void Eliminate(List<Position> linkPath,
                              List<int> initHurts,
                              bool changeModel,
                              out List<TileObject> retOriDatas,
                              out List<TileObject> retNewDatas,
                              out List<int> retOrders,
                              out List<ItemQtt[]> retElimRewards,
                              out List<Position> extraAtkPos,
                              out List<ColorType> extraAtkColor,
                              out List<int> extraAtkOrders)
        {
            List<Position> elimPath = null;
            List<int> elimOrders = null;
            List<int> elimHurtValues = null;
            CalcElimPathByLinkPath(linkPath, out elimPath, out elimOrders);

            List<int> elimPathInitHurts = null;
            if (initHurts != null)
            {
                elimPathInitHurts = new List<int>();
                for (int i = 0, n = elimPath.Count; i < n; i++)
                {
                    int idx = linkPath.IndexOf(elimPath[i]);
                    elimPathInitHurts.Add(idx >= 0 ? initHurts[idx] : 0);
                }
            }
            //消除路徑中，各個位置的傷害
            elimHurtValues = CalcPositionHurtByElim(elimPath, elimPathInitHurts);
            Q.Assert(elimPath.Count == elimHurtValues.Count, "ElementRuleCtr:Eliminate assert 1");
            Q.Assert(elimPath.Count == elimOrders.Count, "ElementRuleCtr:Eliminate assert 2");

            Dictionary<TileType, int[,]> hurtMap = new Dictionary<TileType, int[,]>();
            Dictionary<TileType, int[,]> orderMap = new Dictionary<TileType, int[,]>();
            for (int i = 0, n = elimPath.Count; i < n; i++)
            {
                Position srcPos = elimPath[i];
                int srcOrder = elimOrders[i];

                //計算“消除”行為
                Dictionary<TileType, int> ret = CalcTileHurtByElim(srcPos, elimHurtValues[i]);
                foreach (var pair in ret)
                {
                    TileType t = pair.Key;
                    if (!hurtMap.ContainsKey(t))
                    {
                        hurtMap.Add(t, new int[numRow, numCol]);
                        orderMap.Add(t, new int[numRow, numCol]);
                    }
                    hurtMap[t][srcPos.Row, srcPos.Col] = pair.Value;
                    orderMap[t][srcPos.Row, srcPos.Col] = srcOrder;
                }

                //計算“影響”行為
                //只有有消除物被消除了，才會觸發了“影響行為”
                if (hurtMap.ContainsKey(TileType.Element) &&
                    hurtMap[TileType.Element][srcPos.Row, srcPos.Col] > 0)
                {
                    Dictionary<TileType, Dictionary<Position, int>> ret2 =
                        CalcTileHurtByAffect(srcPos, 1);

                    foreach (var pair in ret2)
                    {
                        TileType t = pair.Key;
                        if (!hurtMap.ContainsKey(t))
                        {
                            hurtMap.Add(t, new int[numRow, numCol]);
                            orderMap.Add(t, new int[numRow, numCol]);
                        }

                        foreach (var pair2 in pair.Value)
                        {
                            Position p = pair2.Key;
                            //傷害在同一位置是疊加
                            hurtMap[t][p.Row, p.Col] += pair2.Value;

                            //傷害，在同一位置是取最大值
                            //if (hurtMap[t][p.Row, p.Col] < pair2.Value)
                            //    hurtMap[t][p.Row, p.Col] = pair2.Value;

                            //被“影響”的地形物，EliminateOrder至少是1
                            if (orderMap[t][p.Row, p.Col] == 0)
                                orderMap[t][p.Row, p.Col] = srcOrder + 1;
                            else
                                orderMap[t][p.Row, p.Col] = Math.Min(orderMap[t][p.Row, p.Col], srcOrder + 1);
                        }
                    }
                }
            }

            CalcElimResult(hurtMap,
                orderMap,
                out retOriDatas,
                out retNewDatas,
                out retOrders,
                out retElimRewards);
            Q.Assert(retOriDatas.Count == retNewDatas.Count);
            //計算額外攻擊
            CalcExtraAtk(retOriDatas, retOrders, out extraAtkPos, out extraAtkColor, out extraAtkOrders);

            if(changeModel)
            {
                //提交結果到數據層
                for (int i = 0, n = retOriDatas.Count; i < n; i++)
                {
                    TileObject oriData = retOriDatas[i];
                    TileObject newData = retNewDatas[i];
                    //更改數據層
                    ChangeModelData(new Position(oriData.Row, oriData.Col), oriData, newData);
                }
            }


            //for test
            //for (int i = 0, n = retOriDatas.Count; i < n; i++)
            //{
            //    Q.Assert(retOriDatas[i] != null, i.ToString());
            //    Q.Assert(retOriDatas[i].Config != null, i.ToString());

            //    Debug.LogFormat("p={0}, t={1}, o={2}, oriId={3}, newId={4}",
            //        new Position(retOriDatas[i].Row, retOriDatas[i].Col),
            //        retOriDatas[i].Config.ObjectType,
            //        retOrders[i],
            //        retOriDatas[i].Config.ID,
            //        retNewDatas[i] != null ? retNewDatas[i].ConfigID : -1);
            //}
            //for test end
        }


        /// <summary>
        /// 計算出單個位置的消除物的“消除範圍”，並且考慮級聯的情況
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        public List<Position> CalcElimRangeWithCascade(Position pos)
        {
            List<Position> ret = new List<Position>();
            List<Position> linkPath = new List<Position>();
            List<int> retOrders = new List<int>();
            linkPath.Add(pos);

            //初始的消除順序，就是連接順序
            List<int> orders = new List<int>(linkPath.Count);
            for (int i = 0, n = linkPath.Count; i < n; i++)
            {
                orders.Add(i);
            }
            //計算出消除路徑
            RecursiveCalcElimPath(linkPath, orders, ret, retOrders);

            return ret;
        }


        /// <summary>
        /// 計算出單個位置的消除物的“消除範圍”
        /// 這個函數可以計算出消除順序
        /// </summary>
        /// <param name="pos">位置</param>
        /// <param name="elimOrder">消除順序</param>
        /// <param name="conf">地形物（要保證一定是消除物）</param>
        /// <param name="retElimRanges">返回的消除範圍</param>
        /// <param name="retElimOrder">消除範圍裡，每個元素的消除順序</param>
        public void CalcElimRangeAt(Position pos,
                                    int elimOrder,
                                    TileObjectConfig conf,
                                    out List<Position> retElimRanges,
                                    out List<int> retElimOrders)
        {
            retElimRanges = null;
            retElimOrders = null;
            if (conf == null || conf.ObjectType != TileType.Element)
            {
                Q.Assert(false, "ElementRuleCtr:CalcEliminateRangeAt assert 1");
                return;
            }

            //非炸彈，“消除範圍”為當前位置
            if (conf.ElementType == ElementType.Normal || conf.ElementType == ElementType.ConvertBlock || conf.ElementType == ElementType.MultiColor)
            {
                retElimRanges = new List<Position>();
                retElimOrders = new List<int>();
                retElimOrders.Add(elimOrder);
                retElimRanges.Add(pos);
            }
            else
            {
                Q.Assert(conf.ElementType == ElementType.Bomb, "ElementRuleCtr:CalcEliminateRangeAt assert 2");

                retElimRanges = CalcElementRange(conf, pos, true, numRow, numCol);
                retElimOrders = new List<int>();
                //根據距離，計算消除order
                for (int i = 0, n = retElimRanges.Count; i < n; i++)
                {
                    Position newP = retElimRanges[i];
                    int deltaOrder = Math.Abs(newP.Row - pos.Row) + Math.Abs(newP.Col - pos.Col);
                    retElimOrders.Add(elimOrder + deltaOrder);
                }
            }
        }

        /// <summary>
        /// 根據“連接路徑”計算出“消除路徑”
        /// </summary>
        /// <param name="linkPath">連接路徑</param>
        /// <param name="retElimPath">返回的消除路徑</param>
        /// <param name="retElimOrders">返回的對應的消除順序</param>
        /// <param name="retHurtValues">返回的對應位置的傷害值</param>
        /// <returns></returns>
        public void CalcElimPathByLinkPath(List<Position> linkPath,
                                           out List<Position> retElimPath,
                                           out List<int> retElimOrders)
        {
            retElimPath = new List<Position>();
            retElimOrders = new List<int>();

            //初始的消除順序，就是連接順序
            List<int> orders = new List<int>(linkPath.Count);
            for (int i = 0, n = linkPath.Count; i < n; i++)
            {
                orders.Add(i);
            }
            //計算出消除路徑
            RecursiveCalcElimPath(linkPath, orders, retElimPath, retElimOrders);
        }//CalcEliminatePath


        /// <summary>
        /// 給定一個路徑，遞歸地算出這個路徑擴展出來的消除路徑
        /// </summary>
        /// <param name="path"></param>
        /// <param name="orders"></param>
        /// <param name="retPath"></param>
        /// <param name="retOrders"></param>
        private void RecursiveCalcElimPath(List<Position> path,
                                           List<int> orders,
                                           List<Position> retPath,
                                           List<int> retOrders)
        {
            List<Position> nextPath = new List<Position>();
            List<int> nextOrders = new List<int>();
            for (int i = 0, n = path.Count; i < n; i++)
            {
                Position pos = path[i];

                //該位置已經添加到結果中，不重複添加
                if (retPath.Contains(pos))
                    continue;


                //添加到retPath中，說明已經計算過該位置的EliminateRange
                retPath.Add(pos);
                retOrders.Add(orders[i]);

                TileObject element = GetTileAt(pos, TileType.Element);
                if (element == null)
                {
                    continue;
                }

                //計算出消除範圍
                List<Position> elimRange = null;
                List<int> elimOrders = null;
                CalcElimRangeAt(pos, orders[i], element.Config, out elimRange, out elimOrders);
                for (int j = 0, m = elimRange.Count; j < m; j++)
                {
                    Position pos2 = elimRange[j];
                    int idx = retPath.IndexOf(pos2);

                    //在retPath中的，表示已經被調用過CalcEliminateRangeAt()
                    //在path中，表示在這一輪就會被調用CalcEliminateRangeAt()
                    //所以都要跳過，不放在newPath裡

                    if (idx >= 0)
                    {
                        //已經有了，但order值不同，那麼只更新order值，只取小值
                        if (retOrders[idx] > elimOrders[j])
                            retOrders[idx] = elimOrders[j];

                        continue;
                    }

                    if (path.IndexOf(pos2) >= 0)
                        continue;

                    nextPath.Add(pos2);
                    nextOrders.Add(elimOrders[j]);
                }
            }

            if (nextPath.Count > 0)
            {
                RecursiveCalcElimPath(nextPath, nextOrders, retPath, retOrders);
            }
        }



        /// <summary>
        ///根據消除路徑，計算出每個位置需要承受的傷害值
        /// </summary>
        /// <param name="eliminatePath"></param>
        /// <param name="initHurts">初始的傷害分佈</param>
        /// <returns></returns>
        private List<int> CalcPositionHurtByElim(List<Position> eliminatePath,
                                                 List<int> initHurts = null)
        {
            int[,] hurtValueMap = new int[numRow, numCol];
            //根據initHurts，設置初始傷害值
            for (int i = 0, n = eliminatePath.Count; initHurts != null && i < n; i++)
            {
                Position p = eliminatePath[i];
                hurtValueMap[p.Row, p.Col] = initHurts[i];
            }

            for (int i = 0, n = eliminatePath.Count; i < n; i++)
            {
                Position p = eliminatePath[i];
                TileObject tile = GetTileAt(p, TileType.Element);

                if (tile == null)
                {
                    continue;
                }

                List<Position> retElimRange = null;
                List<int> retOrders = null;
                CalcElimRangeAt(p, 0, tile.Config, out retElimRange, out retOrders);
                int hurt = Convert.ToInt32(tile.Config.Arg2);
                for (int j = 0, m = retElimRange.Count; j < m; j++)
                {
                    Position p2 = retElimRange[j];
                    //傷害在同一位置是疊加
                    hurtValueMap[p2.Row, p2.Col] += hurt;

                    //傷害，在同一位置是取最大值
                    //if (hurtValueMap[p2.Row, p2.Col] < hurt)
                    //    hurtValueMap[p2.Row, p2.Col] = hurt;
                }
            }

            List<int> ret = new List<int>();
            for (int i = 0, n = eliminatePath.Count; i < n; i++)
            {
                Position p = eliminatePath[i];
                ret.Add(hurtValueMap[p.Row, p.Col]);
            }

            return ret;
        }


        /// <summary>
        /// 在某位置發起“消除”行為後，這個位置所有的地形物各自傷害的傷害
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="hurtValue"></param>
        /// <returns></returns>
        private Dictionary<TileType, int> CalcTileHurtByElim(Position pos, int hurtValue)
        {
            Dictionary<TileType, int> ret = new Dictionary<TileType, int>();

            //消除優先級：覆蓋物，障礙物，消除物，地形物
            //特殊規則：地形物會隨著消除物的消除，而消1層
            TileType[] types = { TileType.Cover, TileType.Obstacle, TileType.Element };
            //是否消除了消除物
            bool breakElement = false;
            for (int i = 0, n = types.Length; i < n && hurtValue > 0; i++)
            {
                TileType t = types[i];
                TileObject oriData = GetTileAt(pos, t);
                if (oriData != null)
                {
                    int tileHurt = MinusHurtByTile(oriData, ref hurtValue);
                    if (tileHurt > 0)
                    {
                        if (t == TileType.Element)
                        {
                            breakElement = true;
                        }
                        ret.Add(t, tileHurt);
                    }
                }
            }

            //單獨處理地形物
            if (hurtValue > 0 || breakElement)
            {
                //消除了消除物，對地形物有連帶作用
                if (breakElement)
                    hurtValue++;

                if (hurtValue > 0)
                {
                    //如果已經破壞了一個消除物，那麼在破壞底層物時，攻擊力+1
                    TileObject oriData = GetTileAt(pos, TileType.Bottom);
                    if (oriData != null && oriData.Config.Level != -1)
                    {
                        int tileHurt = MinusHurtByTile(oriData, ref hurtValue);
                        if (tileHurt > 0)
                            ret.Add(TileType.Bottom, tileHurt);
                    }
                }
            }

            return ret;
        }


        /// <summary>
        /// 計算多色石造成的額外攻擊
        /// </summary>
        /// <param name="oriDatas"></param>
        /// <param name="elimOrders"></param>
        /// <param name="mainColor"></param>
        /// <param name="extraAtkPos"></param>
        /// <param name="extraAtkColor"></param>
        /// <param name="extraAtkOrders"></param>
        private void CalcExtraAtk(List<TileObject> oriDatas,
                                  List<int> elimOrders,
                                  out List<Position> extraAtkPos,
                                  out List<ColorType> extraAtkColor,
                                  out List<int> extraAtkOrders)
        {
            extraAtkPos = new List<Position>();
            extraAtkColor = new List<ColorType>();
            extraAtkOrders = new List<int>();

            //遍歷oriDatas，看看哪些是多色石，如果是多色石，就會造成額外攻擊
            for (int i = 0, n = oriDatas.Count; i < n; i++)
            {
                if(oriDatas[i].Config.ElementType == ElementType.MultiColor)
                {
                    TileObject ta = oriDatas[i];
                    for(int j = 0, m = ta.Config.AllColors.Count;j< m;j++)
                    {
                        extraAtkPos.Add(new Position(ta.Row, ta.Col));
                        extraAtkColor.Add(ta.Config.AllColors[j]);
                        extraAtkOrders.Add(elimOrders[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 某個位置發起“消除”行為後，周圍地形物受到“影響”造成的傷害值
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="hurtValue"></param>
        /// <returns></returns>
        private Dictionary<TileType, Dictionary<Position, int>> CalcTileHurtByAffect(Position pos, int hurtValue)
        {
            Dictionary<TileType, Dictionary<Position, int>> ret = new Dictionary<TileType, Dictionary<Position, int>>();

            //依次檢測間隔物、覆蓋物、障礙物
            Direction[] dires = { Direction.U, Direction.D, Direction.L, Direction.R };
            TileType[] types = { TileType.SeperatorH, TileType.SeperatorV, TileType.Cover, TileType.Obstacle };
            for (int i = 0, n = dires.Length; i < n; i++)
            {
                Direction dire = dires[i];
                for (int j = 0, m = types.Length, h = hurtValue; j < m && h > 0; j++)
                {
                    TileType t = types[j];
                    TileObject oldData = GetTileAt(pos, dire, t, 1);

                    if (oldData == null)
                        continue;

                    if (oldData.Config.Level == -1)
                    {
                        //level=-1，會吸收所有傷害，不再往下傳遞
                        h = 0;
                        break;
                    }

                    int tileHurt = MinusHurtByTile(oldData, ref h);
                    if (tileHurt > 0)
                    {
                        if (!ret.ContainsKey(t))
                            ret.Add(t, new Dictionary<Position, int>());

                        ret[t].Add(new Position(oldData.Row, oldData.Col), tileHurt);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        ///整合出一個“消除”動作的結果數據
        /// </summary>
        /// <param name="hurtMap"></param>
        /// <param name="orderMap"></param>
        /// <param name="retOriDatas"></param>
        /// <param name="retNewDatas"></param>
        /// <param name="retOrders"></param>
        /// <param name="retRewards"></param>
        private void CalcElimResult(Dictionary<TileType, int[,]> hurtMap,
                                    Dictionary<TileType, int[,]> orderMap,
                                    out List<TileObject> retOriDatas,
                                    out List<TileObject> retNewDatas,
                                    out List<int> retOrders,
                                    out List<ItemQtt[]> retRewards)
        {
            retOriDatas = new List<TileObject>();
            retNewDatas = new List<TileObject>();
            retOrders = new List<int>();
            retRewards = new List<ItemQtt[]>();
            foreach (var pair in hurtMap)
            {
                TileType type = pair.Key;
                int[,] subHurtMap = pair.Value;
                int[,] subOrderMap = orderMap[type];
                for (int r = 0, len0 = subHurtMap.GetLength(0); r < len0; r++)
                {
                    for (int c = 0, len1 = subHurtMap.GetLength(1); c < len1; c++)
                    {
                        if (subHurtMap[r, c] == 0)
                            continue;

                        TileObject oriData = GetTileAt(new Position(r, c), type);
                        TileObject newData = null;
                        ItemQtt[] reward = null;
                        HurtTile(oriData, subHurtMap[r, c], out newData, out reward);

                        retOriDatas.Add(oriData);
                        retNewDatas.Add(newData);
                        retOrders.Add(subOrderMap[r, c]);
                        retRewards.Add(reward);
                    }
                }
            }
        }



        /// <summary>
        /// 計算傷害經過一層地形物之後的遞減，會改變hurt參數的值
        /// </summary>
        /// <param name="target"></param>
        /// <param name="hurt">會變成經過這層地形物之後，殘留的傷害值</param>
        /// <returns>地形物受到的傷害值</returns>
        private int MinusHurtByTile(TileObject target, ref int hurt)
        {
            if (target == null || hurt == 0)
                return 0;

            TileObjectConfig conf = target.Config;
            int ret = 0;
            while (hurt > 0)
            {
                //表示無法繼續被消除，傷害全部被吸收了
                if (conf.Level == -1)
                {
                    hurt = 0;
                    break;
                }

                hurt--;
                ret++;

                //表示該地形物已經被完全消除
                if (conf.ChangeObjectId == 0)
                {
                    break;
                }

                conf = tileObjConfDict[conf.ChangeObjectId];
            }

            //for test
            //Debug.LogFormat("MinusHurtByTile: {0}, t={1}, h={2}",
            //    new Position(target.Row, target.Col),
            //    target.Config.ObjectType,
            //    ret);

            return ret;
        }


        /// <summary>
        /// 破壞一個地形物之後，得到的結果
        /// </summary>
        /// <param name="target"></param>
        /// <param name="h"></param>
        /// <param name="retNewData"></param>
        /// <param name="reward"></param>
        private void HurtTile(TileObject target,
                              int hurt,
                              out TileObject retNewData,
                              out ItemQtt[] retReward)
        {
            retReward = null;
            retNewData = null;
            if (target == null)
                return;


            TileObjectConfig conf = target.Config;
            int h = hurt;
            while (h > 0)
            {
                //表示無法繼續被消除，傷害全部被吸收了
                if (conf.Level == -1)
                {
                    h = 0;
                    break;
                }

                h--;

                if (!string.IsNullOrEmpty(conf.EliminateAdded))
                {
                    ItemQtt[] newReward = ItemQtt.ParseMulti(conf.EliminateAdded);
                    if (retReward == null)
                        retReward = newReward;
                    else//如果出現多層獎勵，則直接添加在最後
                        Array.Copy(newReward, retReward, newReward.Length);
                }

                //表示該地形物已經被完全消除
                if (conf.ChangeObjectId == 0)
                {
                    conf = null;
                    break;
                }

                conf = tileObjConfDict[conf.ChangeObjectId];
            }

            if (conf != null)
            {
                retNewData = new TileObject(target.Row, target.Col, conf);
                retNewData.OriID = target.OriID;
            }
            else
            {
                retNewData = null;
            }

            //Debug.LogFormat("HurtTile: {0}, t={1}, h={2}, newId={3}, reward={4}",
            //    new Position(target.Row, target.Col),
            //    target.Config.ObjectType,
            //    hurt,
            //    retNewData == null ? "null" : retNewData.ConfigID.ToString(),
            //    retReward != null ? retReward.Length : 0);
        }



        #region 查改數據的接口
        /// <summary>
        /// 改變Model層的數據的專用接口
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="newData"></param>
        private void ChangeModelData(Position pos,
                                     TileObject oriData,
                                     TileObject newData)
        {
            if (UnitTestMode)
            {
                //do nothing
                return;
            }

            //Debug.LogFormat("ChangeModelData {0}.{1}.{2}",
            //    newData != null,
            //    new Position(oriData.Row, oriData.Col),
            //    oriData.Config.ObjectType);

            batModel.SetDataAt(
                newData,
                oriData.Row,
                oriData.Col,
                oriData.Config.ObjectType
            );
        }



        /// <summary>
        /// 獲取某個位置的地形物配置
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private TileObjectConfig GetTileConfAt(Position pos, TileType tileType)
        {
            return GetTileAt(pos, tileType).Config;
        }


        /// <summary>
        /// 獲取某個位置的地形物配置
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private TileObject GetTileAt(Position pos, TileType tileType)
        {
            if (pos.Row < 0 || pos.Row >= numRow || pos.Col < 0 || pos.Col >= numCol)
            {
                return null;
            }

            if (UnitTestMode)
            {
                switch (tileType)
                {
                    case TileType.Cover:
                        return __TestCovers[pos.Row, pos.Col];
                    case TileType.Obstacle:
                        return __TestObstacles[pos.Row, pos.Col];
                    case TileType.Element:
                        return __TestElements[pos.Row, pos.Col];
                    case TileType.SeperatorH:
                        return __TestSeprH[pos.Row, pos.Col];
                    case TileType.SeperatorV:
                        return __TestSeprV[pos.Row, pos.Col];
                    case TileType.Bottom:
                        return __TestBottoms[pos.Row, pos.Col];
                }
                return null;
            }
            else
            {
                return batModel.GetDataAt(pos.Row, pos.Col, tileType);
            }
        }


        /// <summary>
        /// 獲取某個方向其他類型的地形物
        /// </summary>
        /// <param name="dire"></param>
        /// <param name="type"></param>
        /// <param name="distance">距離</param>
        /// <returns></returns>
        private TileObject GetTileAt(Position srcPos,
                                     Direction dire,
                                     TileType type,
                                     int distance = 1)
        {
            //Debug.LogFormat("GetTileAt {0},{1},{2},{3}", srcPos, dire, type, distance);
            if (srcPos.Row < 0 || srcPos.Row >= numRow || srcPos.Col < 0 || srcPos.Col >= numCol)
                return null;

            if (type == TileType.SeperatorH)
            {
                if (dire == Direction.U && srcPos.Row - distance >= 0)
                {
                    return GetTileAt(new Position(srcPos.Row - distance, srcPos.Col), type);
                }
                else if (dire == Direction.D && srcPos.Row + distance - 1 < numRow)
                {
                    return GetTileAt(new Position(srcPos.Row + distance - 1, srcPos.Col), type);
                }
                //橫向間隔物只支持上下方向
                else
                {
                    return null;
                }
            }
            else if (type == TileType.SeperatorV)
            {
                if (dire == Direction.L && srcPos.Col - distance >= 0)
                {
                    return GetTileAt(new Position(srcPos.Row, srcPos.Col - distance), type);
                }
                else if (dire == Direction.R && srcPos.Col + distance - 1 < numCol)
                {
                    return GetTileAt(new Position(srcPos.Row, srcPos.Col + distance - 1), type);
                }
                //縱向間隔物只支持左右方向
                else
                {
                    return null;
                }
            }
            else
            {
                switch (dire)
                {
                    case Direction.U:
                        return GetTileAt(new Position(srcPos.Row - distance, srcPos.Col), type);
                    case Direction.D:
                        return GetTileAt(new Position(srcPos.Row + distance, srcPos.Col), type);
                    case Direction.L:
                        return GetTileAt(new Position(srcPos.Row, srcPos.Col - distance), type);
                    case Direction.R:
                        return GetTileAt(new Position(srcPos.Row, srcPos.Col + distance), type);
                    case Direction.DL:
                        return GetTileAt(new Position(srcPos.Row + distance, srcPos.Col - distance), type);
                    case Direction.DR:
                        return GetTileAt(new Position(srcPos.Row + distance, srcPos.Col + distance), type);
                    case Direction.UL:
                        return GetTileAt(new Position(srcPos.Row - distance, srcPos.Col - distance), type);
                    case Direction.UR:
                        return GetTileAt(new Position(srcPos.Row - distance, srcPos.Col + distance), type);
                    default:
                        return null;
                }
            }
        }

        #endregion

        #region 單元測試相關

        public TileObject[,] __TestCovers;
        public TileObject[,] __TestElements;
        public TileObject[,] __TestObstacles;
        public TileObject[,] __TestSeprH;
        public TileObject[,] __TestSeprV;
        public TileObject[,] __TestBottoms;

        public void __TestRecursiveCalcElimPath(List<Position> path, List<int> orders, List<Position> retPath, List<int> retOrders)
        {
            RecursiveCalcElimPath(path, orders, retPath, retOrders);
        }

        public void __TestHurtTile(TileObject target,
                                   int hurt,
                                   out TileObject retNewData,
                                   out ItemQtt[] retReward)
        {
            HurtTile(target, hurt, out retNewData, out retReward);
        }


        public Dictionary<TileType, int> __TestCalcTileHurtByElim(Position pos, int hurt)
        {
            return CalcTileHurtByElim(pos, hurt);
        }


        public Dictionary<TileType, Dictionary<Position, int>> __TestCalcTileHurtByAffect(Position pos, int hurtValue)
        {
            return CalcTileHurtByAffect(pos, hurtValue);
        }


        public void __TestEliminate(List<Position> linkPath,
                              out List<TileObject> retOriDatas,
                              out List<TileObject> retNewDatas,
                              out List<int> retOrders,
                              out List<ItemQtt[]> retElimRewards)
        {
            Eliminate(linkPath, out retOriDatas, out retNewDatas, out retOrders, out retElimRewards);
        }

        #endregion
    }
}
