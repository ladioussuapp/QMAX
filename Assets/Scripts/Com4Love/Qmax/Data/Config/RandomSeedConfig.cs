/********************************************************************************
** auth： johnsonybq
** date： 2015/8/4 星期二 16:36:29
** FileName：RandomSeed
** desc： 战斗消除物掉落种子表
** Ver.:  V1.0.0
*********************************************************************************/

using System.Collections.Generic;

namespace Com4Love.Qmax.Data.Config
{
    public enum TriggerRateType
    {
        NONE,
        StageGold,
        EnemySkill,
        LeftStep,
        LeftElement
    }

    public struct StageGoalItem
    {
        public int ObjectId;
        public int JudgeSymbol;
        public int ObjectNum;

        public void ParseItemData(TriggerRateType type, string data)
        {
            string[] items = data.Split(',');
            int count = items.Length;
            if (count == 3)
            {
                ObjectId = int.Parse(items[0]);
                JudgeSymbol = int.Parse(items[1]);
                ObjectNum = int.Parse(items[2]);
            }
        }
    }
    public class RandomSeedConfig
    {
        /// <summary>
        /// 种子类型
        /// </summary>
        public int SeedType;
        /// <summary>
        /// 种子ID
        /// </summary>
        public int SeedId;
        /// <summary>
        /// 对应物品ID
        /// </summary>
        public int ObjectId;
        /// <summary>
        /// 基础权重
        /// </summary>
        public int SeedWeight;
        /// <summary>
        /// 胜利权重比率
        /// </summary>
        public float WinRateWeight;
        /// <summary>
        /// 失败权重比率
        /// </summary>
        public float AbortionWeight;
        /// <summary>
        /// 动态权重条件类型
        /// </summary>
        public Dictionary<TriggerRateType, StageGoalItem> TriggerItems;
        /// <summary>
        /// 动态权重参数1
        /// </summary>
        public string Arg0;
        /// <summary>
        /// 动态权重参数2
        /// </summary>
        public string Arg1;
        /// <summary>
        /// 动态权重参数3
        /// </summary>
        public string Arg2;
        /// <summary>
        /// 判断是不是基础权重
        /// </summary>
        public bool isBaseWeight;

        /// <summary>
        /// 单次产生的最大数量
        /// </summary>
        public int ObjectNum;
        /// <param name="inStream"></param>

        public RandomSeedConfig(XMLInStream inStream)
        {
            isBaseWeight = true;
            string ConditionExpr;
            inStream.Attribute("seedType", out SeedType)
                .Attribute("seedId", out SeedId)
                .Attribute("objectId", out ObjectId)
                .Attribute("seedWeight", out SeedWeight)
                .Attribute("winrateWeight", out WinRateWeight)
                .Attribute("abortionWeight", out AbortionWeight)
                .Attribute("expr", out ConditionExpr)
                .Attribute("arg0", out Arg0)
                .Attribute("arg1", out Arg1)
                .Attribute("arg2", out Arg2)
                .Attribute("objectNum", out ObjectNum);

            TriggerItems = new Dictionary<TriggerRateType,StageGoalItem>();
            if(ConditionExpr == "0")
            {
                isBaseWeight = true;
            }
            else
            {
                isBaseWeight = false;
                string[] condis = ConditionExpr.Split(',');
                for (int i = 0; i < condis.Length; i++)
                {
                    TriggerRateType type = (TriggerRateType)int.Parse(condis[i]);
                    StageGoalItem item = new StageGoalItem();
                    if (i == 0)
                    {
                        item.ParseItemData(type, Arg0);
                    }
                    else if (i == 1)
                    {
                        item.ParseItemData(type, Arg1);
                    }
                    else if (i == 2)
                    {
                        item.ParseItemData(type, Arg2);
                    }
                    TriggerItems.Add(type, item);
                }
            }
        }
    }
}
