using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.Stage;
using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace Com4Love.Qmax.Data
{
    /// <summary>
    /// 戰鬥時的數據層
    /// </summary>
    public class BattleModel : IDisposable
    {
        /// <summary>
        /// 當無法連線時，嘗試重組棋盤的次數
        /// </summary>
        public const int REARRANGE_TRY_TIMES = 50;

        /// <summary>
        /// 當前陣型配置
        /// </summary>
        public LevelConfig CrtLevelConfig;

        /// <summary>
        /// 當前關卡配置
        /// </summary>
        public StageConfig CrtStageConfig;


        public int StageLimit;

        /// <summary>
        /// 當前關卡信息，如目標，實力次數......
        /// </summary>
        public Stage CurStage;

        /// <summary>
        /// 當前使用的技能配置
        /// </summary>
        public Dictionary<ColorType, SkillConfig> SkillConfDict;

        /// <summary>
        /// 當前使用的技能的集氣狀態（已經收集了幾個消除物）
        /// </summary>
        public Dictionary<ColorType, int> SkillCDDict;

        /// <summary>
        /// 當前使用的伙伴
        /// </summary>
        public Dictionary<ColorType, Unit> CrtUnitDict;

        /// <summary>
        /// 敵人數據
        /// 如果某個敵人被消滅，該位置會被賦為null
        /// </summary>
        public List<Unit> EnemiesData;

        ///<summary>
        /// 當前敵人技能配置
        /// </summary>
        public Dictionary<int, List<SkillConfig>> EneriesSkillConDict;

        int remainSteps = -1;

        /// <summary>
        /// 剩餘步數
        /// </summary>
        public int RemainSteps
        {
            get
            {
                return remainSteps;
            }
            set
            {
                if (value != remainSteps)
                {
                    remainSteps = value;

                    if (GameController.Instance.ModelEventSystem.OnBattleInfoUpdate != null)
                    {
                        GameController.Instance.ModelEventSystem.OnBattleInfoUpdate(ModelEventSystem.BattleInfoType.Step);
                    }
                }
            }
        }

        int remainTime = -1;

        /// <summary>
        /// 剩餘時間
        /// </summary>
        public int RemainTime
        {
            get
            {
                return remainTime;
            }
            set
            {
                if (value != remainTime)
                {
                    remainTime = value;

                    if (GameController.Instance.ModelEventSystem.OnBattleInfoUpdate != null)
                    {
                        GameController.Instance.ModelEventSystem.OnBattleInfoUpdate(ModelEventSystem.BattleInfoType.Time);
                    }
                }
            }
        }

        /// <summary>
        /// 關卡目標中的消除物目標
        /// RelativeID -> 已經消除數量
        /// </summary>
        public Dictionary<int, int> ObjectGoal;

        /// <summary>
        /// 當前已經消滅的怪的數量
        /// </summary>
        public int UnitGoal;

        /// <summary>
        /// 當前的目標
        /// </summary>
        public List<StageConfig.Goal> CurrentGoal;

        /// <summary>
        /// 消除記錄
        /// </summary>
        public List<Step> Steps;

        /// <summary>
        /// 玩家上次玩該關卡獲得的等級
        /// </summary>
        public int PreStar;

        /// <summary>
        /// 當前關卡等級
        /// </summary>
        public int CrtStar;

        /// <summary>
        /// 用於提示可以連接的路徑
        /// </summary>
        public Stack<TileObject> LinkableStack;


        public List<StepAnimationItem> StepAwardList;

        public Dictionary<int, int> TotalRandomSeed;

        /// <summary>
        /// 關卡勝利的次數
        /// </summary>
        public short WinCount;

        /// <summary>
        /// 關卡失敗的次數
        /// </summary>
        public short FailCount;

        /// <summary>
        /// 標記可以提交遊戲數據////
        /// </summary>
        public bool IsCanSubmit;

        /// <summary>
        /// 玩家每局分數//
        /// </summary>
        public int Score;

        /// <summary>
        /// 三星分数
        /// </summary>
        public int[] StarScore;

        /// <summary>
        /// 是否可以減少步數，比如道具使用不減少步數///
        /// </summary>
        public bool IsCanMinusSteps;


        private int _crtEnemyIndex = -1;
        /// <summary>
        /// 當前敵人的位置
        /// -1
        /// </summary>
        public int CrtEnemyIndex
        {
            get { return _crtEnemyIndex; }
            set
            {
                _crtEnemyIndex = value;
                Q.Log("CrtEnemyIndex={0}", _crtEnemyIndex);
            }
        }


        protected TileObject[,] elementData;
        protected TileObject[,] collectData;
        protected TileObject[,] obstacleData;
        protected TileObject[,] coverData;
        protected TileObject[,] seperatorHData;
        protected TileObject[,] seperatorVData;
        protected TileObject[,] bottomData;

        private ModelEventSystem eventSystem;
        private Dictionary<RewardType, int> rewards;

        private GameSystemConfig gameSystemConfig;
        private Dictionary<int, TileObjectConfig> tileObjectConfigs;
        private Dictionary<int, ComboConfig> comboConfigs;
        private Dictionary<int, SkillConfig> skillConfigs;

        /// <summary>
        /// 時間模式計時器
        /// </summary>
        private Timer timeModeTimer;

        private bool IsWin;

        public BattleModel(Dictionary<int, TileObjectConfig> tileObjectConfigs,
                           GameSystemConfig gameSystemConfig,
                           Dictionary<int, SkillConfig> skillConfigs,
                           Dictionary<int, ComboConfig> comboConfigs,
                           ModelEventSystem eventSystem)
        {
            Q.Assert(tileObjectConfigs != null);
            Q.Assert(gameSystemConfig != null);
            Q.Assert(skillConfigs != null);
            Q.Assert(comboConfigs != null);

            this.gameSystemConfig = gameSystemConfig;
            this.tileObjectConfigs = tileObjectConfigs;
            this.comboConfigs = comboConfigs;
            this.skillConfigs = skillConfigs;

            this.eventSystem = eventSystem;
            SkillConfDict = new Dictionary<ColorType, SkillConfig>();
            EneriesSkillConDict = new Dictionary<int, List<SkillConfig>>();
            SkillCDDict = new Dictionary<ColorType, int>();
            rewards = new Dictionary<RewardType, int>();
            StepAwardList = new List<StepAnimationItem>();
            TotalRandomSeed = new Dictionary<int, int>();
            //GameController.Instance.ModelEventSystem.OnStageRewardRespone += OnStageReward;
            IsCanSubmit = true;
            Score = 0;
            StageLimit = 0;
            IsCanMinusSteps = true;
            IsWin = false;
        }


        public void Dispose()
        {
            //GameController.Instance.ModelEventSystem.OnStageRewardRespone -= OnStageReward;
            //Nothing to do.
        }

        /// <summary>
        /// 初始化數據
        /// </summary>
        /// <param name="numRow"></param>
        /// <param name="numCol"></param>
        public void InitDatas(int numRow, int numCol)
        {
            elementData = new TileObject[numRow, numCol];
            collectData = new TileObject[numRow, numCol];
            obstacleData = new TileObject[numRow, numCol];
            coverData = new TileObject[numRow, numCol];
            seperatorHData = new TileObject[numRow, numCol];
            seperatorVData = new TileObject[numRow, numCol];
            bottomData = new TileObject[numRow, numCol];
        }

        public virtual TileObject[,] GetElementMap()
        {
            return elementData;
        }

        /// <summary>
        /// 在某個位置是否有地形物
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool ContainsDataAt(int r, int c, TileType type)
        {
            TileObject obj = GetDataAt(r, c, type);
            return obj != null && obj.Config != null;
        }


        public virtual void SetDataAt(TileObject data, int r, int c, TileType type)
        {
            Q.Assert(r >= 0 && r < CrtLevelConfig.NumRow);
            Q.Assert(c >= 0 && c < CrtLevelConfig.NumCol);
            TileObject[,] list = GetTypeList(type);
            list[r, c] = data;
        }

        public virtual TileObject GetDataAt(int row, int col, TileType type)
        {
            Q.Assert(row >= 0 && row < CrtLevelConfig.NumRow);
            Q.Assert(col >= 0 && col < CrtLevelConfig.NumCol);
            TileObject[,] list = GetTypeList(type);
            return list[row, col];
        }

        public virtual TileObject GetElementAt(int row, int col)
        {
            return GetDataAt(row, col, TileType.Element);
        }


        /// <summary>
        /// 獲取某個位置的敵人的數據
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Unit GetEnemyAt(int index)
        {
            if (EnemiesData == null || index < 0 || index >= EnemiesData.Count)
                return null;

            return EnemiesData[index];
        }


        /// <summary>
        /// 獲取當前敵人的數據
        /// </summary>
        public Unit GetCrtEnemy() { return GetEnemyAt(CrtEnemyIndex); }


        /// <summary>
        /// 檢查是否技能CD已滿
        /// 如果某個技能Cd已滿，則扔相應的地形物到棋盤上，以同樣顏色的普通消除物優先
        /// </summary>
        public List<TileObject> CheckSkillCD()
        {
            List<TileObject> alternativeTiles = null;
            Action GetAlternativeTiles = delegate()
            {
                if (alternativeTiles != null)
                    return;

                alternativeTiles = new List<TileObject>(49);
                for (int r = 0, numRow = elementData.GetLength(0); r < numRow; r++)
                {
                    //bool validRow = false;
                    for (int c = 0, numCol = elementData.GetLength(1); c < numCol; c++)
                    {
                        TileObject t = elementData[r, c];
                        TileObject cover = coverData[r, c];
                        //ConfigID <= 5 為基本消除物
                        if (t == null || t.ConfigID > 5 || cover != null)
                            continue;

                        //validRow = true;
                        alternativeTiles.Add(t);
                    }
                }
            };

            Dictionary<ColorType, int> readOnlySkillDict = new Dictionary<ColorType, int>(SkillCDDict);
            List<TileObject> ret = new List<TileObject>();
            foreach (KeyValuePair<ColorType, int> pair in readOnlySkillDict)
            {
                if (!SkillConfDict.ContainsKey(pair.Key) || pair.Value < SkillConfDict[pair.Key].SkillCD)
                    continue;

                SkillConfig skillConf = SkillConfDict[pair.Key];

                //Skill CD已滿
                SkillCDDict[pair.Key] = 0;
                //蒐集前兩排同樣顏色的消除物
                List<TileObject> sameColorTiles = new List<TileObject>(49);
                GetAlternativeTiles();
                Q.Assert(alternativeTiles.Count > 0);
                ///沒有了技能掉落位置///
                if (alternativeTiles.Count == 0)
                {
                    return ret;
                }
                for (int i = 0, n = alternativeTiles.Count; i < n; i++)
                {
                    if (alternativeTiles[i].Config.ColorType == pair.Key)
                        sameColorTiles.Add(alternativeTiles[i]);
                }

                TileObject target;
                int count = 0;
                bool correct = true;
                do
                {
                    count++;
                    correct = true;
                    if (sameColorTiles.Count > 0)
                    {
                        target = sameColorTiles[UnityEngine.Random.Range(0, sameColorTiles.Count - 1)];
                    }
                    else
                    {
                        target = alternativeTiles[UnityEngine.Random.Range(0, alternativeTiles.Count - 1)];
                    }

                    foreach (var item in ret)
                    {
                        if (item.Row == target.Row && item.Col == target.Col)
                        {
                            correct = false;
                            break;
                        }
                    }
                } while (!correct && count <= 20);

                Q.Assert(target != null);
                TileObjectConfig targetTileConfig = tileObjectConfigs[skillConf.arg0];

                TileObject newTileObj = new TileObject(target.Row, target.Col, targetTileConfig);
                //替換為新TileObject
                elementData[target.Row, target.Col] = newTileObj;
                ret.Add(newTileObj);
            }
            return ret;
        }


        /// <summary>
        /// 生成棋盤陣型初始化的事件的參數
        /// </summary>
        /// <returns></returns>
        public ModelEventSystem.InitEventArgs GenerateBoardInitEventArgment()
        {
            ModelEventSystem.InitEventArgs ret = new ModelEventSystem.InitEventArgs();
            ret.Elements = elementData;
            ret.Collects = collectData;
            ret.Obstacles = obstacleData;
            ret.Covers = coverData;
            ret.SeperatorH = seperatorHData;
            ret.SeperatorV = seperatorVData;
            ret.Bottom = bottomData;
            ret.Units = CrtUnitDict;
            return ret;
        }

        public int CountStar()
        {
            ///三星條件對應的條件///

            if (Score >= StarScore[2])
            {
                CrtStar = 3;
            }
            else if (Score >= StarScore[1])
            {
                CrtStar = 2;
            }
            else if (Score >= StarScore[0])
            {
                CrtStar = 1;
            }

            return CrtStar;
        }
        /// <summary>
        /// 敵人發動技能攻擊
        /// </summary>
        /// <param name="enemyPoint"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public TileObject InvokeMonstorSkill(EnemyPoint enemyPoint)
        {
            //Unit enemy = enemyPoint.Data;
            List<TileObject> alternativeTiles = null;
            //選出所有的有效的元素
            Action GetAlternativeTiles = delegate()
            {
                if (alternativeTiles != null)
                    return;

                alternativeTiles = new List<TileObject>();
                //lineCount為有效排計數
                for (int r = 0, numRow = elementData.GetLength(0); r < numRow; r++)
                {
                    for (int c = 0, numCol = elementData.GetLength(1); c < numCol; c++)
                    {
                        TileObject t = elementData[r, c];
                        //ConfigID <= 5 為基本消除物
                        if (t == null || t.ConfigID > 5)
                            continue;

                        if (coverData[r, c] != null)
                            continue;
                        alternativeTiles.Add(t);
                    }
                }
            };

            GetAlternativeTiles();
            TileObject target;
            target = alternativeTiles[UnityEngine.Random.Range(0, alternativeTiles.Count - 1)];
            Q.Assert(target != null);
            SkillConfig skillCfg = enemyPoint.GetSkillConfigByType(SkillType.Enemy);
            if (skillCfg == null)
            {
                EnemySkillID = -1;
                return null;
            }


            EnemySkillID = skillCfg.ID;
            TileObjectConfig targetTileConfig = tileObjectConfigs[skillCfg.arg0];
            TileObject newTileObj = new TileObject(target.Row, target.Col, targetTileConfig);
            if (targetTileConfig.ObjectType == TileType.Obstacle)
            {
                obstacleData[target.Row, target.Col] = newTileObj;
            }
            else if (targetTileConfig.ObjectType == TileType.Cover)
            {
                coverData[target.Row, target.Col] = newTileObj;
            }

            return newTileObj;
        }

        
        public int CalcAttackResult(Unit enemy, 
                                    ColorType mainColor,
                                    List<Position> elimPath,
                                    out int resultComboLevel, 
                                    out float resultComboRate, 
                                    out bool beShield)
        {
            Dictionary<ColorType, int> elements = GetElementCountByPath(elimPath);
            return CalcAttackResult(enemy, mainColor, elements, out resultComboLevel, out resultComboRate, out beShield);
        }

        /// <summary>
        /// 計算針對當前的敵人計算攻擊數值
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="mainColor"></param>
        /// <param name="elements"></param>
        /// <param name="resultComboLevel">返回Combo等級</param>
        /// <param name="resultComboRate">返回Combo</param>
        /// <param name="beShield">是否被抵擋了</param>
        /// <returns></returns>
        public int CalcAttackResult(Unit enemy, 
                                    ColorType mainColor,
                                    Dictionary<ColorType, int> elements,
                                    out int resultComboLevel, 
                                    out float resultComboRate, 
                                    out bool beShield)
        {
            //Q.Log("--------CalcAttackResult, mainColor={0}", mainColor);

            //因為有可能是分段攻擊，這裡目前要考慮到沒有elements中沒有mainColor Element的問題

            resultComboLevel = 1;
            resultComboRate = 1.0f;
            int mainColorCount = 0;
            int ret = 0;
            beShield = false;

            if (enemy != null)
            {
                ret = CountEliminateHurt(enemy, mainColor, elements, out mainColorCount, out beShield);
            }
            else
            {
                //沒有敵人時，只輸出Combo信息
                ret = 0;
                foreach (KeyValuePair<ColorType, int> pair in elements)
                {
                    if (mainColor == pair.Key)
                        mainColorCount = pair.Value;
                }
            }

            //Q.Log("MainColor={0}, count={1}", mainColor, mainColorCount);
            if (mainColorCount == 0)
            {
                resultComboLevel = 1;
                resultComboRate = 1.0f;
            }
            else
            {
                ComboConfig comboConf = comboConfigs[mainColorCount];
                resultComboLevel = comboConf.ComboLevel;
                resultComboRate = comboConf.ComboRate;
            }

            return ret;
        }



        /// <summary>
        /// 專門用於計算剩餘步數獎勵，因為在計算剩餘步數時，需要把攻擊乘以一個倍數
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="mainColor"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public int CalcLeftRewardScore(Unit enemy,
                                       ColorType mainColor,
                                       Dictionary<ColorType, int> elements)
        {
            Dictionary<ColorType, float> mp = new Dictionary<ColorType, float>();
            ColorType[] colors = { ColorType.Earth, ColorType.Fire, ColorType.Golden, ColorType.Water, ColorType.Wood };
            for (int i = 0, n = colors.Length; i < n; i++)
            {
                ColorType c = colors[i];
                if (CrtUnitDict.ContainsKey(c))
                {
                    mp.Add(c, CrtUnitDict[c].Config.AttackMp);
                }
                else
                {
                    mp.Add(c, gameSystemConfig.ElementAttackMp);
                }
            }

            int mainColorCount = 0;
            bool beShield = false;
            return CountEliminateHurt(enemy, mainColor, elements, mp, out mainColorCount, out beShield);
        }


        public int CountEliminateHurt(Unit enemy,
                                      ColorType mainColor,
                                      Dictionary<ColorType, int> elements,
                                      out int mainColorCount,
                                      out bool beShield)
        {
            Dictionary<ColorType, float> mp = new Dictionary<ColorType, float>();
            mp.Add(ColorType.Earth, 1.0f);
            mp.Add(ColorType.Fire, 1.0f);
            mp.Add(ColorType.Golden, 1.0f);
            mp.Add(ColorType.Water, 1.0f);
            mp.Add(ColorType.Wood, 1.0f);
            return CountEliminateHurt(enemy, mainColor, elements, mp, out mainColorCount, out beShield);
        }

        /// <summary>
        /// 計算傷害值，傷害值不會因為沒有敵人而不存在
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="mainColor"></param>
        /// <param name="elements"></param>
        /// <param name="isMultiple"></param>
        /// <returns></returns>
        public int CountEliminateHurt(Unit enemy,
                                      ColorType mainColor,
                                      List<Position> elimPath)
        {
            int mainColorCount = 0;
            bool beShield = false;
            //區分各種顏色的消除的數量
            Dictionary<ColorType, int> elements = GetElementCountByPath(elimPath);
            return CountEliminateHurt(enemy, mainColor, elements, out mainColorCount, out beShield);
        }


        /// <summary>
        /// 給定顏色和路徑，算出該路徑中，對應顏色的消除物的數量
        /// </summary>
        /// <param name="elimPath"></param>
        /// <param name="targetColor"></param>
        /// <returns></returns>
        public int GetColorNumByPath(List<Position> elimPath, ColorType targetColor)
        {
            int ret = 0;
            for (int i = 0, n = elimPath.Count; i < n; i++)
            {
                Position pos = elimPath[i];
                TileObjectConfig conf = GetElementAt(pos.Row, pos.Col).Config;
                if (conf.ColorType == targetColor || conf.AllColors.Contains(targetColor))
                    ret++;
            }

            return ret;
        }



        private Dictionary<ColorType, int> GetElementCountByPath(List<Position> elimPath)
        {
            //區分各種顏色的消除的數量
            Dictionary<ColorType, int> ret = new Dictionary<ColorType, int>();
            ret.Add(ColorType.Earth, 0);
            ret.Add(ColorType.Fire, 0);
            ret.Add(ColorType.Wood, 0);
            ret.Add(ColorType.Water, 0);
            ret.Add(ColorType.Golden, 0);

            for (int i = 0, n = elimPath.Count; i < n; i++)
            {
                Position p = elimPath[i];
                TileObject data = GetElementAt(p.Row, p.Col);
                if (data == null)
                    continue;

                //如果一個消除物有多個顏色（多色石），則在相除過程中相當於視為多個普通消除物
                for (int j = 0, m = data.Config.AllColors.Count; j < m; j++)
                {
                    ColorType c = data.Config.AllColors[j];
                    if (ret.ContainsKey(c))
                    {
                        ret[c]++;
                    }
                }
            }
            return ret;
        }




        /// <summary>
        /// 計算傷害值，傷害值不會因為沒有敵人而不存在
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="mainColor"></param>
        /// <param name="elements"></param>
        /// <param name="mainColorCount"></param>
        /// <param name="beShield"></param>
        /// <param name="isCalcLeftRewardScore">傷害是否根據配置翻倍</param>
        /// <returns></returns>
        public int CountEliminateHurt(Unit enemy,
                                      ColorType mainColor,
                                      Dictionary<ColorType, int> elements,
                                      Dictionary<ColorType, float> mp,
                                      out int mainColorCount,
                                      out bool beShield)
        {
            int ret = 0;
            int atkProp = 0;

            beShield = false;
            mainColorCount = 0;

            Dictionary<ColorType, int> normalATKDict = gameSystemConfig.ElementATKDict;
            SkillConfig skill = null;
            if (enemy != null && skillConfigs.ContainsKey(enemy.Config.UnitSkillId))
                skill = skillConfigs[enemy.Config.UnitSkillId];

            foreach (KeyValuePair<ColorType, int> pair in elements)
            {
                if (pair.Value == 0 || pair.Key == ColorType.None)
                    continue;

                //如果該沒有夥伴時，有默認攻擊力
                int colorAtk =
                    CrtUnitDict.ContainsKey(pair.Key) ?
                    CrtUnitDict[pair.Key].Config.UnitAtk :
                    normalATKDict[pair.Key];

                atkProp = Convert.ToInt32(colorAtk * mp[pair.Key]);

                int defProp = 0;
                if (skill != null
                    && skill.SkillType == SkillType.Shield
                    && skill.SkillColor == pair.Key)
                {
                    beShield = true;
                    defProp = 999999;
                }
                float comboRate = 1.0f;
                //只有主顏色才計算Combo
                if (mainColor == pair.Key)
                {
                    mainColorCount = pair.Value;
                    comboRate = comboConfigs[mainColorCount].ComboRate;
                }
                ret += Math.Max(0, (int)(pair.Value * (atkProp - defProp) * comboRate));
            }//foreach

            return ret;
        }



        /// <summary>
        /// 減少當前敵人的血量
        /// </summary>
        /// <param name="minusValue"></param>
        /// <returns>如果當前有敵人，返回值表示敵人是否仍然存在；如果沒有敵人，返回false</returns>
        public bool MinusCrtEnemyHp(int minusValue)
        {
            Unit enemy = GetCrtEnemy();
            if (enemy == null)
                return false;

            enemy.Hp = Math.Max(0, enemy.Hp - minusValue);
            bool ret = enemy.Hp > 0;
            if (!ret)
            {
                UnitGoal++;
                //消滅敵人後，這里數據要賦為null
                EnemiesData[CrtEnemyIndex] = null;
                //CheckLevelGoal();
            }

            if (GameController.Instance.ModelEventSystem.OnBattleInfoUpdate != null)
            {
                GameController.Instance.ModelEventSystem.OnBattleInfoUpdate(ModelEventSystem.BattleInfoType.EnemyHp);
            }

            return ret;
        }

        /// <summary>
        /// 減少步數，返回當前時候可以減少步數//
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool MinusRemainSteps(int num = 1)
        {
            if (IsCanMinusSteps)
                RemainSteps = RemainSteps - num;

            return IsCanMinusSteps;
        }

        //public void StepsChange(int addStep)
        //{
        //    RemainSteps += addStep;

        //    if (GameController.Instance.ModelEventSystem.OnBattleInfoUpdate != null)
        //    {
        //        GameController.Instance.ModelEventSystem.OnBattleInfoUpdate(ModelEventSystem.BattleInfoType.Step);
        //    }
        //}

        public int EnemySkillID = -1;
        public void ChangeRandomWeight()
        {
            TotalRandomSeed.Clear();
            ///1、計算基礎權重
            foreach (RandomSeedConfig itemBase in CrtStageConfig.baseRandomSeeds.Values)
            {
                int weight = itemBase.SeedWeight;
                int stageAddWeight = weight +
                    Mathf.CeilToInt((WinCount - FailCount) * itemBase.WinRateWeight) +
                    Mathf.CeilToInt((FailCount - WinCount) * itemBase.AbortionWeight);
                if (TotalRandomSeed.ContainsKey(itemBase.ObjectId))
                {
                    TotalRandomSeed[itemBase.ObjectId] += stageAddWeight;
                }
                else
                {
                    TotalRandomSeed.Add(itemBase.ObjectId, stageAddWeight);
                }
                //if (TotalRandomSeed[itemBase.ObjectId] < 0)
                //{
                //    TotalRandomSeed[itemBase.ObjectId] = 0;
                //}
            }
            ///2、計算動態條件權重
            foreach (RandomSeedConfig item in CrtStageConfig.dynamicRandomSeeds.Values)
            {
                bool isReach = true;
                foreach (var conds in item.TriggerItems)
                {
                    TriggerRateType type = conds.Key;
                    StageGoalItem goItem = conds.Value;
                    if (type == TriggerRateType.StageGold)
                    {
                        isReach = isReach && TriggerStageGoal(goItem);
                    }
                    else if (type == TriggerRateType.EnemySkill)
                    {
                        isReach = isReach && TriggerEnemySkillGoal(goItem);
                    }
                    else if (type == TriggerRateType.LeftStep)
                    {
                        isReach = isReach && TriggerLeftStepGoal(goItem);
                    }
                    else if (type == TriggerRateType.LeftElement)
                    {
                        isReach = isReach && TriggerLeftElementGoal(goItem);
                    }
                }

                if (isReach)
                {
                    ///達到條件後把權重加到總權重上
                    int weight1 = item.SeedWeight;
                    int stageAddWeight = weight1 +
                    Mathf.CeilToInt((WinCount - FailCount) * item.WinRateWeight) +
                    Mathf.CeilToInt((FailCount - WinCount) * item.AbortionWeight);
                    if (TotalRandomSeed.ContainsKey(item.ObjectId))
                    {
                        TotalRandomSeed[item.ObjectId] += stageAddWeight;
                    }
                    else
                    {
                        TotalRandomSeed.Add(item.ObjectId, stageAddWeight);
                    }
                }
            }

            Dictionary<int, int> temp = new Dictionary<int, int>();
            foreach (var tempItem in TotalRandomSeed)
            {
                temp.Add(tempItem.Key, tempItem.Value);
            }
            foreach (var itemKey in temp.Keys)
            {
                if (TotalRandomSeed[itemKey] < 0)
                {
                    TotalRandomSeed[itemKey] = 0;
                }
            }
        }

        public int CreateRandomElement()
        {
            int objectId = -1;

            int totalRate = 0;
            foreach (int itemW in TotalRandomSeed.Values)
            {
                totalRate += itemW;
            }
            int randomNum = UnityEngine.Random.Range(1, totalRate);
            int partNum = 0;

            ///生成權重的隨機概率
            foreach (var item in TotalRandomSeed)
            {
                int w = item.Value;
                partNum += w;
                if (partNum >= randomNum)
                {
                    objectId = item.Key;
                    break;
                }
            }

            return objectId;
        }

        public int GetTileNum(int ObjectId)
        {
            int num = 0;
            for (int r = 0, numRow = elementData.GetLength(0); r < numRow; r++)
            {
                for (int c = 0, numCol = elementData.GetLength(1); c < numCol; c++)
                {
                    TileObject element = elementData[r, c];
                    TileObject obser = obstacleData[r, c];
                    TileObject cover = coverData[r, c];

                    if (element != null && element.ConfigID == ObjectId)
                    {
                        num++;
                    }

                    if (obser != null && obser.ConfigID == ObjectId)
                    {
                        num++;
                    }

                    if (cover != null && cover.ConfigID == ObjectId)
                    {
                        num++;
                    }
                }
            }

            return num;
        }


        /// <summary>
        /// 判斷條件為關卡目標類型條件是否達到
        /// </summary>
        /// <param name="goItem"></param>
        /// <returns></returns>
        private bool TriggerStageGoal(StageGoalItem goItem)
        {
            bool isReach = false;
            for (int i = 0, n = CurrentGoal.Count; i < n; i++)
            {
                StageConfig.Goal g = CurrentGoal[i];
                if (g.RelativeID == goItem.ObjectId)
                {
                    isReach = TriggerJudge(ref goItem, g.Num);
                }
            }

            return isReach;
        }

        bool TriggerLeftElementGoal(StageGoalItem goItem)
        {
            int num = GetTileNum(goItem.ObjectId);
            bool isReach = TriggerJudge(ref goItem, num);
            return isReach;
        }

        /// <summary>
        /// 判斷敵人技能條件是否達到
        /// </summary>
        /// <param name="goItem"></param>
        /// <returns></returns>
        private bool TriggerEnemySkillGoal(StageGoalItem goItem)
        {
            bool isReach = TriggerJudge(ref goItem, EnemySkillID);
            return isReach;
        }

        /// <summary>
        /// 判斷剩餘步驟是否達到
        /// </summary>
        /// <param name="goItem"></param>
        /// <returns></returns>
        private bool TriggerLeftStepGoal(StageGoalItem goItem)
        {
            bool isReach = TriggerJudge(ref goItem, RemainSteps);
            return isReach;
        }

        private bool TriggerJudge(ref StageGoalItem goItem, int target)
        {
            bool isReach = false;
            switch (goItem.JudgeSymbol)
            {
                case 1:
                    if (goItem.ObjectNum == target)
                    {
                        isReach = true;
                    }
                    break;
                case 3:
                    if (goItem.ObjectNum < target)
                    {
                        isReach = true;
                    }
                    break;
                case 2:
                    if (goItem.ObjectNum > target)
                    {
                        isReach = true;
                    }
                    break;
            }
            return isReach;
        }


        private TileObject[,] GetTypeList(TileType type)
        {
            switch (type)
            {
                case TileType.Element:
                    return elementData;
                case TileType.Obstacle:
                    return obstacleData;
                case TileType.SeperatorH:
                    return seperatorHData;
                case TileType.SeperatorV:
                    return seperatorVData;
                case TileType.Cover:
                    return coverData;
                case TileType.Bottom:
                    return bottomData;
                case TileType.Collect:
                    return collectData;
            }
            return null;
        }


        /// <summary>
        /// 檢查勝利後剩餘的元素
        /// </summary>
        public void AddStepAward(RewardType type, int num)
        {
            if (rewards.ContainsKey(type))
            {
                int totalNum = rewards[type] + num;
                rewards[type] = totalNum;
            }
            else
            {
                rewards[type] = num;
            }
        }


        public TileObject GetStepAwardTile()
        {
            TileObject tempresult = null;
            List<TileObject> alternativeTiles = null;
            List<TileObject> BombTiles = null;
            List<TileObject> ScreenBombTiles = null;

            alternativeTiles = new List<TileObject>();
            BombTiles = new List<TileObject>();
            ScreenBombTiles = new List<TileObject>();

            for (int r = 0, numRow = elementData.GetLength(0); r < numRow; r++)
            {
                for (int c = 0, numCol = elementData.GetLength(1); c < numCol; c++)
                {
                    TileObject t = elementData[r, c];
                    if (t == null)
                        continue;

                    if (coverData[r, c] != null)
                        continue;

                    if (t.Config.DeliveryPriority == 2)
                    {
                        ScreenBombTiles.Add(t);
                    }
                    else if (t.Config.DeliveryPriority == 1)
                    {
                        BombTiles.Add(t);
                    }
                    else if (t.Config.DeliveryPriority == 3)
                    {
                        alternativeTiles.Add(t);
                    }
                }
            }

            if (BombTiles.Count > 0)
            {
                tempresult = BombTiles[UnityEngine.Random.Range(0, BombTiles.Count - 1)];
            }
            else if (ScreenBombTiles.Count > 0)
            {
                tempresult = ScreenBombTiles[UnityEngine.Random.Range(0, ScreenBombTiles.Count - 1)];
            }
            else if (alternativeTiles.Count > 0)
            {
                tempresult = alternativeTiles[UnityEngine.Random.Range(0, alternativeTiles.Count - 1)];
            }
            return tempresult;
        }

        /// <summary>
        /// 檢查關卡目標 有可能出現沒有關卡目標的情況，比如活動關卡。
        ///會被重複調用，待研究BUG TODO
        /// </summary>
        public void CheckLevelGoal()
        {
            bool flg = CurrentGoal.Count > 0;
            //先檢查勝利條件
            for (int i = 0, n = CurrentGoal.Count; i < n; i++)
            {
                StageConfig.Goal g = CurrentGoal[i];

                if (g.Type == BattleGoal.none)
                {
                    flg = false;
                    break;
                }

                if (g.Type == BattleGoal.Unit && UnitGoal < g.Num)
                {
                    flg = false;
                    break;
                }
                if (g.Type == BattleGoal.Object && ObjectGoal[g.RelativeID] < g.Num)
                {
                    flg = false;
                    break;
                }

                if (g.Type == BattleGoal.Score && Score < g.Num)
                {
                    flg = false;
                    break;
                }
            }

            if (flg)
            {
                //臨時數據，直接3星
                //CrtStar = 3;
                CountStar();
                if (eventSystem.onReachGoalEvent != null)
                {
                    if (CrtStageConfig.Mode != BattleMode.TimeLimit || (CrtStageConfig.Mode == BattleMode.TimeLimit && RemainTime <= 0))
                    {
                        ///目標達成判定成功
                        IsWin = true;
                        eventSystem.onReachGoalEvent(new ModelEventSystem.ReachGoalEventArgs());
                    }
                }
            }
            //再檢查失敗條件
            else
            {
                if ((CrtStageConfig.Mode == BattleMode.Normal && RemainSteps <= 0) ||
                    (CrtStageConfig.Mode == BattleMode.TimeLimit && RemainTime <= 0))
                {
                    //判定失敗，如果加步數會從新判定
                    IsWin = false;
                    //沒有步數，提示
                    if (eventSystem.OnStepEmpty != null)
                        eventSystem.OnStepEmpty(new ModelEventSystem.StepEmptyEventArgs());
                }
            }
        }

        public bool HasLeftStepAward()
        {
            if (CrtStageConfig.StepGift == 0)
            {
                return false;
            }
            else if (CrtStageConfig.StepGift == 1)
            {
                return true;
            }
            return false;
        }

        public int GetNoScoreGoalNum()
        {
            int num = 0;

            foreach (var go in CurrentGoal)
            {
                if (go.Type != BattleGoal.Score)
                    num++;
            }

            return num;
        }

        public bool CheckLevelGoalCon()
        {
            bool flg = true;
            //先檢查勝利條件
            for (int i = 0, n = CurrentGoal.Count; i < n; i++)
            {
                StageConfig.Goal g = CurrentGoal[i];
                if (g.Type == BattleGoal.Unit && UnitGoal < g.Num)
                {
                    flg = false;
                    break;
                }
                if (g.Type == BattleGoal.Object && ObjectGoal[g.RelativeID] < g.Num)
                {
                    flg = false;
                    break;
                }
            }

            return flg;
        }

        /// <summary>
        /// 提交戰鬥數據，會在BoardBehaviour中調用
        /// </summary>
        public void SubmitFightRequest()
        {
            ///標記不能提交直接返回///
            if (!IsCanSubmit)
                return;

            IsCanSubmit = false;

            ///增加獎勵物品後端實現//
            //AddALLReward();

            Dictionary<byte, int> tRewards = new Dictionary<byte, int>();
            foreach (var pair in rewards)
            {
                tRewards.Add((byte)pair.Key, pair.Value);
            }

            ///掉落獎勵後重新計算星數,失敗直接判定星星數為0
            if (IsWin)
                CountStar();
            else
                CrtStar = 0;

            GameController.Instance.Client.SubmitStageFightRequest(Convert.ToInt32(CrtStageConfig.ID), CrtStar, Score, Steps, tRewards);
            GameController.Instance.ModelEventSystem.OnStageRewardRespone -= OnStageReward;
            GameController.Instance.ModelEventSystem.OnStageRewardRespone += OnStageReward;
        }



        //通關，獲得獎勵返回
        private void OnStageReward(bool res, SubmitStageFightResponse obj)
        {
            GameController.Instance.ModelEventSystem.OnStageRewardRespone -= OnStageReward;
            ModelEventSystem.BattleResultEventArgs args = new ModelEventSystem.BattleResultEventArgs();
            args.Result = res;
            args.Star = obj.stage.star;

            foreach (ValueResult item in obj.valueResultResponse.list)
            {
                if (item.changeType == 0)
                {
                    continue;
                }

                switch (item.valuesType)
                {
                    case (int)RewardType.Key:
                        args.Key = item.changeValue;
                        break;
                    case (int)RewardType.UpgradeA:
                        args.UpgradeA = item.changeValue;
                        break;
                    case (int)RewardType.UpgradeB:
                        args.UpgradeB = item.changeValue;
                        break;
                    case (int)RewardType.Gem:
                        args.Gem = item.changeValue;
                        break;
                    case (int)RewardType.Coin:
                        args.Coin = item.changeValue;
                        break;
                    default:
                        break;
                }
            }
            if (eventSystem.OnBattleResult != null)
                eventSystem.OnBattleResult(args);
        }

        /// <summary>
        /// 設置剩餘時間
        /// </summary>
        /// <param name="time"></param>
        public void SetRemainTime(int time)
        {
            RemainTime = time;
        }

        /// <summary>
        /// 增加剩餘時間 修改為不自動運行計時器
        /// </summary>
        /// <param name="time"></param>
        public void AddRemainTime(int time)
        {
            RemainTime += time;
        }


        /// <summary>
        /// 啟動計時器(僅限於計時模式)
        /// </summary>
        public void StartTimer()
        {
            Q.Assert(CrtStageConfig.Mode == BattleMode.TimeLimit);
            if (RemainTime <= 0)
                return;

            if (timeModeTimer != null)
                timeModeTimer.Stop();

            timeModeTimer = new Timer(1000);
            timeModeTimer.Elapsed += OnTimeModeTimerTick;
            timeModeTimer.Start();
        }

        /// <summary>
        /// 銷毀計時器
        /// </summary>
        public void DestroyTimer()
        {
            if (timeModeTimer != null)
            {
                timeModeTimer.Stop();
                timeModeTimer.Elapsed -= OnTimeModeTimerTick;
            }
        }

        /// <summary>
        /// 設置定時器暫停
        /// </summary>
        /// <param name="isPause"></param>
        public void SetTimerPause(bool isPause)
        {
            if (timeModeTimer == null)
                return;

            if (isPause)
                timeModeTimer.Stop();
            else
                timeModeTimer.Start();
        }

        /// <summary>
        /// 時間模式計時器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimeModeTimerTick(object sender, ElapsedEventArgs e)
        {
            GameController.Instance.InvokeOnMainThread(delegate()
            {
                if (RemainTime > 0)
                {
                    RemainTime--;
                    if (eventSystem.OnLastTime != null)
                        eventSystem.OnLastTime(RemainTime);
                }
                else
                {
                    timeModeTimer.Stop();
                    if (eventSystem.OnTimeEmpty != null)
                        eventSystem.OnTimeEmpty(new ModelEventSystem.TimeEmptyEventArgs());
                }
            });
        }


        #region 測試函數

        public void __SetTileMap(TileObject[,] dataMap, TileType type)
        {
            switch (type)
            {
                case TileType.Element:
                    elementData = dataMap;
                    break;
                case TileType.Cover:
                    coverData = dataMap;
                    break;
                case TileType.Obstacle:
                    obstacleData = dataMap;
                    break;
                case TileType.SeperatorH:
                    seperatorHData = dataMap;
                    break;
                case TileType.SeperatorV:
                    seperatorVData = dataMap;
                    break;
                case TileType.Bottom:
                    bottomData = dataMap;
                    break;
            }
        }

        #endregion

    }
}
