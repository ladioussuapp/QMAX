using System.Collections.Generic;

namespace Com4Love.Qmax.Data.Config
{
    /// <summary>
    /// 關卡陣型配置
    /// </summary>
    public class LevelConfig
    {
        /// <summary>
        /// 列數
        /// </summary>
        public int NumCol;
        /// <summary>
        /// 行數
        /// </summary>
        public int NumRow;

        /// <summary>
        /// 消除物層
        /// </summary>
        public int[,] ElementLayer;
        /// <summary>
        /// 障礙層
        /// </summary>
        public int[,] ObstacleLayer;
        /// <summary>
        /// 覆蓋物層
        /// </summary>
        public int[,] CoveringLayer;
        /// <summary>
        /// 橫向間隔物層
        /// </summary>
        public int[,] SeperatorHLayer;
        /// <summary>
        /// 縱向間隔物
        /// </summary>
        public int[,] SeperatorVLayer;
        /// <summary>
        /// 冰塊層
        /// </summary>
        public int[,] BottomLayer;
        /// <summary>
        /// 收集層
        /// </summary>
        public int[,] CollectLayer;
    }
}
