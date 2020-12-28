using Com4Love.Qmax.Data.VO;
using System;
using System.Collections.Generic;
namespace Com4Love.Qmax.Data.Config
{
    public class StageConfig
    {
        //大樹活動的stage表id
        //public const int TREEACTIVITY_STAGEID = 10001;

        public int ID;
        public int ShowNum;
        public int TypeId;
        public string Name;
        public string NameStringID;

        /// <summary>
        /// 關卡開啟條件， 通關限制單獨提出來。
        /// </summary>
        public StageUnlock[] Unlocks;

        /// <summary>
        /// 關卡開啟條件中的通過上一關卡限制
        /// </summary>
        public int StagePassedUnlock = -1;

        /// <summary>
        /// 消耗寶石    
        /// [0] 物品id 暫時只有鑽石
        /// [1] 數量
        /// </summary>
        public ItemQtt CostGem;
        /// <summary>
        /// 消耗體力
        /// </summary>
        public int CostEnergy;

        /// <summary>
        /// 1.一般模式
        ///2.限時模式
        /// </summary>
        public BattleMode Mode;

        /// <summary>
        /// (類型,索引號,數量)
        ///類型1=怪物 unit
        ///類型2=物件 object
        /// </summary>
        public int[] MonsterUnitID;

        public int RandomSeed;
        /// <summary>
        /// 初始配置表
        /// </summary>
        public string GameSetting;

        /// <summary>
        /// 背景路径
        /// </summary>
        public string Set;

        /// <summary>
        /// 起始怪索引
        /// </summary>
        public int SamplingId;

        /// <summary>
        /// 場景地圖路徑
        /// </summary>
        public string Map;

        /// <summary>
        /// 地圖填充物ID
        /// </summary>
        public int[] DecorateObjIds;

        /// <summary>
        /// 關卡音樂
        /// </summary>
        public string Gamemusic;

        /// <summary>
        /// 關卡按鈕上的夥伴提示頭像
        /// </summary>
        public int UnitHeadId;

        /// <summary>
        /// 關卡數量
        /// </summary>
        public int SettingNum;

        public Dictionary<int, RandomSeedConfig> baseRandomSeeds;

        public Dictionary<int, RandomSeedConfig> dynamicRandomSeeds;
        /// <summary>
        /// 戰鬥引導對話v1
        /// </summary>
        public string DialogId_v1;
        /// <summary>
        /// 戰鬥引導對話
        /// </summary>
        public string DialogId_v2;

        public int StepGift;
        public string[] TipsIds;

        /// <summary>
        /// 1=顯示目標和分數// 
        ///2=顯示目標 //
        ///3=顯示分數//
        /// </summary>//
        public int InterfaceType;

        public StageConfig(XMLInStream inStream, Dictionary<string, RandomSeedConfig> RandomSeedConfigs)
        {
            string monsterUnitID, costgem;
            int mode;
            string unlockStr;
            string decorateObjId;
            string TipsIdStr;

            inStream.Attribute("id", out ID);

            inStream.Attribute("id", out ID)
                .Attribute("showNumber", out ShowNum)
                .Attribute("typeId", out TypeId)
                .Attribute("nameStringId", out NameStringID)
                .Attribute("unlock", out unlockStr)
                .Attribute("costgem", out costgem)
                .Attribute("costEnergy", out CostEnergy)
                .Attribute("gameMode", out mode)
                .Attribute("interface", out InterfaceType)
                .Attribute("randomSeed", out RandomSeed)
                .Attribute("monsterUnitId", out monsterUnitID)
                .Attribute("set", out Set)
                .Attribute("samplingId", out SamplingId)
                .Attribute("map", out Map)
                .Attribute("decorateObjId", out decorateObjId)
                .Attribute("gamemusic", out Gamemusic)
                .Attribute("stepGift", out StepGift)
                .Attribute("unitHeadId", out UnitHeadId)
                .Attribute("gameSetting", out GameSetting)
                .Attribute("settingNum", out SettingNum)
                .Attribute("DialogId_v1", out DialogId_v1)
                .Attribute("DialogId_v2", out DialogId_v2)
                .Attribute("tipsId", out TipsIdStr);

            //GoalStr1 = goal1;
            //GoalStr2 = goal2;
            //GoalStr3 = goal3;
            CostGem = new ItemQtt();
            if (costgem != null && costgem != "")
            {
                string[] arr = costgem.Split(',');
                CostGem.type = (RewardType)Convert.ToInt32(arr[0]);
                CostGem.Qtt = Convert.ToInt32(arr[1]);
            }

            string[] decorateIDs = decorateObjId.Split(',');
            DecorateObjIds = new int[3];
            for (int ii = 0; ii < decorateIDs.Length; ii++)
            {
                DecorateObjIds[ii] = int.Parse(decorateIDs[ii]);
            }

            MonsterUnitID = ParseMonster(monsterUnitID);
            Mode = (BattleMode)mode;
            Unlocks = ParseUnlocks(unlockStr);

            baseRandomSeeds = new Dictionary<int, RandomSeedConfig>();

            dynamicRandomSeeds = new Dictionary<int, RandomSeedConfig>();
            foreach (RandomSeedConfig item in RandomSeedConfigs.Values)
            {
                if (RandomSeed == item.SeedType)
                {
                    if (item.isBaseWeight)
                    {
                        baseRandomSeeds.Add(item.SeedId, item);
                    }
                    else
                    {
                        dynamicRandomSeeds.Add(item.SeedId, item);
                    }
                }
            }

            TipsIds = TipsIdStr.Split(',');
        }

