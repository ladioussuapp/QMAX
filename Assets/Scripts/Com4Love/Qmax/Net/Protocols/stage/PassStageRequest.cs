using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class PassStageRequest:Protocol
    {
        /// <summary>
        ///  关卡id
        /// </summary>
        public int stageId;
        /// <summary>
        ///  多少星
        /// </summary>
        public byte star;
        /// <summary>
        ///  选取的伙伴
        /// </summary>
        public List<int> unitList;
        public PassStageRequest()
        {
        }
        public PassStageRequest(int stageId,byte star,List<int> unitList)
        {
            this.stageId = stageId;
            this.star = star;
            this.unitList = unitList;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stageId = Serializer.ReadInt32(stream, isLittleEndian);
            star = (byte)stream.ReadByte();
            unitList = Serializer.ReadList<int>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stageId, isLittleEndian);
            stream.WriteByte(star);
            Serializer.ToBytes(stream, unitList, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[PassStageRequest]:stageId={0},star={1},unitList={2}",stageId,star,unitList);
        }
    }
}
