using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.goods
{
    public class UseGoodsRequest:Protocol
    {
        /// <summary>
        ///  使用物品id
        /// </summary>
        public int goodsId;
        /// <summary>
        ///  使用物品数量
        /// </summary>
        public int useNum;
        public UseGoodsRequest()
        {
        }
        public UseGoodsRequest(int goodsId,int useNum)
        {
            this.goodsId = goodsId;
            this.useNum = useNum;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            goodsId = Serializer.ReadInt32(stream, isLittleEndian);
            useNum = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, goodsId, isLittleEndian);
            Serializer.ToBytes(stream, useNum, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[UseGoodsRequest]:goodsId={0},useNum={1}",goodsId,useNum);
        }
    }
}
