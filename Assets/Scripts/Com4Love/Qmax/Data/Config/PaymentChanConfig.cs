/********************************************************************************
** auth： johnsonybq
** date： 2016-1-21 17:50:55
** FileName：PaymentChanConfig
** desc： 尚未编写描述
** Ver.:  V1.0.0
*********************************************************************************/
namespace Com4Love.Qmax.Data.Config
{
    public class PaymentChanConfig
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int ID;

        /// <summary>
        /// 是否直购
        /// </summary>
        public int PaymentChan;

        /// <summary>
        /// SDK渠道标识
        /// </summary>
        public string ChannelSdk;

        public PaymentChanConfig(XMLInStream inStream)
        {
            inStream.Attribute("Id", out ID)
                .Attribute("paymentChan", out PaymentChan)
                .Attribute("channelSdk", out ChannelSdk);
        }
    }
}

   
