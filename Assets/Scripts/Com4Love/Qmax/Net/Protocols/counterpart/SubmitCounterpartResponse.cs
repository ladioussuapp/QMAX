using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.counterpart
{
    public class SubmitCounterpartResponse:Protocol
    {
        /// <summary>
        ///  打过的这一关
        /// </summary>
        public Stage.Stage stage;
        /// <summary>
        ///  道具变化信息
        /// </summary>
        public ValueResultListResponse valueResultResponse;
        public SubmitCounterpartResponse()
        {
        }
        public SubmitCounterpartResponse(Stage.Stage stage,ValueResultListResponse valueResultResponse)
        {
            this.stage = stage;
            this.valueResultResponse = valueResultResponse;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stage = Serializer.Read<Stage.Stage>(stream, isLittleEndian);
            valueResultResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stage, isLittleEndian);
            Serializer.ToBytes(stream, valueResultResponse, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[SubmitCounterpartResponse]:stage={0},valueResultResponse={1}",stage,valueResultResponse);
        }
    }
}
