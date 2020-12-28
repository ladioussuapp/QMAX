using Com4Love.Qmax.Data.Config;
using UnityEngine;

namespace Com4Love.Qmax.Data
{
    [System.Serializable]
    public class BattlePathData
    {
        public float speed = 0;
        public Transform[] PathPoints;
        public iTween.EaseType EaseType = iTween.EaseType.easeInOutCirc;
        public bool isConvey = false;
    }
}
