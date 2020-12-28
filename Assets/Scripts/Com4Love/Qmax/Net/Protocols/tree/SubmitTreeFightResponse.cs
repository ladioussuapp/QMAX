using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.tree
{
    public class SubmitTreeFightResponse:Protocol
    {
        /// <summary>
        ///  道具变化信息
        /// </summary>
        public ValueResultListResponse valueResultResponse;
        public SubmitTreeFightResponse()
        {
        }
        public SubmitTreeFightResponse(ValueResultListResponse valueResultResponse)
        {
            this.valueResultResponse = valueResultResponse;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            valueResultResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, valueResultResponse, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[SubmitTreeFightResponse]:valueResultResponse={0}",valueResultResponse);
        }
    }
}