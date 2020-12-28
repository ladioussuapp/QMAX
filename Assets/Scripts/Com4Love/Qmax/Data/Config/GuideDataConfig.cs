using System.Collections.Generic;

namespace Com4Love.Qmax.Data.Config
{
    /// <summary>
    /// 新手引导
    /// </summary>
    public class GuideDataConfig
    {
        public int Level;
        //public List<int[]> GuidePoints;
        public Dictionary<int, List<int[]>> GuidePoints;

        public GuideDataConfig()
        {
            GuidePoints = new Dictionary<int, List<int[]>>();
            //GuidePoints = guidePoints;
        }

        public void AddGuideConfig(int level, List<int[]> guidePoints)
        {
            Level = level;
            if (GuidePoints.ContainsKey(level))
            {
                GuidePoints[level] = guidePoints;
            }
            else
            {
                GuidePoints.Add(level, guidePoints);
            }
        }

        public List<int[]> getCurrentPoints(int stageId)
        {
            if(GuidePoints.ContainsKey(stageId))
            {
                return GuidePoints[stageId];
            }
            return null;
        }

        public int[] GetGuidePointsAt(int stageId,int step){
            
            if(GuidePoints.ContainsKey(stageId))
            {
                List<int[]> stepList = GuidePoints[stageId];
                if (step >=0 && step < GuidePoints.Count)
                {
                    return stepList[step];
                }
                else
                {
                    return null;
                }
            }
            
            return null;
        }

        public int GetStepLength(int stageId)
        {
            if(GuidePoints.ContainsKey(stageId))
            {
                return GuidePoints[stageId].Count;
            }
            return 0;
        }
    }
}
