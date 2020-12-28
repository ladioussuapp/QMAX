using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.stage
{
    public class BuyTimeResponse:Protocol
    {
        /// <summary>
        ///  角色数据变化值
        /// </summary>
        public List<ValueResult> valueResults;
        /// <summary>
        ///  增加的时间
        /// </summary>
        public int addTimeNum;
        public BuyTimeResponse()
        {
        }
        public BuyTimeResponse(List<ValueResult> valueResults,int addTimeNum)
        {
            this.valueResults = valueResults;
            this.addTimeNum = addTimeNum;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            valueResults = Serializer.ReadList<ValueResult>(stream, isLittleEndian);
            addTimeNum = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, valueResults, isLittleEndian);
            Serializer.ToBytes(stream, addTimeNum, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[BuyTimeResponse]:valueResults={0},addTimeNum={1}",valueResults,addTimeNum);
        }
    }
}