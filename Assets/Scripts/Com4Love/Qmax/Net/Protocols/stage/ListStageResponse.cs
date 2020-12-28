using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class ListStageResponse:Protocol
    {
        /// <summary>
        ///  关卡列表
        /// </summary>
        public List<Stage> stages;
        public ListStageResponse()
        {
        }
        public ListStageResponse(List<Stage> stages)
        {
            this.stages = stages;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stages = Serializer.ReadList<Stage>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stages, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ListStageResponse]:stages={0}",stages);
        }
    }
}
