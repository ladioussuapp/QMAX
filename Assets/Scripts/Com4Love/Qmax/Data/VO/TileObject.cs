using Com4Love.Qmax.Data.Config;

namespace Com4Love.Qmax.Data.VO
{
    public class TileObject
    {
        public int ConfigID
        {
            get { return _config == null ? -1 : _config.ID; }
        }

        private TileObjectConfig _config;
        public TileObjectConfig Config
        {
            set
            {
                _config = value;
                if (EliminateCount == 0)
                    OriID = _config == null ? -1 : _config.ID;
            }

            get { return _config; }
        }

        /// <summary>
        /// 已經被消除了幾次
        /// </summary>
        public int EliminateCount;

        /// <summary>
        /// 可以被消除多次的地形物，可能每被消除一次，會Config會改變。
        /// 這個ID保存的是未被消除前（EliminateCount==0）的ConfigID
        /// </summary>
        public int OriID = -1;

        public int Row, Col;

        public TileObject() { }

        public TileObject(TileObject src)
        {
            this.Row = src.Row;
            this.Col = src.Col;
            this.EliminateCount = src.EliminateCount;
            this.Config = src.Config;
            this.OriID = src.OriID;
        }


        public TileObject(int row, int col, TileObjectConfig config = null)
        {
            Row = row;
            Col = col;
            Config = config;
            EliminateCount = 0;
            OriID = ConfigID;
        }
    }
}
