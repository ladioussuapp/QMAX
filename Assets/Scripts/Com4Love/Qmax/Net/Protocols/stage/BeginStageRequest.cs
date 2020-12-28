using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class BeginStageRequest:Protocol
    {
        /// <summary>
        ///  關卡id
        /// </summary>
        public int stageId;
        /// <summary>
        ///  出戰了哪些夥伴
        /// </summary>
        public List<int> unitList;
        /// <summary>
        ///  使用物品 key:id value:num
        /// </summary>
        public Dictionary<int,int> useGoods;
        public BeginStageRequest()
        {
        }
        public BeginStageRequest(int stageId,List<int> unitList,Dictionary<int,int> useGoods)
        {
            this.stageId = stageId;
            this.unitList = unitList;
            this.useGoods = useGoods;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stageId = Serializer.ReadInt32(stream, isLittleEndian);
            unitList = Serializer.ReadList<int>(stream, isLittleEndian);
            useGoods = Serializer.ReadDict<int,int>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stageId, isLittleEndian);
            Serializer.ToBytes(stream, unitList, isLittleEndian);
            Serializer.ToBytes(stream, useGoods, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[BeginStageRequest]:stageId={0},unitList={1},useGoods={2}",stageId,unitList,useGoods);
        }
    }
}
