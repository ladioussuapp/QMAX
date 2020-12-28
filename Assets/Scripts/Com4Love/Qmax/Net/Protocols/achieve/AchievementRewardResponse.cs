using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.goods;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.achievement
{
    public class AchievementRewardResponse:Protocol
    {
        /// <summary>
        ///  已經領取獎勵的成就id
        /// </summary>
        public int achievementId;
        /// <summary>
        ///  扣減獎勵數值變化
        /// </summary>
        public List<ValueResult> list;
        /// <summary>
        ///  獎勵的物品
        /// </summary>
        public List<GoodsItem> goodsItems;
        public AchievementRewardResponse()
        {
        }
        public AchievementRewardResponse(int achievementId,List<ValueResult> list,List<GoodsItem> goodsItems)
        {
            this.achievementId = achievementId;
            this.list = list;
            this.goodsItems = goodsItems;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            achievementId = Serializer.ReadInt32(stream, isLittleEndian);
            list = Serializer.ReadList<ValueResult>(stream, isLittleEndian);
            goodsItems = Serializer.ReadList<GoodsItem>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, achievementId, isLittleEndian);
            Serializer.ToBytes(stream, list, isLittleEndian);
            Serializer.ToBytes(stream, goodsItems, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[AchievementRewardResponse]:achievementId={0},list={1},goodsItems={2}",achievementId,list,goodsItems);
        }
    }
}