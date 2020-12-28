using Com4Love.Qmax.Data.VO;

namespace Com4Love.Qmax.TileBehaviour
{
    class SeperatorBehaviour : BaseTileBehaviour
    {
        public override bool Eliminate(TileObject newData)
        {
            IsLinked = false;

            bool ret = newData == null;//newData == null表示已經被移除

            //不會被消除的間隔物
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
