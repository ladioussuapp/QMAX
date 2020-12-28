
namespace Com4Love.Qmax.Data.Config
{
    public class ShopConfig
    {
        public int UID;
        /// <summary>
        /// 1套装     2道具
        /// </summary>
        public int Tab;
        public int SortId;

        public int GoodsId;

        public int GoodsStringId;

        public int GoodsContentId;

        public int Coin;

        public int Gem;

        public int ByNum;

        public string ShopIcon;

        public ShopConfig(XMLInStream inStream)
        {
            inStream.Attribute("id", out UID)
                .Attribute("sortId", out SortId)
                .Attribute("goodsId", out GoodsId)
                .Attribute("tab" , out Tab)
                .Attribute("goodsStringId", out GoodsStringId)
                .Attribute("goodsContentId", out GoodsContentId)
                .Attribute("coin", out Coin)
                .Attribute("gem", out Gem)
                .Attribute("buyNum", out ByNum)
                 .Attribute("shopIcon", out ShopIcon);

        }
    }
}
