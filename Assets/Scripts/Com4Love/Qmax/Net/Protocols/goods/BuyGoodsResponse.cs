using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.goods
{
    public class BuyGoodsResponse:Protocol
    {
        /// <summary>
        ///  購買後增加的物品
        /// </summary>
        public List<GoodsItem> goodsItems;
        /// <summary>
        ///  購買後值變化
        /// </summary>
        public ValueResultListResponse valueResultListResponse;
        public BuyGoodsResponse()
        {
        }
        public BuyGoodsResponse(List<GoodsItem> goodsItems,ValueResultListResponse valueResultListResponse)
        {
            this.goodsItems = goodsItems;
            this.valueResultListResponse = valueResultListResponse;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            goodsItems = Serializer.ReadList<GoodsItem>(stream, isLittleEndian);
            valueResultListResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, goodsItems, isLittleEndian);
            Serializer.ToBytes(stream, valueResultListResponse, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[BuyGoodsResponse]:goodsItems={0},valueResultListResponse={1}",goodsItems,valueResultListResponse);
        }
    }
}