        private StageUnlock[] ParseUnlocks(string val)
        {
            if (val == "")
            {
                return new StageUnlock[0];
            }

            string[] arr = val.Split('|');
            string[] paramArr;
            StageUnlock unlock;
            StageUnlock[] unlocks = new StageUnlock[arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                string param = arr[i];

                paramArr = param.Split(',');
                unlock = new StageUnlock();
                unlock.Type = int.Parse(paramArr[0]);

                if (unlock.Type == 3)
                {
                    //特殊限制，暫時只有大樹關卡。暫時直接return;
                    return unlocks;
                }

                unlock.param = int.Parse(paramArr[1]);
                unlocks[i] = unlock;

                if (unlock.Type == 1)
                {
                    //關卡限制
                    StagePassedUnlock = unlock.param;
                }
            }

            return unlocks;
        }

        private int[] ParseMonster(string value)
        {
            if (value == "")
                return new int[0];
            string[] arr = value.Split(',');
            int[] ret = new int[arr.Length];
            for (int i = 0, n = arr.Length; i < n; i++)
            {
                ret[i] = Convert.ToInt32(arr[i]);
            }
            return ret;
        }

        //private List<Goal> ParseGoal(string value)
        //{
        //    List<Goal> ret = new List<Goal>();
        //    //1,0,6|2,222,3|2,227,1
        //    string[] arr = value.Split('|');
        //    for (int i = 0, n = arr.Length; i < n; i++)
        //    {
        //        if (arr[i] == "")
        //        {
        //            continue;
        //        }

        //        string[] arr2 = arr[i].Split(',');
        //        Goal g = new Goal();

        //        g.Type = (BattleGoal)Convert.ToInt32(arr2[0]);
        //        g.RelativeID = Convert.ToInt32(arr2[1]);
        //        g.Num = Convert.ToInt32(arr2[2]);
        //        ret.Add(g);
        //    }
        //    return ret;
        //}


        /// <summary>
        /// 勝利條件
        /// </summary>
        public class Goal
        {
            /// <summary>
            /// 勝利條件類型
            /// </summary>
            public BattleGoal Type;
            /// <summary>
            /// 相關索引，TileObjectConfig ID
            /// </summary>
            public int RelativeID;
            /// <summary>
            /// 相關數量
            /// </summary>
            public int Num;

            public Goal CopyTo()
            {
                StageConfig.Goal go = new StageConfig.Goal();
                go.Num = this.Num;
                go.RelativeID = this.RelativeID;
                go.Type = this.Type;

                return go;
            }
        }

        /// <summary>
        /// 開啟關卡的條件
        /// </summary>
        public struct StageUnlock
        {

            /// <summary>
            /// 關卡開啟條件
            /// 1.過完前一關
            ///2.VIP等級
            ///3.特殊條件
            ///4.夥伴數量
            ///5.夥伴等級
            ///6.星星數量
            /// </summary>
            public int Type;
            public int param;
        }
    }
}
