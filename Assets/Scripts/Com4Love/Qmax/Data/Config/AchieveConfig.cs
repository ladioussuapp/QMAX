
namespace Com4Love.Qmax.Data.Config
{
    public class AchieveConfig
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
        public int AchieveStringId;

        /// <summary>
        /// 目标
        /// </summary>
        public int Target;

        /// <summary>
        /// 弹窗标题
        /// </summary>
        public int NameId;

        /// <summary>
        /// 物品描述
        /// </summary>
        public int ContentId;


        public AchieveConfig(XMLInStream inStream)
        {
            int AchieveId, RewardCount, Target, AchieveStringId, NameId, ContentId;
            string RewardIcon;
            float iconScale;
            inStream = inStream.Attribute("achieveId", out AchieveId);
            inStream = inStream.Attribute("achieveNum", out RewardCount);
            inStream = inStream.Attribute("resourceIcon", out RewardIcon);
            inStream = inStream.Attribute("achieveStringId", out AchieveStringId);
            inStream = inStream.Attribute("iconScale", out iconScale);
            inStream = inStream.Attribute("target", out Target);
            inStream = inStream.Attribute("nameId", out NameId);
            inStream = inStream.Attribute("contentId", out ContentId);

            this.Target = Target;
            this.AchieveId = AchieveId;
            this.RewardIcon = RewardIcon;
            this.RewardCount = RewardCount;
            this.IconScale = iconScale;
            this.AchieveStringId = AchieveStringId;
            this.NameId = NameId;
            this.ContentId = ContentId;
        }

    }
}