using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using Com4Love.Qmax.Net.Protocols.goods;
namespace Com4Love.Qmax.Net.Protocols.sign
{
    public class SignResponse:Protocol
    {
        /// <summary>
        ///  当前使用的签到表id
        /// </summary>
        public int signConfigId;
        /// <summary>
        ///  上次签到的时间
        /// </summary>
        public List<GoodsItem> goodsItems;
        /// <summary>
        ///  奖励的物品
        /// </summary>
        public ValueResultListResponse valueResultListResponse;
        public SignResponse()
        {
        }
        public SignResponse(int signConfigId,List<GoodsItem> goodsItems,ValueResultListResponse valueResultListResponse)
        {
            this.signConfigId = signConfigId;
            this.goodsItems = goodsItems;
            this.valueResultListResponse = valueResultListResponse;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            signConfigId = Serializer.ReadInt32(stream, isLittleEndian);
            goodsItems = Serializer.ReadList<GoodsItem>(stream, isLittleEndian);
            valueResultListResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, signConfigId, isLittleEndian);
            Serializer.ToBytes(stream, goodsItems, isLittleEndian);
            Serializer.ToBytes(stream, valueResultListResponse, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[SignResponse]:signConfigId={0},goodsItems={1},valueResultListResponse={2}",signConfigId,goodsItems,valueResultListResponse);
        }
    }
}