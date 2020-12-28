
namespace Com4Love.Qmax.Data.Config
{
    /// <summary>
    /// 连击表
    /// </summary>
    public class ComboConfig
    {
        public int Combo;
        public float ComboRate;
        public int ComboLevel = 0;
        public ComboConfig(XMLInStream inStream)
        {
            inStream.Attribute("combo", out Combo)
                .Attribute("comboRate", out ComboRate)
                .Attribute("comboLevel", out ComboLevel);
        }
    }
}
