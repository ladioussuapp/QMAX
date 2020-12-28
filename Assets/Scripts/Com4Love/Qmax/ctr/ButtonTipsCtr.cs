using UnityEngine;
using System.Collections;

namespace Com4Love.Qmax.Ctr
{
    public class ButtonTipsCtr
    {

        public Transform TipsParent;

        public UIBattleTipsDialog LastTips;

        public int[] StarScore;

        public void Clear()
        {
            TipsParent = null;
            LastTips = null;
            StarScore = null;
        }
    }
}


