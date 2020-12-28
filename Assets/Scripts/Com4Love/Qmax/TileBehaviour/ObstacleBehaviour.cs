﻿using Com4Love.Qmax.Data.VO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.TileBehaviour
{
    /// <summary>
    /// 障礙物行為類
    /// </summary>
    class ObstacleBehaviour : BaseTileBehaviour
    {
        public override bool Eliminate(TileObject newData)
        {
            IsLinked = false;

            bool ret = newData == null;//newData == null表示已經被移除
            //不會被消除的障礙物
            if (newData != null)
            {
                //變換到下一個狀態，並不會被移除
                Data = newData;
            }
            if (viewEvtSys.TileStatusChangeEvent != null)
                viewEvtSys.TileStatusChangeEvent(this.Data, ViewEventSystem.TileStatusChangeMode.ChangeData, null);
            return ret;
        }
    }
}