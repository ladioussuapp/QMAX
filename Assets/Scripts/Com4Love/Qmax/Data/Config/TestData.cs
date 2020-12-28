using System;
using System.Collections.Generic;

namespace Com4Love.Qmax.Data.Config
{
    public class TestData
    {
        public List<int> OwnUnits;
        public List<int> LastFightUnits;
        public int PassStageId;
        public int CrtEnergy;
        public int MaxEnergy;
        public int Gem;
        public int UpgradeA;
        public int UpgradeB;

        public TestData(XMLInStream inStream)
        {
            string ownUnits, lastFightUnits;
            inStream.Attribute("ownUnits", out ownUnits)
                .Attribute("lastFightUnits", out lastFightUnits)
                .Attribute("passStageId", out PassStageId)
                .Attribute("crtEnergy", out CrtEnergy)
                .Attribute("maxEnergy", out MaxEnergy)
                .Attribute("gem", out Gem)
                .Attribute("upgradeA", out UpgradeA)
                .Attribute("upgradeB", out UpgradeB);

            OwnUnits = new List<int>();
            string[] arr = ownUnits.Split(',');
            for (int i = 0, n = arr.Length; i < n; i++)
            {
                OwnUnits.Add(Convert.ToInt32(arr[i]));
            }

            LastFightUnits = new List<int>();
            arr = lastFightUnits.Split(',');
            for (int i = 0, n = arr.Length; i < n; i++)
            {
                LastFightUnits.Add(Convert.ToInt32(arr[i]));
            }
        }

    }
}
