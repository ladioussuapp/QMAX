using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols
{
    public class ValueResult:Protocol
    {
        /// <summary>
        ///  1 鑰 2 黃毛球 3 藍毛球 4 鑽石 5 體力上限 6 體力 7 夥伴
        ///  物品類型
        /// </summary>
        public int valuesType;
        /// <summary>
        ///  變更後的值
        /// </summary>
        public int current;
        /// <summary>
        ///  變化類型
        ///  0為扣減， 1 為獎勵
        /// </summary>
        public byte changeType;
        /// <summary>
        ///  變更了多少值
        /// </summary>
        public int changeValue;
        public ValueResult()
        {
        }
        public ValueResult(int valuesType,int current,byte changeType,int changeValue)
        {
            this.valuesType = valuesType;
            this.current = current;
            this.changeType = changeType;
            this.changeValue = changeValue;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            valuesType = Serializer.ReadInt32(stream, isLittleEndian);
            current = Serializer.ReadInt32(stream, isLittleEndian);
            changeType = (byte)stream.ReadByte();
            changeValue = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, valuesType, isLittleEndian);
            Serializer.ToBytes(stream, current, isLittleEndian);
            stream.WriteByte(changeType);
            Serializer.ToBytes(stream, changeValue, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ValueResult]:valuesType={0},current={1},changeType={2},changeValue={3}",valuesType,current,changeType,changeValue);
        }
    }
}
