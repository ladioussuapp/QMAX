using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.goods
{
    public class GoodsListResponse:Protocol
    {
        /// <summary>
        ///  物品列表
        /// </summary>
        public List<GoodsItem> goodsList;
        public GoodsListResponse()
        {
        }
        public GoodsListResponse(List<GoodsItem> goodsList)
        {
            this.goodsList = goodsList;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            goodsList = Serializer.ReadList<GoodsItem>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, goodsList, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[GoodsListResponse]:goodsList={0}",goodsList);
        }
    }
}
