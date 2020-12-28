using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class ListStageDataRequest:Protocol
    {
        /// <summary>
        ///  关卡起点id
        /// </summary>
        public int startId;
        /// <summary>
        ///  关卡终点id
        /// </summary>
        public int endId;
        public ListStageDataRequest()
        {
        }
        public ListStageDataRequest(int startId,int endId)
        {
            this.startId = startId;
            this.endId = endId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            startId = Serializer.ReadInt32(stream, isLittleEndian);
            endId = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, startId, isLittleEndian);
            Serializer.ToBytes(stream, endId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ListStageDataRequest]:startId={0},endId={1}",startId,endId);
        }
    }
}
