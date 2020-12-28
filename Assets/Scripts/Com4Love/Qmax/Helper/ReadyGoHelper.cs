using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Data;
using System;

namespace Com4Love.Qmax.Helper
{
    public class ReadyGoHelper
    {

        // 打開界面時，排除這些ID暫停時間模式的計時
        private static PopupID[] openExclude = { PopupID.LightLoading };

        // 關閉界面時，排除這些ID彈出ReadyGo
        private static PopupID[] closeExclude = { PopupID.LightLoading, PopupID.UIAddMove, PopupID.UIReconnect, PopupID.UIGetChance, PopupID.UIGoalTip };

        public static bool OnUIPopupOpen(PopupID id, BattleModel model)
        {
            foreach (PopupID popup in openExclude)
            {
                if (popup == id)
                    return false;
            }
            model.SetTimerPause(true);
            return true;
        }

        public static bool OnUIPopupClose(PopupID id, BattleModel model, UIBattleBehaviour beh, ViewEventSystem eventSystem)
        {
            foreach (PopupID popup in closeExclude)
            {
                if (popup == id)
                    return false;
            }
            Action readyEnd = null;
            readyEnd = delegate ()
            {
                eventSystem.ReadyGo -= readyEnd;
                model.SetTimerPause(false);
            };
            eventSystem.ReadyGo += readyEnd;
            beh.PlayReadyTime();
            return true;
        }


    }

}