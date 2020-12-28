using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com4Love.Qmax.Data.Config
{
    public class PaymentSystemConfig
    {
        public string PaymentId;
        public int SortId;
        public string PaymentStringId;
        public int GoodsID;
        public int InCombat;
        public int Rmb;
        public int BuyNum;
        public string BuyGemIcon;
        public int BuyGemSale;
        public string BuyGemSaleIcon;


        public PaymentSystemConfig(XMLInStream inStream)
        {
            inStream.Attribute("paymentId", out PaymentId)
                .Attribute("sortId", out SortId)
                .Attribute("goodsId", out GoodsID)
                .Attribute("paymentStringId", out PaymentStringId)
                .Attribute("rmb", out Rmb)
                .Attribute("inCombat", out InCombat)
                .Attribute("buyNum", out BuyNum)
                .Attribute("buyNum", out BuyNum)
                .Attribute("buyGemIcon", out BuyGemIcon)
                .Attribute("buyGemSale", out BuyGemSale)
                .Attribute("buyGemSaleIcon", out BuyGemSaleIcon);
        }
    }
}
