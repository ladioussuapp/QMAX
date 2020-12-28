using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class SubmitStageFightResponse:Protocol
    {
        /// <summary>
        ///  關卡信息
        /// </summary>
        public Stage stage;
        /// <summary>
        ///  道具變化信息
        /// </summary>
        public ValueResultListResponse valueResultResponse;
        /// <summary>
        ///  下一關卡
        /// </summary>
        public Stage nextStage;
        public SubmitStageFightResponse()
        {
        }
        public SubmitStageFightResponse(Stage stage,ValueResultListResponse valueResultResponse,Stage nextStage)
        {
            this.stage = stage;
            this.valueResultResponse = valueResultResponse;
            this.nextStage = nextStage;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stage = Serializer.Read<Stage>(stream, isLittleEndian);
            valueResultResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            nextStage = Serializer.Read<Stage>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stage, isLittleEndian);
            Serializer.ToBytes(stream, valueResultResponse, isLittleEndian);
            Serializer.ToBytes(stream, nextStage, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[SubmitStageFightResponse]:stage={0},valueResultResponse={1},nextStage={2}",stage,valueResultResponse,nextStage);
        }
    }
}
