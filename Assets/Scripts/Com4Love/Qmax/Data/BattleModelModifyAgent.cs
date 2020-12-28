using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using System.Collections.Generic;
using UnityEngine;
namespace Com4Love.Qmax.Data
{
    /// <summary>
    /// BattleModel的更改代理
    /// </summary>
    public class BattleModelModifyAgent : BattleModel
    {
        private List<TileObject[,]> modifyingHistory;


        public BattleModelModifyAgent(QmaxModel qmaxModel,
                                      ModelEventSystem eventSystem)
            : base(qmaxModel.TileObjectConfigs,
                   qmaxModel.GameSystemConfig,
                   qmaxModel.SkillConfigs,
                   qmaxModel.ComboConfigs,
                   eventSystem)
        {
            modifyingHistory = new List<TileObject[,]>();
        }

        public BattleModelModifyAgent(Dictionary<int, TileObjectConfig> tileObjectConfigs,
                                      GameSystemConfig gameSystemConfig,
                                      Dictionary<int, SkillConfig> skillConfigs,
                                      Dictionary<int, ComboConfig> comboConfigs,
                                      ModelEventSystem eventSystem)
            : base(tileObjectConfigs, gameSystemConfig, skillConfigs, comboConfigs, eventSystem)
        {
            modifyingHistory = new List<TileObject[,]>();
        }


        /// <summary>
        /// 一个新的更改
        /// </summary>
        /// <param name="modifyingList"></param>
        public void NewModifying(List<TileObject> modifyingList)
        {
            TileObject[,] arr = new TileObject[CrtLevelConfig.NumRow, CrtLevelConfig.NumCol];
            for (int i = 0, n = modifyingList.Count; i < n; i++)
            {
                TileObject to = modifyingList[i];
                Q.Assert(to != null);
                arr[to.Row, to.Col] = to;
            }

            modifyingHistory.Add(arr);
            //Q.Log("NewModifying: l={0}", modifyingHistory.Count);
        }


        /// <summary>
        /// 提交所有修改
        /// </summary>
        public void CommitModifying()
        {
            if (modifyingHistory == null || modifyingHistory.Count == 0)
                return;

            int numRow = CrtLevelConfig.NumRow;
            int numCol = CrtLevelConfig.NumCol;
            for (int r = 0; r < numRow; r++)
            {
                for (int c = 0; c < numCol; c++)
                {
                    for (int i = modifyingHistory.Count - 1; i >= 0; i--)
                    {
                        TileObject to = modifyingHistory[i][r, c];
                        if (to == null)
                            continue;

                        elementData[r, c] = to;
                        break;
                    }//for i
                }//for col
            }//for row
            modifyingHistory.Clear();
        }//CommitTempChange



        /// <summary>
        /// 回滚一层更改
        /// </summary>
        /// <returns>返回被更改过的位置</returns>
        public List<Position> RevertOneLayerModifying()
        {
            Q.Assert(modifyingHistory.Count > 0);
            if (modifyingHistory.Count == 0)
                return new List<Position>();

            List<Position> ret = new List<Position>();
            TileObject[,] lastModifying = modifyingHistory[modifyingHistory.Count - 1];
            for (int r = 0, numRow = lastModifying.GetLength(0); r < numRow; r++)
            {
                for (int c = 0, numCol = lastModifying.GetLength(1); c < numCol; c++)
                {
                    if (lastModifying[r, c] != null)
                    {
                        ret.Add(new Position(r, c));
                    }
                }
            }
            modifyingHistory.RemoveAt(modifyingHistory.Count - 1);
            //Q.Log("NewModifying: l={0}", modifyingHistory.Count);
            return ret;
        }


        /// <summary>
        /// 回滚所有更改
        /// </summary>
        public List<Position> RevertAllModifyings()
        {
            if (modifyingHistory == null)
                return null;

            List<Position> ret = new List<Position>();
            int numRow = CrtLevelConfig.NumRow;
            int numCol = CrtLevelConfig.NumCol;
            for (int r = 0; r < numRow; r++)
            {
                for (int c = 0; c < numCol; c++)
                {
                    for (int i = modifyingHistory.Count - 1; i >= 0; i--)
                    {
                        if (modifyingHistory[i][r, c] != null)
                        {
                            ret.Add(new Position(r, c));
                        }
                    }
                }
            }
            modifyingHistory.Clear();
            return ret;
        }


        public override bool ContainsDataAt(int r, int c, TileType type)
        {
            if (type == TileType.Element)
            {
                for (int i = 0, n = modifyingHistory.Count; i < n; i++)
                {
                    TileObject[,] list = modifyingHistory[i];
                    if (list[r, c] != null)
                    {
                        return true;
                    }
                }
            }

            return base.ContainsDataAt(r, c, type);
        }

        public override TileObject GetDataAt(int row, int col, TileType type)
        {
            if (type == TileType.Element)
            {
                for (int i = modifyingHistory.Count - 1; i >= 0; i--)
                {
                    TileObject[,] list = modifyingHistory[i];
                    if (list[row, col] != null)
                    {
                        return list[row, col];
                    }
                }
            }

            return base.GetDataAt(row, col, type);
        }


        public TileObject GetSavedDataAt(int row, int col, TileType type)
        {
            return base.GetDataAt(row, col, type);
        }
    }//class
}
