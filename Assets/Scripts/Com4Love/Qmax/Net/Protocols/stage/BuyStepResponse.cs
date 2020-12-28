using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.stage
{
    public class BuyStepResponse:Protocol
    {
        /// <summary>
        ///  角色数据变化值
        /// </summary>
        public List<ValueResult> valueResults;
        /// <summary>
        ///  增加的步数
        /// </summary>
        public int addStepNum;
        public BuyStepResponse()
        {
        }
        public BuyStepResponse(List<ValueResult> valueResults,int addStepNum)
        {
            this.valueResults = valueResults;
            this.addStepNum = addStepNum;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            valueResults = Serializer.ReadList<ValueResult>(stream, isLittleEndian);
            addStepNum = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, valueResults, isLittleEndian);
            Serializer.ToBytes(stream, addStepNum, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[BuyStepResponse]:valueResults={0},addStepNum={1}",valueResults,addStepNum);
        }
    }
}
