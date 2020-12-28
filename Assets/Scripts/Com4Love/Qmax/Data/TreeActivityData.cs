using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.Stage;
using Com4Love.Qmax.Tools;
using System;
using System.Collections.Generic;

namespace Com4Love.Qmax.Data
{
    /// <summary>
    /// 大树活动的相关数据
    /// 大部分内容在 TreeInfoResponse 中获得
    /// </summary>
    public class TreeActivityData
    {
        /// <summary>
        /// 时间描述配置  比如： 上午 11:20
        /// </summary>
        //public List<string> timeList;

        ///// <summary>
        ///// 活动是否开启
        ///// 如果关闭了，则完全不会出现图标
        ///// </summary>
        //public bool IsOpen = false;

        /// <summary>
        /// 大树等级    默认用40，比较好测试
        /// </summary>
        //public byte Level = 5;
        //public int stageId;

        /// <summary>
        /// 当前活动Id。如果当前不是活动中，默认为0
        /// </summary>
        //public short PlayerActivityId = 0;

        /// <summary>
        /// 活动是否进行中
        /// </summary>
        //public bool IsStart = false;

        /// <summary>
        /// 无论是否在活动周期中，为到下次活动的秒数
        /// </summary>
        //public double secToNextAct;

        /// <summary>
        /// 如果活动周期中，该值为到活动结束的秒数；否则为-1；
        /// </summary>
        //public double secToActEnd = -1;

        /// <summary>
        /// 是否已经参加过当前活动
        /// </summary>
        //public bool hasEnteredAct = false;

        /// <summary>
        /// 上一次进入活动的时间
        /// 进入过哪天的活动。   
        /// 与PlayerActivityId一起才能确定具体是哪个活动
        /// </summary>
        //public int PlayerActivityTime = 0;

        /// <summary>
        /// 玩家是否在活动中   活动正在进行时，玩家不一定在活动中。   
        /// 活动结束时，玩家有可能提前进入并且还没有完成，也算作是在活动中
        /// </summary>
        //public bool PlayerInActivity = false;

        /// <summary>
        /// 一次活动最多打几次
        /// </summary>
        //public int MaxPlayCount;

        /// <summary>
        /// 当前活动还剩余几次
        /// </summary>
        //public int CutLeftCount;
    }
}
