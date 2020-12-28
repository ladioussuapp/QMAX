using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.goods
{
    public class GoodsItem:Protocol
    {
        /// <summary>
        ///  物品id
        /// </summary>
        public int id;
        /// <summary>
        ///  物品数量
        /// </summary>
        public int num;
        public GoodsItem()
        {
        }
        public GoodsItem(int id,int num)
        {
            this.id = id;
            this.num = num;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            id = Serializer.ReadInt32(stream, isLittleEndian);
            num = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, id, isLittleEndian);
            Serializer.ToBytes(stream, num, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[GoodsItem]:id={0},num={1}",id,num);
        }
    }
}
