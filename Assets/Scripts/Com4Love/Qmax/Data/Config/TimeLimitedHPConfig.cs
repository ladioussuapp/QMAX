
namespace Com4Love.Qmax.Data.Config
{
    public class TimeLimitedHPConfig
    {
        public int ID;
        public int Level;
        public int Hp;
        public int hpTotal;     //到當前lvl的總HP數。幫助計算用
        public int UpgradeA;
        public int UpgradeB;
        public int Gem;
        public int Key;
        public int LvupA;
        public int LvupB;

        public TimeLimitedHPConfig(XMLInStream inStream)
        {
            inStream.Attribute("id", out ID)
                .Attribute("level", out Level)
                .Attribute("hp", out Hp)
                .Attribute("hp_total" , out hpTotal)
                .Attribute("UpgradeA", out UpgradeA)
                .Attribute("UpgradeB", out UpgradeB)
                .Attribute("Gem", out Gem)
                .Attribute("key", out Key)
                .Attribute("lvupA", out LvupA)
                .Attribute("lvupB", out LvupB);
        }
    }
}
