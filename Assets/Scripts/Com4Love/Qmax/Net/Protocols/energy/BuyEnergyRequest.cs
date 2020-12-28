using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Energy
{
    public class BuyEnergyRequest:Protocol
    {
        /// <summary>
        ///  类型
        ///  0 表示充满体力至最大值
        ///  1 表示花费50钻石增加20体力
        ///  2 表示花费100钻石增加40体力
        /// </summary>
        public byte type;
        public BuyEnergyRequest()
        {
        }
        public BuyEnergyRequest(byte type)
        {
            this.type = type;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            type = (byte)stream.ReadByte();
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stream.WriteByte(type);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[BuyEnergyRequest]:type={0}",type);
        }
    }
}
