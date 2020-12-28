using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class StageInfoResponse:Protocol
    {
        /// <summary>
        ///  關卡列表
        /// </summary>
        public int passStageId;
        public StageInfoResponse()
        {
        }
        public StageInfoResponse(int passStageId)
        {
            this.passStageId = passStageId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            passStageId = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, passStageId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[StageInfoResponse]:passStageId={0}",passStageId);
        }
    }
}
