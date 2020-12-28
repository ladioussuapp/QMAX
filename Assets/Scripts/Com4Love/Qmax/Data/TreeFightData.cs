using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com4Love.Qmax.Data
{
    public class TreeFightData
    {
       /// <summary>
       /// 当前正在打的关卡
       /// </summary>
       public int cutStageId;

        /// <summary>
        /// 大树的总血量（能量上限）
        /// </summary>
        public int MaxHp = 0;

        /// <summary>
        /// 大树等级
        /// </summary>
        public int MaxLevel = 0;
        
        ///// <summary>
        ///// 大树扣去的血量，（充能的能量值）
        ///// </summary>
        //public int LoseHp = 0;

        /// <summary>
        /// 在等级内的扣血血量
        /// </summary>
        public int LoseHpInLevel = 0;
        public int LoseHpInGroup = 0;
        /// <summary>
        /// 3个配置内的预扣血血量
        /// </summary>
        public int LoseHpInGroupPreView = 0;


        ///// <summary>
        ///// 当前到第几级
        ///// </summary>
        //public int CutLevel = 1;

        /// <summary>
        /// 被攻击的血量总和  会超过大树的总血量，此时没有多余的血量奖励，但是最后会计算攻击力奖励
        /// </summary>
        public int DamageTotal = 0;

        /// <summary>
        /// 倒计时
        /// </summary>
        public double TimeLeft = double.MaxValue;

        /// <summary>
        /// 奖励数据
        /// </summary>
        public TreeFightAwardData AwardData;
    }

    public class TreeFightAwardData
    {
        //橘子桃子钻石
        public int UpgradeA = 0;
        public int UpgradeB = 0;
        public int Gem = 0;

        public override string ToString()
        {
            return string.Format("UpgradeA:{0}     UpgradeB:{1}    Gem:{2}", UpgradeA , UpgradeB, Gem);
        }
    }
}
