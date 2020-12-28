using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.goods;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.getchance
{
    public class GetChanceResponse:Protocol
    {
        /// <summary>
        ///  抽獎id
        /// </summary>
        public int id;
        /// <summary>
        ///  數據結果
        /// </summary>
        public ValueResultListResponse valueResultListResponse;
        /// <summary>
        ///  物品
        /// </summary>
        public List<GoodsItem> valueGoodsItems;
        public GetChanceResponse()
        {
        }
        public GetChanceResponse(int id,ValueResultListResponse valueResultListResponse,List<GoodsItem> valueGoodsItems)
        {
            this.id = id;
            this.valueResultListResponse = valueResultListResponse;
            this.valueGoodsItems = valueGoodsItems;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            id = Serializer.ReadInt32(stream, isLittleEndian);
            valueResultListResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            valueGoodsItems = Serializer.ReadList<GoodsItem>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, id, isLittleEndian);
            Serializer.ToBytes(stream, valueResultListResponse, isLittleEndian);
            Serializer.ToBytes(stream, valueGoodsItems, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[GetChanceResponse]:id={0},valueResultListResponse={1},valueGoodsItems={2}",id,valueResultListResponse,valueGoodsItems);
        }
    }
}