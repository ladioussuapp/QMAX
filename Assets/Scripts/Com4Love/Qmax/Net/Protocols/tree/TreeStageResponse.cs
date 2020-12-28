using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.tree
{
    public class TreeStageResponse:Protocol
    {
        /// <summary>
        ///  当前活动的日期
        /// </summary>
        public int activityTime;
        /// <summary>
        ///  关卡编号。

  		/// 活动每天会开多次，用来区分活动
        /// </summary>
        public byte activityId;
        public TreeStageResponse()
        {
        }
        public TreeStageResponse(int activityTime,byte activityId)
        {
            this.activityTime = activityTime;
            this.activityId = activityId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            activityTime = Serializer.ReadInt32(stream, isLittleEndian);
            activityId = (byte)stream.ReadByte();
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, activityTime, isLittleEndian);
            stream.WriteByte(activityId);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[TreeStageResponse]:activityTime={0},activityId={1}",activityTime,activityId);
        }
    }
}