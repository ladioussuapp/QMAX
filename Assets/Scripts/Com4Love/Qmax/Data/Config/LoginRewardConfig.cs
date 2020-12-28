
namespace Com4Love.Qmax.Data.Config
{
    public class LoginRewardConfig
    {
        public struct GiftItem
        {
            public int GiftId;
            public int AmountOrUnitId;
        }

        public int Id;
        public int Day;
        public bool IsUnitGift;
        public GiftItem[] LoginGifts;  
        public GiftItem GemGift;    //如果是伙伴奖励，并且玩家已经拥有此伙伴时，使用此奖励代替。 并且每拥有一只伙伴，多奖励一份。

        /// <summary>
        /// 弹窗标题和介绍对应的语言ID///
        /// </summary>
        public int TitleID;
        public int TitleID2;

        public int InfoID;
        /// <summary>
        /// 如果是伙伴奖励并且已经拥有伙伴显示信息语言ID//
        /// </summary>
        public int InfoID2;

        public string Icon;
        public string Icon2;        //如果是伙伴奖励，并且玩家已经拥有所有的伙伴时，改为显示此icon图片
 
        public LoginRewardConfig(XMLInStream inStream)
        {
            string loginGiftString;
            string IconString;
            string gemGiftItemString;
            string loginGiftItemString;
            string goodsGiftString;


            inStream.Attribute("id", out Id)
                .Attribute("day", out Day)
                .Attribute("icon", out IconString)
                .Attribute("loginGift", out loginGiftString)
                .Attribute("gemGift", out gemGiftItemString)
                .Attribute("goodsGift", out goodsGiftString)
                .Attribute("nameId1", out TitleID)
                .Attribute("nameId2", out TitleID2)
                .Attribute("contentId1", out InfoID)
                .Attribute("contentId2", out InfoID2);

            string[] loginGiftArr = new string[0];

            if (!string.IsNullOrEmpty(loginGiftString))
            {
                loginGiftArr = loginGiftString.Split('|');
            }
            else if (!string.IsNullOrEmpty(goodsGiftString))
            {
                loginGiftArr = goodsGiftString.Split('|');
            }

            
            LoginGifts = new GiftItem[loginGiftArr.Length];

            for (int i = 0; i < LoginGifts.Length; i++)
            {
                loginGiftItemString = loginGiftArr[i];
                string[] loginGiftItemArr = loginGiftItemString.Split(',');
                GiftItem item = new GiftItem();
                item.GiftId = int.Parse(loginGiftItemArr[0]);
                item.AmountOrUnitId = int.Parse(loginGiftItemArr[1]);
                IsUnitGift = item.GiftId == 7;      //有一个伙伴则此奖励就为伙伴奖励
                LoginGifts[i] = item;
            }

            GemGift = new GiftItem();

            if (!string.IsNullOrEmpty(gemGiftItemString))
            {
                string[] gemGiftItemArr = gemGiftItemString.Split(',');
                GemGift.GiftId = int.Parse(gemGiftItemArr[0]);
                GemGift.AmountOrUnitId = int.Parse(gemGiftItemArr[1]);
            }
            else
            {
                GemGift.AmountOrUnitId = GemGift.GiftId = -1;
            }

            string[] iconArr = IconString.Split('|');

            if (iconArr.Length > 1)
            {
                Icon = iconArr[0];
                Icon2 = iconArr[1];
            }
            else
            {
                Icon = IconString;
            }
        }
    }
}
