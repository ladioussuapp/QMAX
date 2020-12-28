using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.counterpart
{
    public class CounterpartBeginStageRequest : Protocol
    {
        /// <summary>
        ///  關卡id
        /// </summary>
        public int stageId;
        /// <summary>
        ///  夥伴清單
        /// </summary>
        public List<int> unitList;
        public CounterpartBeginStageRequest()
        {
        }
        public CounterpartBeginStageRequest(int stageId,List<int> unitList)
        {
            this.stageId = stageId;
            this.unitList = unitList;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stageId = Serializer.ReadInt32(stream, isLittleEndian);
            unitList = Serializer.ReadList<int>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stageId, isLittleEndian);
            Serializer.ToBytes(stream, unitList, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[BeginStageRequest]:stageId={0},unitList={1}",stageId,unitList);
        }
    }
}
