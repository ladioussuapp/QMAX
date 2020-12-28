using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Energy
{
    public class BuyMaxEnergyRequest:Protocol
    {
        /// <summary>
        ///  类型
        ///  待拓展，可能会是(1.钻石购买  2.黄毛球购买~~~等)
        /// </summary>
        public byte type;
        public BuyMaxEnergyRequest()
        {
        }
        public BuyMaxEnergyRequest(byte type)
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
            return string.Format("[BuyMaxEnergyRequest]:type={0}",type);
        }
    }
}
