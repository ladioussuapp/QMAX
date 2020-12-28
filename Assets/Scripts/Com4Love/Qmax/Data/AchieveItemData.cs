using UnityEngine;
using System;
using System.Collections;

namespace Com4Love.Qmax.Data
{
    public class AchieveItemData : IComparable
    {

        /// <summary>
        /// 成就ID
        /// </summary>
        public int AchieveId;

        /// <summary>
        /// 奖励图标
        /// </summary>
        public string RewardIcon;

        /// <summary>
        /// 图标缩放系数
        /// </summary>
        public float IconScale;

        /// <summary>
        /// 奖励数量
        /// </summary>
        public int RewardCount;

        /// <summary>
        /// 成就描述
        /// </summary>
        public string AchieveDesc;

        /// <summary>
        /// 进度
        /// </summary>
        public int Progress;

        /// <summary>
        /// 目标
        /// </summary>
        public int Target;

        /// <summary>
        /// 状态(1-未达成，2-达成，3-已领取)
        /// </summary>
        public int Status;

        /// <summary>
        /// 弹窗标题
        /// </summary>
        public int NameId;

        /// <summary>
        /// 物品描述
        /// </summary>
        public int ContentId;

        /// <summary>
        /// 逻辑层
        /// </summary>
        public AchieveItemBehaviour Beh;

        public int CompareTo(object obj)
        {
            try
            {
                AchieveItemData item = obj as AchieveItemData;
                if (this.Status == item.Status)
                {
                    if (this.AchieveId < item.AchieveId)
                        return -1;
                    else
                        return 1;
                }
                else
                {
                    // 已完成
                    if (this.Status == 2)
                        return -1;
                    else if (item.Status == 2)
                        return 1;
                    // 已领取
                    else if (this.Status == 3)
                        return 1;
                    else if (item.Status == 3)
                        return -1;
                }
                return 0;
            } catch (Exception ex) { throw new Exception(ex.Message); }
        }
    }
}
