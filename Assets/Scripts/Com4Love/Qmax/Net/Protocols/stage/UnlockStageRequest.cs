using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class UnlockStageRequest:Protocol
    {
        /// <summary>
        ///  關卡id
        /// </summary>
        public int stageId;
        public UnlockStageRequest()
        {
        }
        public UnlockStageRequest(int stageId)
        {
            this.stageId = stageId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stageId = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stageId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[UnlockStageRequest]:stageId={0}",stageId);
        }
    }
}
