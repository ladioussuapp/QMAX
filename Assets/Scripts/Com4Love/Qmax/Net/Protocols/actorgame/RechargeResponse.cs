using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using Com4Love.Qmax.Net.Protocols.goods;

namespace Com4Love.Qmax.Net.Protocols.ActorGame
{
    public class RechargeResponse:Protocol
    {
        /// <summary>
        ///  数值
        /// </summary>
        public ValueResultListResponse valueResultListResponse;
        /// <summary>
        ///  物品
        /// </summary>
        public GoodsItem goodsItem;
        public RechargeResponse()
        {
        }
        public RechargeResponse(ValueResultListResponse valueResultListResponse,GoodsItem goodsItem)
        {
            this.valueResultListResponse = valueResultListResponse;
            this.goodsItem = goodsItem;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            valueResultListResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            goodsItem = Serializer.Read<GoodsItem>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, valueResultListResponse, isLittleEndian);
            Serializer.ToBytes(stream, goodsItem, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[RechargeResponse]:valueResultListResponse={0},goodsItem={1}",valueResultListResponse,goodsItem);
        }
    }
}
