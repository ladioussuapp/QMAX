
namespace Com4Love.Qmax.Data.Config
{
    public class ExchangeCodeConfig
    {
        /// <summary>
        /// 礼包ID///
        /// </summary>
        public int GiftID;
        /// <summary>
        /// 标题id//
        /// </summary>
        public int TitleID;
        /// <summary>
        /// 介绍信息ID///
        /// </summary>
        public int InfoID;

        /// <summary>
        /// 图片路径///
        /// </summary>
        public string TipsID;

        /// <summary>
        /// 礼包兑换奖励//
        /// </summary>
        /// <param name="inStream"></param>
        /// 
        public string ExchangeGift;

        /// <summary>
        /// 礼包兑换奖励类型//
        /// </summary>
        /// <param name="inStream"></param>
        /// 
        public string ExchangeType;

        public ExchangeCodeConfig(XMLInStream inStream)
        {
            inStream.Attribute("id", out GiftID)
                .Attribute("type", out ExchangeType)
                .Attribute("nameId", out TitleID)
                .Attribute("contentId", out InfoID)
                .Attribute("tipsId", out TipsID);
        }

    }
}

