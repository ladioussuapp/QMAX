using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com4Love.Qmax.Data.Config
{
    public class TimeLimitedSummaryConfig
    {
        public int ID;
        public int DamageTotal;
        public int UpgradeA;
        public int UpgradeB;

        public TimeLimitedSummaryConfig(XMLInStream inStream)
        {
            inStream.Attribute("id", out ID)
                .Attribute("damage_total", out DamageTotal)
                .Attribute("UpgradeA", out UpgradeA)
                .Attribute("UpgradeB", out UpgradeB);
        }
    }
}
