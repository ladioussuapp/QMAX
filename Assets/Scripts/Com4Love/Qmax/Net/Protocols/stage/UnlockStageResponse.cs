using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class UnlockStageResponse:Protocol
    {
        /// <summary>
        ///  数值变化
        /// </summary>
        public ValueResultListResponse valueResultListResponse;
        /// <summary>
        ///  解锁的目标关卡
        /// </summary>
        public Stage unlockStage;
        public UnlockStageResponse()
        {
        }
        public UnlockStageResponse(ValueResultListResponse valueResultListResponse,Stage unlockStage)
        {
            this.valueResultListResponse = valueResultListResponse;
            this.unlockStage = unlockStage;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            valueResultListResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            unlockStage = Serializer.Read<Stage>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, valueResultListResponse, isLittleEndian);
            Serializer.ToBytes(stream, unlockStage, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[UnlockStageResponse]:valueResultListResponse={0},unlockStage={1}",valueResultListResponse,unlockStage);
        }
    }
}
