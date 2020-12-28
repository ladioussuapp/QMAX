using Com4Love.Qmax.Data.Config;
namespace Com4Love.Qmax.Data.VO
{
    public class Unit
    {
        public UnitConfig Config;

        public int Hp;
        /// <summary>
        /// 最大血量///
        /// </summary>
        public int HpMax;

        public Unit(UnitConfig config)
        {
            Config = config;

            //初始為滿血狀態
            HpMax = config.UnitHp;
            Hp = HpMax;
        }
    }
}
