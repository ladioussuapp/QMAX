using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.activity
{
    public class ExchangeCodeResponse:Protocol
    {
        /// <summary>
        ///  兌換配置id
        /// </summary>
        public int configId;
        /// <summary>
        ///  值變化
        /// </summary>
        public ValueResultListResponse valueResultListResponse;
        public ExchangeCodeResponse()
        {
        }
        public ExchangeCodeResponse(int configId,ValueResultListResponse valueResultListResponse)
        {
            this.configId = configId;
            this.valueResultListResponse = valueResultListResponse;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            configId = Serializer.ReadInt32(stream, isLittleEndian);
            valueResultListResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, configId, isLittleEndian);
            Serializer.ToBytes(stream, valueResultListResponse, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ExchangeCodeResponse]:configId={0},valueResultListResponse={1}",configId,valueResultListResponse);
        }
    }
}