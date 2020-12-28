
using System.Collections.Generic;
namespace Com4Love.Qmax.Data.Config
{
    public class GameSystemConfig
    {
        /// <summary>
        /// 钻石可购上限
        /// 玩家可最高靠钻石扩充体力到几点
        /// </summary>
        public int gemMaxEnergy;

        /// <summary>
        /// 道具可扩上限
        /// 玩家可最高靠钻石扩充体力到几点
        /// </summary>
        public int itemMaxEnergy;

        /// <summary>
        /// 体力获得秒数
        /// </summary>
        public int recoverEnergyPerSec;

        /// <summary>
        /// 好友互送获得体力
        /// </summary>
        public int friendSentEnergy;

        /// <summary>
        /// 好友获得最大体力
        /// </summary>
        public int friendGetMaxEnergy;

        /// <summary>
        /// 买体力充满(原价)
        /// </summary>
        public int ShopEnergyAddFull;

        /// <summary>
        /// 买体力充满(折扣)
        /// </summary>
        public int ShopEnergyAddFullSale;
 
        /// <summary>
        /// 买体力点数(原价)
        /// </summary>
        public string ShopEnergyAddPoint;

        /// <summary>
        /// 买体力点数(折扣)
        /// </summary>
        public int ShopEnergyAddPointSale;

        /// <summary>
        /// 买体力上限(原价)
        /// </summary>
        public string shopEnergyMaxUp;

        /// <summary>
        /// 买体力上限(折扣)
        /// "购买体力上限
        /// 折扣状况，未开启填-1"
        /// </summary>
        public int shopEnergyMaxUpSale;
 
        /// <summary>
        /// 购买步数价格
        /// </summary>
        public int buyMoves;

        /// <summary>
        /// 购买时间价格
        /// </summary>
        public int buyTime;

        /// <summary>
        /// 购买钥匙价格
        /// </summary>
        public NumPrice BuyKey;

        /// <summary>
        /// 金币购买材料数量
        /// </summary>
        public int BuyUpgradeAForGem;

        /// <summary>
        /// 刷新成就消耗宝石数量
        /// </summary>
        public int AchieveRefreshGem;

        /// <summary>
        /// 攻击倍数
        /// </summary>
        public float ElementAttackMp;

        public int BuyUpgradeBForGem;

        public Dictionary<ColorType, int> ElementATKDict;

        public int FriendWorship;
 
        public GameSystemConfig(XMLInStream inStream)
        {
            int elementPurpleAttack;
            int elementRedAttack;
            int elementGreenAttack;
            int elementBlueAttack;
            int elementYellowAttack;
            string buyMovesStr;
            string buyTimeStr;
            string buyKeyStr;

            inStream.Attribute("gemMaxEnergy", out gemMaxEnergy)
                .Attribute("itemMaxEnergy", out itemMaxEnergy)
                .Attribute("recoverEnergyPerSec", out recoverEnergyPerSec)
                .Attribute("friendSentEnergy", out friendSentEnergy)
                .Attribute("friendGetMaxEnergy", out friendGetMaxEnergy)
                .Attribute("shopEnergyAddFull", out ShopEnergyAddFull)
                .Attribute("shopEnergyAddFullSale", out ShopEnergyAddFullSale)
                .Attribute("elementAttackMp", out ElementAttackMp)
                .Attribute("shopEnergyAddPoint", out ShopEnergyAddPoint)
                .Attribute("shopEnergyAddPointSale", out ShopEnergyAddPointSale)
                .Attribute("shopEnergyMaxUp", out shopEnergyMaxUp)
                .Attribute("shopEnergyMaxUpSale", out shopEnergyMaxUpSale)
                .Attribute("elementPurpleAttack", out elementPurpleAttack)
                .Attribute("elementRedAttack", out elementRedAttack)
                .Attribute("elementGreenAttack", out elementGreenAttack)
                .Attribute("elementBlueAttack", out elementBlueAttack)
                .Attribute("buyMoves", out buyMovesStr)
                .Attribute("buyTime", out buyTimeStr)
                .Attribute("buyUpgradeAForGem", out BuyUpgradeAForGem)
                .Attribute("achieveRefreshGem", out AchieveRefreshGem)
                .Attribute("buyUpgradeBForGem", out BuyUpgradeBForGem)
                .Attribute("buyKey", out buyKeyStr)
                .Attribute("elementYellowAttack", out elementYellowAttack)
                .Attribute("friendWorship", out FriendWorship);
            ElementATKDict = new Dictionary<ColorType, int>(5);
            ElementATKDict.Add(ColorType.Earth, elementPurpleAttack);
            ElementATKDict.Add(ColorType.Fire, elementRedAttack);
            ElementATKDict.Add(ColorType.Wood, elementGreenAttack);
            ElementATKDict.Add(ColorType.Water, elementBlueAttack);
            ElementATKDict.Add(ColorType.Golden, elementYellowAttack);

            buyMoves = int.Parse(buyMovesStr.Split(',')[1]);
            buyTime = int.Parse(buyTimeStr.Split(',')[1]);

            BuyKey = new NumPrice();
            string[] buykeyArr = buyKeyStr.Split(',');
            BuyKey.Num = int.Parse(buykeyArr[0]);
            BuyKey.Price = int.Parse(buykeyArr[1]);
        }

        public struct NumPrice
        {
            public int Num;
            public int Price;
        }
    }
}
