using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.counterpart
{
    public class CounterpartStageListResponse : Protocol
    {
        /// <summary>
        ///  關卡清單
        /// </summary>
        public List<Stage.Stage> stages;
        public CounterpartStageListResponse()
        {
        }
        public CounterpartStageListResponse(List<Stage.Stage> stages)
        {
            this.stages = stages;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stages = Serializer.ReadList<Stage.Stage>(stream, isLittleEndian);
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