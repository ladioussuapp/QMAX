using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Unit
{
    public class BuyUpgradeRequest:Protocol
    {
        /// <summary>
        ///  key： 类型，1：是upgradeA，0：upgradeB;value:消耗钻石数
        /// </summary>
        public Dictionary<byte,int> buyArgs;
        public BuyUpgradeRequest()
        {
        }
        public BuyUpgradeRequest(Dictionary<byte,int> buyArgs)
        {
            this.buyArgs = buyArgs;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            buyArgs = Serializer.ReadDict<byte,int>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, buyArgs, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[BuyUpgradeRequest]:buyArgs={0}",buyArgs);
        }
    }
}
