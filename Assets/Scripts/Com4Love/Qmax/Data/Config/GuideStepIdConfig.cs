using UnityEngine;
using System.Collections;

namespace Com4Love.Qmax.Data.Config
{
    public class GuideStepIdConfig
    {
        public int GuideStep;
        public int GuideID;

        public GuideStepIdConfig(XMLInStream itemStream)
        {
            itemStream.Attribute("stepId", out GuideStep)
                      .Attribute("guideId", out GuideID);
        }
    }
}

