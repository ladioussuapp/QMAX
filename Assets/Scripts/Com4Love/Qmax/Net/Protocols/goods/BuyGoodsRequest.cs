using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.goods
{
    public class BuyGoodsRequest:Protocol
    {
        /// <summary>
        ///  購買配置id
        /// </summary>
        public List<int> cfgIds;
        public BuyGoodsRequest()
        {
        }
        public BuyGoodsRequest(List<int> cfgIds)
        {
            this.cfgIds = cfgIds;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            cfgIds = Serializer.ReadList<int>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, cfgIds, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[BuyGoodsRequest]:cfgIds={0}",cfgIds);
        }
    }
}
