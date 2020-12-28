
namespace Com4Love.Qmax.Data.Config
{
    public class SkillConfig
    {
        public int ID;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string SkillStringId;
        /// <summary>
        /// 效果编号    暂时直接读
        /// </summary>
        public string EffectStringId;
        /// <summary>
        /// 1.伙伴技能
        ///2.敌方技能
        ///3.护盾技能
        ///4.主動技能
        /// </summary>
        public SkillType SkillType;
        /// <summary>
        /// 需要消除几个才能施放
        /// </summary>
        public int SkillCD;
        /// <summary>
        /// 1土/紫,2火/红,3木/绿,4金/黄,5水/蓝
        /// </summary>
        public ColorType SkillColor;
        /// <summary>
        /// 技能图标的路径
        /// </summary>
        public string ResourceIcon;
        /// <summary>
        /// 特效路径
        /// </summary>
        public string ResourceEffect;

        /// <summary>
        /// 目前是对应的TileObjectId
        /// </summary>
        public int arg0;

        public SkillConfig(XMLInStream inStream)
        {
            int skillColor, skillType;
            inStream = inStream.Attribute("skillId", out ID);
            inStream = inStream.Attribute("skillStringId", out SkillStringId);
            inStream = inStream.Attribute("effectStringId", out EffectStringId);

            inStream = inStream.Attribute("skillType", out skillType);
            inStream = inStream.Attribute("skillCd", out SkillCD);
            inStream = inStream.Attribute("skillColor", out skillColor);

            inStream = inStream.Attribute("resourceIcon", out ResourceIcon);
            inStream = inStream.Attribute("resourceEffect", out ResourceEffect);
            inStream = inStream.Attribute("arg0", out arg0);
            SkillColor = (ColorType)skillColor;
            SkillType = (SkillType)skillType;
        }
    }
}
