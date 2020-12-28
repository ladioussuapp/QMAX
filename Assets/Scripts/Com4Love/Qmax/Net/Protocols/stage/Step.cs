using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class Step:Protocol
    {
        /// <summary>
        ///  消除的所有颜色方块 {Remove.id between 1 and 5}
        /// </summary>
        public List<Remove> removeSquare;
        /// <summary>
        ///  消除的所有其他道具
        /// </summary>
        public List<Remove> removeOtherGoods;
        /// <summary>
        ///  是否使用物品,0:不使用，1:使用
        /// </summary>
        public byte useGoods;
        public Step()
        {
        }
        public Step(List<Remove> removeSquare,List<Remove> removeOtherGoods,byte useGoods)
        {
            this.removeSquare = removeSquare;
            this.removeOtherGoods = removeOtherGoods;
            this.useGoods = useGoods;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            removeSquare = Serializer.ReadList<Remove>(stream, isLittleEndian);
            removeOtherGoods = Serializer.ReadList<Remove>(stream, isLittleEndian);
            useGoods = (byte)stream.ReadByte();
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, removeSquare, isLittleEndian);
            Serializer.ToBytes(stream, removeOtherGoods, isLittleEndian);
            stream.WriteByte(useGoods);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[Step]:removeSquare={0},removeOtherGoods={1},useGoods={2}",removeSquare,removeOtherGoods,useGoods);
        }
    }
}
